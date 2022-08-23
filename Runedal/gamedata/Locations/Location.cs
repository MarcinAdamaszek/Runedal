using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData;
using Runedal.GameData.Characters;
using Runedal.GameData.Items;


namespace Runedal.GameData.Locations
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
        }
        public Location(int[] coordinates, string[] descriptive, bool[] openBools) : base(descriptive)
        {
            this.X = coordinates[0];
            this.Y = coordinates[1]; 
            NorthPassage = openBools[0];                
            EastPassage = openBools[1];                   
            SouthPassage = openBools[2];                  
            WestPassage = openBools[3];                   
            Characters = new List<Character>();
            Items = new List<Item>();
        }

        //location coordinates
        public int X { get; set; }
        public int Y { get; set; }

        //passages to other locations
        public bool NorthPassage { get; set; }
        public bool EastPassage { get; set; }
        public bool SouthPassage { get; set; }
        public bool WestPassage { get; set; }

        //list of character-entities currently present in the location
        public List<Character>? Characters { get; set; }

        //list of items lying in the location
        public List<Item>? Items { get; set; }

        //methods for adding and removing characters
        public void AddCharacter(Character character)
        {
            this.Characters!.Add(character);
        }
        public void RemoveCharacter(Character character)
        {
            this.Characters!.Remove(character);
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
            }

            return false;
        }
    }
}
