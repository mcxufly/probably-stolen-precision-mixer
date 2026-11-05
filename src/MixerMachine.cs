using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProbablyStolenMixer
{
    public sealed class MixerMachine
    {
        private const string PercentTag = "XUFLY_MIX_PERCENT_TENTHS";
        private const string LiquidTag = "XUFLY_MIX_LIQUID";
        private const int EnergyPerMix = 2;
        private readonly GameSlotInventory battery = new GameSlotInventory();
        private readonly GameSlotInventory target = new GameSlotInventory(2, 3);
        private readonly GameSlotInventory source = new GameSlotInventory(2, 3);
        private readonly MachineryText substance = new MachineryText();
        private readonly MachineryText percentage = new MachineryText();
        private readonly MachineryText targetInfo = new MachineryText();
        private readonly MachineryText sourceInfo = new MachineryText();
        private readonly MachineryText preview = new MachineryText();
        private readonly MachineryText status = new MachineryText("放入两瓶液体，选择成分和比例。");
        private readonly MachinerySlider slider = new MachinerySlider(30f, 180).SetRange(0f, 100f, 0.1f);
        private GameItem machine;

        public static GameItem Create() => new MixerMachine().Build();

        private GameItem Build()
        {
            target.mayInventoryAddItemFunc = (item, _) => AcceptBottle(item);
            source.mayInventoryAddItemFunc = (item, _) => AcceptBottle(item);
            var controls = MachineryStack.Vertical(
                substance,
                MachineryStack.Horizontal(new MachineryButton("上一成分", () => SelectLiquid(-1)), new MachineryButton("下一成分", () => SelectLiquid(1))),
                percentage, slider,
                MachineryStack.Horizontal(new MachineryButton("-0.1%", () => ChangePercent(-1)), new MachineryButton("+0.1%", () => ChangePercent(1))),
                targetInfo, sourceInfo, preview,
                MachineryStack.Horizontal(new MachineryButton("刷新预览", Refresh), new MachineryButton("开始配比", Mix)), status);
            var window = MachineHelper.CreateMachineryWindow(true,
                MachineHelper.PowerPanel(battery), MachineHelper.Panel("待处理瓶", target, "target"),
                MachineHelper.Panel("掺杂原料瓶", source, "source"), MachineHelper.Panel("精密配比", controls, "control"));
            machine = ItemDirectory.CreateEmptyItem().SetContentWindow(window)
                .SetSpriteAndShapeFromMod(ModEntry.Id, "precision_mixer");
            machine.name = "精密配比机";
            machine.shortDescription = "消耗原料瓶中的混合液，把待处理瓶的指定成分调至目标百分比。每次耗电2；比例步进0.1%。";
            machine.flavorText = "让每一滴都恰到好处。";
            machine.unitValue = 650;
            machine.SetGameItemType(TypeHelper.MACHINE);
            machine.EnableTag(ContainerHelper.CONTAINER_TAG);
            MachineHelper.SetupBatterySlot(battery, machine);
            machine.ModifyTag(PercentTag, s => s.Enable().SetInt(300));
            machine.ModifyTag(LiquidTag, s => s.Enable().SetString(Liquid.LIQUID_WATER));
            slider.onChanged += value => SetPercent((int)Math.Round(value * 10));
            target.onSlotAddItemEndFunc += (_, __, ___) => Refresh();
            source.onSlotAddItemEndFunc += (_, __, ___) => Refresh();
            target.onSlotRemoveItemEndFunc += (_, __, ___) => Refresh();
            source.onSlotRemoveItemEndFunc += (_, __, ___) => Refresh();
            machine.onLoaded += _ => { slider.SetValue(Percent / 10f); Refresh(); };
            machine.onOneSecondItemFunc += (_, __, ___) => Refresh();
            Refresh();
            return machine;
        }

        private static bool AcceptBottle(GameItem item) => GeneralHelper.IsItemOwned(item)
            && item.IsTag(WaterHelper.LIQUID_CONTAINER_TAG) && !item.IsTag(WaterHelper.FILTER_CONTAINER_TAG)
            && !item.IsTag("WINE_CONTAINER_TAG") && WaterFeatureHelper.GetWaterFeature(item) != null;

        private int Percent => Math.Max(0, Math.Min(1000, machine.GetTagReadonly(PercentTag).GetInt()));
        private string SelectedId => machine.GetTagReadonly(LiquidTag).GetString();
        private int SelectedIndex => Liquid.Liquids.FindIndex(l => l.identifier == SelectedId);

        private void SetPercent(int tenths)
        {
            machine.ModifyTag(PercentTag, s => s.Enable().SetInt(Math.Max(0, Math.Min(1000, tenths))));
            Refresh();
        }

        private void ChangePercent(int delta)
        {
            SetPercent(Percent + delta);
            slider.SetValue(Percent / 10f);
        }

        private void SelectLiquid(int direction)
        {
            var choices = new List<Liquid>();
            foreach (var liquid in Liquid.Liquids)
                if (liquid.identifier == SelectedId || Amount(target.childItem, liquid) > 0 || Amount(source.childItem, liquid) > 0)
                    choices.Add(liquid);
            if (choices.Count == 0) return;
            int index = choices.FindIndex(l => l.identifier == SelectedId);
            index = (Math.Max(0, index) + direction + choices.Count) % choices.Count;
            machine.ModifyTag(LiquidTag, s => s.Enable().SetString(choices[index].identifier));
            Refresh();
        }

        private static int Amount(GameItem item, Liquid liquid) => item == null ? 0 : item.GetTagReadonly(liquid.currentPart).GetInt();
        private static string Number(double value, string format = "0.###") => value.ToString(format, CultureInfo.InvariantCulture);
        private static void Text(MachineryText text, string value) { if (text.GetText() != value) text.SetText(value); }

        private static string BottleInfo(string label, GameItem bottle, Liquid liquid)
        {
            if (bottle == null) return label + "：未放入";
            int total = WaterHelper.GetTotalVolume(bottle);
            double fraction = total > 0 ? Amount(bottle, liquid) * 100.0 / total : 0;
            return label + "：" + Number(total / 1000.0) + "mL，" + Number(fraction, "0.000") + "%";
        }

        private void Refresh()
        {
            int index = SelectedIndex;
            if (index < 0 || machine == null) return;
            Liquid liquid = Liquid.Liquids[index];
            Text(substance, "目标成分：" + liquid.displayName);
            Text(percentage, "目标比例：" + Number(Percent / 10.0, "0.0") + "%");
            Text(targetInfo, BottleInfo("待处理", target.childItem, liquid));
            Text(sourceInfo, BottleInfo("原料", source.childItem, liquid));
            if (TryPlan(out int amount, out double fraction, out string reason))
                Text(preview, amount == 0 ? "已达到目标，无需处理。" : "加入 " + Number(amount / 1000.0) + "mL → " + Number(fraction * 100, "0.000") + "%");
            else Text(preview, reason);
        }

        private bool TryPlan(out int amount, out double fraction, out string reason)
        {
            amount = 0; fraction = 0; reason = "请放入待处理瓶和原料瓶。";
            GameItem t = target.childItem, s = source.childItem;
            int index = SelectedIndex;
            if (t == null || index < 0) return false;
            if (!AcceptBottle(t) || (s != null && (!AcceptBottle(s) || ReferenceEquals(t, s))))
            { reason = "仅支持自有普通液体瓶，不支持滤水容器和酒类容器。"; return false; }
            int total = WaterHelper.GetTotalVolume(t);
            int sourceTotal = s == null ? 0 : WaterHelper.GetTotalVolume(s);
            var plan = MixPlanner.Plan(total, Amount(t, Liquid.Liquids[index]), WaterHelper.GetTotalCapacity(t),
                sourceTotal, Amount(s, Liquid.Liquids[index]), Percent / 10.0);
            if (!plan.Success && plan.Failure != MixFailure.PrecisionLimit)
            { reason = FailureText(plan.Failure); return false; }
            amount = plan.TransferMicroliters;
            fraction = plan.PredictedFraction;
            if (plan.Success && amount == 0) return true;
            if (plan.Failure == MixFailure.PrecisionLimit)
            {
                double sourceFraction = Amount(s, Liquid.Liquids[index]) / (double)sourceTotal;
                double desired = Percent / 1000.0;
                double ideal = (desired * total - Amount(t, Liquid.Liquids[index])) / (sourceFraction - desired);
                amount = (int)Math.Round(ideal);
            }
            int[] parts = WaterHelper.GetLiquidParts(s);
            int max = Math.Min(sourceTotal, WaterHelper.GetFreeCapacity(t));
            double best = double.MaxValue;
            int bestAmount = amount;
            // Check actual integer component allocation before changing either bottle.
            for (int offset = -32; offset <= 32; offset++)
            {
                long candidate = (long)amount + offset;
                if (candidate <= 0 || candidate > max) continue;
                int[] transferred = WaterHelper.SplitProportionally(parts, (int)candidate);
                double projected = ((long)Amount(t, Liquid.Liquids[index]) + transferred[index]) / (double)(total + candidate);
                double error = Math.Abs(projected - Percent / 1000.0);
                if (error < best) { best = error; bestAmount = (int)candidate; fraction = projected; }
            }
            if (best > 0.00005) { reason = "微升精度下无法达到目标，请增大配液量。"; return false; }
            amount = bestAmount;
            return true;
        }

        private void Mix()
        {
            if (!GeneralHelper.IsItemOwned(machine)) { Text(status, "请先购买机器。"); return; }
            if (!TryPlan(out int amount, out _, out string reason)) { Text(status, reason); return; }
            if (amount == 0) { Text(status, "已达到目标，未消耗原料或电量。"); return; }
            if (battery.childItem == null || !PowerHelper.CanDrawPowerSource(battery.childItem, EnergyPerMix))
            { Text(status, "电量不足，需要2点电量。"); return; }
            GameItem t = target.childItem, s = source.childItem;
            int[] beforeTarget = WaterHelper.GetLiquidParts(t);
            int[] beforeSource = WaterHelper.GetLiquidParts(s);
            int[] moved = WaterHelper.SplitProportionally(beforeSource, amount);
            ChemicalHelper.HandleReceiveLiquidTemp(t, amount, s);
            for (int i = 0; i < moved.Length; i++)
            {
                if (moved[i] == 0) continue;
                int newTarget = beforeTarget[i] + moved[i], newSource = beforeSource[i] - moved[i];
                string tag = Liquid.Liquids[i].currentPart;
                t.ModifyTag(tag, state => state.Enable().SetInt(newTarget), skipValidate: true);
                s.ModifyTag(tag, state => state.Enable().SetInt(newSource), skipValidate: true);
            }
            WaterFeatureHelper.OnLiquidContainerPourInto(t, s, false);
            WaterFeatureHelper.OnLiquidContainerPourOutOf(s);
            WaterHelper.UpdateContainerGoodType(t);
            WaterHelper.UpdateContainerGoodType(s);
            WaterHelper.ResetLastScanResult(t);
            WaterHelper.ResetLastScanResult(s);
            t.Validate();
            s.Validate();
            PowerHelper.DrawPowerSource(battery.childItem, EnergyPerMix);
            Refresh();
            int total = WaterHelper.GetTotalVolume(t);
            double actual = Amount(t, Liquid.Liquids[SelectedIndex]) * 100.0 / total;
            Text(status, "完成：加入" + Number(amount / 1000.0) + "mL，当前" + Number(actual, "0.000") + "%。");
        }

        private static string FailureText(MixFailure failure)
        {
            switch (failure)
            {
                case MixFailure.EmptyTarget: return "待处理瓶为空，请先加入一些基液。";
                case MixFailure.EmptySource: return "原料瓶为空或未放入。";
                case MixFailure.Unreachable: return "此原料无法达到目标，请更换浓度合适的原料。";
                case MixFailure.InsufficientSource: return "原料量不足，请补充原料或减少待处理液。";
                case MixFailure.InsufficientCapacity: return "待处理瓶空间不足，请先转移一部分液体。";
                case MixFailure.PrecisionLimit: return "微升精度下无法达到目标，请增大配液量。";
                default: return "瓶内成分或容量数据无效。";
            }
        }
    }
}
