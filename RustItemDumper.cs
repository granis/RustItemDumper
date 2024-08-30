using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using System.Linq;
using Newtonsoft.Json;
using Rust;
using System.Diagnostics;
using Oxide.Game.Rust.Libraries;
using System.Net.Sockets;

namespace Oxide.Plugins
{
    [Info("RustItemDumper", "granis", "1.0.0")]

    public class RustItemDumper : RustPlugin
    {
        public class JsonWear
        {
            public required int OccupationOver { get; init; }
            public required int OccupationUnder { get; init; }
            public required bool IsBackpack { get; init; }
            public Dictionary<string, float>? ProtectionProperties { get; init; }
        }
        public class JsonItem
        {
            public required int ItemId { get; init; }
            public required string? ShortName { get; init; }
            public required string DisplayName { get; init; }
            public required string DisplayDescription { get; init; }
            public required string Category { get; init; }
            public required bool IsWearable { get; init; }
            public required JsonWear? Wearable { get; init; }
        };
        public List<JsonItem> ItemList = new List<JsonItem>();
        private void OnServerInitialized()
        {
            foreach (var item in ItemManager.itemList.OrderBy(x => x.itemid).ToList())
            {
                ItemModWearable itemModWearable = item.ItemModWearable;
                JsonWear? jsonwear = null;
                if (itemModWearable != null)
                {
                    jsonwear = new JsonWear
                    {
                        OccupationOver = (int)(object)itemModWearable.targetWearable.occupationOver,
                        OccupationUnder = (int)(object)itemModWearable.targetWearable.occupationUnder,
                        IsBackpack = itemModWearable.targetWearable.IsBackpack,
                        ProtectionProperties = itemModWearable.protectionProperties
                        ?
                        itemModWearable.protectionProperties.amounts.Select((x, i) => new KeyValuePair<string, float>(((DamageType)i).ToString(), x)).ToDictionary(t => t.Key, t => t.Value)
                        :
                        null,
                    };
                }
                var output = new JsonItem
                {
                    ItemId = item.itemid,
                    ShortName = item.shortname,
                    DisplayName = item.displayName.english,
                    DisplayDescription = item.displayDescription.english,
                    Category = item.category.ToString(),
                    IsWearable = item.isWearable,
                    Wearable = jsonwear,
                };
                ItemList.Add(output);
            }
            Interface.Oxide.DataFileSystem.WriteObject("Items", ItemList);
            Process.GetCurrentProcess().Kill();
        }
    }
}
