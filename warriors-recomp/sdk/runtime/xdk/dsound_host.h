#pragma once

namespace xdk {

// Stop every buffer and flush every stream. Used when a Bink movie ends/skips so its
// DirectSound stream cannot keep mixing under the next scene.
void DirectSoundSilenceAll();

}  // namespace xdk
