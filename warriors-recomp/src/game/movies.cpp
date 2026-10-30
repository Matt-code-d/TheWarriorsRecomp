#include <cstdint>

#include "core/log.h"
#include "kernel/kernel.h"
#include "xdk/dsound_host.h"
#include "xdk/hle.h"

using namespace kernel;

// PlayMovie(name, skippable) @ 0x24A040 (cdecl). Boot plays "logo" (0x41D9E0, Rockstar) as the only
// non-skippable clip; force skippable so a black/stuck logo cannot hang the title.
// On return (end or skip) silence DirectSound — Bink feeds streams that keep mixing after skip.
constexpr uint32_t kRockstarLogo = 0x41D9E0;

X86_EXTERN_FUNC(__imp__sub_0024A040);
X86_FUNC(sub_0024A040) {
    uint32_t name = R32(ctx.esp + 4);
    if (name == kRockstarLogo) W32(ctx.esp + 8, 1);
    char label[48] = {};
    if (name) {
        const char* p = static_cast<const char*>(HostPtr(name));
        size_t i = 0;
        for (; i + 1 < sizeof(label) && p[i] >= 32 && p[i] < 127; i++) label[i] = p[i];
        label[i] = 0;
    }
    LOGR("movie: PlayMovie begin name=%08X '%s' skip=%u", name, label[0] ? label : "?", R32(ctx.esp + 8));
    __imp__sub_0024A040(ctx, base);
    xdk::DirectSoundSilenceAll();
    LOGR("movie: PlayMovie done name=%08X '%s' — DirectSound silenced", name, label[0] ? label : "?");
}
