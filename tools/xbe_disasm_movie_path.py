import struct
from pathlib import Path

try:
    from capstone import Cs, CS_ARCH_X86, CS_MODE_32
except ImportError:
    import subprocess
    import sys

    subprocess.check_call([sys.executable, "-m", "pip", "install", "capstone", "-q"])
    from capstone import Cs, CS_ARCH_X86, CS_MODE_32

p = Path(r"D:\Games\TheWarriorsRecomp\warriors-recomp\assets\default.xbe")
d = p.read_bytes()
base = struct.unpack_from("<I", d, 0x104)[0]
nsect = struct.unpack_from("<I", d, 0x11C)[0]
sh = struct.unpack_from("<I", d, 0x120)[0]
sects = []
off = sh - base
for i in range(nsect):
    flags, va, vsz, raw, rsz = struct.unpack_from("<5I", d, off + i * 0x38)
    sects.append((va, vsz, raw, rsz))


def va_to_off(va: int):
    for sva, vsz, raw, rsz in sects:
        if sva <= va < sva + vsz:
            return raw + (va - sva)
    return None


# Disassemble around path selection ~0x249d00-0x249e80
start = 0x249C80
end = 0x249E80
fo = va_to_off(start)
code = d[fo : fo + (end - start)]
md = Cs(CS_ARCH_X86, CS_MODE_32)
md.detail = False
for insn in md.disasm(code, start):
    mark = ""
    if insn.address in (0x249D89, 0x249DA0):
        mark = "  <<<"
    print(f"0x{insn.address:08X}: {insn.mnemonic:8} {insn.op_str}{mark}")

# Also peek string table around logo
fo = va_to_off(0x41D9E0)
print("--- strings at 0x41D9E0 ---")
print(d[fo : fo + 64])

# Disassemble %s_w related area - find refs to 0x437804
needle = struct.pack("<I", 0x437804)
print("--- refs to %s_w ---")
i = 0
while True:
    j = d.find(needle, i)
    if j < 0:
        break
    # map to va
    for sva, vsz, raw, rsz in sects:
        if raw <= j < raw + rsz:
            print(hex(sva + (j - raw)))
            break
    i = j + 1
