"""Draw the original 64px mixer sprite. Run with Pillow available on PYTHONPATH."""
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1]
image = Image.new("RGBA", (64, 64))
d = ImageDraw.Draw(image)
ink = "#171e28"
steel = "#536778"
light = "#a4b7b7"
# Cut-corner industrial cabinet, cast top lip and right-hand depth.
d.polygon([(6, 3), (53, 3), (62, 11), (62, 57), (57, 62), (5, 62), (1, 57), (1, 11)], fill=ink)
d.polygon([(6, 5), (51, 5), (58, 11), (5, 11)], fill="#91a3a4")
d.line([(7, 5), (50, 5), (55, 9)], fill="#c8d0bd")
d.polygon([(55, 11), (60, 13), (60, 56), (55, 59)], fill="#334553")
d.rectangle((3, 12, 54, 57), fill=steel)
d.line((3, 12, 3, 56), fill=light)
d.line((4, 13, 53, 13), fill="#829899")
d.rectangle((5, 15, 52, 53), fill="#2e404b")
# Recessed glass chamber with warm grey machined surround.
d.rectangle((7, 17, 37, 48), fill=ink)
d.line((8, 17, 36, 17), fill="#9baaa2")
d.line((7, 18, 7, 47), fill="#8b9992")
d.rectangle((9, 19, 35, 46), fill="#20373f")
d.polygon([(10, 20), (20, 20), (10, 32)], fill="#304e55")
d.line((10, 20, 10, 44), fill="#54767a")
# Twin graduated glass cylinders, stopper collars and separate feed colours.
for x, mid, bright, deep, level in [(13, "#39b8b0", "#a0ead5", "#247c85", 29), (26, "#dfa65b", "#ffe2a0", "#a46a43", 33)]:
    d.rectangle((x - 1, 22, x + 7, 43), fill="#101e2b")
    d.rectangle((x, 23, x + 6, 42), fill="#85aeb0")
    d.rectangle((x + 1, 24, x + 5, 41), fill="#34535d")
    d.rectangle((x + 1, level, x + 5, 41), fill=deep)
    d.rectangle((x + 1, level, x + 4, 39), fill=mid)
    d.line((x + 1, level, x + 5, level), fill=bright)
    d.line((x + 1, 25, x + 1, 40), fill="#b1dbd3")
    for y in (27, 31, 35, 39): d.point((x + 5, y), fill="#c5dfd0")
    d.rectangle((x - 1, 21, x + 7, 23), fill="#9aaba4")
    d.line((x, 21, x + 6, 21), fill="#d3d6bf")
    d.rectangle((x - 1, 42, x + 7, 44), fill="#647d83")
    d.line((x, 44, x + 6, 44), fill="#162b35")
# Manifold, curved pressure pipe and brass transfer valve.
d.line([(17, 20), (17, 18), (29, 18), (29, 20)], fill="#a2b1a7", width=2)
d.line([(17, 45), (17, 50), (29, 50), (29, 45)], fill=ink, width=3)
d.line([(17, 45), (17, 49), (29, 49), (29, 45)], fill="#8e9c91")
d.rectangle((21, 47, 25, 51), fill="#c8975d")
d.line((20, 48, 26, 48), fill="#f1d3a0")
d.point((23, 50), fill="#6f4f39")
# Compact numeric display, status LEDs, analog dial and handwheel.
d.rectangle((40, 17, 50, 25), fill=ink)
d.rectangle((41, 18, 49, 24), fill="#173b3c")
for x in (42, 46):
    d.line((x, 19, x + 2, 19), fill="#83d8b3")
    d.line((x + 2, 19, x + 2, 23), fill="#83d8b3")
    d.line((x, 21, x + 2, 21), fill="#83d8b3")
    d.line((x, 23, x + 2, 23), fill="#83d8b3")
d.ellipse((40, 28, 50, 38), fill=ink)
d.ellipse((41, 29, 49, 37), fill="#c7c6a5")
d.arc((42, 30, 48, 36), 190, 340, fill="#59696b")
d.line((45, 34, 47, 31), fill="#a75b47")
d.point((45, 34), fill=ink)
d.ellipse((41, 41, 49, 49), fill=ink)
d.ellipse((42, 42, 48, 48), fill="#a07351")
d.line((43, 44, 47, 46), fill="#e3ba7c")
d.rectangle((40, 51, 43, 52), fill="#6ec6a5")
d.rectangle((46, 51, 49, 52), fill="#c09059")
# Screws, vents, worn paint and heavy feet.
for x, y in [(5, 14), (52, 14), (5, 55), (52, 55)]:
    d.point((x, y), fill="#cad1b9")
    d.point((x + 1, y + 1), fill="#243441")
for y in (21, 25, 29, 33, 37): d.line((57, y, 59, y + 1), fill="#182b37")
d.rectangle((7, 55, 20, 57), fill="#a48f68")
d.line((8, 56, 15, 56), fill="#2e404b")
d.line((24, 55, 46, 55), fill="#819792")
d.line((25, 56, 43, 56), fill="#1c303c")
d.line((5, 59, 54, 59), fill="#738580")
d.rectangle((6, 60, 14, 63), fill=ink)
d.rectangle((44, 60, 55, 63), fill=ink)
d.line((6, 60, 14, 60), fill="#83908a")
d.line((44, 60, 55, 60), fill="#83908a")
# Shape stays a full 4x4 footprint at the game's 16px cell sampling centres.
assert all(image.getpixel((x, y))[3] for x in (8, 24, 40, 56) for y in (8, 24, 40, 56))
image.save(root / "assets/Textures/Items/precision_mixer.png")
print("Created 64x64 original industrial mixer sprite; 4x4 footprint verified.")
