using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Locations;

namespace Runedal.GameData
{
    //base abstract class for all (almost) entities in game
    public abstract class Entity
    {
        //default constructor for json deserialization
        public Entity() {}

        //constructor for placeholder for empty Player's item slots
        public Entity(string placeholder)
        {
            Name = placeholder;
            Description = "none";
        }
        public Entity(string[] descriptiveParameters)
        {
            Name = descriptiveParameters[0];
            Description = descriptiveParameters[1];
        }

        //copy constructor
        public Entity(Entity en)
        {
            Name = en.Name;
            Description = en.Description;
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
