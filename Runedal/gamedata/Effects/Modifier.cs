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

        public Modifier.ModType Type { get; set; }

        public bool IsPercentage { get; set; }

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

        //parent of a modifier, which is the name of a source that caused it
        public string Parent { get; set; }

        public void ResetDuration()
        {
            DurationInTicks = Duration * 10;
        }
    }
}
