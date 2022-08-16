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
            Items = new Dictionary<string, int>();
            Inventory = new List<Item>();
        }
        public Character(string[] descriptive, string[][] responses, int gold) : base(descriptive)
        {
            Items = new Dictionary<string, int>();
            Inventory = new List<Item>();
            PassiveResponses = responses[0];
            AggressiveResponses = responses[1];
            Start = descriptive[2];
            Gold = gold;
        }

        //Amount of gold and list of items in the characters inventory
        public int Gold { get; set; }
        public List<Item>? Inventory { get; set; }

        //reference to location, where the character is currently located
        public Location? CurrentLocation { get; set; }

        //array of characters passive responses
        public string[]? PassiveResponses { get; set; }
        
        //array of character's aggressive responses
        public string[]? AggressiveResponses { get; set; }

        //character's starting location
        public string? Start { get; set; }
        
        //set and quantity of items to load into character's inventory on game launch
        public Dictionary<string, int>? Items { get; set; }

        //method for adding items into character's inventory
        public void AddItem(Item newItem, int quantity)
        {
            int itemIndex = -1;

            itemIndex = Inventory!.FindIndex(item => item.Name == newItem.Name);

            //if there is already another item with the same name in character's inventory, add to it's quantity 
            if (itemIndex != -1)
            {
                Inventory![itemIndex].Quantity += newItem.Quantity;
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
        public bool RemoveItem(string itemName, int quantity)
        {
            int itemIndex = -1;
            Item itemToRemove = new Item();

            itemIndex = Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());
            itemToRemove = Inventory[itemIndex];

            //if the item exists in character's inventory and desired quantity is not greater than actual quantity
            if (itemIndex == -1 && itemToRemove.Quantity >= quantity)
            {
                //if the desided quantity is exact the same as actual quantity - remove item from traders inventory,
                //otherwise subtract quantity
                if (itemToRemove.Quantity == quantity)
                {
                    Inventory.Remove(itemToRemove);
                    return true;
                }
                else
                {
                    itemToRemove.Quantity -= quantity;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
