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
        public SpellCraft(CombatCharacter crafter, Spell spellToCraft) : base(crafter, 100)
        {
            SpellToCraft = spellToCraft;
        }

        public Spell SpellToCraft { get; set; }
    }
}
