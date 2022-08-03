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
        public Entity()
        {
            Name = "placeholder";
            Description = "none";
        }
        public Entity(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
