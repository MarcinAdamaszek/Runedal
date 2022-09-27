using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Runedal.GameData.Characters.Monster;

namespace Runedal.GameData.Characters
{
    public class Hero : CombatCharacter
    {
        public Hero() : base() { }
        public Hero(string[] descriptive, int[] combatStats, string[][] responses, int gold)
            : base(descriptive, combatStats, responses, gold)
        {

        }
        
        //copy constructor
        public Hero(Hero hero) : base(hero)
        {
            Aggressiveness = hero.Aggressiveness;
        }
        public AggressionType Aggressiveness { get; set; }
    }
}
