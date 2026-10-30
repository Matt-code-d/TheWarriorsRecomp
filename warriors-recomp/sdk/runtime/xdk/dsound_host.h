#pragma once

#include <cstdint>

namespace xdk {

// Stop every buffer and flush every stream. Used when a Bink movie ends/skips so its
// DirectSound stream cannot keep mixing under the next scene.
void DirectSoundSilenceAll();

// XACT routes every 3D SFX through DSBCAPS_MIXIN submixes + Set3DVoiceData.
// Stubs that ignore these leave music (2D) working and all effect cues silent.
void DirectSoundSetOutputBuffer(uint32_t voice, uint32_t mixin);
void DirectSoundSet3DVoiceData(uint32_t voice, uint32_t data);
void DirectSoundUse3DVoiceData(uint32_t voice, bool use);
void DirectSoundGetVoiceProperties(uint32_t voice, uint32_t props);
void DirectSoundSetPitch(uint32_t voice, int32_t pitch);

}  // namespace xdk
