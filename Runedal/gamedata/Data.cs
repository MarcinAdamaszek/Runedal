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
            //make json deserializer ignore letter cases in property names
            Options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            
            Locations = new List<Location>();
            Characters = new List<Character>();
           
        }
        public string? FileName { get; set; }
        public string? JsonString { get; set; }
        public JsonSerializerOptions Options { get; set; }
        public List<Location>? Locations { get; set; }
        public List<Character>? Characters { get; set; }
        public Player? Player { get; set; }
      
        
        //method reading json file into json string
        private string JsonToString(string filePath)
        {
            string jsonString = string.Empty;
            FileName = filePath;
            jsonString = File.ReadAllText(FileName);
            return jsonString;
        }

        //method loading characters from json file
        public void LoadCharacters()
        {

            //load player from json file
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Player.json");
            Player[] playerArray = JsonSerializer.Deserialize<Player[]>(JsonString, Options)!;

            //put player in his starting location
            PopulateLocations(playerArray);
            Player = playerArray[0];
            Characters!.Add(Player);

            //load traders from json
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Traders.json");
            Character[] tradersArray = JsonSerializer.Deserialize<Trader[]>(JsonString, Options)!;
            PopulateLocations(tradersArray);

            //load monsters
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\monsters.json");
            Character[] monstersArray = JsonSerializer.Deserialize<Monster[]>(JsonString, Options)!;
            PopulateLocations(monstersArray);
        } 

        //method loading locations from json file
        public void LoadLocations()
        {
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\locations.json");
            Location[] locationsArray = JsonSerializer.Deserialize<Location[]>(JsonString, Options)!;

            foreach (var loc in locationsArray)
            {
                Locations!.Add(loc);
            }
        }

        //helper method for pushing loaded characters objects into Characters list and assigning them into their starting location
        private void PopulateLocations(Character[] charactersArray)
        {
            Location startingLocation;

            foreach (var character in charactersArray)
            {
                Characters!.Add(character);

                //Assign trader to it's starting location
                startingLocation = Locations!.Find(loc => loc.Name!.ToLower() == character.Start!.ToLower())!;
                startingLocation.AddCharacter(character);
                character.CurrentLocation = startingLocation;
            }
        }
    }
}
