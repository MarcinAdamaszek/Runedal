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
        public Location(int x, int y, string? name, string? description, bool nOpen = true, bool eOpen = true, bool sOpen = true, bool wOpen = true)
        {
            this.X = x;
            this.Y = y; 
            this.Name = name; 
            this.Description = description;
            NorthPassage = new Passage(nOpen);
            EastPassage = new Passage(eOpen);
            SouthPassage = new Passage(sOpen);
            WestPassage = new Passage(wOpen);
            Entities = new List<Entity>();
        }
        public int X { get; set; }
        public int Y { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Passage NorthPassage { get; set; }
        public Passage EastPassage { get; set; }
        public Passage SouthPassage { get; set; }
        public Passage WestPassage { get; set; }
        public List<Entity> Entities { get; set; }

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
