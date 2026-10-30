import struct
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_32

p = Path(r"D:\Games\TheWarriorsRecomp\warriors-recomp\assets\default.xbe")
d = p.read_bytes()
nsect = struct.unpack_from("<I", d, 0x11C)[0]
sh = struct.unpack_from("<I", d, 0x120)[0]
base = struct.unpack_from("<I", d, 0x104)[0]
sects = []
off = sh - base
for i in range(nsect):
    flags, va, vsz, raw, rsz = struct.unpack_from("<5I", d, off + i * 0x38)
    sects.append((va, vsz, raw, rsz))


def to_va(fo):
    for va, vsz, raw, rsz in sects:
        if raw <= fo < raw + rsz:
            return va + (fo - raw)
    return None


def va_to_off(va):
    for sva, vsz, raw, rsz in sects:
        if sva <= va < sva + vsz:
            return raw + (va - sva)
    return None


# Dump strings at 0x4377f0
fo = va_to_off(0x4377F0)
print("strings", d[fo : fo + 64])

# Find instructions that OR/AND with 2 into [reg+4] near video init
# Search for "or dword/byte ptr [eax+4], 2" patterns by scanning code refs
# Simpler: find XGetVideoStandard / AvQuery usages - search for 0x400300 or 0x300 (PAL)

for val in (0x00400300, 0x00000300, 0x00400100, 0x00000100):
    needle = struct.pack("<I", val)
    refs = []
    i = 0
    while True:
        j = d.find(needle, i)
        if j < 0:
            break
        va = to_va(j)
        if va and va < 0x400000:  # code-ish
            refs.append(hex(va))
        i = j + 1
    print(hex(val), "refs", refs[:25], "n", len(refs))

# Disassemble around first few 0x400300 refs
md = Cs(CS_ARCH_X86, CS_MODE_32)
needle = struct.pack("<I", 0x00400300)
i = 0
shown = 0
while shown < 5:
    j = d.find(needle, i)
    if j < 0:
        break
    va = to_va(j)
    i = j + 1
    if not va or va >= 0x400000:
        continue
    start = va - 0x20
    fo = va_to_off(start)
    print(f"\n--- context {hex(va)} ---")
    for insn in md.disasm(d[fo : fo + 0x50], start):
        mark = " <<<" if insn.address == va else ""
        print(f"0x{insn.address:08X}: {insn.mnemonic:8} {insn.op_str}{mark}")
    shown += 1
