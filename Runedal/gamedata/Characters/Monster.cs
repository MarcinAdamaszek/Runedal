using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class Monster : CombatCharacter
    {
        public Monster() : base()
        {

        }
        public Monster(string[] descriptive, int[] combatStats, string[][] responses, int gold, AggressionType aggressiveness)
            : base(descriptive, combatStats, responses, gold)
        {
            Aggressiveness = aggressiveness;
        }

        public Monster(Monster mon) : base(mon)
        {
            Aggressiveness = mon.Aggressiveness;
        }
        public enum AggressionType
        {
            Aggressive,
            Passive,
            Social
        }

        public AggressionType Aggressiveness { get; set; }
        
    }
}
