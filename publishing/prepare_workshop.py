#!/usr/bin/env python3
"""Generate a machine-local SteamCMD VDF, preserving its published item ID."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
TARGET = ROOT / ".tools/steamcmd/workshop.vdf"

def quote(value):
    return '"' + str(value).replace('\\', '\\\\').replace('"', '\\"').replace('\r', '') + '"'


def main():
    recorded = ROOT / "publishing/workshop-id.txt"
    item_id = recorded.read_text().strip() if recorded.exists() else "0"
    if not item_id.isdigit():
        raise SystemExit("Invalid recorded workshop ID.")
    if TARGET.exists():
        match = re.search(r'"publishedfileid"\s+"(\d+)"', TARGET.read_text())
        if not match:
            raise SystemExit("Existing workshop.vdf has no publishedfileid; refusing to risk duplicate publication.")
        local_id = match.group(1)
        if local_id != "0" and item_id != "0" and local_id != item_id:
            raise SystemExit("Local workshop ID differs from the recorded public item; refusing to upload.")
        if local_id != "0":
            item_id = local_id
    folder = ROOT / "dist/PrecisionMixer"
    if not (folder / "manifest.xml").is_file() or not (folder / "ProbablyStolenMixer.dll").is_file():
        raise SystemExit("Build the mod before preparing upload.")
    import xml.etree.ElementTree as ET
    version = ET.parse(folder / "manifest.xml").findtext("ModVersion")
    if version != "0.1.9":
        raise SystemExit(f"Workshop metadata still describes 0.1.9, not staged {version}; update and review it before publishing.")
    values = {
        "appid": "4348910",
        "publishedfileid": item_id,
        "contentfolder": str(folder),
        "previewfile": str(ROOT / "publishing/workshop-preview.png"),
        "visibility": "0",
        "title": "Precision Mixer / 精密配比机",
        "description": (ROOT / "publishing/workshop-description.txt").read_text(),
        "changenote": "v0.1.9: adopt GNU GPL v3 only; include LICENSE, NOTICE and corresponding source links. Gameplay unchanged from v0.1.8.",
    }
    TARGET.parent.mkdir(parents=True, exist_ok=True)
    TARGET.write_text('"workshopitem"\n{\n' + ''.join(f'    {quote(k)} {quote(v)}\n' for k, v in values.items()) + '}\n')
    print(f"Prepared {TARGET} (publishedfileid={item_id}; visibility=public)")

if __name__ == "__main__":
    main()
