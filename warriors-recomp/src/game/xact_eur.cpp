// EUR XACT SoundBank/SoundSource Stop wrappers.
// Title Stop runs for cue bookkeeping; then retire mixin feeders left looping
// (ambient beds hard-stop; music without a mix-in output is untouched).

#include <cstdint>

#include "core/log.h"
#include "cpu/x86_ops.h"
#include "kernel/kernel.h"
#include "xdk/dsound_host.h"
#include "xdk/hle.h"

using namespace kernel;

namespace {

constexpr uint8_t kStopBankSig[] = {0x56, 0x57, 0xE8};
constexpr uint8_t kStopSrcSig[] = {0xE9};

bool BytesMatch(uint8_t* base, uint32_t va, const uint8_t* sig, size_t n) {
    for (size_t i = 0; i < n; i++) {
        if (*(base + va + i) != sig[i]) return false;
    }
    return true;
}

}  // namespace

X86_EXTERN_FUNC(__imp__sub_003CE1D7);
X86_EXTERN_FUNC(__imp__sub_003CE3B7);

X86_FUNC(sub_003CE1D7) {
    static bool once = false;
    if (!once && BytesMatch(base, 0x003CE1D7, kStopBankSig, sizeof(kStopBankSig))) {
        once = true;
        LOGR("xact: SoundBank_Stop @ 003CE1D7 → title Stop + placed-loop wind-down");
    }
    __imp__sub_003CE1D7(ctx, base);
    xdk::DirectSoundWindDownPlacedLoops();
}

X86_FUNC(sub_003CE3B7) {
    static bool once = false;
    if (!once && BytesMatch(base, 0x003CE3B7, kStopSrcSig, sizeof(kStopSrcSig))) {
        once = true;
        LOGR("xact: SoundSource_Stop @ 003CE3B7 → title Stop + placed-loop wind-down");
    }
    __imp__sub_003CE3B7(ctx, base);
    xdk::DirectSoundWindDownPlacedLoops();
}
