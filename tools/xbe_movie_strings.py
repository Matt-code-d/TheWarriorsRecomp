import struct
from pathlib import Path

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


def to_va(fo: int):
    for va, vsz, raw, rsz in sects:
        if raw <= fo < raw + rsz:
            return va + (fo - raw)
    return None


def find_c(s: bytes):
    hits = []
    i = 0
    while True:
        j = d.find(s, i)
        if j < 0:
            break
        va = to_va(j)
        if va is not None:
            hits.append((va, j))
        i = j + 1
    return hits


needles = [
    b"logo\x00",
    b"plogo\x00",
    b"D:\\bik_pal\\%s.bik\x00",
    b"D:\\bik\\%s.bik\x00",
    b"_pal",
    b"%s_w",
    b"bik_pal",
]
for s in needles:
    hits = find_c(s)
    print(repr(s), [(hex(v), hex(f)) for v, f in hits[:12]], "count", len(hits))

j = d.find(b"logo\x00plogo\x00")
print("logo|plogo file", hex(j), "va", hex(to_va(j) or 0))
if j >= 0:
    print(d[j : j + 120])

pal_hits = find_c(b"D:\\bik_pal\\%s.bik\x00")
ntsc_hits = find_c(b"D:\\bik\\%s.bik\x00")
print("pal", pal_hits)
print("ntsc", ntsc_hits)
for label, hits in (("pal", pal_hits), ("ntsc", ntsc_hits)):
    if not hits:
        continue
    addr = hits[0][0]
    needle = struct.pack("<I", addr)
    refs = []
    i = 0
    while True:
        j = d.find(needle, i)
        if j < 0:
            break
        va = to_va(j)
        if va:
            refs.append(hex(va))
        i = j + 1
    print(label, "fmt", hex(addr), "refs", refs[:40])
