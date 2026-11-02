// AMD FidelityFX Super Resolution 1.0 — RCAS pass (MIT). Headers: ffx_a.h / ffx_fsr1.h
cbuffer B : register(b1) {
    uint4 const0;
    float4 opts;  // x = apply gamma lut
};

Texture2D InputTexture : register(t0);
Texture2D lut : register(t1);
SamplerState ps : register(s1);

#define A_GPU 1
#define A_HLSL 1
#include "ffx_a.h"
#define FSR_RCAS_F 1
AF4 FsrRcasLoadF(ASU2 p) { return InputTexture.Load(int3(ASU2(p), 0)); }
void FsrRcasInputF(inout AF1 r, inout AF1 g, inout AF1 b) {}
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
    AF1 r, g, b;
    FsrRcasF(r, g, b, AU2(i.p.xy), const0);
    float3 c = float3(r, g, b);
    if (opts.x > 0)
        c = float3(lut.Sample(ps, float2(c.r, 0.5)).r, lut.Sample(ps, float2(c.g, 0.5)).g,
                   lut.Sample(ps, float2(c.b, 0.5)).b);
    return float4(c, 1);
}
