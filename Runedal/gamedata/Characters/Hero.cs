using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class Hero : CombatCharacter
    {
        public Hero() : base() { }
        public Hero(string[] descriptive, int[] combatStats, string[][] responses, int gold)
            : base(descriptive, combatStats, responses, gold)
        {

        }
    }
}
