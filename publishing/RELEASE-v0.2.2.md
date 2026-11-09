# 0.2.2 — 3×3 footprint and native-resolution art (local build)

## Changes / 修改

- Machine footprint shrinks from 4×4 to a full 3×3 inventory square (48×48px at the current game's 16px/cell).
- Four original native-resolution sprites redrawn by a delegated GPT-6.1 Sol visual/code Agent using Python/Pillow, visually referencing the installed game's blender, chemical finisher, evaporator and heating plate. No original game assets are included.
- Empty / target only / source only / both occupancy states share identical binary alpha. Two bottle stations and a small control panel remain recognizable; previews use integer nearest-neighbour scaling.
- Reload callback reapplies the sprite-derived footprint because the game's save decoder restores the old saved shape after the item factory. Refresh still only switches visual states; it does not rebuild shape every second.
- Bottle/module storage order, internal capacities, mixing behavior, prices and stock policy remain unchanged.

机器由4×4缩至3×3；原生48px重绘四态，不是缩小旧图。旧存档载入时重新按贴图计算占格，不改动内容物和存储顺序。升级前请备份，首次加载检查位置、旋转、两瓶与已安装模组，再保存退出重载确认。

## Verification / 验证

- Current installed Steam build 26525552, internal display VERSION 100-Rev13, Unity product version 0.46D.
- Real-game assembly build: 0 warnings / 0 errors.
- Independent source and staged sprite checks: native48px RGBA, identical binary alpha, all nine game-sampled centres occupied, distinct and independently composable bottle states.
- Baseline24 groups, metering8 groups (including500 seeded cases), manual-dose/stock policy tests pass.
- Real WaterHelper proportional splitting:9,992 cases and all five metering levels pass.
- Real manifest parser/version normalization and ModScanner pass; no capabilities or hard refusal flags.
- Both locales:72 matching keys and valid placeholders.
- Four-state art was visually inspected; deterministic generation checked.
- No interactive game test of this new footprint, old-save migration or rotated placement has been performed. Existing v0.1.8 chemical-module test confirmation is not treated as acceptance of this new version.

## Scope / 范围

Local development build only. No game-directory installation, Steam upload, GitHub push/release, or release tag was performed for this task. Use the paired installation/source archives; the source includes the reproducible art script and build instructions.
