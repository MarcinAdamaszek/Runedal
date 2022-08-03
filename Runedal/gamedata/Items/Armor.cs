using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Armor : Item
    {
        public Armor(ArmorType type) : base()
        {
            Defense = 0;
            Type = type;
        }
        public Armor (string name, string description, int weight, int cost, int defense, ArmorType type) : base(name, description, weight, cost)
        {
            Defense = defense;
            Type = type;
        }

        //enum for type of armor item, indicating it's destined body part
        public enum ArmorType
        {
            FullBody,
            Helmet,
            Gloves,
            Shoes,
        }
        public int Defense { get; private set; }
        public ArmorType Type { get; private set; }

    }
}
