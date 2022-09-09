using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Characters;
using Runedal.GameData.Items;


namespace Runedal.GameData
{
    public class Location : Entity
    {
        //default constructor for json deserializer
        public Location() : base()
        {
            NorthPassage = true;
            EastPassage = true;
            SouthPassage = true;
            WestPassage = true;
            Characters = new List<Character>();
            Items = new List<Item>();
            Gold = 0;
        }
        public Location(int[] coordinates, string[] descriptive, bool[] openBools) : base(descriptive)
        {
            X = coordinates[0];
            Y = coordinates[1];
            Z = coordinates[2];
            NorthPassage = openBools[0];
            EastPassage = openBools[1];
            SouthPassage = openBools[2];
            WestPassage = openBools[3];
            UpPassage = openBools[4];
            DownPassage = openBools[5];
            Characters = new List<Character>();
            Items = new List<Item>();
            Gold = 0;
        }

        //location coordinates
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        //passages to other locations
        public bool NorthPassage { get; set; }
        public bool EastPassage { get; set; }
        public bool SouthPassage { get; set; }
        public bool WestPassage { get; set; }
        public bool UpPassage { get; set; }
        public bool DownPassage { get; set; }

        //list of character-entities currently present in the location
        public List<Character>? Characters { get; set; }

        //list of characters to load into the location at game launch
        public Dictionary<string, int>? CharsToAdd { get; set; }

        //list of items lying on the ground
        public List<Item>? Items { get; set; }

        //gold lying on the ground
        public int Gold { get; set; }

        //methods for adding and removing items
        public void AddItem(Item item, int quantity)
        {
            int itemIndex = Items!.FindIndex(item => item.Name!.ToLower() == item.Name.ToLower());
            Item itemToAdd;

            if (itemIndex != -1)
            {
                Items[itemIndex].Quantity += quantity;
            }
            else
            {
                itemToAdd = new Item(item, quantity);
                Items.Add(itemToAdd);
            }
        }

        public bool RemoveItem(string itemName, int quantity)
        {
            int itemIndex = Items!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());
            Item itemToRemove;

            if (itemIndex != -1)
            {
                itemToRemove = Items[itemIndex];
                if (quantity < itemToRemove.Quantity)
                {
                    itemToRemove.Quantity -= quantity;
                    return true;
                }
                else if (quantity == itemToRemove.Quantity)
                {
                    Items.Remove(itemToRemove);
                    return true;
                }
            }
            return false;
        }

        //methods for adding and removing characters
        public void AddCharacter(Character character)
        {
            Characters!.Add(character);
            character.CurrentLocation = this;
        }
        public void RemoveCharacter(Character character)
        {
            Characters!.Remove(character);
        }
        public bool GetPassage(string direction)
        {
            switch (direction)
            {
                case "n":
                    return NorthPassage;
                case "e":
                    return EastPassage;
                case "s":
                    return SouthPassage;
                case "w":
                    return WestPassage;
                case "u":
                    return UpPassage;
                case "d":
                    return DownPassage;
            }

            return false;
        }
    }
}
