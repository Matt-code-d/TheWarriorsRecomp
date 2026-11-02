// AMD FidelityFX Super Resolution 1.0 — EASU pass (MIT). Headers: ffx_a.h / ffx_fsr1.h
cbuffer B : register(b1) {
    uint4 const0;
    uint4 const1;
    uint4 const2;
    uint4 const3;
};

Texture2D InputTexture : register(t0);
SamplerState samLinearClamp : register(s0);

#define A_GPU 1
#define A_HLSL 1
#include "ffx_a.h"
#define FSR_EASU_F 1
AF4 FsrEasuRF(AF2 p) { return InputTexture.GatherRed(samLinearClamp, p, int2(0, 0)); }
AF4 FsrEasuGF(AF2 p) { return InputTexture.GatherGreen(samLinearClamp, p, int2(0, 0)); }
AF4 FsrEasuBF(AF2 p) { return InputTexture.GatherBlue(samLinearClamp, p, int2(0, 0)); }
#include "ffx_fsr1.h"

struct O {
    float4 p : SV_Position;
};

O vs(uint id : SV_VertexID) {
    O o;
    float2 t = float2((id << 1) & 2, id & 2);
    o.p = float4(t * float2(2, -2) + float2(-1, 1), 0, 1);
    return o;
}

float4 psmain(O i) : SV_Target {
    AF3 c;
    FsrEasuF(c, AU2(i.p.xy), const0, const1, const2, const3);
    return float4(c, 1);
}
