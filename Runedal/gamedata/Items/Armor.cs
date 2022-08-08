using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Armor : Item
    {
        //constructor for placeholder for empty Player's item slots
        public Armor(ArmorType type, string placeholder) : base(placeholder)
        {
            Defense = 0;
            Type = type;
        }
        public Armor(string[] descriptive, int[] stats, int defense, ArmorType type) : base(descriptive, stats)
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
        public int Defense { get; set; }
        public ArmorType Type { get; set; }

    }
}
