// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.
using System.Linq;

namespace ProbablyStolenMixer
{
    internal static class DetectorAccess
    {
        public const string ScannerId = "chem_scanner";
        public static bool HasScanner() => PlayerStore.Instance != null && EmporiumEntry.Instance != null
            && EmporiumEntry.Instance.GetAllOwnedItems(false, true).Any(item => item != null && item.identifier == ScannerId
                && item.unitCount > 0 && GeneralHelper.IsItemEffectivelyOwned(item));
    }
}
