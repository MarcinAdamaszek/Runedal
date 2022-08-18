using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class Monster : CombatCharacter
    {
        //default constructor for json deserialization
        public Monster() : base() 
        {
            AssignId();
        }
        public Monster(string[] descriptive, int[] combatStats, string[][] responses, int gold)
            : base(descriptive, combatStats, responses, gold) 
        {
            AssignId();
        }

        //destructor for releasing id number
        ~Monster()
        {
            TakenIds.Remove(this.Id);
        }
        
        //list of id's assigned to all existing monsters
        public static List<int> TakenIds { get; set; } = new List<int>();
        public int Id { get; set; }

        //method assigning unique id
        private void AssignId()
        {
            Id = 0;

            //if Id already exists, increment it until new, original number is found
            while (TakenIds!.Exists(id => id == Id))
            {
                Id++;
            }
        }
        
    }
}
