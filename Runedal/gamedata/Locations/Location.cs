using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Entities;

namespace Runedal.GameData.Locations
{
    public class Location
    {
        public Location(int x, int y, string? name, string? description)
        {
            this.X = x;
            this.Y = y; 
            this.Name = name; 
            this.Description = description; 
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Passage NorthPassage { get; set; } = new Passage();
        public Passage EastPassage { get; set; } = new Passage();
        public Passage SouthPassage { get; set; } = new Passage();
        public Passage WestPassage { get; set; } = new Passage();
        public List<Entity> Entities { get; set; } = new List<Entity>();

        public void AddEntity(Entity entity)
        {
           this.Entities.Add(entity);
        }
        public void RemoveEntity(Entity entity)
        {
            this.Entities.Remove(entity);
        }
    }
}
