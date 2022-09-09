using Runedal.GameData.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Actions
{
    public class FleeAttempt : CharAction
    {
        public FleeAttempt(CombatCharacter pussy, Location escapeDestination, int actionPointsCost)
            : base(pussy, actionPointsCost)
        {
            EscapeDestination = escapeDestination;
        }
        public Location EscapeDestination { get; set; }
    }
}
