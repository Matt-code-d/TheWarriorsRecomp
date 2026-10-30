# The Warriors (EUR) — XBE Recomp

Native PC static recompilation bring-up for the **European** original Xbox release of *The Warriors* (En, Fr, De, Es, It), built with [Xbox XBE Recomp](https://github.com/TGP482/XboxXBERecomp).

> Bring your own dump. No ISO / XBE / assets are committed.

## Target

| Field | Value |
|-------|--------|
| Region | Europe / Australia multi-lang |
| XBE | `assets/default.xbe` |
| SHA1 | `1126266F440762D00303760C23990C3278025797` |
| Movies | `bik_pal/` |
| Languages | XC_LANGUAGE 1/3/4/5/6 (En/De/Fr/Es/It) |

## Play via launcher

See the workspace root `README.md`. Prefer:

```powershell
.\out\build\release\WarriorsLauncher.exe
```

## Build EUR host

1. Keep extracted game under `assets/` (from your ISO).
2. Install VS 2022 with C++ + Clang + CMake tools.
3. Run `build.bat` (first time) or `..\tools\rebuild_host.bat` (incremental).

First configure runs `xbe_symbols` against the EUR XBE (XDK 5849 expected).

## Language

Launcher / env:

- `XBE_LANGUAGE` or `XC_LANGUAGE` = Xbox language id  
- or `language.cfg` beside the exe (single integer)

Patched in `sdk/runtime/kernel/xboxkrnl.cpp` (`GuestLanguage`).
