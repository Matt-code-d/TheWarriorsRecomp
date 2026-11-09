// Steady far-plane / fog: divert the hitch pull at 0x32A5E7 so both outcomes of that jbe
// fall through (recomp cannot patch the lifted compare the way a live-image nop would).
// Also pin the threshold/pull RAM cells each frame as a belt-and-braces guard.

#include <cstdint>

#include "core/log.h"
#include "cpu/x86_ops.h"
#include "kernel/kernel.h"
#include "xdk/hle.h"

using namespace kernel;

namespace {

constexpr uint32_t kFogThresholdVa = 0x00445334;
constexpr uint32_t kFogPullVa = 0x00445338;
constexpr uint32_t kFogJbeVa = 0x0032A5E7;

}  // namespace

X86_HOST_EXPORT bool FogSkipHitchPull(X86Context& ctx, uint8_t* base) {
    (void)ctx;
    (void)base;
    // Keep hitch pull armed off even if guest RAM is rewritten.
    W32(kFogThresholdVa, 0xBF800000u);  // -1.f
    W32(kFogPullVa, 0);
    static bool once = false;
    if (!once) {
        once = true;
        LOGR("drawdist: FogSkipHitchPull — jbe %08X diverted (steady fog)", kFogJbeVa);
    }
    return true;
}
