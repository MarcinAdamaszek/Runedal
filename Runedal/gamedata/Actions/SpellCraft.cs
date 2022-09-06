using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Characters;

namespace Runedal.GameData.Actions
{
    public class SpellCraft : CharAction
    {
        public SpellCraft(Player crafter, Spell craftedSpell) : base(crafter, 100)
        {
            CraftedSpell = craftedSpell;
        }

        public Spell CraftedSpell { get; set; }
    }
}
