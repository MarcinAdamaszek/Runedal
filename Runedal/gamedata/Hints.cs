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
        }

        public enum HintType
        {
            Go,
            Attack,
            Pickup,
            Trade,
            LevelUp,
            Inventory,
            LookInventoryItem
        }
        public bool HintsOnOff { get; set; }
        public bool GoHint { get; set; }
        public bool AttackHint { get; set; }
        public bool PickupHint { get; set; }
        public bool TradeHint { get; set; }
        public bool LevelUpHint { get; set; }
        public bool InventoryHint { get; set; }
        public bool LookInventoryItemHint { get; set; }

    }
}
