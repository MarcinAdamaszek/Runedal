using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class Hints
    {
        public Hints()
        {
            HintsOnOff = true;
            GoHint = true;
            AttackHint= true;
            PickupHint= true;
            TradeHint= true;
            LevelUpHint = true;
            InventoryHint= true;
            LookInventoryItemHint = true;
            BuySellHint = true;
            LookAdjacentLocHint = true;
            LookHint = true;
            StatsHint = true;
        }

        public enum HintType
        {
            Go,
            Attack,
            Pickup,
            Trade,
            LevelUp,
            Inventory,
            Look,
            LookInventoryItem,
            BuySell,
            LookAdjacentLoc,
            Stats
        }
        public bool HintsOnOff { get; set; }
        public bool GoHint { get; set; }
        public bool AttackHint { get; set; }
        public bool PickupHint { get; set; }
        public bool TradeHint { get; set; }
        public bool LevelUpHint { get; set; }
        public bool InventoryHint { get; set; }
        public bool LookHint { get; set; }
        public bool LookInventoryItemHint { get; set; }
        public bool BuySellHint { get; set; }
        public bool LookAdjacentLocHint { get; set; }
        public bool StatsHint { get; set; }

    }
}
