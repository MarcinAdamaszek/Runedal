using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Items;
using System.Windows.Documents;

namespace Runedal.GameData.Characters
{

    //base class for all characters (player, npcs, creatures)
    public class Character : Entity
    {
        //default constructor for json deserialization
        public Character() : base()
        {
            Items = new Dictionary<string, int>();
            Inventory = new List<Item>();
            //AssignId();
        }
        
        //constructor for placeholder
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

        //copy constructor
        public Character(Character ch) : base(ch)
        {
            //create deep copy of modifiers collection
            Inventory = ch.Inventory!.ConvertAll(item => new Item(item));

            Items = ch.Items!;
            PassiveResponses = ch.PassiveResponses;
            AggressiveResponses = ch.AggressiveResponses;
            Questions = ch.Questions;
            Answers = ch.Answers;
            Gold = ch.Gold;
            WelcomePhrase = ch.WelcomePhrase;
            //AssignId();
        }



        public ulong Id { get; set; }

        //Amount of gold and list of items in the characters inventory
        public int Gold { get; set; }

        public List<Item>? Inventory { get; set; }

        //reference to location, where the character is currently located
        public Location? CurrentLocation { get; set; }

        //array of characters passive responses
        public string[]? PassiveResponses { get; set; }
        
        //array of character's aggressive responses
        public string[]? AggressiveResponses { get; set; }

        //array of questions player can ask character
        public string[]? Questions { get; set; }

        //array of answers character will respond when asked question
        public string[]? Answers { get; set; }

        //welcome phrase for talker characters
        public string WelcomePhrase { get; set; }

        //set and quantity of items to load into character's inventory on game launch
        public Dictionary<string, int>? Items { get; set; }

        //method for adding items into character's inventory
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
