using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Items;
using System.Windows.Documents;

namespace Runedal.GameData.Characters
{
    public class Character : Entity
    {
        public Character() : base()
        {
            Items = new Dictionary<string, int>();
            Inventory = new List<Item>();
            //AssignId();
        }
        
        public Character(string placeholder) : base(placeholder)
        {
            Items = new Dictionary<string, int>();
            Inventory = new List<Item>();
        }
        public Character(string[] descriptive, string[][] dialogues, int gold) : base(descriptive)
        {
            Items = new Dictionary<string, int>();
            Inventory = new List<Item>();
            PassiveResponses = dialogues[0];
            AggressiveResponses = dialogues[1];
            Questions = dialogues[2];
            Answers = dialogues[3];
            Gold = gold;
            //AssignId();
        }

        public Character(Character ch) : base(ch)
        {
            Inventory = ch.Inventory!.ConvertAll(item => new Item(item));

            Items = ch.Items!;
            PassiveResponses = ch.PassiveResponses;
            AggressiveResponses = ch.AggressiveResponses;
            Questions = ch.Questions;
            Answers = ch.Answers;
            Gold = ch.Gold;
            WelcomePhrase = ch.WelcomePhrase;
        }



        public ulong Id { get; set; }

        public int Gold { get; set; }

        public List<Item>? Inventory { get; set; }

        public Location? CurrentLocation { get; set; }

        public string[]? PassiveResponses { get; set; }
        
        public string[]? AggressiveResponses { get; set; }

        public string[]? Questions { get; set; }

        public string[]? Answers { get; set; }

        public string WelcomePhrase { get; set; }

        public Dictionary<string, int>? Items { get; set; }

        public void AddItem(Item newItem, int quantity = 1)
        {
            if (quantity == 0)
            {
                return;
            }

            int itemIndex = -1;

            itemIndex = Inventory!.FindIndex(item => item.Name!.ToLower() == newItem.Name!.ToLower());

            //if there is already another item with the same name in character's inventory, add to it's quantity 
            if (itemIndex != -1)
            {
                Inventory![itemIndex].ChangeQuantity(quantity);
            }
            else
            {
                Inventory!.Add(new Item(newItem, quantity));
            }
        }

        /// <summary>
        /// method for removing items from character's inventory
        /// </summary>
        /// <param name="oldItem"></param>
        /// <returns>true if the item is removed successfully, false if it didn't exist in character's 
        /// inventory in the first place</returns>
        public bool RemoveItem(string itemName, int quantity = 1)
        {
            if (quantity == 0)
            {
                return false;
            }

            int itemIndex = Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());
            Item itemToRemove;

            if (itemIndex != -1)
            {
                itemToRemove = Inventory[itemIndex];
                if (quantity < itemToRemove.Quantity)
                {
                    itemToRemove.ChangeQuantity(- quantity);
                    return true;
                }
                else if (quantity == itemToRemove.Quantity)
                {
                    Inventory.Remove(itemToRemove);
                    return true;
                }
            }
            return false;
        }

    }
}
