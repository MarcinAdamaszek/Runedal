using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Effects;

namespace Runedal.GameData.Items
{
    public class Weapon : Item
    {
        public Weapon() : base()  { }

        public Weapon(string placeholder) : base(placeholder)
        {
            Attack = 0;
        }
        public Weapon(string[] descriptive, int[] stats, int attack) : base (descriptive, stats)
        {
            Attack = attack;
        }

        public Weapon(Weapon wp) : base(wp)
        {
            Attack = wp.Attack;
        }
        public enum WeaponType
        {
            Dagger,
            Blade,
            Blunt
        }

        public WeaponType Type { get; set; }
        public int Attack { get; set; }
    }
}
