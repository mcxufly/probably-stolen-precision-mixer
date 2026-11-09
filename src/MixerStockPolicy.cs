// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.
namespace ProbablyStolenMixer
{
    public static class MixerStockPolicy
    {
        public const int ChancePercent = 25;
        public static bool DailyAvailable(string runId, int day)
        {
            // Stable across process restarts and reloads before the first shop visit.
            uint hash = 2166136261;
            unchecked
            {
                foreach (char c in (runId ?? "") + ":precision-mixer:" + day.ToString(System.Globalization.CultureInfo.InvariantCulture))
                { hash ^= c; hash *= 16777619; }
                // Avalanche sequential day values rather than exposing a simple pattern.
                hash ^= hash >> 16; hash *= 0x7feb352d;
                hash ^= hash >> 15; hash *= 0x846ca68b;
                hash ^= hash >> 16;
            }
            return hash % 100 < ChancePercent;
        }
        public static bool NeedsRoll(bool purchased, bool ownsMachine, int day, int previousDay) =>
            !purchased && !ownsMachine && day != previousDay;
        public static bool ShouldOffer(bool purchased, bool ownsMachine, bool dailyAvailable) =>
            !purchased && !ownsMachine && dailyAvailable;
        public static bool IsPurchaseCandidate(string itemId, string sellerId, bool previouslyOwned) =>
            itemId == "xufly_precision_mixer.machine" && sellerId == "inventor" && !previouslyOwned;
    }
}
