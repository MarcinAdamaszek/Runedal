using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Locations;

namespace Runedal.GameData.Entities
{
    public class Entity
    {
        public Entity(string name, string description, int strength, int intelligence, int agility)
        {
            Name = name;
            Description = description;
            Strength = strength;
            Intelligence = intelligence;
            Agility = agility;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Agility { get; set; }

        public Location? CurrentLocation { get; set; }
    }
}
