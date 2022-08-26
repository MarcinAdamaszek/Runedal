using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Weapon : Item
    {
        private int _Attack;

        //default constructor for json deserializer
        public Weapon() : base() { }

        //constructor for placeholder items worn by Player when player wears no item
        public Weapon(string placeholder) : base(placeholder)
        {
            Attack = 0;
        }
        public Weapon(string[] descriptive, int[] stats, int attack) : base (descriptive, stats)
        {
            Attack = attack;
        }

        //copy constructor
        public Weapon(Weapon wp) : base(wp)
        {
            Attack = wp.Attack;
        }

        public int Attack
        {
            get { return _Attack; }
            set
            {
                if (_Attack != value)
                {
                    int difference = value - _Attack;

                    Modifiers!.Add(new Modifier(Characters.CombatCharacter.StatType.Attack, difference));

                    _Attack = value;
                }
            }
        }

    }
}
