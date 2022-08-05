using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Weapon : Item
    {
        //default constructor for json deserializer
        public Weapon() : base() { }

        //constructor for placeholder items worn by Player when player wears no item
        public Weapon(string placeholder) : base(placeholder)
        {
            Attack = 0;
        }
        public Weapon(string name, string description, int weight, int cost, int attack)
            : base (name, description, weight, cost)
        {
            Attack = attack;
        }

        public int Attack { get; private set; }
        
    }
}
