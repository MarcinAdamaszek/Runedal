using System;
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
        public enum EffectType
        {
            Heal,
            Stun,
            Lifesteal
        }
        
        public double Value { get; set; }
        public double Duration { get; set; }
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
