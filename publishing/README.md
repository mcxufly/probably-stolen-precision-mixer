# Publishing / 发布

- Repository: https://github.com/mcxufly/probably-stolen-precision-mixer
- Release: https://github.com/mcxufly/probably-stolen-precision-mixer/releases/tag/v0.2.2
- Existing public Workshop item: https://steamcommunity.com/sharedfiles/filedetails/?id=3834261506

Version0.2.2 adds a full3×3 footprint, original48px four-state art, and footprint restoration on load. It also includes the dual-mode/analyzer gate and Jackson stock changes since the last public release0.1.9. Release notes: `RELEASE-v0.2.2.md`.

## Build and validate

```sh
bash build.sh
python3 verification/check_localization.py
python3 verification/check_sprites.py
python3 verification/check_sprites.py dist/PrecisionMixer
python3 publishing/draw_preview.py
python3 publishing/prepare_workshop.py
```

Run the algorithm and real-game API checks described in the root README. Only `dist/PrecisionMixer` is Workshop content. Do not include game DLLs, reference sprites, decompiled output, development scripts, credentials, archives or artwork source in that folder. Preview artwork is original and its enlarged sprite uses integer nearest-neighbour sampling; keep the file below1MB.

## GitHub

Keep the manifest, documentation, `v0.2.2` tag, release and archive names in sync. Attach `PrecisionMixer-0.2.2.zip` plus `PrecisionMixer-0.2.2-source.zip`; the latter must contain tracked original sources, resources, reproducible art/build scripts, LICENSE and NOTICE. Preserve older tags. Verify the public release status and downloaded archive hashes after upload.

## Steam Workshop

App ID4348910, visibility public. `workshop-id.txt` records the existing item3834261506. The VDF generator preserves its ID and aborts on a conflicting nonzero ID. Never reset this ID to retry an update.

```sh
bash publishing/upload_workshop.sh YOUR_STEAM_LOGIN_NAME
```

The script prefers the user's installed `steamcmd` or `STEAMCMD_BIN`, falling back to the project-local copy. It uses SteamCMD's normal account cache without redefining HOME. A previous cache under `.tools/steam-home` is not automatically copied or reused. Password/Steam Guard must be entered by the user in their own interactive terminal; never send them in chat or put them in command arguments/logs/source. Authentication caches remain outside tracked sources.

After upload, verify the public item page, creator, App, preview, bilingual description, subscribe controls and update version. Cached or delayed Workshop content is not proof of a failed upload; check the actual update timestamp and upload result before retrying.

## Test status

The build, algorithms, real game fluid allocation, manifest compatibility, sprite sampling and native scanner are automatically checked. The user explicitly confirmed successful testing of the0.2.2 update on2026-11-09, in addition to the earlier0.1.8 chemical-module confirmation. This is not a claim that every UI/save-load/rotated-placement boundary was individually tested.
