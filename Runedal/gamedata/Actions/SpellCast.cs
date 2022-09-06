using Runedal.GameData.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Actions
{
    public class SpellCast : CharAction
    {
        public SpellCast() : base(new CombatCharacter("placeholder"), 50) { }
        public SpellCast(CombatCharacter caster, CombatCharacter target, Spell spellToCast) : base(caster, 50)
        {
            Target = target;
            SpellToCast = spellToCast;
        }
        public Spell? SpellToCast { get; set; }
        public CombatCharacter? Target { get; set; }
    }
}
