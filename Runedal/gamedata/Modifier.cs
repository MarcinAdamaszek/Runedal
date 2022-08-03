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
        public Modifier(Character.StatType type, int value)
        {
            Type = type;
            Value = value;
        }

        //enum for type of modifier (which statistic it modifies)
        
        public Character.StatType Type { get; private set; }

        //value of how much is statistic going to be modified
        public int Value { get; private set; }
    }
}
