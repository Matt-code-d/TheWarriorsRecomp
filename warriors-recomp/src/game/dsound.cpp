#include <cstdint>
#include <cstring>

#include "kernel/memory.h"
#include "xdk/hle.h"

using namespace kernel;

// DirectSound / XACT methods the SDK has no named HLE for. Host buffer/stream objects must not run guest
// console code (Use3DVoiceData writes flags at object+0xB and AVs on a host pointer).

namespace {

void Done(X86Context& ctx, int args = 2) { Return(ctx, 0, args); }

void ZeroVoiceProps(uint32_t props) {
    if (!props) return;
    std::memset(HostPtr(props), 0, 92);
}

template <int Args>
void Ignore(X86Context& ctx) {
    Return(ctx, 0, Args);
}

}  // namespace

// SetLFO — host mixer has none
X86_FUNC(sub_003A9648) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003AA08A) {
    (void)base;
    Done(ctx);
}

// GetVoiceProperties(DSVOICEPROPS*) — XACT 0x3D347C when moving a voice between outputs
X86_FUNC(sub_003A97CE) {
    (void)base;
    ZeroVoiceProps(R32(ctx.esp + 8));
    Done(ctx);
}
X86_FUNC(sub_003AA0D8) {
    (void)base;
    ZeroVoiceProps(R32(ctx.esp + 8));
    Done(ctx);
}

// SetPitch
X86_FUNC(sub_003A95FA) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003AA085) {
    (void)base;
    Done(ctx);
}

// Set3DVoiceData / Use3DVoiceData — guest would touch host object +0xB / +0xB8
X86_FUNC(sub_003A981C) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003AA0DD) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003A986A) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003AA0E2) {
    (void)base;
    Done(ctx);
}

// Fill HRTF function table once (DirectSoundUseFullHRTF is a no-op on the host)
constexpr uint32_t kHrtfTable = 0x3CD868;
X86_EXTERN_FUNC(__imp__sub_003AFC2B);
X86_EXTERN_FUNC(__imp__sub_003AC6DF);
X86_FUNC(sub_003AC6DF) {
    if (!R32(kHrtfTable)) {
        X86_PUSH32(0);
        __imp__sub_003AFC2B(ctx, base);
    }
    __imp__sub_003AC6DF(ctx, base);
}

// IDirectSound_CreateSoundStream → DirectSoundCreateStream
X86_EXTERN_FUNC(sub_003AD589);
X86_FUNC(sub_003AD1C1) {
    uint32_t ret = R32(ctx.esp), desc = R32(ctx.esp + 8), out = R32(ctx.esp + 12);
    ctx.esp += 20;
    X86_PUSH32(out);
    X86_PUSH32(desc);
    X86_PUSH32(ret);
    sub_003AD589(ctx, base);
}

// Unused DS3D cone / mode / factor setters
X86_FUNC(sub_003ABD16) {
    (void)base;
    Ignore<4>(ctx);
}
X86_FUNC(sub_003ABD3A) {
    (void)base;
    Ignore<5>(ctx);
}
X86_FUNC(sub_003ABD6F) {
    (void)base;
    Ignore<3>(ctx);
}
X86_FUNC(sub_003ABDD7) {
    (void)base;
    Ignore<3>(ctx);
}
X86_FUNC(sub_003ABE61) {
    (void)base;
    Ignore<3>(ctx);
}
X86_FUNC(sub_003ABE85) {
    (void)base;
    Ignore<3>(ctx);
}
X86_FUNC(sub_003ABEA9) {
    (void)base;
    Ignore<3>(ctx);
}

// Stream SetOutputBuffer-style thunks XACT uses for submix routing
X86_FUNC(sub_003ABF36) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003ABF40) {
    (void)base;
    Done(ctx);
}
X86_FUNC(sub_003ABF45) {
    (void)base;
    Done(ctx);
}

// PlayEx(this, tLow, tHigh, flags) — same stack as Play; timestamps ignored (play now).
// Ignoring PlayEx/PauseEx left intro/XACT voices running into the tutorial ("wrong" audio).
X86_EXTERN_FUNC(__imp__sub_003A9EA0);  // IDirectSoundBuffer_Play
X86_FUNC(sub_003A9EC4) {
    __imp__sub_003A9EA0(ctx, base);
}

// PauseEx(this, tLow, tHigh, flags) → Pause(this, flags) immediately
X86_EXTERN_FUNC(__imp__sub_003A9F24);  // IDirectSoundBuffer_Pause
X86_FUNC(sub_003A9F40) {
    uint32_t ret = R32(ctx.esp), thiz = R32(ctx.esp + 4), flags = R32(ctx.esp + 16);
    ctx.esp += 20;
    X86_PUSH32(flags);
    X86_PUSH32(thiz);
    X86_PUSH32(ret);
    __imp__sub_003A9F24(ctx, base);
}

X86_FUNC(sub_003AA00C) {
    (void)base;
    Done(ctx);
}
