using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class Spell : Entity
    {
        public Spell() : base() 
        {
            Modifiers = new List<Modifier>();
        }
        public Spell(string placeholder) : base(placeholder)
        {
            Modifiers = new List<Modifier>();
        }
        public Spell(Spell sp) : base(sp)
        {
            DefaultTarget = sp.DefaultTarget;
            Damage = sp.Damage;
            ManaCost = sp.ManaCost;

            //create deep copy of modifiers collection
            Modifiers = sp.Modifiers!.ConvertAll(mod => new Modifier(mod));
        }
        public enum Target
        {
            Enemy,
            Self
        }
        public Target DefaultTarget { get; set; }
        public double Damage { get; set; }
        public double ManaCost { get; set; }
        public List<Modifier> Modifiers { get; set; }
    }
}
