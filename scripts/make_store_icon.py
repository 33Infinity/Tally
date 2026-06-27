"""Render Tally's app icon to a 512x512 PNG for the Google Play store listing.

Reproduces Resources/AppIcon/appicon.svg (background) + appiconfg.svg (foreground)
at Play's required 512x512, 32-bit-with-alpha size. SVG is 456 units; scale = 512/456.
Drawn supersampled (4x) then downsampled for clean anti-aliased edges.
"""
from PIL import Image, ImageDraw

OUT = "store-assets/play-icon-512.png"
SVG = 456
SIZE = 512
SS = 4  # supersample factor
S = SIZE * SS
k = S / SVG  # svg-units -> supersampled px

INDIGO = (79, 70, 229, 255)   # #4F46E5
WHITE = (255, 255, 255, 255)
GREEN = (34, 197, 94, 255)     # #22C55E


def r(*v):
    """Scale svg units to supersampled pixels."""
    return tuple(round(x * k) for x in v)


img = Image.new("RGBA", (S, S), INDIGO)
d = ImageDraw.Draw(img)

# White rounded "receipt" card: x=150 y=120 w=156 h=200 rx=22
x0, y0 = 150, 120
x1, y1 = x0 + 156, y0 + 200
d.rounded_rectangle([*r(x0, y0), *r(x1, y1)], radius=round(22 * k), fill=WHITE)

# Three indigo line items (rounded bars), height 13, rx 6.5
for (lx, ly, lw) in [(176, 158, 104), (176, 188, 104), (176, 218, 66)]:
    d.rounded_rectangle(
        [*r(lx, ly), *r(lx + lw, ly + 13)], radius=round(6.5 * k), fill=INDIGO
    )

# Green checkmark: path M188,270 l25,25 l54,-64, stroke-width 22, round caps/joins
pts = [r(188, 270), r(213, 295), r(267, 231)]
d.line(pts, fill=GREEN, width=round(22 * k), joint="curve")
# round caps + join (PIL line joints aren't fully round) via circles at vertices
rad = round(11 * k)
for px, py in pts:
    d.ellipse([px - rad, py - rad, px + rad, py + rad], fill=GREEN)

img = img.resize((SIZE, SIZE), Image.LANCZOS)
import os
os.makedirs(os.path.dirname(OUT), exist_ok=True)
img.save(OUT, "PNG")
print("wrote", OUT, img.size, img.mode)
