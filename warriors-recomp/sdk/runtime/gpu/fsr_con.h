#pragma once
// CPU-side FSR1 constant setup — same math as AMD ffx_fsr1.h FsrEasuCon / FsrRcasCon (MIT).
#include <cmath>
#include <cstdint>
#include <cstring>

namespace fsr1 {

inline uint32_t Au1FromAf1(float f) {
    uint32_t u;
    static_assert(sizeof(float) == 4, "");
    std::memcpy(&u, &f, 4);
    return u;
}

inline void EasuCon(uint32_t con0[4], uint32_t con1[4], uint32_t con2[4], uint32_t con3[4], float input_w,
                    float input_h, float resource_w, float resource_h, float output_w, float output_h) {
    con0[0] = Au1FromAf1(input_w / output_w);
    con0[1] = Au1FromAf1(input_h / output_h);
    con0[2] = Au1FromAf1(0.5f * input_w / output_w - 0.5f);
    con0[3] = Au1FromAf1(0.5f * input_h / output_h - 0.5f);
    con1[0] = Au1FromAf1(1.0f / resource_w);
    con1[1] = Au1FromAf1(1.0f / resource_h);
    con1[2] = Au1FromAf1(1.0f / resource_w);
    con1[3] = Au1FromAf1(-1.0f / resource_h);
    con2[0] = Au1FromAf1(-1.0f / resource_w);
    con2[1] = Au1FromAf1(2.0f / resource_h);
    con2[2] = Au1FromAf1(1.0f / resource_w);
    con2[3] = Au1FromAf1(2.0f / resource_h);
    con3[0] = Au1FromAf1(0.0f);
    con3[1] = Au1FromAf1(4.0f / resource_h);
    con3[2] = 0;
    con3[3] = 0;
}

// sharpness in stops: 0 = max sharpen, higher = softer. AMD sample often uses 0.2.
inline void RcasCon(uint32_t con[4], float sharpness_stops) {
    float s = std::exp2(-sharpness_stops);
    con[0] = Au1FromAf1(s);
    // packed half2 unused by FSR_RCAS_F path
    con[1] = 0;
    con[2] = 0;
    con[3] = 0;
}

// Suggested internal render percent for each quality (display / scale).
inline uint32_t QualityRenderPercent(uint32_t quality) {
    switch (quality) {
        case 1: return 77;  // Ultra Quality ~1.3x
        case 2: return 67;  // Quality ~1.5x
        case 3: return 59;  // Balanced ~1.7x
        case 4: return 50;  // Performance 2.0x
        default: return 100;
    }
}

inline float QualitySharpness(uint32_t quality) {
    switch (quality) {
        case 4: return 0.2f;
        case 3: return 0.2f;
        case 2: return 0.2f;
        case 1: return 0.2f;
        default: return 0.2f;
    }
}

}  // namespace fsr1
