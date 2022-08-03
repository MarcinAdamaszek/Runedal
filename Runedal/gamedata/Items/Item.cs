using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public abstract class Item : Entity
    {
        public Item() : base()
        {
            Weight = 0;
            Cost = 0;
            Modifiers = new List<Modifier>();
        }
        public Item(string name, string description, int weight, int cost) : base(name, description)
        {
            Weight = weight;
            Cost = cost;
            Modifiers = new List<Modifier>();
        }

        public int Weight { get; private set; }
        public int Cost { get; private set; }

        //list of modifiers which change player's statistics and/or attributes
        public List<Modifier>? Modifiers { get; set; }

    }
}
