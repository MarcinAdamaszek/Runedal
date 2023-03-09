using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Themes;
using Runedal.GameData.Characters;

namespace Runedal.GameData.Effects
{
    public class Modifier
    {
        protected int _Duration;

        //default constructor for json deserialization
        public Modifier()
        {
            Parent = "none";
        }
        public Modifier(Modifier.ModType type, int value, int duration = 0, string parent = "none", bool isPercentage = false)
        {
            Type = type;
            Value = value;
            Duration = duration;
            Parent = parent;
            IsPercentage = isPercentage;
        }

        //copy constructor 
        public Modifier(Modifier mod)
        {
            Type = mod.Type;
            Value = mod.Value;
            Duration = mod.Duration;
            Parent = mod.Parent;
            IsPercentage = mod.IsPercentage;
        }
        public enum ModType
        {
            MaxHp,
            MaxMp,
            HpRegen,
            MpRegen,
            Strength,
            Intelligence,
            Agility,
            Speed,
            Attack,
            AtkSpeed,
            Accuracy,
            Critical,
            Defense,
            Evasion,
            MagicResistance,
            Stun,
            Lifesteal,
            Invisibility,
            ManaShield,
            AdditionalDmg
        }

        //type of modifier 
        public Modifier.ModType Type { get; set; }

        //bool indicating if value is percentage
        public bool IsPercentage { get; set; }

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

        //parent of a modifier, meaning the name of an item/spell etc. that caused it
        public string Parent { get; set; }

        //method setting DurationInTicks to full (Duration * 10)
        public void ResetDuration()
        {
            DurationInTicks = Duration * 10;
        }
    }
}
