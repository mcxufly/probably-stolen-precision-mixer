// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ProbablyStolenMixer
{
    internal static class MixerStock
    {
        private const string Purchased = "jackson_mixer_purchased";
        private const string RollDay = "jackson_mixer_roll_day";
        private const string Available = "jackson_mixer_available";
        private const string Offered = "XUFLY_MIX_JACKSON_OFFER";

        public static void Stock(List<GameItem> ownedItems)
        {
            if (PlayerStore.Instance == null || EmporiumEntry.Instance == null) return;
            bool purchased = ModHelper.GetModDataBool(ModEntry.Id, Purchased);
            bool owns = ownedItems.Any(item => item.identifier == ModEntry.ItemId && GeneralHelper.IsItemEffectivelyOwned(item));
            if (purchased || owns) return;
            int day = StoreStation.GetDayCounter();
            int previous = ModHelper.GetModDataInt(ModEntry.Id, RollDay, -1);
            if (MixerStockPolicy.NeedsRoll(purchased, owns, day, previous))
            {
                ModHelper.SetModDataInt(ModEntry.Id, RollDay, day);
                ModHelper.SetModDataBool(ModEntry.Id, Available, MixerStockPolicy.DailyAvailable(PlayerStore.Instance.runID, day));
            }
            if (!MixerStockPolicy.ShouldOffer(purchased, owns, ModHelper.GetModDataBool(ModEntry.Id, Available))) return;
            if (EmporiumEntry.Instance.GetAllItems().Any(item => item.identifier == ModEntry.ItemId && !GeneralHelper.IsItemOwned(item))) return;
            GameItem machine = DirectoryMaster.Item(ModEntry.ItemId);
            machine.EnableTag(Offered);
            PlayerStore.Instance.AddDirectSellingItemToTable(machine);
        }

        internal static bool PurchaseCandidate(PlayerStore store, GameItem item) => item != null && item.IsTag(Offered)
            && MixerStockPolicy.IsPurchaseCandidate(item.identifier,
                store.currentClientInstance?.GetClientBlueprint()?.identifier, GeneralHelper.IsItemOwned(item));

        internal static void RecordPurchase(GameItem item)
        {
            if (item != null && GeneralHelper.IsItemEffectivelyOwned(item))
                ModHelper.SetModDataBool(ModEntry.Id, Purchased, true);
        }
    }

    // Direct and negotiated single purchases commit only after native payment returns.
    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.BuyItem))]
    internal static class MixerPurchasePatch
    {
        private static void Prefix(PlayerStore __instance, GameItem item, out bool __state) =>
            __state = MixerStock.PurchaseCandidate(__instance, item);
        private static void Postfix(GameItem item, bool __state)
        {
            if (__state) MixerStock.RecordPurchase(item);
        }
    }

    // Native bulk purchase bypasses OnItemBought, so preserve its candidates too.
    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.BuyAllItemFromTable))]
    internal static class MixerBulkPurchasePatch
    {
        private static void Prefix(PlayerStore __instance, out List<GameItem> __state)
        {
            __state = EmporiumEntry.Instance?.backInvinvElement.childItems
                .Where(item => MixerStock.PurchaseCandidate(__instance, item)).ToList() ?? new List<GameItem>();
        }
        private static void Postfix(List<GameItem> __state)
        {
            foreach (var item in __state) MixerStock.RecordPurchase(item);
        }
    }
}
