#!/usr/bin/env python3
"""Generate a machine-local SteamCMD VDF, preserving its published item ID."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
TARGET = ROOT / ".tools/steamcmd/workshop.vdf"

def quote(value):
    return '"' + str(value).replace('\\', '\\\\').replace('"', '\\"').replace('\r', '') + '"'


def main():
    item_id = "0"
    if TARGET.exists():
        match = re.search(r'"publishedfileid"\s+"(\d+)"', TARGET.read_text())
        if not match:
            raise SystemExit("Existing workshop.vdf has no publishedfileid; refusing to risk duplicate publication.")
        item_id = match.group(1)
    folder = ROOT / "dist/PrecisionMixer"
    if not (folder / "manifest.xml").is_file() or not (folder / "ProbablyStolenMixer.dll").is_file():
        raise SystemExit("Build the mod before preparing upload.")
    values = {
        "appid": "4348910",
        "publishedfileid": item_id,
        "contentfolder": str(folder),
        "previewfile": str(ROOT / "publishing/workshop-preview.png"),
        "visibility": "0",
        "title": "Precision Mixer / 精密配比机",
        "description": (ROOT / "publishing/workshop-description.txt").read_text(),
        "changenote": "v0.1.8: initial public upload; chemical-module metering upgrades, bilingual UI and four container-state sprites.",
    }
    TARGET.parent.mkdir(parents=True, exist_ok=True)
    TARGET.write_text('"workshopitem"\n{\n' + ''.join(f'    {quote(k)} {quote(v)}\n' for k, v in values.items()) + '}\n')
    print(f"Prepared {TARGET} (publishedfileid={item_id}; visibility=public)")

if __name__ == "__main__":
    main()
