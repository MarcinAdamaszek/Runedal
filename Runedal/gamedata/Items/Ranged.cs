using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Ranged : Weapon
    {
        //default constructor for json deserializer
        public Ranged() : base() { }

        //constructor for placeholder items worn by Player when player wears no item
        public Ranged(string placeholder) : base(placeholder) { }
        public Ranged(string[] descriptive, int[] stats, int[] combatStats) : base(descriptive, stats, combatStats[0])
        {
            Range = combatStats[1];
        }
        public int Range { get;  private set; }
    }
}
