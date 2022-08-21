using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Characters;

namespace Runedal.GameData
{
    public class Modifier
    {
        protected int _Duration;

        //default constructor for json deserialization
        public Modifier() { }
        public Modifier(CombatCharacter.StatType type, int value, int duration)
        {
            Type = type;
            Value = value;
            Duration = duration;
        }

        //copy constructor 
        public Modifier(Modifier mod)
        {
            Type = mod.Type;
            Value = mod.Value;
            Duration = mod.Duration;
        }

        //enum for type of modifier (which statistic it modifies)
        public CombatCharacter.StatType Type { get; set; }

        //value of how much is statistic going to be modified
        public int Value { get; set; }
        public int Duration
        {
            get { return _Duration; }
            set
            {
                if (_Duration != value)
                {
                    _Duration = value;
                    DurationInTicks = _Duration * 10;
                }
            }
        }
        public int DurationInTicks { get; set; }
    }
}
