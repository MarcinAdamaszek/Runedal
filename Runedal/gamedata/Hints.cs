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
            AttackHint = true;
            AttributesHint = true;
            CraftHint1 = true;
            CraftHint2 = true;
            SpellsHint = true;
            WearHint = true;
            TakeoffHint = true;
            UseHint = true;
            PauseHint = true;
            FleeHint = true;
            GameSpeedHint = true;
            EffectsHint = true;
            TalkHint = true;
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
            Stats,
            Attributes,
            Craft1,
            Craft2,
            Spells,
            Wear,
            Takeoff,
            Use,
            Pause,
            Flee,
            GameSpeed,
            Effects,
            Talk
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
        public bool AttributesHint { get; set; }
        public bool CraftHint1 {get; set; }
        public bool CraftHint2 { get; set; }
        public bool SpellsHint { get; set; }
        public bool WearHint { get; set; }
        public bool TakeoffHint { get; set; }
        public bool UseHint { get; set; }
        public bool PauseHint { get; set; }
        public bool FleeHint { get; set; }
        public bool GameSpeedHint { get; set; }
        public bool EffectsHint { get; set; }
        public bool TalkHint { get; set; }
    }
}
