// Controllers through SDL; optional keyboard/mouse on port 0 (Win32).
// Mouse look matches the usual PC capture model: click to capture, relative deltas, hidden
// cursor — no SetCursorPos recenter (that left a frozen cursor in the middle of the window).
#include "core/input.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <windows.h>

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <iterator>
#include <mutex>
#include <utility>

#include <SDL3/SDL.h>

#include "app/app.h"
#include "core/log.h"

namespace input {

namespace {

// recursive: ReleaseCapture/SetCapture can re-enter WndProc → HandleWindowMessage on this thread.
std::recursive_mutex g_lock;
bool g_sdl = false;
SDL_Gamepad* g_pads[4] = {};

// ---- mouse capture / look (port 0) ----
bool g_captured = false;
bool g_raw_registered = false;
bool g_btn_left = false, g_btn_right = false, g_btn_middle = false;
int g_acc_dx = 0, g_acc_dy = 0;
float g_look_x = 0.f, g_look_y = 0.f;
double g_last_frame = -1.0;
LARGE_INTEGER g_qpc_freq{};

constexpr float kMouseFullSpeed = 1500.f;  // px/s → full stick
constexpr float kMouseDeadzone = 0.15f;
constexpr float kMouseSmooth = 0.05f;
constexpr float kMouseSensitivity = 1.f;

void AddPad(SDL_JoystickID id) {
    for (SDL_Gamepad* pad : g_pads)
        if (pad && SDL_GetGamepadID(pad) == id) return;
    SDL_Gamepad** slot = std::find(std::begin(g_pads), std::end(g_pads), nullptr);
    if (slot == std::end(g_pads)) return;
    *slot = SDL_OpenGamepad(id);
    const char* name = *slot ? SDL_GetGamepadName(*slot) : SDL_GetError();
    LOGI("input: controller %d %s: %s", int(slot - g_pads) + 1, *slot ? "plugged in" : "failed to open", name ? name : "");
}

void RemovePad(SDL_JoystickID id) {
    for (int i = 0; i < 4; i++) {
        if (!g_pads[i] || SDL_GetGamepadID(g_pads[i]) != id) continue;
        SDL_CloseGamepad(g_pads[i]);
        g_pads[i] = nullptr;
        LOGI("input: controller %d unplugged", i + 1);
    }
}

void UpdatePads() {
    static uint64_t last = 0;
    uint64_t now = SDL_GetTicks();
    if (!g_sdl || now == last) return;
    last = now;
    SDL_PumpEvents();
    SDL_Event e;
    while (SDL_PeepEvents(&e, 1, SDL_GETEVENT, SDL_EVENT_FIRST, SDL_EVENT_LAST) == 1) {
        if (e.type == SDL_EVENT_GAMEPAD_ADDED) AddPad(e.gdevice.which);
        else if (e.type == SDL_EVENT_GAMEPAD_REMOVED) RemovePad(e.gdevice.which);
    }
}

bool KeyDown(int vk) { return (GetAsyncKeyState(vk) & 0x8000) != 0; }

int16_t AxisFromKeys(bool neg, bool pos) {
    if (neg == pos) return 0;
    return pos ? int16_t(32767) : int16_t(-32767);
}

void OrAnalog(uint8_t& dst, uint8_t v) {
    if (v > dst) dst = v;
}

double NowSeconds() {
    LARGE_INTEGER t{};
    QueryPerformanceCounter(&t);
    return double(t.QuadPart) / double(g_qpc_freq.QuadPart);
}

void HideCursorFully() {
    while (ShowCursor(FALSE) >= 0) {
    }
}

void ShowCursorFully() {
    while (ShowCursor(TRUE) < 0) {
    }
}

void ResetLook() {
    g_acc_dx = g_acc_dy = 0;
    g_look_x = g_look_y = 0.f;
    g_last_frame = -1.0;
    g_btn_left = g_btn_right = g_btn_middle = false;
}

void EnsureRawInput(HWND hwnd) {
    if (g_raw_registered || !hwnd) return;
    RAWINPUTDEVICE rid{};
    rid.usUsagePage = 0x01;  // generic desktop
    rid.usUsage = 0x02;      // mouse
    rid.dwFlags = RIDEV_INPUTSINK;
    rid.hwndTarget = hwnd;
    if (RegisterRawInputDevices(&rid, 1, sizeof(rid))) {
        g_raw_registered = true;
        LOGI("input: raw mouse registered");
    } else {
        LOGW("input: RegisterRawInputDevices failed (%lu)", GetLastError());
    }
}

void SetCaptured(HWND hwnd, bool on) {
    if (on == g_captured) return;
    if (on) {
        if (!hwnd || !IsWindow(hwnd)) return;
        EnsureRawInput(hwnd);
        ResetLook();
        g_captured = true;
        // Win32 may re-enter WndProc from these calls — lock is recursive on purpose.
        SetCapture(hwnd);
        RECT rc{};
        GetClientRect(hwnd, &rc);
        POINT tl{rc.left, rc.top}, br{rc.right, rc.bottom};
        ClientToScreen(hwnd, &tl);
        ClientToScreen(hwnd, &br);
        RECT clip{tl.x, tl.y, br.x, br.y};
        ClipCursor(&clip);
        HideCursorFully();
        SetCursor(nullptr);
        LOGR("input: mouse captured (F1 or Escape releases)");
    } else {
        g_captured = false;
        ResetLook();
        ClipCursor(nullptr);
        if (GetCapture() == hwnd) ReleaseCapture();
        ShowCursorFully();
        LOGR("input: mouse released");
    }
}

int16_t StickFromLook(float v) {
    float a = std::fabs(v);
    if (a < 0.01f) return 0;
    a = kMouseDeadzone + a * (1.f - kMouseDeadzone);
    if (a > 1.f) a = 1.f;
    return int16_t(std::lrint((v < 0.f ? -a : a) * 32767.f));
}

void UpdateMouseLook(int16_t& rx, int16_t& ry) {
    if (!g_captured) {
        rx = ry = 0;
        return;
    }
    double now = NowSeconds();
    double dt = g_last_frame < 0 ? 1.0 / 60.0 : now - g_last_frame;
    g_last_frame = now;
    if (dt < 0.001) dt = 0.001;
    if (dt > 0.1) dt = 0.1;

    int dx = g_acc_dx, dy = g_acc_dy;
    g_acc_dx = g_acc_dy = 0;

    float inv = app::Get().invert_camera_y ? -1.f : 1.f;
    // Screen y down → stick up is negative dy in WR (they negate dy).
    float tx = float(double(dx) / dt / kMouseFullSpeed) * kMouseSensitivity;
    float ty = float(double(-dy) / dt / kMouseFullSpeed) * kMouseSensitivity * inv;
    float alpha = kMouseSmooth > 0.f ? 1.f - std::exp(float(-dt / kMouseSmooth)) : 1.f;
    g_look_x = std::clamp(g_look_x + (tx - g_look_x) * alpha, -1.f, 1.f);
    g_look_y = std::clamp(g_look_y + (ty - g_look_y) * alpha, -1.f, 1.f);
    rx = StickFromLook(g_look_x);
    ry = StickFromLook(g_look_y);
}

void MergeKeyboardMouse(int port, Pad& p) {
    if (port != 0 || !app::Get().keyboard_controls) return;

    HWND hwnd = static_cast<HWND>(app::Window());
    if (!hwnd || !IsWindow(hwnd)) return;

    // Keys work whenever the game window is foreground (capture only gates mouse look).
    if (GetForegroundWindow() == hwnd) {
        if (KeyDown(VK_UP)) p.buttons |= 1u << 0;
        if (KeyDown(VK_DOWN)) p.buttons |= 1u << 1;
        if (KeyDown(VK_LEFT)) p.buttons |= 1u << 2;
        if (KeyDown(VK_RIGHT)) p.buttons |= 1u << 3;
        if (KeyDown(VK_RETURN)) p.buttons |= 1u << 4;
        // Escape is reserved (mouse capture / release), never mapped to the pad — same as WR.
        // Back = Backspace only.
        if (KeyDown(VK_BACK)) p.buttons |= 1u << 5;

        if (KeyDown(VK_SPACE)) OrAnalog(p.analog[0], 0xFF);
        if (KeyDown(VK_LSHIFT) || KeyDown(VK_RSHIFT)) OrAnalog(p.analog[1], 0xFF);
        if (KeyDown('E')) OrAnalog(p.analog[2], 0xFF);
        if (KeyDown('F') || KeyDown('R')) OrAnalog(p.analog[3], 0xFF);
        if (KeyDown('Q')) OrAnalog(p.analog[5], 0xFF);
        if (KeyDown(VK_TAB)) OrAnalog(p.analog[4], 0xFF);

        int16_t kx = AxisFromKeys(KeyDown('A'), KeyDown('D'));
        int16_t ky = AxisFromKeys(KeyDown('S'), KeyDown('W'));
        // Arrows also move when not used as pure dpad-only — keep WASD primary for stick.
        if (std::abs(kx) > std::abs(p.lx)) p.lx = kx;
        if (std::abs(ky) > std::abs(p.ly)) p.ly = ky;

        if (g_captured) {
            if (g_btn_left) OrAnalog(p.analog[7], 0xFF);
            if (g_btn_right) OrAnalog(p.analog[6], 0xFF);
            if (g_btn_middle) OrAnalog(p.analog[2], 0xFF);
        }

        p.connected = true;
    }

    int16_t mx = 0, my = 0;
    UpdateMouseLook(mx, my);
    if (std::abs(mx) > std::abs(p.rx)) p.rx = mx;
    if (std::abs(my) > std::abs(p.ry)) p.ry = my;
}

}  // namespace

void Init() {
    QueryPerformanceFrequency(&g_qpc_freq);
    if (g_sdl) return;
    SDL_SetHint(SDL_HINT_JOYSTICK_THREAD, "1");
    SDL_SetHint(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1");
    SDL_SetHint(SDL_HINT_JOYSTICK_WGI, "1");
    SDL_SetHint(SDL_HINT_JOYSTICK_RAWINPUT, "0");
    SDL_SetHint(SDL_HINT_XINPUT_ENABLED, "0");
    SDL_SetHint(SDL_HINT_JOYSTICK_DIRECTINPUT, "0");
    if (!SDL_Init(SDL_INIT_GAMEPAD)) {
        LOGE("input: SDL failed to start, no controllers: %s", SDL_GetError());
        return;
    }
    int v = SDL_GetVersion();
    LOGI("input: SDL %d.%d.%d", SDL_VERSIONNUM_MAJOR(v), SDL_VERSIONNUM_MINOR(v), SDL_VERSIONNUM_MICRO(v));
    std::lock_guard lock(g_lock);
    g_sdl = true;
    int count = 0;
    if (SDL_JoystickID* ids = SDL_GetGamepads(&count)) {
        for (int i = 0; i < count; i++) AddPad(ids[i]);
        SDL_free(ids);
    }
    LOGI("input: keyboard on; mouse look: click to capture, F1 toggles, Escape releases");
}

bool MouseCaptured() {
    std::lock_guard lock(g_lock);
    return g_captured;
}

bool HandleWindowMessage(void* hwnd_v, unsigned msg, std::uintptr_t wp, std::intptr_t lp) {
    if (!app::Get().keyboard_controls) return false;
    HWND hwnd = static_cast<HWND>(hwnd_v);
    std::lock_guard lock(g_lock);

    switch (msg) {
        case WM_SETCURSOR:
            if (g_captured && LOWORD(lp) == HTCLIENT) {
                SetCursor(nullptr);
                return true;
            }
            break;

        case WM_ACTIVATE:
            if (LOWORD(wp) == WA_INACTIVE) SetCaptured(hwnd, false);
            break;

        case WM_KILLFOCUS:
            SetCaptured(hwnd, false);
            break;

        case WM_CAPTURECHANGED:
            // Capture stolen (or our own ReleaseCapture): clear look state without calling
            // ReleaseCapture again (that would recurse forever / deadlock on a non-recursive mutex).
            if (g_captured && reinterpret_cast<HWND>(lp) != hwnd) {
                g_captured = false;
                ResetLook();
                ClipCursor(nullptr);
                ShowCursorFully();
                LOGR("input: mouse capture lost");
            }
            break;

        case WM_LBUTTONDOWN:
            if (!g_captured) {
                SetCaptured(hwnd, true);
                return true;  // capturing click is not a pad press
            }
            g_btn_left = true;
            return true;

        case WM_LBUTTONUP:
            if (g_captured) {
                g_btn_left = false;
                return true;
            }
            break;

        case WM_RBUTTONDOWN:
            if (g_captured) {
                g_btn_right = true;
                return true;
            }
            break;

        case WM_RBUTTONUP:
            if (g_captured) {
                g_btn_right = false;
                return true;
            }
            break;

        case WM_MBUTTONDOWN:
            if (g_captured) {
                g_btn_middle = true;
                return true;
            }
            break;

        case WM_MBUTTONUP:
            if (g_captured) {
                g_btn_middle = false;
                return true;
            }
            break;

        case WM_KEYDOWN:
            if (wp == VK_F1 && (lp & (1 << 30)) == 0) {
                SetCaptured(hwnd, !g_captured);
                return true;
            }
            // Escape is reserved: release capture if held; never forward to the title.
            if (wp == VK_ESCAPE) {
                if ((lp & (1 << 30)) == 0 && g_captured) SetCaptured(hwnd, false);
                return true;
            }
            break;

        case WM_INPUT: {
            if (!g_captured) break;
            UINT size = 0;
            GetRawInputData(reinterpret_cast<HRAWINPUT>(lp), RID_INPUT, nullptr, &size, sizeof(RAWINPUTHEADER));
            if (!size || size > 256) break;
            alignas(RAWINPUT) uint8_t buf[256];
            if (GetRawInputData(reinterpret_cast<HRAWINPUT>(lp), RID_INPUT, buf, &size, sizeof(RAWINPUTHEADER)) ==
                UINT(-1))
                break;
            auto* raw = reinterpret_cast<RAWINPUT*>(buf);
            if (raw->header.dwType != RIM_TYPEMOUSE) break;
            const RAWMOUSE& m = raw->data.mouse;
            if (!(m.usFlags & MOUSE_MOVE_ABSOLUTE)) {
                g_acc_dx += m.lLastX;
                g_acc_dy += m.lLastY;
            }
            return false;  // still let DefWindowProc clean up
        }
    }
    return false;
}

Pad Poll(int port) {
    Pad p;
    {
        std::lock_guard lock(g_lock);
        UpdatePads();
        if (SDL_Gamepad* pad = port >= 0 && port < 4 ? g_pads[port] : nullptr) {
            p.connected = true;
            constexpr SDL_GamepadButton kButtons[] = {
                SDL_GAMEPAD_BUTTON_DPAD_UP,   SDL_GAMEPAD_BUTTON_DPAD_DOWN,  SDL_GAMEPAD_BUTTON_DPAD_LEFT,
                SDL_GAMEPAD_BUTTON_DPAD_RIGHT, SDL_GAMEPAD_BUTTON_START,     SDL_GAMEPAD_BUTTON_BACK,
                SDL_GAMEPAD_BUTTON_LEFT_STICK, SDL_GAMEPAD_BUTTON_RIGHT_STICK};
            for (int i = 0; i < 8; i++) p.buttons |= uint16_t(SDL_GetGamepadButton(pad, kButtons[i]) << i);
            constexpr SDL_GamepadButton kAnalog[] = {SDL_GAMEPAD_BUTTON_SOUTH, SDL_GAMEPAD_BUTTON_EAST,
                                                     SDL_GAMEPAD_BUTTON_WEST,  SDL_GAMEPAD_BUTTON_NORTH,
                                                     SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER, SDL_GAMEPAD_BUTTON_LEFT_SHOULDER};
            for (int i = 0; i < 6; i++) p.analog[i] = SDL_GetGamepadButton(pad, kAnalog[i]) ? 0xFF : 0;
            auto axis = [&](SDL_GamepadAxis a) { return int(SDL_GetGamepadAxis(pad, a)); };
            p.analog[6] = uint8_t(std::max(axis(SDL_GAMEPAD_AXIS_LEFT_TRIGGER), 0) >> 7);
            p.analog[7] = uint8_t(std::max(axis(SDL_GAMEPAD_AXIS_RIGHT_TRIGGER), 0) >> 7);
            p.lx = int16_t(axis(SDL_GAMEPAD_AXIS_LEFTX));
            p.ly = int16_t(-1 - axis(SDL_GAMEPAD_AXIS_LEFTY));
            p.rx = int16_t(axis(SDL_GAMEPAD_AXIS_RIGHTX));
            p.ry = int16_t(-1 - axis(SDL_GAMEPAD_AXIS_RIGHTY));
        }
        MergeKeyboardMouse(port, p);
    }
    if (app::kGame.pad) app::kGame.pad(port, p);
    return p;
}

uint16_t HeldOnAnyController() {
    constexpr std::pair<SDL_GamepadButton, uint16_t> kFaceButtons[] = {
        {SDL_GAMEPAD_BUTTON_SOUTH, 0x1000}, {SDL_GAMEPAD_BUTTON_EAST, 0x2000},
        {SDL_GAMEPAD_BUTTON_WEST, 0x4000},  {SDL_GAMEPAD_BUTTON_NORTH, 0x8000}};
    std::lock_guard lock(g_lock);
    UpdatePads();
    uint16_t held = 0;
    for (SDL_Gamepad* pad : g_pads)
        for (auto [button, mask] : kFaceButtons)
            if (pad && SDL_GetGamepadButton(pad, button)) held |= mask;
    if (app::Get().keyboard_controls) {
        if (KeyDown(VK_SPACE)) held |= 0x1000;
        if (KeyDown(VK_LSHIFT) || KeyDown(VK_RSHIFT)) held |= 0x2000;
        if (KeyDown('E')) held |= 0x4000;
        if (KeyDown('F') || KeyDown('R')) held |= 0x8000;
    }
    return held;
}

void Rumble(int port, uint16_t left, uint16_t right) {
    if (app::kGame.rumble) app::kGame.rumble(port, left, right);
    if (!app::Get().vibration) return;
    std::lock_guard lock(g_lock);
    if (port >= 0 && port < 4 && g_pads[port]) SDL_RumbleGamepad(g_pads[port], left, right, 0);
}

}  // namespace input
