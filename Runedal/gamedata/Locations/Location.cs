using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Characters;

namespace Runedal.GameData.Locations
{
    public class Location : Entity
    {
        //default constructor for json deserializer
        public Location() : base()
        {
            NorthPassage = new Passage(true);
            EastPassage = new Passage(true);
            SouthPassage = new Passage(true);
            WestPassage = new Passage(true);
            Characters = new List<Character>();
        }
        public Location(int x, int y, string name, string description, bool nOpen = true, bool eOpen = true, bool sOpen = true, bool wOpen = true) : base(name, description)
        {
            this.X = x;
            this.Y = y; 
            NorthPassage = new Passage(nOpen);                
            EastPassage = new Passage(eOpen);                   
            SouthPassage = new Passage(sOpen);                  
            WestPassage = new Passage(wOpen);                   
            Characters = new List<Character>();
        }

        //location coordinates
        public int X { get; set; }
        public int Y { get; set; }

        //passages to other locations
        public Passage? NorthPassage { get; set; }
        public Passage? EastPassage { get; set; }
        public Passage? SouthPassage { get; set; }
        public Passage? WestPassage { get; set; }

        //list of character-entities currently present in the location
        public List<Character>? Characters { get; private set; }

        //methods for adding and removing characters
        public void AddCharacter(Character character)
        {
            this.Characters!.Add(character);
        }
        public void RemoveCharacter(Character character)
        {
            this.Characters!.Remove(character);
        }
    }
}
