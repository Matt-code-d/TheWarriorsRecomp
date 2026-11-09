// the settings file beside the exe
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <windows.h>

#include <algorithm>
#include <atomic>
#include <fstream>
#include <mutex>
#include <sstream>

#include <toml++/toml.hpp>

#include "app/app.h"
#include "core/log.h"

namespace app {

namespace {

const char* kModes[] = {"fullscreen", "borderless", "windowed"};

std::filesystem::path File() { return ExeDir() / kGame.settings; }

}  // namespace

Settings& Get() {
    static Settings s;
    return s;
}

void Load() {
    Settings& s = Get();
    toml::table t;
    try {
        t = toml::parse_file(File().string());
    } catch (...) {
        return;
    }
    std::string mode = t["display_mode"].value_or(std::string(kModes[uint32_t(s.display_mode)]));
    for (uint32_t i = 0; i < 3; i++)
        if (mode == kModes[i]) s.display_mode = DisplayMode(i);
    s.width = t["width"].value_or(s.width);
    s.height = t["height"].value_or(s.height);
    s.render_percent = t["render_percent"].value_or(s.render_percent);
    s.fps_cap = t["fps_cap"].value_or(s.fps_cap);
    // 0 = uncapped, 1 = monitor refresh, else clamp to a sane cap (dt hooks keep PAL speed).
    if (s.fps_cap > 240 && s.fps_cap != kFpsCapRefresh) s.fps_cap = 240;
    s.vsync = t["vsync"].value_or(s.vsync);
    s.aniso = t["anisotropic_filtering"].value_or(s.aniso);
    s.custom_textures = t["custom_textures"].value_or(s.custom_textures);
    s.dump_textures = t["dump_textures"].value_or(s.dump_textures);
    s.texture_dump_format = std::min(t["texture_dump_format"].value_or(s.texture_dump_format), 1u);
    s.join_ip = t["join_ip"].value_or(s.join_ip);
    s.upscaler = t["upscaler"].value_or(s.upscaler);
    s.upscale_quality = t["upscale_quality"].value_or(s.upscale_quality);
    s.frame_generation = t["frame_generation"].value_or(s.frame_generation);
    s.dlss_preset = t["dlss_preset"].value_or(s.dlss_preset);
    s.sharpness = std::min(t["sharpness"].value_or(s.sharpness), 100u);
    s.perf_overlay = t["perf_overlay"].value_or(s.perf_overlay);
    s.perf_overlay_scale = t["perf_overlay_scale"].value_or(s.perf_overlay_scale);
    s.keyboard_controls = t["keyboard_controls"].value_or(s.keyboard_controls);
    s.invert_camera_y = t["invert_camera_y"].value_or(s.invert_camera_y);
    s.vibration = t["vibration"].value_or(s.vibration);
    s.filmic = t["filmic"].value_or(s.filmic);
    s.filmic_strength = std::min(t["filmic_strength"].value_or(s.filmic_strength), 100u);
    s.filmic_grain = std::min(t["filmic_grain"].value_or(s.filmic_grain), 100u);
    s.filmic_vignette = std::min(t["filmic_vignette"].value_or(s.filmic_vignette), 100u);
    s.filmic_lut = t["filmic_lut"].value_or(s.filmic_lut);
    s.scene_glow = t["scene_glow"].value_or(s.scene_glow);
    s.scene_glow_strength = std::min(t["scene_glow_strength"].value_or(s.scene_glow_strength), 100u);
    s.audio_resampler = t["audio_resampler"].value_or(s.audio_resampler);
    s.pipeline_cache = t["pipeline_cache"].value_or(s.pipeline_cache);
    s.gpu_skinning = t["gpu_skinning"].value_or(s.gpu_skinning);

    // Map reference-style upscaler/quality → FSR1 quality index. DLSS/XeSS need Streamline/XeSS
    // runtime (not in this host yet) — fall back to FSR1 so render scale still helps.
    auto quality_to_fsr = [](const std::string& q) -> uint32_t {
        std::string k = q;
        for (char& c : k)
            if (c >= 'A' && c <= 'Z') c = char(c - 'A' + 'a');
        for (char& c : k)
            if (c == ' ' || c == '-') c = '_';
        if (k == "native" || k == "dlaa" || k == "off") return 0;
        if (k == "ultra_quality" || k == "ultraquality") return 1;
        if (k == "quality") return 2;
        if (k == "balanced") return 3;
        if (k == "performance") return 4;
        if (k == "ultra_performance" || k == "ultraperformance") return 4;
        return 2;
    };
    {
        std::string u = s.upscaler;
        for (char& c : u)
            if (c >= 'A' && c <= 'Z') c = char(c - 'A' + 'a');
        bool has_upscaler = t.contains("upscaler");
        bool has_legacy_fsr = t.contains("fsr");
        if (has_upscaler) {
            if (u == "off" || u.empty()) {
                s.fsr = 0;
            } else if (u == "fsr" || u == "fsr1") {
                s.fsr = quality_to_fsr(s.upscale_quality);
            } else {
                // dlss / xess / unknown — spatial FSR1 stand-in until vendor SDKs are wired.
                s.fsr = quality_to_fsr(s.upscale_quality);
                LOGR("settings: upscaler='%s' not built into this host — using FSR 1.0 (quality=%s)",
                     s.upscaler.c_str(), s.upscale_quality.c_str());
            }
        } else if (has_legacy_fsr) {
            s.fsr = std::min(t["fsr"].value_or(s.fsr), 4u);
            if (s.fsr) {
                s.upscaler = "fsr";
                static const char* qn[] = {"off", "ultra_quality", "quality", "balanced", "performance"};
                s.upscale_quality = qn[std::min(s.fsr, 4u)];
            }
        }
        if (s.frame_generation != "off" && !s.frame_generation.empty()) {
            LOGR("settings: frame_generation='%s' is not available in this host yet",
                 s.frame_generation.c_str());
        }
    }
    for (const Option& o : kGame.options) {
        if (o.flag) *o.flag = t[o.key].value_or(*o.flag);
        else *o.value = t[o.key].value_or(*o.value);
    }
    if (kGame.load) kGame.load(&t);
}

namespace {

toml::table Table() {
    const Settings& s = Get();
    toml::table t{{"display_mode", kModes[uint32_t(s.display_mode)]},
                  {"width", int64_t(s.width)},
                  {"height", int64_t(s.height)},
                  {"render_percent", int64_t(s.render_percent)},
                  {"fps_cap", int64_t(s.fps_cap)},
                  {"vsync", s.vsync},
                  {"upscaler", s.upscaler},
                  {"upscale_quality", s.upscale_quality},
                  {"frame_generation", s.frame_generation},
                  {"dlss_preset", s.dlss_preset},
                  {"sharpness", int64_t(s.sharpness)},
                  {"fsr", int64_t(s.fsr)},
                  {"anisotropic_filtering", int64_t(s.aniso)},
                  {"perf_overlay", int64_t(s.perf_overlay)},
                  {"perf_overlay_scale", int64_t(s.perf_overlay_scale)},
                  {"keyboard_controls", s.keyboard_controls},
                  {"invert_camera_y", s.invert_camera_y},
                  {"vibration", s.vibration},
                  {"custom_textures", s.custom_textures},
                  {"dump_textures", s.dump_textures},
                  {"texture_dump_format", int64_t(s.texture_dump_format)},
                  {"join_ip", s.join_ip},
                  {"filmic", s.filmic},
                  {"filmic_strength", int64_t(s.filmic_strength)},
                  {"filmic_grain", int64_t(s.filmic_grain)},
                  {"filmic_vignette", int64_t(s.filmic_vignette)},
                  {"filmic_lut", s.filmic_lut},
                  {"scene_glow", s.scene_glow},
                  {"scene_glow_strength", int64_t(s.scene_glow_strength)},
                  {"audio_resampler", s.audio_resampler},
                  {"pipeline_cache", s.pipeline_cache},
                  {"gpu_skinning", s.gpu_skinning}};
    for (const Option& o : kGame.options) {
        if (o.flag) t.insert(o.key, *o.flag);
        else t.insert(o.key, int64_t(*o.value));
    }
    if (kGame.save) kGame.save(&t);
    return t;
}

}  // namespace

void Save() {
    std::ofstream f(File());
    f << Table() << "\n";
}

void LogSettings() {
    std::ostringstream text;
    text << Table();
    std::istringstream lines(text.str());
    LOGR("Settings:");
    for (std::string line; std::getline(lines, line);)
        if (!line.empty()) LOGR("  %s", line.c_str());
}

Mode Desktop() {
    HMONITOR mon = MonitorFromWindow(static_cast<HWND>(Window()), MONITOR_DEFAULTTOPRIMARY);
    MONITORINFOEXW mi{};
    mi.cbSize = sizeof(mi);
    DEVMODEW dm{};
    dm.dmSize = sizeof(dm);
    if (GetMonitorInfoW(mon, &mi) && EnumDisplaySettingsW(mi.szDevice, ENUM_REGISTRY_SETTINGS, &dm))
        return {dm.dmPelsWidth, dm.dmPelsHeight};
    return {uint32_t(GetSystemMetrics(SM_CXSCREEN)), uint32_t(GetSystemMetrics(SM_CYSCREEN))};
}

Mode Output() {
    const Settings& s = Get();
    if (s.width && s.height) return {s.width, s.height};
    return Desktop();
}

bool Widescreen() {
    // asked per frame and per HUD quad: the desktop mode lookup only runs when the resolution setting changes
    static std::atomic<uint64_t> key{~0ull};
    static std::atomic<bool> wide{true};
    const Settings& s = Get();
    uint64_t k = uint64_t(s.width) << 32 | s.height;
    if (key.exchange(k) != k) {
        Mode m = Output();
        wide = m.width * 2 >= m.height * 3;  // 3:2 or wider
    }
    return wide;
}

// Output for every frame: the desktop mode lookup reads the registry, so it runs again only when the resolution
// setting or the window's monitor changes
Mode CachedOutput() {
    struct Key {
        uint32_t width;
        uint32_t height;
        HMONITOR monitor;
        bool operator==(const Key&) const = default;
    };
    static std::mutex lock;
    static Key key{};
    static Mode mode{};
    const Settings& s = Get();
    Key now{s.width, s.height, MonitorFromWindow(static_cast<HWND>(Window()), MONITOR_DEFAULTTOPRIMARY)};
    std::lock_guard<std::mutex> guard(lock);
    if (!(now == key) || !mode.height) {
        key = now;
        mode = Output();
    }
    return mode;
}

float RenderScale(uint32_t guest_height) {
    float h = CachedOutput().height * (Get().render_percent / 100.0f);
    return guest_height ? std::max(h / guest_height, 0.1f) : 1.0f;
}

}  // namespace app
