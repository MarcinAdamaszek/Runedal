using Runedal.GameData.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Runedal.GameData.Characters;
using Runedal.GameData.Items;

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
            Items = new List<Item>();
            PriceMultiplier = 1.2;
        }
        public double PriceMultiplier { get; set; }
        public string? FileName { get; set; }
        public string? JsonString { get; set; }
        public JsonSerializerOptions Options { get; set; }
        public List<Location>? Locations { get; set; }
        public List<Character>? Characters { get; set; }
        public List<Item>? Items { get; set; }
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

        //method loading items from json
        public void LoadItems()
        {
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Consumables.json");
            Item[] consumablesArray = JsonSerializer.Deserialize<Consumable[]>(JsonString, Options)!;

            PopulateItems(consumablesArray);


            //Fill characters inventories with items
            Characters!.ForEach(character => FillInventory(character));
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

        //helper method for pushing loaded items objects into Items list
        private void PopulateItems(Item[] itemsArray)
        {
            foreach (var item in itemsArray)
            {
                Items!.Add(item);
            }
        }

        //method for filling trader inventories with proper items
        private void FillInventory(Character character)
        {
            Item itemToAdd;

            if (character.GetType() == typeof(Trader))
            {
                foreach(KeyValuePair<string, int> kvp in (character as Trader)!.Items!)
                {
                    itemToAdd = Items!.Find(item => item.Name == kvp.Key)!;
                    character.AddItem(itemToAdd, kvp.Value);
                }
            }
        } 
    }
}
