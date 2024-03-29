﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Characters;
using Runedal.GameData.Effects;
using Runedal.GameData.Items;


namespace Runedal.GameData
{
    public class Location : Entity
    {
        public Location() : base()
        {
            IsVisible = false;
            NorthPassage = true;
            EastPassage = true;
            SouthPassage = true;
            WestPassage = true;
            Characters = new List<Character>();
            Items = new List<Item>();
            CharsIds = new List<ulong>();
            Gold = 0;
        }
        public Location(int[] coordinates, string[] descriptive, bool[] openBools) : base(descriptive)
        {
            X = coordinates[0];
            Y = coordinates[1];
            Z = coordinates[2];
            IsVisible = false;
            NorthPassage = openBools[0];
            EastPassage = openBools[1];
            SouthPassage = openBools[2];
            WestPassage = openBools[3];
            UpPassage = openBools[4];
            DownPassage = openBools[5];
            Characters = new List<Character>();
            Items = new List<Item>();
            CharsIds = new List<ulong>();
            Gold = 0;
        }
        public Location(Location lc) : base(lc)
        {
            IsVisible = false;
            NorthPassage = lc.NorthPassage;
            EastPassage = lc.EastPassage;
            SouthPassage = lc.SouthPassage;
            WestPassage = lc.WestPassage;
            UpPassage = lc.UpPassage;
            DownPassage = lc.DownPassage;
            X = lc.X;
            Y = lc.Y;
            Z = lc.Z;
            Characters = new List<Character>();
            CharsToAdd = lc.CharsToAdd;
            CharsIds = new List<ulong>();
            Items = new List<Item>();
            Gold = lc.Gold;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public bool NorthPassage { get; set; }
        public bool EastPassage { get; set; }
        public bool SouthPassage { get; set; }
        public bool WestPassage { get; set; }
        public bool UpPassage { get; set; }
        public bool DownPassage { get; set; }

        public bool IsVisible { get; set; }

        public List<Character>? Characters { get; set; }

        public Dictionary<string, int>? CharsToAdd { get; set; }

        public List<ulong>? CharsIds { get; set; }

        public List<Item>? Items { get; set; }

        public int Gold { get; set; }

        public void AddItem(Item addedItem, int quantity)
        {
            int itemIndex = Items!.FindIndex(item => item.Name!.ToLower() == addedItem.Name!.ToLower());
            Item itemToAdd;

            if (itemIndex != -1)
            {
                Items[itemIndex].Quantity += quantity;
            }
            else
            {
                itemToAdd = new Item(addedItem, quantity);
                Items.Add(itemToAdd);
            }
        }

        public bool RemoveItem(string itemName, int quantity)
        {
            int itemIndex = Items!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());
            Item itemToRemove;

            if (itemIndex != -1)
            {
                itemToRemove = Items[itemIndex];
                if (quantity < itemToRemove.Quantity)
                {
                    itemToRemove.Quantity -= quantity;
                    return true;
                }
                else if (quantity == itemToRemove.Quantity)
                {
                    Items.Remove(itemToRemove);
                    return true;
                }
            }
            return false;
        }

        public void AddCharacter(Character character)
        {
            Characters!.Add(character);
            character.CurrentLocation = this;
        }
        public void RemoveCharacter(Character character)
        {
            Characters!.Remove(character);
        }
        public bool GetPassage(string direction)
        {
            switch (direction)
            {
                case "n":
                    return NorthPassage;
                case "e":
                    return EastPassage;
                case "s":
                    return SouthPassage;
                case "w":
                    return WestPassage;
                case "u":
                    return UpPassage;
                case "d":
                    return DownPassage;
            }

            return false;
        }
    }
}
