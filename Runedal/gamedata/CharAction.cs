using Runedal.GameData.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class CharAction
    {
        public CharAction() 
        {
            Performer = new CombatCharacter("placeholder");
        }
        public CharAction(CombatCharacter performer, double actionPointsCost)
        {
            ActionPointsCost = actionPointsCost;
            Performer = performer;
        }
        public double ActionPointsCost { get; set; }
        public CombatCharacter? Performer { get; set; }
    }
}
