using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class EffectOnPlayer
    {
        public EffectOnPlayer() { }
        public EffectOnPlayer(string name, string description, int duration) 
        {
            Name = name;
            Description = description;
            DurationInTicks = duration;
        }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int DurationInTicks { get; set; }
    }
}
