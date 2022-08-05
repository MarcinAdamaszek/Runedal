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
        public Ranged(string name, string description, int weight, int cost, int attack, int range) : base(name, description, weight, cost, attack)
        {
            Range = range;
        }
        public int Range { get;  private set; }
    }
}
