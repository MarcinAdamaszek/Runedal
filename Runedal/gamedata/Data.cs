using Runedal.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Entities;

namespace Runedal.GameData
{
    public class Data
    {
        public Data() 
        {


            Locations = new List<Location>();
            Locations.Add(new Location(1, 1, "Karczma", "Drewniane ściany, stoliki i krzesła. Zapach piwa i smażonego bekonu. Na wprost widzisz widzisz kontuar i karczmarza" +
                "przecierającego szmatą brudną szklanicę", true, true, true, true));
            Locations.Add(new Location(1, 2, "Główna ulica", "Przed sobą widzisz parę zaniedbanych, drewnianych chat krytych strzechą. Obok stoi studnia," +
                "na brzegu której siedzi bury kot. Pod stopami czujesz ubitą ziemię, a w nozdrzach zapach końskich odchodów i ludzkich szczyn", true, true, true, true));

            Player = new Entity("Daever", "none", 20, 15, 35);
            Player.CurrentLocation = Locations[0];
        }

        public List<Location> Locations { get; set; }
        public Entity Player { get; set; }

    }
}
