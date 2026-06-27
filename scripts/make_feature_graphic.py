"""Render Tally's Google Play feature graphic (1024x500 PNG).

Brand-matched to the app icon: indigo background (#4F46E5), the white
receipt-with-green-check, app name + tagline. Supersampled then downsampled.
Text sizes are measured and auto-fit so nothing overflows or overlaps.
"""
import os
from PIL import Image, ImageDraw, ImageFont

OUT = "store-assets/play-feature-graphic-1024x500.png"
W, H = 1024, 500
SS = 3
w, h = W * SS, H * SS

INDIGO = (79, 70, 229, 255)      # #4F46E5
INDIGO_DK = (60, 53, 180, 255)
WHITE = (255, 255, 255, 255)
GREEN = (34, 197, 94, 255)
SUBTLE = (214, 212, 248, 255)


def font(paths, size):
    for p in paths:
        try:
            return ImageFont.truetype(p, size)
        except OSError:
            continue
    return ImageFont.load_default()


BOLD = ["C:/Windows/Fonts/segoeuib.ttf", "C:/Windows/Fonts/arialbd.ttf"]
REG = ["C:/Windows/Fonts/segoeui.ttf", "C:/Windows/Fonts/arial.ttf"]

img = Image.new("RGBA", (w, h), INDIGO)

# Subtle vertical darkening toward the bottom
grad = Image.new("RGBA", (w, h), (0, 0, 0, 0))
gd = ImageDraw.Draw(grad)
for i in range(h):
    gd.line([(0, i), (w, i)], fill=(0, 0, 30, int(40 * (i / h))))
img = Image.alpha_composite(img, grad)

# Faint oversized check watermark, tucked into the top-right corner
wm = Image.new("RGBA", (w, h), (0, 0, 0, 0))
wd = ImageDraw.Draw(wm)
sc = SS
cx, cy = int(w * 0.9), int(h * 0.26)
pts = [(cx - 90 * sc, cy + 10 * sc), (cx - 30 * sc, cy + 70 * sc),
       (cx + 95 * sc, cy - 80 * sc)]
wd.line(pts, fill=(255, 255, 255, 26), width=int(30 * sc), joint="curve")
img = Image.alpha_composite(img, wm)

# ---- Icon tile (left), mirrors the app icon ----
tile = 250 * SS
tx, ty = 80 * SS, (h - tile) // 2
card = Image.new("RGBA", (w, h), (0, 0, 0, 0))
cd = ImageDraw.Draw(card)
cd.rounded_rectangle([tx, ty, tx + tile, ty + tile], radius=54 * SS, fill=INDIGO_DK)

def ic(x, y):
    return (tx + x / 456 * tile, ty + y / 456 * tile)

def ic_box(x0, y0, x1, y1):
    a, b = ic(x0, y0), ic(x1, y1)
    return [a[0], a[1], b[0], b[1]]

cd.rounded_rectangle(ic_box(150, 120, 306, 320), radius=int(22 / 456 * tile), fill=WHITE)
for lx, ly, lw in [(176, 158, 104), (176, 188, 104), (176, 218, 66)]:
    cd.rounded_rectangle(ic_box(lx, ly, lx + lw, ly + 13), radius=int(6.5 / 456 * tile), fill=INDIGO)
chk = [ic(188, 270), ic(213, 295), ic(267, 231)]
cd.line(chk, fill=GREEN, width=int(22 / 456 * tile), joint="curve")
rad = int(11 / 456 * tile)
for px, py in chk:
    cd.ellipse([px - rad, py - rad, px + rad, py + rad], fill=GREEN)
img = Image.alpha_composite(img, card)

d = ImageDraw.Draw(img)

# ---- Text block (right of icon), vertically centered ----
text_x = tx + tile + 60 * SS
max_w = w - text_x - 60 * SS  # right margin

def fit(text, paths, start, max_width):
    size = start
    while size > 10:
        f = font(paths, size)
        if d.textlength(text, font=f) <= max_width:
            return f, size
        size -= 2
    return font(paths, 10), 10

title = "Tally"
tagline = "Offline point of sale"
subtitle = "Scan, ring up & track sales — all on your device"

title_f, ts = fit(title, BOLD, 130 * SS, max_w)
tag_f, gs = fit(tagline, REG, 56 * SS, max_w)
sub_f, _ = fit(subtitle, REG, 34 * SS, max_w)


def text_h(f):
    b = d.textbbox((0, 0), "Ag", font=f)
    return b[3] - b[1]


gap1, gap2 = 18 * SS, 14 * SS
th, gh, sh = text_h(title_f), text_h(tag_f), text_h(sub_f)
block = th + gap1 + gh + gap2 + sh
y = (h - block) // 2

# draw using anchor at baseline-ish via textbbox offset correction
def draw_line(text, f, yy, fill):
    b = d.textbbox((0, 0), text, font=f)
    d.text((text_x, yy - b[1]), text, font=f, fill=fill)
    return b[3] - b[1]

draw_line(title, title_f, y, WHITE)
y += th + gap1
draw_line(tagline, tag_f, y, WHITE)
y += gh + gap2
draw_line(subtitle, sub_f, y, SUBTLE)

img = img.resize((W, H), Image.LANCZOS).convert("RGB")  # feature graphic = no alpha
os.makedirs(os.path.dirname(OUT), exist_ok=True)
img.save(OUT, "PNG")
print("wrote", OUT, img.size, img.mode)
