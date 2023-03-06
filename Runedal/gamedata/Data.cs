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
using System.Runtime.CompilerServices;
using System.Threading;
using Runedal.GameEngine;

namespace Runedal.GameData
{
    public class Data
    {
        public const string JsonDirectoryPath = @"C:\Users\adamach\source\repos\Runedal\Runedal\GameData\Resources\Json\";
        public Data()
        {
            //make json deserializer ignore letter cases in property names
            Options = new JsonSerializerOptions()
            {
                WriteIndented = true,
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
            TakenIds = new List<ulong>();
            AllCharactersTestProp = new List<Character>();
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
        public List<ulong>? TakenIds { get; set; }
        public Player? Player { get; set; }
        public List<Character> AllCharactersTestProp {get; set;}
      
        
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
            JsonString = JsonToString(JsonDirectoryPath + @"StackingEffects.json");
            StackingEffects = JsonSerializer.Deserialize<string[]>(JsonString, Options)!;
        }

        //method loading player
        public void LoadPlayer(string playerName)
        {
            //load player from json file
            JsonString = JsonToString(JsonDirectoryPath + @"Characters\Player.json");
            Player[] playerArray = JsonSerializer.Deserialize<Player[]>(JsonString, Options)!;

            Player = null;

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
            JsonString = JsonToString(JsonDirectoryPath + @"Characters\Traders.json");
            Character[] tradersArray = JsonSerializer.Deserialize<Trader[]>(JsonString, Options)!;
            AddCharactersToList(tradersArray);

            //load monsters
            JsonString = JsonToString(JsonDirectoryPath + @"Characters\Monsters.json");
            Character[] monstersArray = JsonSerializer.Deserialize<Monster[]>(JsonString, Options)!;
            AddCharactersToList(monstersArray);

            //load heroes
            JsonString = JsonToString(JsonDirectoryPath + @"Characters\Heroes.json");
            Character[] heroesArray = JsonSerializer.Deserialize<Hero[]>(JsonString, Options)!;
            AddCharactersToList(heroesArray);

            AddSpellsToCharacters();
        } 

        //method loading locations from json file
        public void LoadLocations()
        {
            Location[] locationsArray;
            string[] locationFilepaths;
            string[] locationDirectoriesPaths = Directory.GetDirectories(JsonDirectoryPath + @"Locations\");

            foreach (string dirPath in locationDirectoriesPaths)
            {
                locationFilepaths = Directory.GetFiles(dirPath);

                foreach (string path in locationFilepaths)
                {
                    JsonString = JsonToString(path);
                    locationsArray = JsonSerializer.Deserialize<Location[]>(JsonString, Options)!;

                    foreach (var loc in locationsArray)
                    {
                        Locations!.Add(loc);
                    }
                }
            }  
        }

        //method loading items from json
        public void LoadItems()
        {
            //load consumables
            JsonString = JsonToString(JsonDirectoryPath + @"Consumables.json");
            Item[] consumablesArray = JsonSerializer.Deserialize<Consumable[]>(JsonString, Options)!;
            AddItemsToList(consumablesArray);

            //load weapons
            JsonString = JsonToString(JsonDirectoryPath + @"Weapons\WeaponsDaggers.json");
            Item[] daggersArray = JsonSerializer.Deserialize<Weapon[]>(JsonString, Options)!;
            AddItemsToList(daggersArray);
            JsonString = JsonToString(JsonDirectoryPath + @"Weapons\WeaponsBlades.json");
            Item[] bladesArray = JsonSerializer.Deserialize<Weapon[]>(JsonString, Options)!;
            AddItemsToList(bladesArray);
            JsonString = JsonToString(JsonDirectoryPath + @"Weapons\WeaponsBlunts.json");
            Item[] bluntsArray = JsonSerializer.Deserialize<Weapon[]>(JsonString, Options)!;
            AddItemsToList(bluntsArray);
            JsonString = JsonToString(JsonDirectoryPath + @"Weapons\WeaponsStaves.json");
            Item[] stavesArray = JsonSerializer.Deserialize<Weapon[]>(JsonString, Options)!;
            AddItemsToList(stavesArray);

            //load armors
            JsonString = JsonToString(JsonDirectoryPath + @"Armors\ArmorsRobe.json");
            Item[] robeArmorsArray = JsonSerializer.Deserialize<Armor[]>(JsonString, Options)!;
            AddItemsToList(robeArmorsArray);
            JsonString = JsonToString(JsonDirectoryPath + @"Armors\ArmorsLight.json");
            Item[] lightArmorsArray = JsonSerializer.Deserialize<Armor[]>(JsonString, Options)!;
            AddItemsToList(lightArmorsArray);
            JsonString = JsonToString(JsonDirectoryPath + @"Armors\ArmorsHeavy.json");
            Item[] heavyArmorsArray = JsonSerializer.Deserialize<Armor[]>(JsonString, Options)!;
            AddItemsToList(heavyArmorsArray);

            //load runes
            JsonString = JsonToString(JsonDirectoryPath + @"Runes.json");
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
            string[] spellsFilepaths = Directory.GetFiles(JsonDirectoryPath + @"Spells\");
            Spell[] spellsArray;

            foreach (string path in spellsFilepaths)
            {
                JsonString = JsonToString(path);
                spellsArray = JsonSerializer.Deserialize<Spell[]>(JsonString, Options)!;

                foreach (var spell in spellsArray)
                {
                    Spells!.Add(spell);
                }
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

        public void PopulateLocations()
        {
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
                            Monster monster = new Monster((character as Monster)!);
                            monster.Id = GetNewId();
                            location.AddCharacter(monster);
                            AllCharactersTestProp.Add(monster);
                        }
                    }
                    else if (character.GetType() == typeof(Trader))
                    {
                        for (i = 0; i < kvp.Value; i++)
                        {
                            Trader trader = new Trader((character as Trader)!);
                            trader.Id = GetNewId();
                            location.AddCharacter(trader);
                            AllCharactersTestProp.Add(trader);
                        }
                    }
                    else if (character.GetType() == typeof(Hero))
                    {
                        for (i = 0; i < kvp.Value; i++)
                        {
                            Hero hero = new Hero((character as Hero)!);
                            hero.Id = GetNewId();
                            location.AddCharacter(hero);
                            AllCharactersTestProp.Add(hero);
                        }
                    }
                }

                //fill character's inventories
                //location.Characters!.ForEach(character => FillInventory(character));

            });
        }

        //method filling inventories of all characters with their items
        public void PopulateInventories()
        {
            Characters!.ForEach(character =>
            {
                FillInventory(character);
            });
        }

        //method saving game
        public void SaveGame(string savePath, Hints hints)
        {
            GameSave save = new GameSave();
            Player player = Player!;

            //save hints
            save.Hints = hints;

            //save takenids list
            save.TakenIds = TakenIds!;

            //remove player from his location 
            player.CurrentLocation!.Characters!.Remove(player);

            //save player current location as location with only
            //coordinates to restore later by matching with original
            //location
            Location playerLocation = new Location();
            playerLocation.X = player.CurrentLocation.X;
            playerLocation.Y = player.CurrentLocation.Y;
            playerLocation.Z = player.CurrentLocation.Z;

            player.CurrentLocation = playerLocation;

            //add player to gamesave
            save.Player = player;

            //prevent weird bug causing player hp/mp to become lower values when
            //loading game save
            save.PlayerHp = player.Hp;
            save.PlayerMp = player.Mp;

            Locations!.ForEach(loc =>
            {
                loc.CharsIds!.Clear();

                loc.Characters!.ForEach(character =>
                {

                    //clear currentLocation reference to avoid circural
                    //reference
                    character.CurrentLocation = null;

                    //add ids of characters to charIds
                    loc.CharsIds.Add(character.Id);

                    if (character.GetType() == typeof(Monster))
                    {
                        save.Monsters.Add((character as Monster)!);
                    }
                    else if (character.GetType() == typeof(Hero))
                    {
                        save.Heroes.Add((character as Hero)!);
                    }
                    else if (character.GetType() == typeof(Trader))
                    {
                        save.Traders.Add((character as Trader)!);
                    }
                });
            });

            //save locations to gamesave
            save.Locations = Locations;

            //remember to reassign currentlocation references to every character in the locations

            JsonString = JsonSerializer.Serialize(save, Options);
            File.WriteAllText(savePath, JsonString);

            //after saving game reassign currentLocation references for every character
            //and restore player position by finding location with the same x, y, and z
            //as previously saved temporary player's current location
            Locations.ForEach(loc =>
            {
                loc.Characters!.ForEach(character =>
                {
                    character.CurrentLocation = loc;
                });
            });
            Location temp = Locations.Find(loc =>
            
                loc.X == player.CurrentLocation!.X &&
                loc.Y == player.CurrentLocation!.Y &&
                loc.Z == player.CurrentLocation!.Z
            )!;

            temp.AddCharacter(player);
        }
        
        //method loading gamesave
        public void LoadGame(string savePath, out Hints hints)
        {
            JsonString = File.ReadAllText(savePath);

            GameSave save = JsonSerializer.Deserialize<GameSave>(JsonString, Options)!;

            //load hints
            hints = save.Hints;

            //clear locations
            Locations!.Clear();

            //add all locations from gamesave
            save.Locations!.ForEach(loc =>
            {

                //clear location "flat" characters
                loc.Characters!.Clear();

                loc.CharsIds!.ForEach(charId =>
                {
                    if (save.Traders!.Exists(tr => tr.Id == charId))
                    {
                        loc.AddCharacter(save.Traders.Find(tr => tr.Id == charId)!);
                    }
                    else if (save.Monsters!.Exists(tr => tr.Id == charId))
                    {
                        loc.AddCharacter(save.Monsters.Find(tr => tr.Id == charId)!);
                    }
                    else if (save.Heroes!.Exists(tr => tr.Id == charId))
                    {
                        loc.AddCharacter(save.Heroes.Find(tr => tr.Id == charId)!);
                    }

                });

                Locations.Add(loc);
            });

            //reassign currentLOcation for every character
            Locations.ForEach(loc =>
            {
                loc.Characters!.ForEach(character =>
                {
                    character.CurrentLocation = loc;
                });
            });

            //reassign player object to it's reference in data
            Player = save.Player;

            //prevent weird bug causing player hp/mp to become lower values when
            //loading game save
            Player!.Hp = save.PlayerHp;
            Player!.Mp = save.PlayerMp;

            //prevents weird bug causing effectiveMaxMp (and probably Hp also) to load before modifiers
            Player!.EffectiveMaxMp = Player!.GetEffectiveMaxMp();
            Player!.EffectiveMaxHp = Player!.GetEffectiveMaxHp();

            //load player to his location
            Location playerLocation = Locations.Find(loc => loc.X == Player!.CurrentLocation!.X &&
            loc.Y == save.Player!.CurrentLocation!.Y && loc.Z == Player.CurrentLocation!.Z)!;
            playerLocation.AddCharacter(Player!);

            
        }




        //==========================================HELPER METHODS========================================

        //helper method for pushing loaded characters objects into Characters list
        public void AddCharactersToList(Character[] charactersArray)
        {
            if (charactersArray.GetType() == typeof(Monster[]))
            {
                foreach (var character in charactersArray)
                {
                    if ((character as Monster)!.Level < 30)
                    {
                        character.Gold += (character as Monster)!.Level * 3;
                    }
                    else
                    {
                        character.Gold += (character as Monster)!.Level * 7;
                    }
                    Characters!.Add(character);
                }
            }
            else if (charactersArray.GetType() == typeof(Hero[]))
            {
                foreach (var character in charactersArray)
                {
                    if ((character as Hero)!.Level < 40)
                    {
                        character.Gold += (character as Hero)!.Level * 10;
                    }
                    else
                    {
                        character.Gold += (character as Hero)!.Level * 25;
                    }
                    Characters!.Add(character);
                }
            }
            else
            {
                foreach (var character in charactersArray)
                {
                    Characters!.Add(character);
                }
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
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -10, 0, weapon.Name!, true));
                        //weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Critical, -20, 0, weapon.Name!, true));
                        //weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -20, 0, weapon.Name!, true));
                    }
                    else if (weapon.Type == WeaponType.Blunt)
                    {
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Attack, 40, 0, weapon.Name!, true));
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.AtkSpeed, -20, 0, weapon.Name!, true));
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Critical, -60, 0, weapon.Name!, true));
                        weapon.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -60, 0, weapon.Name!, true));
                    }
                }

                //if it's armor, add modifiers(penalties) depending on it's weight
                if (item.GetType() == typeof(Armor))
                {
                    Armor armor = (Armor)item;

                    if (armor.Type == Armor.ArmorType.Torso)
                    {
                        if (armor.Weight >= 500)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Speed, -10, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -20, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -15, 0, armor.Name!, true));
                        }
                        else if (armor.Weight < 500 && armor.Weight >= 200)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -10, 0, armor.Name!, true));
                        }
                    }
                    if (armor.Type == Armor.ArmorType.Pants)
                    {
                        if (armor.Weight >= 450)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Speed, -7, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -15, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -10, 0, armor.Name!, true));
                        }
                        else if (armor.Weight < 450 && armor.Weight >= 150)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -7, 0, armor.Name!, true));
                        }
                    }
                    if (armor.Type == Armor.ArmorType.Helmet || armor.Type == Armor.ArmorType.Gloves || armor.Type == Armor.ArmorType.Shoes)
                    {
                        if (armor.Weight >= 250)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Speed, -5, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.Evasion, -7, 0, armor.Name!, true));
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -5, 0, armor.Name!, true));
                        }
                        else if (armor.Weight < 250 && armor.Weight >= 100)
                        {
                            armor.Modifiers!.Add(new Modifier(Modifier.ModType.MpRegen, -3, 0, armor.Name!, true));
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

                    if (combatNpc.StartingSpells!.Length > 0)
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
                if (Items!.Exists(it => it.Name!.ToLower() == kvp.Key.ToLower()))
                {
                    itemToAdd = Items!.Find(item => item.Name!.ToLower() == kvp.Key.ToLower())!;
                    character.AddItem(itemToAdd, kvp.Value);
                }
            }
        }

        //method finding new id for a character
        private ulong GetNewId()
        {
            ulong newId = 1;

            //if Id already exists, increment it until new, original number is found
            while (TakenIds!.Exists(id => id == newId))
            {
                newId++;
            }
            TakenIds.Add(newId);
            return newId;
        }
    }
}
