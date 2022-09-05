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
using System.Text.Json.Serialization;

namespace Runedal.GameData
{
    public class Data
    {
        public Data() 
        {
            //make json deserializer ignore letter cases in property names
            Options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            
            Locations = new List<Location>();
            Characters = new List<Character>();
            Monsters = new List<Monster>();
            Items = new List<Item>();
            Spells = new List<Spell>();
            PriceMultiplier = 1.2;
        }
        public double PriceMultiplier { get; set; }
        public string? FileName { get; set; }
        public string? JsonString { get; set; }
        public string[]? StackingEffects { get; set; }
        public JsonSerializerOptions Options { get; set; }
        public List<Location>? Locations { get; set; }
        public List<Character>? Characters { get; set; }
        public List<Monster>? Monsters { get; set; }
        public List<Item>? Items { get; set; }
        public List<Spell>? Spells { get; set; }
        public Player? Player { get; set; }
      
        
        //method reading json file into json string
        private string JsonToString(string filePath)
        {
            string jsonString = string.Empty;
            FileName = filePath;
            jsonString = File.ReadAllText(FileName);
            return jsonString;
        }

        //method loading StackingEffects array of string representing object names which effects stack on player
        private void LoadStackingEffects()
        {
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\StackingEffects.json");
            StackingEffects = JsonSerializer.Deserialize<string[]>(JsonString, Options)!;
        }

        //method loading characters from json file
        public void LoadCharacters()
        {

            //load player from json file
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Player.json");
            Player[] playerArray = JsonSerializer.Deserialize<Player[]>(JsonString, Options)!;

            
            PopulateCharactersList(playerArray);
            Player = playerArray[0];

            //put player in his starting location
            Location startingLocation = Locations!.Find(loc => loc.Name!.ToLower() == Player.Start.ToLower())!;
            Player.CurrentLocation = startingLocation;
            startingLocation.AddCharacter(Player);

            //load traders from json
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Traders.json");
            Character[] tradersArray = JsonSerializer.Deserialize<Trader[]>(JsonString, Options)!;
            PopulateCharactersList(tradersArray);

            //load monsters
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\monsters.json");
            Character[] monstersArray = JsonSerializer.Deserialize<Monster[]>(JsonString, Options)!;
            PopulateCharactersList(monstersArray);

            
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
            //load consumables
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Consumables.json");
            Item[] consumablesArray = JsonSerializer.Deserialize<Consumable[]>(JsonString, Options)!;
            PopulateItems(consumablesArray);

            //load weapons
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Weapons.json");
            Item[] weaponsArray = JsonSerializer.Deserialize<Weapon[]>(JsonString, Options)!;
            PopulateItems(weaponsArray);

            //load armors
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Armors.json");
            Item[] armorsArray = JsonSerializer.Deserialize<Armor[]>(JsonString, Options)!;
            PopulateItems(armorsArray);

            //load runes
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Runes.json");
            Item[] runesArray = JsonSerializer.Deserialize<RuneStone[]>(JsonString, Options)!;
            PopulateItems(runesArray);

            ////load ranged
            //JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Ranged.json");
            //Item[] rangedArray = JsonSerializer.Deserialize<Ranged[]>(JsonString, Options)!;
            //PopulateItems(rangedArray);
        }

        //method loading spells from json
        public void LoadSpells()
        {
            //load consumables
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Spells.json");
            Spell[] spellsArray = JsonSerializer.Deserialize<Spell[]>(JsonString, Options)!;

            foreach (var spell in spellsArray)
            {
                Spells!.Add(spell);
            }
        }

        //method filling locations with characters, characters with items, initializing hps etc
        public void InitializeEverything()
        {
            //fill every combat-character's hp/mp pools accordingly to their effective max hp/mp
            Characters!.ForEach(character =>
            {
                if (character is CombatCharacter)
                {
                    (character as CombatCharacter)!.InitializeHpMp();
                }
            });

            //fill every location with it's starting characters
            //and it's characters inventories with starting items
            Locations!.ForEach(location =>
            {
                PopulateLocation(location);
                location.Characters!.ForEach(character => FillInventory(character));
            });

            //load stacking effects
            LoadStackingEffects();
        }

        //helper method for pushing loaded characters objects into Characters list and assigning them into their starting location
        private void PopulateCharactersList(Character[] charactersArray)
        {
            //Location startingLocation;

            foreach (var character in charactersArray)
            {
                Characters!.Add(character);
            }
        }

        private void PopulateLocation(Location location)
        {
            Character character = new Character();
            int i;

            foreach (KeyValuePair<string, int> kvp in location.CharsToAdd!)
            {
                character = Characters!.Find(character => character.Name!.ToLower() == kvp.Key.ToLower())!;

                if (character.GetType() == typeof(Monster))
                {
                    //monster = new Monster((character as Monster)!);

                    for (i = 0; i < kvp.Value; i++)
                    {
                        location.AddCharacter(new Monster((character as Monster)!));
                    }
                }
                else if (character.GetType() == typeof(Trader))
                {
                    //trader = new Trader((character as Trader)!);

                    for (i = 0; i < kvp.Value; i++)
                    {
                        location.AddCharacter(new Trader((character as Trader)!));
                    }
                }
                else if (character.GetType() == typeof(Hero))
                {
                    //hero = new Hero((character as Hero)!);

                    for (i = 0; i < kvp.Value; i++)
                    {
                        location.AddCharacter(new Hero((character as Hero)!));
                    }
                }
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

            foreach(KeyValuePair<string, int> kvp in character.Items!)
            {
                itemToAdd = Items!.Find(item => item.Name == kvp.Key)!;
                character.AddItem(itemToAdd, kvp.Value);
            }
        } 
    }
}
