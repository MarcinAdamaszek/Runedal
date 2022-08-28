using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Characters;

namespace Runedal.GameData
{
    public class AttackInstance
    {
        public AttackInstance(CombatCharacter attacker, CombatCharacter receiver)
        {
            Attacker = attacker;
            Receiver = receiver;
        }

        public CombatCharacter Attacker { get; set; }
        public CombatCharacter Receiver { get; set; }
    }
}
