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
using Runedal.GameData.Effects;
using static Runedal.GameData.Items.Weapon;
using System.Xml.Linq;

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
        public void LoadStackingEffects()
        {
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\StackingEffects.json");
            StackingEffects = JsonSerializer.Deserialize<string[]>(JsonString, Options)!;
        }

        //method loading player
        public void LoadPlayer(string playerName)
        {
            //load player from json file
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Player.json");
            Player[] playerArray = JsonSerializer.Deserialize<Player[]>(JsonString, Options)!;


            AddCharactersToList(playerArray);
            Player = playerArray[0];

            //give name to player
            Player.Name = playerName;

            //put player in his starting location
            Location startingLocation = Locations!.Find(loc => loc.Name!.ToLower() == Player!.Start!.ToLower())!;
            Player!.CurrentLocation = startingLocation;
            startingLocation.AddCharacter(Player);
        }

        //method loading characters from json file
        public void LoadCharacters()
        {

            //load traders from json
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Traders.json");
            Character[] tradersArray = JsonSerializer.Deserialize<Trader[]>(JsonString, Options)!;
            AddCharactersToList(tradersArray);

            //load monsters
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\monsters.json");
            Character[] monstersArray = JsonSerializer.Deserialize<Monster[]>(JsonString, Options)!;
            AddCharactersToList(monstersArray);

            //load heroes
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Heroes.json");
            Character[] heroesArray = JsonSerializer.Deserialize<Hero[]>(JsonString, Options)!;
            AddCharactersToList(heroesArray);

        } 

        //method loading locations from json file
        public void LoadLocations()
        {
            string[] locationFilepaths = Directory.GetFiles(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Locations\");

            foreach (string path in locationFilepaths)
            {
                JsonString = JsonToString(path);
                Location[] locationsArray = JsonSerializer.Deserialize<Location[]>(JsonString, Options)!;


                foreach (var loc in locationsArray)
                {
                    Locations!.Add(loc);
                }
            }
        }

        //method loading items from json
        public void LoadItems()
        {
            //load consumables
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Consumables.json");
            Item[] consumablesArray = JsonSerializer.Deserialize<Consumable[]>(JsonString, Options)!;
            AddItemsToList(consumablesArray);

            //load weapons
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Weapons.json");
            Item[] weaponsArray = JsonSerializer.Deserialize<Weapon[]>(JsonString, Options)!;
            AddItemsToList(weaponsArray);

            //load armors
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Armors.json");
            Item[] armorsArray = JsonSerializer.Deserialize<Armor[]>(JsonString, Options)!;
            AddItemsToList(armorsArray);

            //load runes
            JsonString = JsonToString(@"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Json\Runes.json");
            Item[] runesArray = JsonSerializer.Deserialize<RuneStone[]>(JsonString, Options)!;
            AddItemsToList(runesArray);

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

        //method initializing hp/mp values
        public void InitializeHpMpValues()
        {
            //fill every combat-character's hp/mp pools accordingly to their effective max hp/mp
            Characters!.ForEach(character =>
            {
                if (character is CombatCharacter)
                {
                    (character as CombatCharacter)!.InitializeHpMp();
                }
            });
        }

        public void PopulateLocationsAndCharacters()
        {
            AddSpellsToCharacters();

            Character character = new Character();
            int i;

            Locations!.ForEach(location =>
            {

                foreach (KeyValuePair<string, int> kvp in location.CharsToAdd!)
                {
                    character = Characters!.Find(character => character.Name!.ToLower() == kvp.Key.ToLower())!;

                    if (character.GetType() == typeof(Monster))
                    {
                        for (i = 0; i < kvp.Value; i++)
                        {
                            location.AddCharacter(new Monster((character as Monster)!));
                        }
                    }
                    else if (character.GetType() == typeof(Trader))
                    {
                        for (i = 0; i < kvp.Value; i++)
                        {
                            location.AddCharacter(new Trader((character as Trader)!));
                        }
                    }
                    else if (character.GetType() == typeof(Hero))
                    {
                        for (i = 0; i < kvp.Value; i++)
                        {
                            location.AddCharacter(new Hero((character as Hero)!));
                        }
                    }
                }

                //fill character's inventories
                location.Characters!.ForEach(character => FillInventory(character));

            });
        }

        






        //==========================================HELPER METHODS========================================

        //helper method for pushing loaded characters objects into Characters list
        public void AddCharactersToList(Character[] charactersArray)
        {
            //Location startingLocation;

            foreach (var character in charactersArray)
            {
                Characters!.Add(character);
            }
        }

        //helper method for pushing loaded items objects into Items list
        private void AddItemsToList(Item[] itemsArray)
        {
            foreach (var item in itemsArray)
            {
                //if it's weapon, add modifiers depending on weapon type
                if (item.GetType() == typeof(Weapon))
                {
                    Weapon weapon = (Weapon)item;
                    if (weapon.Type == WeaponType.Blade)
                    {
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -25, 0, weapon.Name!, true));
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Critical, -30, 0, weapon.Name!, true));
                    }
                    else if (weapon.Type == WeaponType.Blunt)
                    {
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -50, 0, weapon.Name!, true));
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Critical, -70, 0, weapon.Name!, true));
                    }
                }

                //if it's armor, add modifiers depending on it's weight
                if (item.GetType() == typeof(Armor))
                {
                    Armor armor = (Armor)item;

                    if (armor.Type == Armor.ArmorType.Torso)
                    {
                        if (armor.Weight >= 500)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -20, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -7, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -20, 0, armor.Name!, true));
                        }
                        else if (armor.Weight < 500 && armor.Weight >= 200)
                        {
                            //armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -20, 0, armor.Name!, true));
                            //armor.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -7, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -20, 0, armor.Name!, true));
                        }
                    }
                    if (armor.Type == Armor.ArmorType.Pants)
                    {
                        if (armor.Weight >= 400)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -15, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -5, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -15, 0, armor.Name!, true));
                        }
                        else if (armor.Weight < 400 && armor.Weight >= 150)
                        {
                            //armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -20, 0, armor.Name!, true));
                            //armor.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -7, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -15, 0, armor.Name!, true));
                        }
                    }
                    if (armor.Type == Armor.ArmorType.Helmet || armor.Type == Armor.ArmorType.Gloves || armor.Type == Armor.ArmorType.Gloves)
                    {
                        if (armor.Weight >= 250)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -10, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -3, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -10, 0, armor.Name!, true));
                        }
                        else if (armor.Weight < 250 && armor.Weight >= 100)
                        {
                            //armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -20, 0, armor.Name!, true));
                            //armor.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -7, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -10, 0, armor.Name!, true));
                        }
                    }
                }

                Items!.Add(item);
            }
        }

        //method filling npc combat-characters with spells
        private void AddSpellsToCharacters()
        {
            int i;
            int spellIndex;

            Characters!.ForEach(character =>
            {
                if (character.GetType() == typeof(Monster) || character.GetType() == typeof(Hero))
                {
                    CombatCharacter combatNpc = (CombatCharacter)character;

                    if (combatNpc.StartingSpells.Length > 0)
                    {
                        for (i = 0; i < combatNpc.StartingSpells.Length; i++)
                        {
                            spellIndex = Spells!.FindIndex(sp => sp.Name!.ToLower() == combatNpc.StartingSpells[i].ToLower());

                            if (spellIndex != -1)
                            {
                                combatNpc.RememberedSpells.Add(new Spell(Spells![spellIndex]));
                            }
                        }
                    }
                }
            });
        }

        //method for filling trader inventories with proper items
        private void FillInventory(Character character)
        {
            Item itemToAdd;

            foreach (KeyValuePair<string, int> kvp in character.Items!)
            {
                itemToAdd = Items!.Find(item => item.Name!.ToLower() == kvp.Key.ToLower())!;
                character.AddItem(itemToAdd, kvp.Value);
            }
        }
    }
}
