using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Melee : Weapon
    {
        //default constructor for json deserializer
        public Melee() : base() { }

        //constructor for placeholder items worn by Player when player wears no item
        public Melee(string placeholder) : base(placeholder) { }
        public Melee(string name, string description, int weight, int cost, int attack) : base(name, description, weight, cost, attack)
        {

        }

    }
}
