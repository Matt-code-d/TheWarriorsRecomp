import struct
from pathlib import Path
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


def disasm(start: int, size: int = 0x200):
    fo = va_to_off(start)
    code = d[fo : fo + size]
    md = Cs(CS_ARCH_X86, CS_MODE_32)
    for insn in md.disasm(code, start):
        print(f"0x{insn.address:08X}: {insn.mnemonic:8} {insn.op_str}")


# Show format strings near %s_w
fo = va_to_off(0x437800)
print("--- formats near 0x437800 ---")
print(d[fo : fo + 80])

print("\n=== around 0x24A200 (name build / %s_w) ===")
disasm(0x24A200, 0x120)

# Who writes/reads 0x4CFE90+4 bit2? Search refs to 0x4CFE90
print("\n=== refs to 0x4CFE90 ===")
needle = struct.pack("<I", 0x4CFE90)
i = 0
refs = []
while True:
    j = d.find(needle, i)
    if j < 0:
        break
    for sva, vsz, raw, rsz in sects:
        if raw <= j < raw + rsz:
            refs.append(sva + (j - raw))
            break
    i = j + 1
print([hex(x) for x in refs[:40]], "count", len(refs))

# Look for strcat/_pal construction - disasm function containing 0x249d7a earlier from start of func
# Find function start by scanning backwards for typical prologue near 0x249B00
print("\n=== movie open func start guess 0x249B80 ===")
disasm(0x249B80, 0x100)
