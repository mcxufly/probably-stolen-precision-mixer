"""Rebuild the Workshop preview from original runtime art (Pillow + DejaVu Sans)."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
version = ET.parse(root / 'assets/manifest.xml').findtext('ModVersion')
font_path = next((p for p in (Path('/usr/share/fonts/TTF/DejaVuSans.ttf'),
                            Path('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf')) if p.is_file()), None)
if font_path is None:
    raise SystemExit('Install DejaVu Sans or update the font path in this script.')
canvas = Image.new('RGB', (768, 768), '#25232b')
draw = ImageDraw.Draw(canvas)
draw.rectangle((28, 28, 739, 739), outline='#697574', width=3)
draw.rectangle((40, 40, 727, 727), outline='#3b3647', width=2)
def title(y, text, size, color):
    draw.text((384, y), text, anchor='mt', fill=color, font=ImageFont.truetype(str(font_path), size))
title(74, 'PRECISION MIXER', 42, '#b3d1b6')
title(132, '3 x 3  /  NATIVE 48px ART', 22, '#a4ae96')
with Image.open(root / 'assets/Textures/Items/precision_mixer_both.png') as sprite:
    enlarged = sprite.resize((432, 432), Image.Resampling.NEAREST)
    canvas.paste(enlarged, (168, 174), enlarged)
title(636, 'MANUAL DOSING  /  AUTO MIXING', 24, '#bcc4a8')
title(682, f'PROBABLY STOLEN  |  v{version}', 22, '#7f8b82')
output = root / 'publishing/workshop-preview.png'
canvas.save(output)
assert output.stat().st_size < 1_000_000
print(f'Preview created: 768px, integer9x sprite enlargement; {output.stat().st_size} bytes.')
