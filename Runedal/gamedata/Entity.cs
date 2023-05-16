using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public abstract class Entity
    {
        public Entity() {}

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

        public Entity(Entity en)
        {
            Name = en.Name;
            Description = en.Description;
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
