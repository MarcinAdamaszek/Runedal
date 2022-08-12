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
        //default constructor for json deserialization
        public Modifier() { }
        public Modifier(CombatCharacter.StatType type, int value)
        {
            Type = type;
            Value = value;
        }

        //copy constructor 
        public Modifier(Modifier mod)
        {
            Type = mod.Type;
            Value = mod.Value;
        }

        //enum for type of modifier (which statistic it modifies)
        
        public CombatCharacter.StatType Type { get; private set; }

        //value of how much is statistic going to be modified
        public int Value { get; private set; }
    }
}
