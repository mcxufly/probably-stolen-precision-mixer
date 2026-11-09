"""Draw original 48px (3x3 cells) mixer sprites; run with python3 and Pillow.

All sprite pixels are authored at native resolution, without resampling game art.
Only the labelled contact sheet uses nearest-neighbour enlargement.
"""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "assets/Textures/Items"
SIZE, CELL, ZOOM = 48, 16, 4
# Low-contrast green-grey enamel, plum cast shadows, and desaturated cyan glass.
C = dict(ink="#292730", deep="#35303e", shadow="#48414f", seam="#5a5361",
         steel="#626c69", metal="#7c8579", light="#9ba18a", edge="#b1b69a",
         well="#3e3947", teal="#3e686b", glass="#67918b", glint="#a2b7a2",
         screen="#34464c", foot="#39353f")


def draw_vessel(d, x):
    """A fixed occupancy pictogram, not a live liquid-level/colour indicator."""
    d.polygon([(x, 22), (x + 10, 22), (x + 9, 32), (x + 7, 34),
               (x + 3, 34), (x + 1, 32)], fill=C["ink"])
    d.polygon([(x + 1, 23), (x + 9, 23), (x + 8, 32), (x + 6, 33),
               (x + 3, 32)], fill=C["teal"])
    d.line((x + 2, 24, x + 3, 31), fill=C["glass"])
    d.line((x + 8, 24, x + 7, 31), fill=C["glass"])
    d.point((x + 2, 24), fill=C["glint"])
    d.line((x + 4, 33, x + 6, 33), fill=C["glass"])
    for y in (26, 29):
        d.line((x + 5, y, x + 6, y), fill=C["glass"])
    # Faceted open mouth: cool rim around a dark ellipse, no bright white streaks.
    d.polygon([(x, 21), (x + 2, 19), (x + 8, 19), (x + 10, 21),
               (x + 9, 23), (x + 2, 23)], fill=C["glass"])
    d.line((x + 3, 19, x + 7, 19), fill=C["glint"])
    d.line((x + 2, 21, x + 8, 21), fill=C["screen"])
    d.line((x + 3, 22, x + 7, 22), fill=C["teal"])


def draw_machine(state):
    im = Image.new("RGBA", (SIZE, SIZE))
    d = ImageDraw.Draw(im)
    # Chamfered casting. Its continuous back and deck fill all nine cell centres.
    d.polygon([(6, 2), (40, 2), (45, 6), (46, 10), (45, 40),
               (42, 44), (6, 44), (2, 40), (2, 9), (3, 5)], fill=C["ink"])
    # Back cover seen from above; a right side face gives the shallow perspective.
    d.polygon([(7, 3), (39, 3), (44, 7), (42, 11), (6, 11), (3, 8)], fill=C["steel"])
    d.polygon([(8, 4), (38, 4), (41, 6), (39, 9), (7, 9), (5, 7)], fill=C["metal"])
    d.line((9, 3, 37, 3), fill=C["light"])
    d.line((6, 10, 40, 10), fill=C["shadow"])
    # Three short recessed vents, rather than scaled-down diagonal hatch marks.
    for x in (12, 21, 30):
        d.line((x, 5, x + 4, 5), fill=C["steel"])
        d.line((x, 6, x + 4, 6), fill=C["shadow"])
        d.line((x + 1, 7, x + 4, 7), fill=C["light"])
    d.polygon([(43, 11), (45, 9), (44, 39), (41, 42), (41, 15)], fill=C["shadow"])
    d.line((44, 13, 44, 36), fill=C["seam"])
    # Solid inner rear wall and low bottom deck keep silhouette state-independent.
    d.rectangle((4, 12, 42, 37), fill=C["steel"])
    d.rectangle((15, 13, 41, 34), fill=C["well"])
    d.line((16, 13, 40, 13), fill=C["seam"])
    d.rectangle((16, 15, 40, 18), fill=C["deep"])
    # Slim dosing manifold, with separate nozzles above the two stations.
    d.line((18, 15, 38, 15), fill=C["steel"])
    d.line((19, 14, 37, 14), fill=C["metal"])
    for x in (22, 35):
        d.line((x, 16, x, 18), fill=C["seam"])
        d.point((x + 1, 17), fill=C["metal"])
    d.line((28, 19, 28, 31), fill=C["deep"])
    # Small left control panel, set into a sloped surround; readable, not luminous.
    d.polygon([(5, 15), (12, 15), (14, 18), (14, 34), (5, 34)], fill=C["shadow"])
    d.rectangle((5, 16, 12, 28), fill=C["ink"])
    d.rectangle((6, 17, 11, 26), fill=C["screen"])
    d.line((6, 17, 10, 17), fill=C["glass"])
    for y in (19, 22, 25):
        d.line((7, y, 8, y), fill=C["glass"])
        d.point((10, y), fill=C["light"])
    for x in (7, 11):
        d.rectangle((x - 1, 30, x + 1, 32), fill=C["ink"])
        d.point((x, 30), fill=C["glass"])
        d.point((x, 31), fill=C["teal"])
    d.line((5, 35, 12, 35), fill=C["metal"])
    # Deck and twin sockets: the empty state is visibly a working surface.
    d.polygon([(15, 32), (40, 32), (42, 36), (39, 39), (15, 39)], fill=C["steel"])
    for bit, x in ((1, 17), (2, 30)):
        d.ellipse((x - 1, 32, x + 11, 37), fill=C["deep"])
        d.ellipse((x, 32, x + 10, 35), fill=C["metal"])
        d.ellipse((x + 2, 32, x + 8, 34), fill=C["well"])
        d.line((x + 2, 36, x + 8, 36), fill=C["seam"])
        if state & bit:
            draw_vessel(d, x)
    # Rounded front apron and two tiny occupancy lamps, not a large light bar.
    d.polygon([(4, 37), (14, 37), (16, 39), (40, 39), (42, 37),
               (42, 41), (39, 43), (7, 43), (4, 40)], fill=C["shadow"])
    d.line((7, 42, 38, 42), fill=C["seam"])
    d.line((5, 37, 12, 37), fill=C["light"])
    for x in (6, 9, 12):
        d.line((x, 39, x, 40), fill=C["deep"])
    for bit, x in ((1, 21), (2, 34)):
        d.line((x - 1, 40, x + 1, 40), fill=C["ink"])
        d.point((x, 40), fill=C["glass"] if state & bit else C["seam"])
    # Sparse fasteners and two short feet; no evenly distributed visual noise.
    for point in ((5, 12), (41, 12), (5, 36)):
        d.point(point, fill=C["light"])
    d.rectangle((7, 44, 12, 45), fill=C["foot"])
    d.rectangle((35, 44, 40, 45), fill=C["foot"])
    d.line((8, 44, 11, 44), fill=C["seam"])
    d.line((36, 44, 39, 44), fill=C["seam"])
    return im


def main():
    names = ("precision_mixer", "precision_mixer_target", "precision_mixer_source", "precision_mixer_both")
    images = [draw_machine(state) for state in range(4)]
    alpha = images[0].getchannel("A").tobytes()
    for name, image in zip(names, images):
        assert image.mode == "RGBA" and image.size == (48, 48), "Native 48px RGBA required"
        assert SIZE == 3 * CELL, "The footprint must be 3x3 at 16px/cell"
        assert set(image.getchannel("A").tobytes()) == {0, 255}, "Hard pixel edges only"
        # Unity samples y from the bottom; Pillow uses the top-left origin.
        assert all(image.getpixel((x, 47 - y))[3] == 255
                   for x in (8, 24, 40) for y in (8, 24, 40)), "All 9 game-sampled cell centres must be opaque"
        assert image.getchannel("A").tobytes() == alpha, "State changes cannot alter the footprint"
        image.save(OUT / f"{name}.png")
    # Preview only: integer 4x nearest-neighbour, with four explicit state labels.
    preview = Image.new("RGB", (864, 252), "#25232b")
    p = ImageDraw.Draw(preview)
    labels = ("EMPTY", "TARGET", "SOURCE", "BOTH")
    for i, (label, image) in enumerate(zip(labels, images)):
        x = 12 + i * 216
        enlarged = image.resize((SIZE * ZOOM, SIZE * ZOOM), Image.Resampling.NEAREST)
        preview.paste(enlarged, (x, 12), enlarged)
        p.text((x + 2, 212), label, fill=C["light"])
        p.text((x + 2, 229), "L: TARGET   R: SOURCE", fill=C["metal"])
    preview.save(ROOT / "art/mixer_states.png")
    print("Created four native 48px RGBA sprites; binary shared alpha and all nine 3x3 cell centres verified.")


if __name__ == "__main__":
    main()
