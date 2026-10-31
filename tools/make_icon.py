"""Generate BayerRawImageViewer/app.ico: a 2x2 Bayer RGGB tile icon.

Usage: python3 tools/make_icon.py BayerRawImageViewer/app.ico
Requires Pillow.
"""
import io
import struct
import sys

from PIL import Image, ImageDraw

RED, GREEN, BLUE = (229, 57, 53, 255), (67, 160, 71, 255), (30, 136, 229, 255)
LAYOUT = [[RED, GREEN], [GREEN, BLUE]]
SIZES = [16, 24, 32, 48, 64, 128, 256]


def draw(size):
    """Draw the icon at one size, so small sizes keep a crisp 1-pixel gap."""
    scale = 4 if size >= 32 else 1          # supersample larger sizes for smooth corners
    s = size * scale
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    margin = max(1, round(s * 0.04)) if size >= 24 else 0
    gap = max(scale, round(s * 0.06))
    cell = (s - 2 * margin - gap) // 2       # integer layout keeps the gap exact at small sizes
    radius = round(cell * 0.18) if size >= 24 else 0
    for r in range(2):
        for c in range(2):
            x0 = margin + c * (cell + gap)
            y0 = margin + r * (cell + gap)
            d.rounded_rectangle([x0, y0, x0 + cell - 1, y0 + cell - 1], radius=radius, fill=LAYOUT[r][c])
    return img.resize((size, size), Image.LANCZOS) if scale > 1 else img


def write_ico(path, images):
    """Write an ICO with one PNG-encoded entry per image (Pillow would rescale them)."""
    blobs = []
    for img in images:
        buf = io.BytesIO()
        img.save(buf, format="PNG")
        blobs.append(buf.getvalue())

    header = struct.pack("<HHH", 0, 1, len(images))
    offset = 6 + 16 * len(images)
    entries = b""
    for img, blob in zip(images, blobs):
        dim = img.width if img.width < 256 else 0   # 0 means 256 in the ICO directory
        entries += struct.pack("<BBBBHHII", dim, dim, 0, 0, 1, 32, len(blob), offset)
        offset += len(blob)

    with open(path, "wb") as f:
        f.write(header + entries + b"".join(blobs))


if __name__ == "__main__":
    write_ico(sys.argv[1], [draw(n) for n in SIZES])
