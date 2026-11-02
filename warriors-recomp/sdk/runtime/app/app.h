#pragma once
#include <chrono>
#include <cstdint>
#include <filesystem>
#include <mutex>
#include <string>
#include <utility>
#include <vector>

namespace input {
struct Pad;
}

// the host side: window, settings
namespace app {

enum class DisplayMode : uint32_t { Fullscreen, Borderless, Windowed };
inline constexpr uint32_t kFpsCapRefresh = 1;  // fps_cap: the monitor's refresh rate

struct Settings {
    DisplayMode display_mode = DisplayMode::Borderless;
    uint32_t width = 0, height = 0;  // 0 is the desktop mode
    uint32_t render_percent = 100;   // of the output height
    // EUR PAL content is 50 Hz. Default 50 — 60 makes the whole game ~20% fast (VBlank/logic).
    // 0 = unlimited (also accelerates), kFpsCapRefresh = monitor Hz.
    uint32_t fps_cap = 50;
    bool vsync = true;
    // FSR 1.0 present upscale: 0 off, 1 ultra quality, 2 quality, 3 balanced, 4 performance.
    // When on, BlitFrame uses EASU+RCAS instead of bilinear (pair with render_percent < 100).
    uint32_t fsr = 0;
    uint32_t aniso = 0;              // 0 as the game asks
    bool custom_textures = true;     // images in textures\replacement\ beside the exe replace the game's
    bool dump_textures = false;      // each texture once into textures\dumps\ beside the exe
    uint32_t texture_dump_format = 0;  // 0 PNG, 1 DDS
    std::string join_ip;                   // co-op: a host past the LAN the system link search also asks
};

Settings& Get();
void Load();
void Save();
void LogSettings();  // every setting, as the settings file has them
void LogSystem();    // the machine and the settings, at a run's start
void ApplyDisplay();  // any thread
struct Mode {
    uint32_t width, height;
};
Mode Desktop();
Mode Output();
float RenderScale(uint32_t guest_height);
// the output is 3:2 or wider: the console reports a widescreen TV and the frame is shown 16:9, else 4:3 as on a 4:3 TV
bool Widescreen();
// EUR Warriors is PAL-I: guest VBlank / game clock is 50 Hz (not 60). Faster clocks accelerate gameplay
// until sim is decoupled from present rate (see FrameTimeScale / roadmap).
inline constexpr uint32_t kGuestRefreshHz = 50;
inline constexpr uint32_t GuestVBlankUs() { return 1000000u / kGuestRefreshHz; }

// Display target Hz from settings (0 → uncapped / monitor; may exceed guest when dt hooks active).
uint32_t TargetDisplayHz();
// guest_hz / display_hz — 1 at PAL 50; <1 when presenting faster (fixed-step scale).
float FrameTimeScale();
// Last Swap period in seconds (0 until two presents); used to feed real dt when unlocked.
float MeasuredFrameDt();
// Shared frame epoch: VBlank + Pace phase-lock (both follow TargetDisplayHz).
std::chrono::steady_clock::time_point FrameEpoch();
void FrameClockWaitGuest();   // VerticalBlank thread: next blank (display-timed)
void NoteBlockUntilVBlank();  // stats: title waited on VB
void NotePresentedFrame();    // stats + measured dt
// Prefer Present(0)+Pace when monitor Hz does not divide evenly by the target (e.g. 50 on 164).
bool PresentUseVsync();
// Microseconds for one display frame (BlockUntil / raster); follows TargetDisplayHz.
uint32_t DisplayFrameUs();

std::filesystem::path ExeDir();
// the game in assets (game_files.cpp): its folder and this executable's xbe, from the extracted files or a disc
// image (.iso), which it mounts as D:; false after telling the player what is missing
struct GameFiles {
    std::filesystem::path dir;
    std::vector<uint8_t> xbe;
};
bool FindGame(GameFiles& game);
std::string Sha1(const std::vector<uint8_t>& data);  // lower case hex
// the recompiled code, <exe>.dll, built from the game in assets when missing or out of date (game_code.cpp) and
// loaded; false after telling the player why the game cannot start, or when they chose not to start it yet
bool PrepareGameCode(const GameFiles& game);
int Instance();  // --instance N on the command line, 0 without: a second copy on this PC for system link tests
void* Window();  // HWND
// boot splash: GameInfo::splash .bmp or .png beside the exe, up before the game window
bool OpenSplash();        // window thread, at launch; false without one, and the game window opens at once
void SplashFirstFrame();  // the game's first Swap: waits out the splash's minimum, then opens the game window
void CloseSplash();       // window thread
void OpenGameWindow(bool apply = true);  // any thread but the window thread's; returns once the window is up
// relaunched: the previous instance keeps its window up until this one has shown its first frames (handover.cpp)
bool StartHandover();   // window thread, at launch; true: the game window starts hidden
bool HandingOver();
void FinishHandover();  // any thread but the window thread's: opens the game window, lets the old instance go

// held around ImGui: the window thread's messages and the renderer's frame
std::recursive_mutex& UiLock();

// what the game repo defines once
struct Option {  // a setting in the settings file, flag or value
    const char* key;
    bool* flag;
    uint32_t* value;
};
struct GameInfo {
    const wchar_t* title;        // window and splash
    const wchar_t* short_title;  // message boxes
    const wchar_t* name;         // window classes, crash dumps, handover event
    const char* log;             // log prefix
    const char* settings;        // settings file beside the exe
    const wchar_t* splash;       // .bmp or .png beside the exe
    const wchar_t* missing;      // message when neither the game files nor a disc image are found
    std::vector<std::pair<const char*, const char*>> images;  // xbe file name, executable name
    std::vector<Option> options;
    // the settings file's toml::table, for what options cannot hold
    void (*load)(const void* table);
    void (*save)(void* table);
    void (*init)();                // after the settings load, before the window and boot
    void (*loaded)();              // image in memory, before its entry runs
    void (*window)(void* hwnd);    // the game window is up, before boot
    // window thread, each message after ImGui's; true returns result instead of the host's handling
    bool (*message)(void* hwnd, uint32_t msg, uintptr_t wp, intptr_t lp, intptr_t* result);
    void (*ui)(double fps);        // each frame inside the ImGui frame, UiLock held
    void (*pad)(int port, input::Pad& pad);  // a polled pad before the game gets it
    void (*rumble)(int port, uint16_t& left, uint16_t& right);
};
extern const GameInfo kGame;

// waits out the rest of the display frame (same epoch as guest VBlank while locked to 50)
void Pace();

}  // namespace app
