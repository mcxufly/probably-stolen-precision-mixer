// Copyright (C) 2026 XuFly
// SPDX-License-Identifier: GPL-3.0-only
// See LICENSE and NOTICE for license terms and warranty disclaimer.

using System.Collections.Generic;

namespace ProbablyStolenMixer
{
    public sealed class ModEntry : IMod
    {
        public const string Id = "xufly_precision_mixer";
        public const string ItemId = "xufly_precision_mixer.machine";
        private ModLog log;

        public void Init(ModManifest manifest) => log = new ModLog(manifest);

        public void OnEnable()
        {
            ModHook.OnModItemDirectoryInit += Register;
            ModHook.OnPlaceInventorInventoryItemLate += Stock;
            ModHook.OnModAssetsLoaded += AssetsLoaded;
            ModHook.OnGameLoadedNormal += RegisterDebugButton;
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= Register;
            ModHook.OnPlaceInventorInventoryItemLate -= Stock;
            ModHook.OnModAssetsLoaded -= AssetsLoaded;
            ModHook.OnGameLoadedNormal -= RegisterDebugButton;
            ModHelper.RemoveDebugFunctions(Id);
        }

        private void AssetsLoaded(LoadedMod mod)
        {
            if (mod.Manifest.ID != Id) return;
            RegisterDebugButton();
            log.Log(L10n.Text("loaded_message"));
        }

        private static void RegisterDebugButton()
        {
            ModHelper.RemoveDebugFunctions(Id);
            ModHelper.AddDebugFunction(Id, ModHelper.DEBUG_CATEGORY_SPAWN, L10n.Text("spawn_machine"), () =>
                ModHelper.MoveItemToInventory(ModHelper.SpawnItem(ItemId)));
        }

        private static void Register(ModItemDirectory directory) => directory.Add(ItemId, MixerMachine.Create);
        private static void Stock(List<GameItem> items) => MixerStock.Stock(items);
    }
}
