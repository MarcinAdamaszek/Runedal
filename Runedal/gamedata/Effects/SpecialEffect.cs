﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Effects
{
    public class SpecialEffect
    {
        private EffectType _Type;
        public SpecialEffect() { }
        public SpecialEffect(EffectType type, int duration, int value = 0)
        {
            Type = type;
            Duration = duration;
            Value = value;
        }
        public SpecialEffect(SpecialEffect sp) 
        {
            Type = sp.Type;
            Value = sp.Value;
            Duration = sp.Duration;
        }
        public enum EffectType
        {
            HealPercent,
            Heal,
            Stun,
            Lifesteal,
            Teleport,
            Invisibility,
            ManaShield,
            AdditionalDmg
        }
        
        public int Value { get; set; }
        public int Duration { get; set; }
        public EffectType Type
        {
            get { return _Type; }
            set
            {
                _Type = value;

                if (Type == EffectType.Heal)
                {
                    Duration = 0;
                }
            }
        }
    }
}
