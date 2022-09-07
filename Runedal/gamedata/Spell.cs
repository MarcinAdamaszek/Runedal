using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Effects;

namespace Runedal.GameData
{
    public class Spell : Entity
    {
        public Spell() : base() 
        {
            Modifiers = new List<Modifier>();
            SpecialEffects = new List<SpecialEffect>();
        }
        public Spell(string placeholder) : base(placeholder)
        {
            Modifiers = new List<Modifier>();
            SpecialEffects = new List<SpecialEffect>();
        }
        public Spell(Spell sp) : base(sp)
        {
            DefaultTarget = sp.DefaultTarget;
            Power = sp.Power;
            ManaCost = sp.ManaCost;

            //create deep copy of all collections
            Modifiers = sp.Modifiers!.ConvertAll(mod => new Modifier(mod));
            SpecialEffects = sp.SpecialEffects!.ConvertAll(spec => new SpecialEffect(spec));
        }
        public enum Target
        {
            Enemy,
            Self
        }
        public Target DefaultTarget { get; set; }
        public double Power { get; set; }
        public double ManaCost { get; set; }
        public List<Modifier> Modifiers { get; set; }
        public List<SpecialEffect> SpecialEffects { get; set; }
    }
}
