# Progress — The Warriors EUR Recomp

## Done

- [x] EUR ISO extract → `assets/` (`bik_pal`, wad, `default.xbe`)
- [x] EUR SHA1 `1126266F440762D00303760C23990C3278025797` · Title `5454009B` · XDK 5849
- [x] Host build `out/build/release/Warriors.exe` + DLL
- [x] Launcher SETUP / LANGUAGE / PLAY
- [x] `crt_exit_callback` @ `0x35637D` + `XPP_GamepadType`
- [x] APU sample clock @ `FE80200C` (`src/common/apu_clock.cpp`)
- [x] Contiguous RAM: host 128 MB + raise guest `highest` 64 MB ceiling
- [x] Boot reaches D3D12 and stays running (no AV / no contig spam)
- [x] EUR movies: `bik`→`bik_pal` junction + `*_pal.bik` hardlink aliases; host reports PAL-I AV region
- [x] Rockstar logo skip hook (`src/game/movies.cpp`)
- [x] XACT unnamed DirectSound stubs (`src/game/dsound.cpp`) — no more Use3DVoiceData AV
- [x] Boot stays up 60s+ past effects image (no Exit(0) / no C0000005)

## Run

```powershell
cd warriors-recomp\out\build\release
$env:XBE_LANGUAGE = 6   # 1=En 3=De 4=Fr 5=Es 6=It
.\Warriors.exe
```

Or `WarriorsLauncher.exe` (prefers this EUR build when present).

After rebuilding `Warriors.exe`, either delete `compiler\work\Warriors\built.txt` and let the first-start UI finish + press Start, or ensure `Warriors.dll` is newer than the exe so the stamp check is skipped.

If you re-extract the ISO, re-run `tools\alias_eur_movies.ps1` so NTSC-named `.bik` hardlinks exist again.

## Launcher graphics

- Tab **GRAPHICS**: API (D3D12 / Vulkan reserved), display mode, resolution, FPS cap, render scale, aniso, VSync
- Writes `settings.toml` beside `Warriors.exe` on Apply / PLAY

## Next

- [ ] Tutorial soft-light square: A/B SoftLight classes ruled out as sole painter. Hunter: `softlight MISS:` (blend+WRAP ≤64 verts outside vol/scr). Run tutorial, collect novel MISS lines.
- [x] Cutscene audio under tutorial: SilenceAll had **paused** 65 XACT buffers; game `Pause(0)` resumed mid-cue presentation music. Now **hard stop + rewind** (inspired by stop-on-skip pattern, not pause). Drop late stream Process 2s.
- [x] Frame pacing: waitable swap chain + max latency 2. **EUR PAL guest clock 50 Hz** (VBlank was 60 → game ~20% fast even at “60 FPS”). `Pace` clamps to ≤50; default `fps_cap=50`.
- [x] **FSR 1.0** (AMD MIT EASU+RCAS) on present blit when `fsr=1..4` and render scale < output; launcher GRAPHICS dropdown + `settings.toml`.
- [x] Launcher GRAPHICS tabs: GAME / DISPLAY / RENDERING / CONTROLS / LANGUAGE / ADVANCED. `settings.toml` writes `upscaler` / `upscale_quality` / `frame_generation` / `sharpness` / controls keys; host maps FSR path (DLSS/XeSS/FG not wired — fallback FSR1).
- [x] Launcher UI follows selected game language (En/De/Fr/Es/It)
- [x] Launcher Warriors theme (jersey yellow / vest red / asphalt)
- [ ] Confirm in-game: logos / menu / language / audio / first mission / tutorial glow
- [ ] Confirm SFX after mixin+3D voice data port (2026-10-30): music was 2D-only; XACT 3D effects need DSBCAPS_MIXIN SetOutputBuffer + Set3DVoiceData/Use3DVoiceData (were stubs → silent SFX)

## Roadmap

- [ ] **Decouple sim from FPS** — like other recomps: high FPS without speeding up gameplay.
  - [x] **Phase 1 (2026-10-30):** shared frame epoch for guest VBlank + `Pace`; `PresentUseVsync` only when refresh ÷ target; timing stats in log.
  - [x] **Phase 2 (2026-10-30):** hooks on timing getters/setter `@ 0xB7AD0/AE0/AF0/B00/7850` scale floats that look like frame dt (~1/50); `MeasuredFrameDt`; display may exceed 50 Hz; launcher FPS unlocked again.
  - [ ] **Phase 3:** verify in-game at 60/120/uncapped (movement, animations, audio); widen hooks if some systems still use unscaled `push 0x3CA3D70A`.
- [ ] **Mouse + keyboard** — full KBM support, enable/disable from the launcher (alongside gamepad)
- [ ] **Online co-op multiplayer** — networked co-op (host/join); scope after single-player bring-up is solid
