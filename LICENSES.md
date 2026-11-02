# Licenses

Short map of who owns what. Legal text lives in the linked files.

## This repository (original work)

- **License:** MIT  
- **File:** [LICENSE](LICENSE)  
- **Copyright:** 2026 Matt-code-d (Matteo Santoro)  
- **Covers:** launcher, EUR host patches, docs, configs, and other original files authored in this tree.  
- **Does not cover:** game data, or third-party SDKs/tools (those keep their own terms).

## The game

- **Not licensed by this project.**  
- Disc image, `default.xbe`, WAD / audio / Bink / other assets stay with Rockstar / Take-Two / related rights holders.  
- Users must supply a legally obtained **European** Xbox copy. Nothing from the game is redistributed here.

## Recompiler / runtime SDK

- **Xbox XBE Recomp** ([TGP482/XboxXBERecomp](https://github.com/TGP482/XboxXBERecomp)) — see that repo for its license and notices.  
- Local copy / notices under `warriors-recomp/sdk/` (including [warriors-recomp/sdk/LICENSE](warriors-recomp/sdk/LICENSE) where present).

## Third-party libraries (bundled / linked)

Summarized with full text in [warriors-recomp/sdk/THIRD_PARTY.txt](warriors-recomp/sdk/THIRD_PARTY.txt):

| Component | Typical license |
|-----------|-----------------|
| toml++ | MIT |
| Dear ImGui | MIT |
| SDL | zlib |
| AMD FidelityFX FSR 1.0 | MIT |

Also used at build / extract time (not always shipped in the release zip): LLVM/Clang, extract-xiso — follow their upstream licenses.

## Release binaries

The public Windows zip ships host + launcher + redistributable VC runtime DLLs as needed.  
It does **not** include ISO / XBE / `assets/` game content.
