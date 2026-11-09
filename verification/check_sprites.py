"""Check shipped sprites against the game's mod-sprite footprint sampling (Pillow)."""
from pathlib import Path
import sys
from PIL import Image

root = Path(__file__).resolve().parents[1]
folder = Path(sys.argv[1]) if len(sys.argv) > 1 else root / "assets"
keys = ["precision_mixer", "precision_mixer_target", "precision_mixer_source", "precision_mixer_both"]
images = []
for key in keys:
    with Image.open(folder / "Textures/Items" / f"{key}.png") as source:
        assert source.mode == "RGBA", f"{key}: expected RGBA"
        image = source.copy()
    assert image.size == (48, 48), f"{key}: expected native 48x48 art"
    alpha = image.getchannel("A")
    assert set(alpha.tobytes()) == {0, 255}, f"{key}: non-pixel-art alpha"
    # Unity samples from the bottom left; Pillow's image origin is the top left.
    occupied = [alpha.getpixel((col * 16 + 8, 47 - ((2 - row) * 16 + 8))) > 0
                for row in range(3) for col in range(3)]
    assert all(occupied), f"{key}: expected all nine cells occupied: {occupied}"
    if images:
        assert alpha.tobytes() == images[0].getchannel("A").tobytes(), f"{key}: silhouette changed"
    images.append(image)
assert len({image.tobytes() for image in images}) == 4, "All four occupancy states must be distinct"
# A target/source bottle must change only its own pixels; both combines those two changes.
pixels = [[image.getpixel((x, y)) for y in range(48) for x in range(48)] for image in images]
for empty, target, source, both in zip(*pixels):
    assert target == empty or source == empty, "Bottle-state changes overlap"
    assert both == (target if target != empty else source), "Both-state art does not combine the two bays"
print(f"Sprites passed ({folder}): 4 native 48x48 states, identical binary alpha, full 3x3 footprint; independent bottle changes.")
