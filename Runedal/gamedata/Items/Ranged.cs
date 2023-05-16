using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Ranged : Weapon
    {
        public Ranged() : base() { }

        public Ranged(string placeholder) : base(placeholder) { }
        public Ranged(string[] descriptive, int[] stats, int[] combatStats) : base(descriptive, stats, combatStats[0])
        {
            Range = combatStats[1];
        }
        public int Range { get; set; }
    }
}
