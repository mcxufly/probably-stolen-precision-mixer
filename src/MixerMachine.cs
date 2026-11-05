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
        private ComponentDropdown componentDropdown;
        private readonly List<Liquid> componentChoices = new List<Liquid>();
        private readonly MachineryText percentage = new MachineryText();
        private readonly MachineryText targetInfo = new MachineryText();
        private readonly MachineryText sourceInfo = new MachineryText();
        private readonly MachineryText preview = new MachineryText();
        private readonly MachineryText status = new MachineryText(L10n.Text("initial_status"));
        private readonly MachinerySlider slider = new MachinerySlider(30f, 180).SetRange(0f, 100f, 0.1f);
        private GameItem machine;
        private readonly List<Action> refreshLabels = new List<Action>();
        private string statusKey = "initial_status";
        private object[] statusArgs = Array.Empty<object>();

        private MachineryButton Button(string key, Action action)
        {
            var button = new MachineryButton(L10n.Text(key), action);
            string previous = L10n.Text(key);
            refreshLabels.Add(() =>
            {
                string current = L10n.Text(key);
                if (current != previous) { button.SetText(current); previous = current; }
            });
            return button;
        }

        private MachineryPanel Panel(string key, PixelElement content, string role)
        {
            var panel = MachineHelper.Panel(L10n.Text(key), content, role);
            string previous = L10n.Text(key);
            refreshLabels.Add(() =>
            {
                string current = L10n.Text(key);
                if (current != previous) { panel.SetLabel(current); previous = current; }
            });
            return panel;
        }

        private void Status(string key, params object[] args)
        {
            statusKey = key;
            statusArgs = args;
            Text(status, L10n.Text(key, args));
        }

        public static GameItem Create() => new MixerMachine().Build();

        private GameItem Build()
        {
            target.mayInventoryAddItemFunc = (item, _) => AcceptBottle(item);
            source.mayInventoryAddItemFunc = (item, _) => AcceptBottle(item);
            componentDropdown = new ComponentDropdown(SelectLiquid);
            var controls = MachineryStack.Vertical(
                substance, componentDropdown,
                percentage, Button("enter_percent", OpenPercentInput), slider,
                MachineryStack.Horizontal(Button("decrease_percent", () => ChangePercent(-1)), Button("increase_percent", () => ChangePercent(1))),
                targetInfo, sourceInfo, preview,
                MachineryStack.Horizontal(Button("refresh_preview", Refresh), Button("start_mix", Mix)), status);
            var window = MachineHelper.CreateMachineryWindow(true,
                MachineHelper.PowerPanel(battery), Panel("panel_target", target, "target"),
                Panel("panel_source", source, "source"), Panel("panel_control", controls, "control"));
            machine = ItemDirectory.CreateEmptyItem().SetContentWindow(window)
                .SetSpriteAndShapeFromMod(ModEntry.Id, "precision_mixer");
            machine.name = L10n.Text("item_name");
            machine.shortDescription = L10n.Text("item_description");
            machine.flavorText = L10n.Text("item_flavor");
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

        private void OpenPercentInput()
        {
            if (CustomUIManager.Instance == null) return;
            string value = Number(Percent / 10.0, "0.0");
            CustomUIWindow dialog = null;
            void Apply()
            {
                string text = value.Trim().TrimEnd('%', '％').Trim();
                if (!double.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out double parsed) || double.IsNaN(parsed) || double.IsInfinity(parsed)
                    || parsed < 0 || parsed > 100)
                {
                    dialog?.Get("error")?.SetText(L10n.Text("input_invalid"));
                    return;
                }
                double tenths = Math.Round(parsed * 10, MidpointRounding.AwayFromZero);
                if (Math.Abs(parsed * 10 - tenths) > 0.000001)
                {
                    dialog?.Get("error")?.SetText(L10n.Text("input_precision"));
                    return;
                }
                SetPercent((int)tenths);
                slider.SetValue(Percent / 10f);
                Status("percent_set", Number(Percent / 10.0, "0.0"));
                dialog?.Close();
            }
            dialog = CustomUIManager.Window(ModEntry.Id + ".percent", L10n.Text("input_title"), CustomUIManager.LAYER_PAPER)
                .UseMachineryStyle().SetSize(400, 230)
                .AddLabel(L10n.Text("input_hint"))
                .AddInput(L10n.Text("input_placeholder"), value, id: "percent", onTyping: text => value = text)
                .AddLabel("", "error")
                .AddButton(L10n.Text("apply"), Apply)
                .AddButton(L10n.Text("cancel"), () => dialog?.Close())
                .Show();
        }

        private void ChangePercent(int delta)
        {
            SetPercent(Percent + delta);
            slider.SetValue(Percent / 10f);
        }

        private void SelectLiquid(int index)
        {
            if (index < 0 || index >= componentChoices.Count) return;
            machine.ModifyTag(LiquidTag, s => s.Enable().SetString(componentChoices[index].identifier));
            Refresh();
        }

        private void RefreshComponents()
        {
            componentChoices.Clear();
            var labels = new List<string>();
            int selected = 0;
            foreach (var liquid in Liquid.Liquids)
            {
                if (liquid.identifier != SelectedId && Amount(target.childItem, liquid) <= 0 && Amount(source.childItem, liquid) <= 0)
                    continue;
                if (liquid.identifier == SelectedId) selected = componentChoices.Count;
                componentChoices.Add(liquid);
                labels.Add(L10n.LiquidName(liquid));
            }
            componentDropdown.SetOptions(labels, selected);
        }

        private static int Amount(GameItem item, Liquid liquid) => item == null ? 0 : item.GetTagReadonly(liquid.currentPart).GetInt();
        private static string Number(double value, string format = "0.###") => value.ToString(format, CultureInfo.InvariantCulture);
        private static void Text(MachineryText text, string value) { if (text.GetText() != value) text.SetText(value); }

        private static string BottleInfo(string label, GameItem bottle, Liquid liquid)
        {
            if (bottle == null) return L10n.Text("bottle_missing", label);
            int total = WaterHelper.GetTotalVolume(bottle);
            double fraction = total > 0 ? Amount(bottle, liquid) * 100.0 / total : 0;
            return L10n.Text("bottle_info", label, Number(total / 1000.0), Number(fraction, "0.000"));
        }

        private void Refresh()
        {
            int index = SelectedIndex;
            if (index < 0 || machine == null) return;
            foreach (var update in refreshLabels) update();
            RefreshComponents();
            machine.name = L10n.Text("item_name");
            machine.shortDescription = L10n.Text("item_description");
            machine.flavorText = L10n.Text("item_flavor");
            Text(status, L10n.Text(statusKey, statusArgs));
            Liquid liquid = Liquid.Liquids[index];
            Text(substance, L10n.Text("component_label", L10n.LiquidName(liquid)));
            Text(percentage, L10n.Text("percent_label", Number(Percent / 10.0, "0.0")));
            Text(targetInfo, BottleInfo(L10n.Text("target_label"), target.childItem, liquid));
            Text(sourceInfo, BottleInfo(L10n.Text("source_label"), source.childItem, liquid));
            if (TryPlan(out int amount, out double fraction, out string reason))
                Text(preview, amount == 0 ? L10n.Text("preview_noop") : L10n.Text("preview_transfer", Number(amount / 1000.0), Number(fraction * 100, "0.000")));
            else Text(preview, reason);
        }

        private bool TryPlan(out int amount, out double fraction, out string reason)
        {
            amount = 0; fraction = 0; reason = L10n.Text("missing_bottles");
            GameItem t = target.childItem, s = source.childItem;
            int index = SelectedIndex;
            if (t == null || index < 0) return false;
            if (!AcceptBottle(t) || (s != null && (!AcceptBottle(s) || ReferenceEquals(t, s))))
            { reason = L10n.Text("unsupported_bottle"); return false; }
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
            if (best > 0.00005) { reason = L10n.Text("precision_limit"); return false; }
            amount = bestAmount;
            return true;
        }

        private void Mix()
        {
            if (!GeneralHelper.IsItemOwned(machine)) { Status("buy_first"); return; }
            if (!TryPlan(out int amount, out _, out string reason)) { Status("operation_failed"); return; }
            if (amount == 0) { Status("noop_status"); return; }
            if (battery.childItem == null || !PowerHelper.CanDrawPowerSource(battery.childItem, EnergyPerMix))
            { Status("low_power"); return; }
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
            Status("mix_complete", Number(amount / 1000.0), Number(actual, "0.000"));
        }

        private static string FailureText(MixFailure failure)
        {
            switch (failure)
            {
                case MixFailure.EmptyTarget: return L10n.Text("empty_target");
                case MixFailure.EmptySource: return L10n.Text("empty_source");
                case MixFailure.Unreachable: return L10n.Text("unreachable");
                case MixFailure.InsufficientSource: return L10n.Text("insufficient_source");
                case MixFailure.InsufficientCapacity: return L10n.Text("insufficient_capacity");
                case MixFailure.PrecisionLimit: return L10n.Text("precision_limit");
                default: return L10n.Text("invalid_bottle");
            }
        }
    }
}
