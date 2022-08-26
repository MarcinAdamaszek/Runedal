﻿ using System;
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
        public Modifier() 
        {
            Parent = "none";    
        }
        public Modifier(CombatCharacter.StatType type, int value, int duration, string parent = "none")
        {
            Type = type;
            Value = value;
            Duration = duration;
            Parent = parent;
        }

        //copy constructor 
        public Modifier(Modifier mod)
        {
            Type = mod.Type;
            Value = mod.Value;
            Duration = mod.Duration;
            Parent = mod.Parent;
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
                    ResetDuration();
                }
            }
        }
        public int DurationInTicks { get; set; }
        public string Parent { get; set; }

        //method setting DurationInTicks to full (Duration * 10)
        public void ResetDuration()
        {
            DurationInTicks = Duration * 10;
        }
    }
}
