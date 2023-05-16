using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Armor : Item
    {
        public Armor() : base() { }

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

        public Armor(Armor arm) : base(arm)
        {
            Defense = arm.Defense;
            Type = arm.Type;
        }

        public enum ArmorType
        {
            Torso,
            Pants,
            Helmet,
            Gloves,
            Shoes,
        }
        public int Defense { get; set; }
        public ArmorType Type { get; set; }

    }
}
