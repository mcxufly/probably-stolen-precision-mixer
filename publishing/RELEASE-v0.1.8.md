# v0.1.8 — Precision Mixer / 精密配比机

A battery-free metered fluid mixer for Probably Stolen. Buy from Dr. Jackson (base value 350), insert target/source bottles, choose a component, and enter a target percentage.

- Native component dropdown and percentage entry (0.1% input steps).
- Up to four original chemical modules improve actual dosing: 10 mL → 5 mL → 1 mL → 0.1 mL → 1 µL. Modules lock after installation.
- Previews show the predicted concentration and deviation in percentage points.
- Real source fluid is consumed; no generated fluid, artificial impurity, or purity ceiling.
- English/Chinese localization; four bottle-occupancy sprite states.
- Tested against Unity product version `0.46D`, Steam build `26337366`.

## 中文

无需电池的定量配比机，在杰克逊博士处购买，基础价值350。放入待处理瓶与原料瓶，选择目标物质并填写百分比。

- 原生物质下拉框、百分比输入，输入步进0.1%。
- 最多4个原版化学模组，实际加料步长依次为10mL、5mL、1mL、0.1mL、1µL；安装后锁定。
- 预览显示预计成品比例及偏差，不随机造假、不凭空产生液体或杂质、不设人为纯度上限。
- 中英文界面及四种容器占用图标。

## Installation / 安装

Extract `PrecisionMixer` into the game's `Mods` folder, activate it in the Mods menu, then restart.

把安装包中的 `PrecisionMixer` 文件夹解压到游戏的 `Mods` 目录，在模组菜单启用后重启。

## Status and saves / 状态与存档

Automated compilation, metering tests, game apportionment, manifest compatibility, and game scanner checks passed. **In-game UI clicks, acquisition, and save/load have not yet been manually verified.** Back up saves and try a test slot first. Remove bottles and dispose of all mixer machines before uninstalling. Installed chemical modules are locked and are lost if their machine is discarded.

自动编译、计量测试、游戏真实液体分配、版本匹配和扫描器检查已通过；**尚未手动验收游戏内点击、获取及读档流程**。请先备份并使用测试存档。卸载前取出瓶子并处理掉机器；已安装化学模组锁定在机器中，丢弃机器会连同模组一起失去。
