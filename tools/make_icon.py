# Renders src/Pama.Desktop/Assets/pama.ico; keep in sync with Resources/Logo.xaml
# Usage: python3 tools/make_icon.py src/Pama.Desktop/Assets/pama.ico preview.png
import math, struct, zlib, sys

def rrect(x, y, cx, cy, hw, hh, r):
    qx, qy = abs(x-cx)-hw+r, abs(y-cy)-hh+r
    return math.hypot(max(qx,0), max(qy,0)) + min(max(qx,qy),0) - r

def sparkle(u, v, cx, cy, r):
    # Astroid-like star: |x|^0.5 + |y|^0.5 <= r^0.5 gives four concave points
    x, y = abs(u-cx), abs(v-cy)
    return math.sqrt(x) + math.sqrt(y) <= math.sqrt(r)

def shade(u, v):
    # coordinates in 0..1; returns rgba floats
    if rrect(u, v, .5, .5, .5, .5, .22) > 0:
        return (0, 0, 0, 0)
    t = (u + v) / 2  # diagonal gradient indigo -> violet
    bg = (0.31 + 0.24*t, 0.27 + 0.0*t, 0.90 - 0.05*t)
    ring = abs(math.hypot(u-.5, v-.40) - .155) - .045
    shackle = ring < 0 and v < .46
    body = rrect(u, v, .5, .62, .23, .17, .05) < 0
    hole = math.hypot(u-.5, v-.585) < .045 or (abs(u-.5) < .022 and .585 < v < .70)
    if (shackle or body) and not hole:
        return (1, 1, 1, 1)
    # Four-point sparkle, top-right: "generate"; deliberately not a colored dot, which reads as a notification badge
    if sparkle(u, v, .80, .20, .13) or sparkle(u, v, .66, .12, .05):
        return (1, 1, 1, 1)
    return (*bg, 1)

def pixels(size, ss=4):
    out = []
    for py in range(size):
        row = []
        for px in range(size):
            acc = [0.0]*4
            for sy in range(ss):
                for sx in range(ss):
                    r, g, b, a = shade((px+(sx+.5)/ss)/size, (py+(sy+.5)/ss)/size)
                    acc[0] += r*a; acc[1] += g*a; acc[2] += b*a; acc[3] += a
            rgb = [int(255*c/acc[3]) if acc[3] else 0 for c in acc[:3]]
            row.append((*rgb, int(255*acc[3]/(ss*ss))))
        out.append(row)
    return out

def bmp(size):
    # Classic 32-bit DIB entry: the format every ICO consumer decodes at small sizes
    px = pixels(size)
    header = struct.pack('<IiiHHIIiiII', 40, size, size*2, 1, 32, 0, 0, 0, 0, 0, 0)
    xor = b''.join(bytes((b, g, r, a)) for row in reversed(px) for (r, g, b, a) in row)
    mask_row = ((size + 31)//32)*4
    return header + xor + b'\0'*(mask_row*size)

def render(size, ss=4):
    rows = []
    for py in range(size):
        row = bytearray([0])
        for px in range(size):
            acc = [0.0]*4
            for sy in range(ss):
                for sx in range(ss):
                    r, g, b, a = shade((px+(sx+.5)/ss)/size, (py+(sy+.5)/ss)/size)
                    acc[0] += r*a; acc[1] += g*a; acc[2] += b*a; acc[3] += a
            n = ss*ss; a = acc[3]/n
            rgb = [int(255*c/acc[3]) if acc[3] else 0 for c in acc[:3]]
            row += bytes(rgb + [int(255*a)])
        rows.append(bytes(row))
    raw = b''.join(rows)
    chunk = lambda t, d: struct.pack('>I', len(d)) + t + d + struct.pack('>I', zlib.crc32(t+d) & 0xffffffff)
    return b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', size, size, 8, 6, 0, 0, 0)) \
        + chunk(b'IDAT', zlib.compress(raw, 9)) + chunk(b'IEND', b'')

sizes = [16, 24, 32, 48, 64, 128, 256]
pngs = [render(s) if s == 256 else bmp(s) for s in sizes]
out_ico, out_png = sys.argv[1], sys.argv[2]
with open(out_ico, 'wb') as f:
    f.write(struct.pack('<HHH', 0, 1, len(sizes)))
    offset = 6 + 16*len(sizes)
    for s, p in zip(sizes, pngs):
        f.write(struct.pack('<BBBBHHII', s % 256, s % 256, 0, 0, 1, 32, len(p), offset)); offset += len(p)
    for p in pngs: f.write(p)
open(out_png, 'wb').write(render(256))
