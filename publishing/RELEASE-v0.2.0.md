# v0.2.0 — Manual dosing and analyzer-gated mixing / 仅掺杂与检测仪门槛

## Changes / 变更

- New **dosing-only mode**: enter an mL dose, rounded down to the current chemical-module dose step. Shows volume only; does not display composition, concentration, target-component options or final mixture ratios. Empty target bottles are allowed.
- **Automatic concentration mixing requires a currently owned `chem_scanner`** (full chemical analyzer). Water strips/aquascan do not qualify. Losing the analyzer forces dosing-only mode and clears prior diagnostic output.
- Detector possession includes recursively owned shop/afterhours inventory; scanners belonging to NPCs or inside non-owned goods do not qualify.
- Dr. Jackson offers the mixer with a **25% daily chance**, stable for the save's run ID and day. Revisiting or reloading the pre-visit save does not reroll that day.
- At most **one successful Jackson purchase per run**. Merely viewing/offering stock does not consume the purchase. Selling/discarding a machine after purchase does not reset it. Existing owned machines suppress new stock; old purchases cannot be retroactively identified. Debug spawning is not a purchase.
- Base value350, battery-free operation, four chemical-module upgrade tiers, sprites, GPL-3.0-only licensing and existing bottle storage order are preserved.

新增仅掺杂模式：手动指定mL加入量，按计量步长向下取整，不显示真实成分/浓度/成品比例。自动浓度配比需当前拥有完整化学检测仪`chem_scanner`；失去仪器会强制切回仅掺杂并清理旧检测输出。水质试纸和水质扫描器不算完整检测仪。

博士每日25%概率上架，按存档局标识和日期稳定决定，同日重进或读档不能重抽。每存档最多成功购买一台，买后卖掉/丢弃不再补货。已有机器暂时阻止再上架，调试生成不记购买，无法追溯旧版购买记录。

## Testing / 测试

Includes pure manual-dose and deterministic stock-policy tests, original metered-mixture tests, localization checks, real-game allocation/manifest/scanner validation. **These new runtime flows still require in-game manual testing**: analyzer removal, mode UI privacy, actual direct/negotiated/bulk purchase recording, save/reload, and daily shop transitions. Earlier chemical-module gameplay tests do not cover these new flows.

## Installation / 安装

Close the game, replace the `PrecisionMixer` Mod folder, enable and restart. Back up saves first. Source ZIP includes build scripts and GPL license notices. Built against Unity product version0.46D. This package is prepared locally; GitHub/Workshop are not updated automatically by building it.
