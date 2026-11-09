// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.

namespace ProbablyStolenMixer
{
    internal static class L10n
    {
        public static string Text(string key, params object[] args) => ModHelper.GetLocalized(ModEntry.Id, key, args);

        public static string LiquidName(Liquid liquid)
        {
            string key;
            switch (liquid.identifier)
            {
                case "microbe": key = "liquid_microbes"; break;
                case "fat": key = "liquid_fat_oil"; break;
                case "coffee_mush": key = "liquid_coffee_bean_mush"; break;
                case "contraceptive": key = "liquid_contraceptive_hormones"; break;
                case "methylol": key = "liquid_n_methylol_base"; break;
                case "debris": key = "liquid_insoluble_debris"; break;
                default: key = "liquid_" + liquid.identifier; break;
            }
            string translated = LocHelper.GetLocalizedMechanic(key);
            return string.IsNullOrEmpty(translated) || translated == key ? liquid.displayName : translated;
        }
    }
}
