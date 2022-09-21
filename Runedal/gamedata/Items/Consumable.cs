using Runedal.GameData.Effects;
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
            if (AdditionalEffect == null)
            {
                AdditionalEffect = new List<Modifier>();
            }
        } 
        public Consumable(string[] descriptive, int[] stats, string useActivityName) : base(descriptive, stats) 
        {
            UseActivityName = useActivityName;
            AdditionalEffect = new List<Modifier>();
        }

        public string? UseActivityName { get; set; }
        public List<Modifier>? AdditionalEffect { get; set; }
    }
}
