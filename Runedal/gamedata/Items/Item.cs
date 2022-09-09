using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Effects;

namespace Runedal.GameData.Items
{
    public class Item : Entity
    {
        //default constructor for json deserialization
        public Item() : base()
        {
            Modifiers = new List<Modifier>();
            Quantity = 1;
        }

        //constructor for placeholder for empty Player's item slots
        public Item(string placeholder) : base(placeholder)
        {
            Quantity = 1;
            Weight = 0;
            Price = 0;
            Modifiers = new List<Modifier>();
        }
        public Item(string[] descriptive, int[] stats) : base(descriptive)
        {
            Quantity = 1;
            Weight = stats[0];
            Price = stats[1];
            Modifiers = new List<Modifier>();
        }

        //copy constructor
        public Item(Item it, int quantity = 1)
        {
            Quantity = quantity;
            Name = it.Name;
            Description = it.Description;
            Weight = it.Weight;
            Price = it.Price;
            RealWeight = it.Weight * quantity;

            //create deep copy of modifiers collection
            Modifiers = it.Modifiers!.ConvertAll(mod => new Modifier(mod));
        }

        public int Weight { get; set; }
        public int RealWeight { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }

        //list of modifiers which change player's statistics and/or attributes
        public List<Modifier>? Modifiers { get; set; }

        public void ChangeQuantity(int quantity)
        {
            Quantity += quantity;
            RealWeight += quantity * Weight;
        }

    }
}
