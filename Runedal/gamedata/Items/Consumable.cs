using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Items
{
    public class Consumable : Item
    {
        public Consumable() : base() 
        {

        } 
        public Consumable(string[] descriptive, int[] stats, string effect) : base(descriptive, stats) 
        {
            Effect = effect;
        }

        public string? Effect { get; set; }
    }
}
