"""Draw original game-style mixer sprites and their container-state preview (Pillow)."""
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1]
out = root / "assets/Textures/Items"
# Muted grey-green metal, plum shadows and teal glass, inspired by the game palette.
C = dict(outline="#26252e", shadow="#3b3647", recess="#302b39", rim="#565363",
         steel="#697574", mid="#7f8b82", light="#a4ae96", glint="#bcc4a8",
         teal="#3f7879", glass="#6da59c", gleam="#b3d1b6", fluid="#305b65")


def draw_machine(mask):
    im = Image.new("RGBA", (64, 64))
    d = ImageDraw.Draw(im)
    # Low-profile cast housing with a faceted, diagonally vented lid.
    d.polygon([(6, 2), (56, 2), (62, 8), (62, 58), (58, 62), (4, 62), (1, 58), (1, 8)], fill=C["outline"])
    d.polygon([(7, 3), (55, 3), (60, 8), (57, 16), (5, 16), (3, 9)], fill=C["steel"])
    d.polygon([(8, 4), (53, 4), (57, 8), (54, 13), (7, 13), (5, 9)], fill=C["mid"])
    for x in range(8, 51, 8):
        d.line((x, 5, x + 5, 10), fill=C["steel"], width=2)
        d.line((x, 4, x + 5, 9), fill=C["light"])
    d.line((8, 3, 54, 3), fill=C["light"])
    d.line((5, 14, 56, 14), fill=C["shadow"])
    d.line((6, 15, 55, 15), fill=C["teal"])
    # Dark front face and slightly recessed side wall.
    d.rectangle((3, 17, 57, 57), fill=C["steel"])
    d.rectangle((58, 16, 60, 56), fill=C["shadow"])
    d.line((3, 17, 3, 56), fill=C["mid"])
    d.line((5, 58, 57, 58), fill=C["rim"])
    # Small left control tower: no large luminous instrumentation.
    d.rectangle((5, 19, 18, 47), fill=C["outline"])
    d.rectangle((6, 20, 17, 37), fill="#384650")
    d.line((7, 20, 16, 20), fill=C["glass"])
    for y, widths in [(23, (3, 2)), (26, (2, 3)), (29, (4, 2)), (32, (2, 4))]:
        d.line((8, y, 8 + widths[0], y), fill=C["gleam"])
        d.line((13, y, min(16, 13 + widths[1]), y), fill=C["glass"])
    for x in (8, 14):
        d.ellipse((x - 1, 40, x + 2, 43), fill=C["teal"])
        d.point((x, 40), fill=C["glass"])
        d.point((x + 1, 42), fill=C["outline"])
    d.rectangle((7, 46, 16, 47), fill=C["rim"])
    # Twin open chambers, backed by plum heat exchanger fins.
    d.rectangle((21, 19, 56, 49), fill=C["outline"])
    d.rectangle((22, 20, 55, 47), fill=C["recess"])
    for y in (23, 26, 29):
        d.line((23, y, 54, y), fill="#574354")
        d.line((24, y + 1, 54, y + 1), fill="#3e3244")
    d.line((22, 20, 55, 20), fill=C["rim"])
    for bit, x in [(1, 24), (2, 41)]:
        # Raised round cradle is visible when no bottle is inserted.
        d.ellipse((x - 1, 43, x + 13, 48), fill=C["shadow"])
        d.ellipse((x, 43, x + 12, 46), fill=C["steel"])
        d.line((x + 2, 46, x + 10, 46), fill=C["rim"])
        d.ellipse((x + 3, 43, x + 9, 45), fill=C["recess"])
        if not mask & bit:
            continue
        # Cylindrical glass vessel with faceted mouth, spout and graduation marks.
        d.polygon([(x, 29), (x + 12, 29), (x + 11, 42), (x + 9, 44), (x + 3, 44), (x + 1, 42)], fill="#223b43")
        d.polygon([(x + 1, 30), (x + 11, 30), (x + 10, 41), (x + 8, 43), (x + 4, 43), (x + 2, 41)], fill=C["teal"])
        d.polygon([(x + 2, 35), (x + 10, 35), (x + 9, 41), (x + 7, 42), (x + 4, 42), (x + 3, 40)], fill=C["fluid"])
        d.line((x + 3, 35, x + 9, 35), fill=C["glass"])
        d.line((x + 2, 31, x + 3, 40), fill=C["glass"])
        d.line((x + 3, 32, x + 4, 39), fill=C["gleam"])
        d.line((x + 10, 32, x + 9, 41), fill=C["glass"])
        d.line((x + 4, 43, x + 8, 43), fill=C["glass"])
        for y in (33, 37, 40):
            d.line((x + 6, y, x + 7, y), fill=C["gleam"])
        d.polygon([(x, 27), (x + 3, 25), (x + 10, 25), (x + 13, 27), (x + 11, 30), (x + 3, 31), (x, 29)], fill=C["glass"])
        d.polygon([(x + 2, 27), (x + 4, 26), (x + 10, 26), (x + 11, 27), (x + 9, 29), (x + 4, 29)], fill=C["teal"])
        d.line((x + 3, 25, x + 9, 25), fill=C["gleam"])
        d.line((x, 27, x + 2, 30), fill=C["gleam"])
        d.line((x + 10, 27, x + 13, 27), fill=C["recess"])
    # Narrow manifold and per-bay status lights on lower fascia.
    d.rectangle((22, 50, 55, 54), fill=C["shadow"])
    d.line((24, 51, 52, 51), fill=C["rim"])
    for bit, x in [(1, 25), (2, 43)]:
        d.rectangle((x, 53, x + 2, 54), fill=C["light"] if mask & bit else C["rim"])
        d.point((x + 4, 53), fill=C["teal"])
    d.rectangle((5, 51, 17, 55), fill=C["shadow"])
    for x in (7, 10, 13): d.line((x, 52, x, 54), fill=C["light"])
    # Chipped paint, screw heads and dark base feet.
    for x, y in [(5, 17), (55, 17), (5, 57), (55, 57)]:
        d.point((x, y), fill=C["light"])
        d.point((x + 1, y + 1), fill=C["shadow"])
    d.line((20, 57, 44, 57), fill=C["mid"])
    d.rectangle((5, 60, 13, 63), fill=C["outline"])
    d.rectangle((48, 60, 57, 63), fill=C["outline"])
    d.line((6, 60, 12, 60), fill=C["rim"])
    d.line((49, 60, 56, 60), fill=C["rim"])
    return im


names = ["precision_mixer", "precision_mixer_target", "precision_mixer_source", "precision_mixer_both"]
images = [draw_machine(mask) for mask in range(4)]
for name, image in zip(names, images):
    assert image.size == (64, 64)
    assert all(image.getpixel((x, y))[3] for x in (8, 24, 40, 56) for y in (8, 24, 40, 56))
    assert image.getchannel("A").tobytes() == images[0].getchannel("A").tobytes()
    image.save(out / f"{name}.png")
# Deliverable contact sheet at integer nearest-neighbour zoom; not used by the game.
preview = Image.new("RGB", (832, 252), "#25232b")
p = ImageDraw.Draw(preview)
for i, (label, image) in enumerate(zip(["EMPTY", "TARGET", "SOURCE", "BOTH"], images)):
    enlarged = image.resize((192, 192), Image.Resampling.NEAREST)
    preview.paste(enlarged, (8 + i * 208, 12), enlarged)
    p.text((12 + i * 208, 224), label, fill=C["light"])
preview.save(root / "art/mixer_states.png")
print("Created 4 game-style 64px sprites; matching alpha and 4x4 footprint verified.")
