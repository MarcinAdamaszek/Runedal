using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Items;
using Runedal.GameData.Characters;

namespace Runedal.GameData.Actions
{
    public class ItemUse : CharAction
    {
        public ItemUse() : base(new CombatCharacter("placeholder"), 30) 
        {
            ItemToUse = new Item("placeholder");
            if (ItemToUse.GetType() == typeof(Consumable))
            {
                ActionPointsCost = 30;
            }
        }
        public ItemUse(CombatCharacter user, Item itemToUse) : base(user, 30)
        {
            ItemToUse = itemToUse;
            if (ItemToUse.GetType() == typeof(Consumable))
            {
                ActionPointsCost = 30;
            }
        }

        public Item ItemToUse { get; set; }
    }
}
