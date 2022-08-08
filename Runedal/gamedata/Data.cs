using Runedal.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Runedal.GameData.Characters;

namespace Runedal.GameData
{
    public class Data
    {
        public Data() 
        {
            //make json deserializer ignore letter cames in property names
            Options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            
            Locations = new List<Location>();
           
        }
        public string? FileName { get; set; }
        public string? JsonString { get; set; }
        public JsonSerializerOptions Options { get; set; }
        public List<Location>? Locations { get; set; }
        public Player? Player { get; set; }
      
        


        //method loading player from json file
        public void LoadCharacters()
        {
            FileName = @"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Player.json";
            JsonString = File.ReadAllText(FileName);
            Player = JsonSerializer.Deserialize<Player>(JsonString, Options)!;

            //put player in his starting location
            Locations!.Find(loc => loc.Name == Player.Start)!.Characters!.Add(Player);
        } 

        //method loading locations from json file
        public void LoadLocations()
        {
            FileName = @"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\locations.json";
            JsonString = File.ReadAllText(FileName);
            Location[] locationsArray = JsonSerializer.Deserialize<Location[]>(JsonString, Options)!;

            foreach (var loc in locationsArray)
            {
                Locations!.Add(loc);
            }
        }
    }
}
