using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Locations;
using Runedal.GameData.Items;

namespace Runedal.GameData.Characters
{
    //base class for all characters (player, npcs, creatures)
    public class Character : Entity
    {
        //default constructor for json deserialization
        public Character() : base()
        {
            Inventory = new List<Item>();
        }
        public Character(string name, string description,
            string[] passiveResponses, string[] aggressiveResponses) : base(name, description)
        {

            Inventory = new List<Item>();
            PassiveResponses = passiveResponses;
            AggressiveResponses = aggressiveResponses;
        }

        //Amount of gold and list of items in the characters inventory
        public int Gold { get; set; }
        public List<Item>? Inventory { get; set; }

        //reference to location, where the character is currently located
        public Location? CurrentLocation { get; set; }

        //array of characters passive responses
        public string[]? PassiveResponses { get; private set; }
        
        //array of character's aggressive responses
        public string[]? AggressiveResponses { get; private set; }

    }
}
