# Contributing

## Project

**The Warriors Recomp** — unofficial native PC bring-up of *The Warriors* (original Xbox, European disc).

| Role | Who |
|------|-----|
| Author / maintainer | [Matteo Santoro](https://github.com/Matt-code-d) ([Matt-code-d](https://github.com/Matt-code-d)) |
| Repository | https://github.com/Matt-code-d/TheWarriorsRecomp |

## Upstream & tools

This port stands on other people’s work. Credit where it is due:

| Project | What we use it for |
|---------|-------------------|
| [TGP482 / XboxXBERecomp](https://github.com/TGP482/XboxXBERecomp) | Recompiler, host runtime (D3D12, kernel stubs, audio, input), build glue |
| [XboxDev / extract-xiso](https://github.com/XboxDev/extract-xiso) | Optional ISO → folder extract for the launcher |
| LLVM / Clang | Compiling recompiled game code and the host |
| [SDL](https://github.com/libsdl-org/SDL) | Host input / window plumbing (via the SDK) |
| [Dear ImGui](https://github.com/ocornut/imgui) | Debug / overlay UI in the SDK |
| [toml++](https://github.com/marzer/tomlplusplus) | `settings.toml` parsing |
| [AMD FidelityFX FSR 1.0](https://github.com/GPUOpen-Effects/FidelityFX-FSR) | Optional upscaling on the present blit |

Full license texts for bundled third-party code: [warriors-recomp/sdk/THIRD_PARTY.txt](warriors-recomp/sdk/THIRD_PARTY.txt) and [LICENSES.md](LICENSES.md).

## Scope of this repo’s original work

Written / maintained here for the EUR bring-up (not an exhaustive list):

- WPF launcher (ISO install, language packs, graphics → `settings.toml`)
- EUR-specific host hooks (language, movies / `bik_pal`, timing, audio mixins, soft-light experiments, …)
- Docs, branding, release packaging

## Game rights

*The Warriors* remains © Rockstar / Take-Two (and related rights holders).  
This project is unofficial and unaffiliated. **No ISO, XBE, or game assets ship in this repository or in public release zips.** Bring your own legally owned European dump.

## How to contribute

Issues and PRs on GitHub are welcome for host bugs, launcher UX, docs, and EUR edge cases.  
Please do **not** open PRs that add game dumps, cracked XBEs, or redistributed assets.
