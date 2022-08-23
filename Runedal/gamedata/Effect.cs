using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class Effect
    {
        public Effect() { }
        public Effect(string description, int duration) 
        {
            Description = description;
            DurationInTicks = duration;
        }
        public string? Description { get; set; }
        public int DurationInTicks { get; set; }
    }
}
