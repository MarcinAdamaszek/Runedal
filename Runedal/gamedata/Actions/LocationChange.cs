using Runedal.GameData.Characters;
using Runedal.GameData.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Actions
{
    public class LocationChange : CharAction
    {
        public LocationChange(CombatCharacter player, Location nextLocation, 
            string directionString, int actionPointsCost) : base(player, actionPointsCost)
        {
            DirectionString = directionString;
            this.nextLocation = nextLocation;
        }

        public string DirectionString { get; set; }
        public Location nextLocation { get; set; }
    }
}
