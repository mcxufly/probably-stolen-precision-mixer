# 精密配比机 / Precision Mixer

Source: https://github.com/mcxufly/probably-stolen-precision-mixer · [Releases](https://github.com/mcxufly/probably-stolen-precision-mixer/releases) · [Steam Workshop / 创意工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3834261506) · [Publishing / 发布说明](publishing/README.md)

《深空当铺：可能是偷的》Mod，版本 0.1.9。按本机 Steam build 26337366 的真实程序集编译。兼容版本填写 Unity `PlayerSettings.bundleVersion` / `Application.version`：`0.46D`；游戏内部显示字符串 `VERSION 100-Rev6` 不用于模组兼容性匹配。

## 许可证 / License

Copyright (C) 2026 XuFly。项目的原创源码、脚本、文档、翻译和美术采用 **GNU General Public License v3.0 only**，SPDX 标识为 **`GPL-3.0-only`**。许可证全文见 [LICENSE](LICENSE)，版权、无担保与对应源码说明见 [NOTICE](NOTICE)。游戏、Unity及其他第三方组件保留各自许可，不随安装包提供。

本程序可在 GPL v3 条件下修改、再分发，不提供担保。分发修改版时，请遵守许可证的源码提供、相同许可、保留声明及标注修改等要求。

Corresponding source for this build, including build and installation instructions, is available at [v0.1.9](https://github.com/mcxufly/probably-stolen-precision-mixer/releases/tag/v0.1.9) as `PrecisionMixer-0.1.9-source.zip`, or from [the source tag](https://github.com/mcxufly/probably-stolen-precision-mixer/tree/v0.1.9). This mod is free software under GPL version 3 only and comes without warranty.

## 安装

将发布包中的 `PrecisionMixer` 整个文件夹放到游戏目录的 `Mods` 下：

```text
Probably Stolen/
└── Mods/
    └── PrecisionMixer/
        ├── manifest.xml
        ├── ProbablyStolenMixer.dll
        └── Textures/Items/precision_mixer.png
```

启动游戏，在「模组」中启用「精密配比机」，然后退出并重启游戏。发布包只包含自己的 DLL、图片、清单及说明，不包含任何游戏程序集。

开发时打包产物保存在本项目 `dist/PrecisionMixer`。本项目不会自动改动游戏目录。

## 获取机器

在杰克逊博士的售卖清单中购买，基础价值 350，最终售价由游戏计算。安装前已经生成的售卖清单可能要等刷新。

也可以在游戏可执行文件旁新建空的 `dev.txt`，重启、进入一局游戏，按 F9，在 Spawn 栏点击「生成精密配比机」。这会生成机器，不会生成液体原料。`dev.txt` 是可选的开发工具开关，不需要随 Mod 安装。

## 操作

1. 「待处理瓶」放入已有基液的瓶子；「掺杂原料瓶」放入提供掺杂液的瓶子。机器不需要电池或供电。
2. 点击目标物质下拉框，直接选择要控制的成分，采用与标签机相同类型的游戏原生 `TMP_Dropdown`。列表包含两瓶现有成分及当前选定成分；换瓶后自动更新，物质名沿用游戏语言。
3. 点击「填写比例 (%)」，输入 `30` 或 `12.5` 后点击「应用」，分别表示 30% 和 12.5%；允许末尾带 `%` 或 `％`。范围为 0–100，最多一位小数，非法输入会提示且不改变原值。也可以使用滑块与 ±0.1% 按钮，输入结果与滑块同步。
4. 可将原版化学模组放进「化学模组」仓，最多4个，沿用原版安装后锁定规则。界面显示已安装数量与加料计量步长；模组提升实际成品准确度，不改变百分比输入位数。
5. 查看预计加入的 mL 数、处理后百分比和相对目标的偏差（单位为百分点）。按「开始配比」执行一次定量配比；完成后的瓶子仍在原槽位。

| 化学模组数量 | 加料最小计量单位 |
|---:|---:|
| 0 | 10mL |
| 1 | 5mL |
| 2 | 1mL |
| 3 | 0.1mL |
| 4 | 0.001mL（1µL） |

机器在当前步长的整数倍中选择更接近目标的用量；低档位的成品可能偏离目标，预览会明确显示。原料必须足够完成理论配比，且待处理瓶须有相应空间；不会为了凑数使用不符合计量步长的余量。若当前档位不能继续改善成品，会提示升级或增加配液量，反复点击不会刷出随机结果。所有档位都保留目标输入0.1%的步进；装满模组后按整数微升尽量配准，不设置人为纯度上限。

例如：待处理瓶里有 100mL、目标成分占 60% 的液体，原料瓶为不含该成分的稀释液，目标设为 30%，则消耗 100mL 原料，得到 200mL、30% 的液体。待处理瓶需要至少 200mL 容量。

原料也可以是混合液。机器抽取其中一部分，所有成分都按原料原有比例一起进入待处理瓶，不会只抽出某个成分。

## 配比规则

- 单次操作处理一个目标成分和一瓶原料。降低浓度时用更低浓度原料，提高浓度时用更高浓度原料。
- 通过真实转移完成配比，不凭空生成、删除、分离或自动丢弃液体。
- 如果满瓶、容量不足、原料不足或该原料无法达到目标，拒绝操作，不进行部分转移，也不消耗原料。先移出一些待处理液，或换大瓶、补原料，再处理。
- 空的待处理瓶需要先加入基液。本版不提供多瓶配方或自动换液。
- 只接受自有、带普通水质信息的液体瓶；过滤容器及酒类专用容器暂不接入。
- 比例按整个瓶内所有液体的体积计算；设定步进0.1%，不随模组变化。实际加料分辨率随化学模组升级，所有档位都显示实际计量偏差；装满4个模组时达到1µL计量分辨率，但极小液量仍可能有不可消除的取整偏差。已在±0.005个百分点内时不重复耗料。
- 每次有效操作立即完成，不使用夜间生产周期，无需电池或供电。模组仓只接受自有化学模组，一次一个，最多4个；模组不会在配比时消耗，安装后锁定，不可普通拖出。
- 机器占用 4×4 个背包格，采用原创工业配液机像素图标。成分和目标百分比保存在每台机器的物品标签中，随存档保存；机器内的两瓶液体与已安装化学模组通过原生容器系统保存。模组仓追加在已有存储节点之后，保留0.1.5以后两瓶的存档顺序。
- 图标使用灰绿机壳、紫灰凹槽和青色玻璃器皿的原创像素美术，提供空机、仅待处理瓶、仅原料瓶、两瓶齐全四种状态。放入或取出容器后立即切换，读档后自动恢复；图示器皿是槽位占用标记，不冒充真实液量或浓度。四张贴图尺寸与透明轮廓一致，切换不改变4×4占格。
- 原料温度按体积混合；水质、图标、物品类型和旧检测结果会刷新。游戏原有的化学反应继续有效，因此后续反应可能改变刚配好的比例。

## 本地化 / i18n

- 使用游戏原生 `ModHelper.GetLocalized` 接口，随游戏语言选择中文（`zh`）或英文（`en`）；其他语言及缺失条目回退到英文。
- 文案位于 `assets/Localization/zh.csv` 和 `en.csv`，UTF-8、无表头，每行一个 `key,text`；含英文逗号的值使用 CSV 双引号，格式占位符为 `{0}`、`{1}`。
- 机器名称、描述、按钮、容器标签、百分比输入窗口、用量预览、结果与错误提示均已翻译；物质名取游戏自己的本地化表。
- 已有机器在刷新时更新名称、描述、按钮与状态；游戏中切换语言后约一秒内刷新。已打开的百分比输入窗口需要关闭后重新打开；F9 调试按钮在启动及载入存档时更新语言。
- 模组菜单清单是静态元数据，使用中英双语名称和说明。数字统一使用小数点（例如 `12.5`），单位保持 `%` 和 `mL`。
- 添加其他语言只需复制 `en.csv` 为游戏支持的语言代码并翻译其值；不需要改 C#。不把 `.inspect` 中的游戏资料放进语言包。

## 存档与卸载

配比会真实改变瓶内成分和原料余量，请先在测试存档使用。卸载前取出机器内的瓶子，并售出或丢弃所有精密配比机，再保存、退出、停用 Mod。建议保留安装前的存档备份。

## 构建

需要 .NET SDK 8 或更新版（项目目标为 netstandard2.1）。Linux 下从本项目目录执行：

```bash
./build.sh
```

默认引用 `/data/SteamLibrary/steamapps/common/Probably Stolen/Probably Stolen_Data/Managed`，其他位置可执行：

```bash
./build.sh -p:GameDir="/path/to/Probably Stolen"
```

编译时只引用游戏 DLL，不复制游戏 DLL 到发布目录。构建产物自动暂存到 `dist/PrecisionMixer`。

## 已完成验证与边界

- 本机游戏程序集编译：0 警告、0 错误。
- 24 组纯算法测试通过，包含 10,000 个资源/容量精确边界组合及 225 个混合配比组合。
- 新增8组计量升级测试，覆盖全部5档、重复操作、取整、容量/缺料边界、500组随机案例和大整数；调用真实游戏 `WaterHelper.SplitProportionally` 验证9,992组混合液在各档的体积守恒、预测一致及升级准确度不倒退。
- 调用真实游戏 `ModScanner` 检查发布目录：没有额外能力声明需求，没有拒绝项。
- 中英文各 47 条文案的键名、占位符及源码引用检查通过；本地化检查命令：`python verification/check_localization.py`。
- 按未发布开发版处理：直接移除电池槽，不保留旧电池回收或旧存储布局兼容。测试过旧版本的存档请先取出旧机器中的物品并重新生成机器，或使用新测试存档。
- 四种容器贴图的选择与放入/取出状态序列检查通过；图标脚本还校验了所有状态透明轮廓及4×4占格相同。
- 用户已确认v0.1.8的化学模组相关游戏内测试没有问题；本次v0.1.9增加许可证和发布声明，玩法逻辑未改变。其他未逐项确认的场景不视为已完成全面验收。

算法测试：`dotnet run --project tests/MixPlanner.Tests.csproj -c Release`。
实际游戏 API 检查：`dotnet run --project verification/GameApiChecks.csproj -c Release -- 0.46D`。最后一个参数是从本机 Unity PlayerSettings 读取的产品版本；检查使用游戏自身的版本归一化方法，并验证清单兼容性。

## 文件范围

`src` 为原创 Mod 源码，`assets` 为发布资源，`art/draw_mixer.py` 是原创图标的可复现绘制脚本（需要 Pillow），`tests` 为算法测试，`verification` 为调用本机游戏接口的验证程序。`.inspect` 仅是本地研究接口的临时资料，不随安装包或源码包发布；`.tools`、`.dotnet`、`.nuget` 是本项目内的开发工具和缓存。

官方开发文档：https://github.com/dragonpan2/probably-stolen-modding
