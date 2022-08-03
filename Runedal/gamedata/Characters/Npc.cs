using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class Npc : Character
    {
        public Npc(string name, string description, int hp, int mp, int speed, int attack, int atkSpeed, int accuracy, int critical, int defense, int evasion, int magicResistance,
            int gold, string[] passiveResponses, string[] aggressiveResponses) : base(name, description, hp, mp, speed, attack, atkSpeed, accuracy, critical,
                defense, evasion, magicResistance, gold)
        {
            PassiveResponses = passiveResponses;
            AggressiveResponses = aggressiveResponses;
        }

        //arrays of npc's verbal responses. Passive for ordinary talk and aggressive for attack and/or being attacked
        public string[]? PassiveResponses { get; private set; }
        public string[]? AggressiveResponses { get; private set; }
    }
}
