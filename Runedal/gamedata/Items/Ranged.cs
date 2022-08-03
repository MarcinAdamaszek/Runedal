using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Ranged : Weapon
    {
        public Ranged(string name, string description, int weight, int cost, int attack, int atkSpeed,
            int accuracy, int range) : base(name, description, weight, cost, attack, atkSpeed, accuracy)
        {
            Range = range;
        }
        public int Range { get;  private set; }
    }
}
