using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class SpellCast : Action
    {
        public SpellCast() : base() { }
        public SpellCast(Spell spellToCast)
        {
            SpellToCast = spellToCast;
        }
        public Spell SpellToCast { get; set; }
    }
}
