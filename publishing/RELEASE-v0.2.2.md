# 0.2.2 — 3×3 footprint and native-resolution art

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
- On 2026-11-09 the user explicitly confirmed that this 0.2.2 update had been tested successfully. The user did not enumerate every old-save, rotated-placement or UI boundary scenario; this is not a claim of exhaustive coverage. The earlier v0.1.8 chemical-module test confirmation is retained separately.

## Included since the last public release / 相对上次公开版本

The last public release was0.1.9; this release also includes the subsequent dual-mode dosing and stock changes:

- Manual mL dosing works without a detector, including into an empty target bottle. Amounts round down to the chemical-module metering step; the preview shows the actual dose.
- Automatic percentage mixing requires an owned original `chem_scanner` (not water-test paper or a basic water scanner). Losing it returns the machine to manual mode and clears analysis UI.
- Dr. Jackson stocks the machine with a stable25% daily chance, with one successful purchase per save. The in-machine bilingual help explains the rules.

仅掺杂模式无需检测仪；自动配比必须持有原版完整化学检测仪。博士每日25%稳定概率上架，每存档限购一台；机器内提供中英文说明。

## Upgrade / 升级

Subscribe/update through the existing Workshop item or replace the manual-install `PrecisionMixer` folder, then restart the game. Back up saves first. The machine is smaller but storage-node order and contents are unchanged. Before uninstalling, remove bottles and dispose of all mixers; installed chemical modules remain locked.

This release updates the existing public GitHub repository and Workshop item3834261506; it does not create a duplicate item. Installation and corresponding-source archives include GPL-3.0-only licensing and build instructions.
