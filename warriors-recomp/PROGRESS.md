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

- [ ] Tutorial soft-light square: diag `src=302 dst=303` alpha cookies; soft_cookie RGB-only + WRAP border (non-unit). **FAILED 2026-10-30 (reverted):** `outc.a *= falloff` and/or force `pk.quad=0` on soft draws → Rembrandt/Coney Island hard lit box. Do not reapply (see `.cursor/rules/soft-light-rembrandt.mdc`).
- [x] Cutscene audio under tutorial: SilenceAll had **paused** 65 XACT buffers; game `Pause(0)` resumed mid-cue presentation music. Now **hard stop + rewind** (inspired by stop-on-skip pattern, not pause). Drop late stream Process 2s.
- [x] Frame pacing: with VSync on and `fps_cap` ≥ monitor Hz, skip software `Pace()` after `Present` (double-wait was the hitch). Cap below refresh still software-limited.
- [x] Launcher UI follows selected game language (En/De/Fr/Es/It)
- [x] Launcher Warriors theme (jersey yellow / vest red / asphalt)
- [ ] Confirm in-game: logos / menu / language / audio / first mission / tutorial glow
- [ ] Confirm SFX after mixin+3D voice data port (2026-10-30): music was 2D-only; XACT 3D effects need DSBCAPS_MIXIN SetOutputBuffer + Set3DVoiceData/Use3DVoiceData (were stubs → silent SFX)

## Roadmap

- [ ] **Mouse + keyboard** — full KBM support, enable/disable from the launcher (alongside gamepad)
- [ ] **Online co-op multiplayer** — networked co-op (host/join); scope after single-player bring-up is solid
