// Decouple sim from display FPS: scale PAL fixed-step floats (~0.02 = 1/50) when presenting faster.
// Getters/setters live next to the Swap wrapper @ 0xB7AC0 (object fields +0xDC..+0xE8).

#include <algorithm>
#include <cmath>
#include <cstdint>

#include "app/app.h"
#include "cpu/x86_ops.h"
#include "xdk/hle.h"

namespace {

// PAL 50 Hz step and nearby fixed rates the title may store (25–120 Hz).
constexpr float kDtLo = 1.f / 120.f;  // ~0.0083
constexpr float kDtHi = 1.f / 25.f;   // 0.04

bool LooksLikeFrameDt(float v) {
    return std::isfinite(v) && v >= kDtLo && v <= kDtHi;
}

float ScaleDt(float v) {
    float s = app::FrameTimeScale();
    if (s >= 0.999f || !LooksLikeFrameDt(v)) return v;
    // Always scale the title's authored step. Using measured present-dt here made fog/brightness
    // hitch every time Pace skipped a period (especially with fps_cap >> guest Hz).
    float guest = 1.f / float(app::kGuestRefreshHz);
    return std::min(v * s, guest * 1.25f);
}

void ScaleStackFloat(X86Context& ctx, uint8_t* base, uint32_t stack_off) {
    uint32_t ea = ctx.esp + stack_off;
    float v = X86_MEMF32(ea);
    float out = ScaleDt(v);
    if (out != v) X86_MEMF32(ea) = out;
}

}  // namespace

X86_EXTERN_FUNC(__imp__sub_000B7AD0);  // fld [this+0xE4]
X86_EXTERN_FUNC(__imp__sub_000B7AE0);  // fld [this+0xE8]
X86_EXTERN_FUNC(__imp__sub_000B7AF0);  // fld [this+0xDC]
X86_EXTERN_FUNC(__imp__sub_000B7B00);  // fld [this+0xE0]
X86_EXTERN_FUNC(__imp__sub_000B7850);  // set four floats → +0xDC..+0xE8

X86_FUNC(sub_000B7AD0) {
    __imp__sub_000B7AD0(ctx, base);
    X87_ST(0) = ScaleDt(float(X87_ST(0)));
}

X86_FUNC(sub_000B7AE0) {
    __imp__sub_000B7AE0(ctx, base);
    X87_ST(0) = ScaleDt(float(X87_ST(0)));
}

X86_FUNC(sub_000B7AF0) {
    __imp__sub_000B7AF0(ctx, base);
    X87_ST(0) = ScaleDt(float(X87_ST(0)));
}

X86_FUNC(sub_000B7B00) {
    __imp__sub_000B7B00(ctx, base);
    X87_ST(0) = ScaleDt(float(X87_ST(0)));
}

// thiscall: ret, f0, f1, f2, f3 at +4..+16
X86_FUNC(sub_000B7850) {
    ScaleStackFloat(ctx, base, 4);
    ScaleStackFloat(ctx, base, 8);
    ScaleStackFloat(ctx, base, 12);
    ScaleStackFloat(ctx, base, 16);
    __imp__sub_000B7850(ctx, base);
}

namespace warriors {
void TimingTouch() {
    (void)app::FrameTimeScale();
    (void)app::MeasuredFrameDt();
}
}  // namespace warriors
