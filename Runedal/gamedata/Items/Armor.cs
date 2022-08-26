using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Armor : Item
    {
        private int _Defense;

        //json constructor
        public Armor() : base() { }

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

        //copy constructor
        public Armor(Armor arm) : base(arm)
        {
            Defense = arm.Defense;
            Type = arm.Type;
        }

        //enum for type of armor item, indicating it's destined body part
        public enum ArmorType
        {
            Body,
            Pants,
            Helmet,
            Gloves,
            Shoes,
        }
        public int Defense
        {
            get { return _Defense; }
            set
            {
                if (_Defense != value)
                {
                    int difference = value - _Defense;

                    Modifiers!.Add(new Modifier(Characters.CombatCharacter.StatType.Defense, difference));

                    _Defense = value;
                }
            }
        }
        public ArmorType Type { get; set; }

    }
}
