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
            ModHelper.AddDebugFunction(Id, ModHelper.DEBUG_CATEGORY_SPAWN, "生成精密配比机", () =>
                ModHelper.MoveItemToInventory(ModHelper.SpawnItem(ItemId)));
            log.Log("精密配比机已加载，可在杰克逊博士处购买。");
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= Register;
            ModHook.OnPlaceInventorInventoryItemLate -= Stock;
            ModHelper.RemoveDebugFunctions(Id);
        }

        private static void Register(ModItemDirectory directory) => directory.Add(ItemId, MixerMachine.Create);
        private static void Stock(List<GameItem> items) => PlayerStore.Instance.AddDirectSellingItemToTable(ItemId);
    }
}
