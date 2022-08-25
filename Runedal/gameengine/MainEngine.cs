using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Runedal.GameData;
using Runedal.GameData.Locations;
using Runedal.GameData.Characters;
using Runedal.GameData.Items;
using System.Windows.Media.Effects;


namespace Runedal.GameEngine
{
    public class MainEngine
    {
        public MainEngine(MainWindow window)
        { 
            this.Window = window;
            this.Data = new Data();

            //set game clock for game time
            GameClock = new DispatcherTimer(DispatcherPriority.Send);
            GameClock.Interval = TimeSpan.FromMilliseconds(100);
            GameClock.Tick += GameClockTick!;

            Data.LoadLocations();
            Data.LoadCharacters();
            Data.LoadItems();

            GameClock.Start();

            Data.Player.Hp -= 100;
            (Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp -= 10;
        }

        //enum type for type of message displayed in PrintMessage method for displaying messages in different colors
        enum MessageType
        {
            Default,
            UserCommand,
            Action,
            SystemFeedback,
            Gain,
            Loss,
            EffectOn,
            EffectOff
        }

        public MainWindow Window { get; set; }
        public Data Data { get; set; }
        public DispatcherTimer GameClock;

        //method processing user input commands
        public void ProcessCommand()
        {
            string userCommand = string.Empty;
            string command = string.Empty;
            string argument1 = string.Empty;
            string argument2 = string.Empty;
            string[] commandParts;

            //get user input from inputBox
            userCommand = Window.inputBox.Text;
            Window.inputBox.Text = string.Empty;

            //clear the input from extra spaces
            userCommand = Regex.Replace(userCommand, @"^\s+", "");
            userCommand = Regex.Replace(userCommand, @"\s+", " ");
            userCommand = Regex.Replace(userCommand, @"\s+$", "");

            //print userCommand in outputBox for user to see
            PrintMessage(userCommand, MessageType.UserCommand);

            //format to lowercase
            userCommand = userCommand.ToLower();

            //split user input into command and it's arguments
            Regex delimeter = new Regex(" ");
            commandParts = delimeter.Split(userCommand);

            //depending on number of arguments, assign them to proper variables
            if (commandParts.Length == 1)
            {
                command = commandParts[0];
            }
            else if (commandParts.Length == 2)
            {
                command = commandParts[0];
                argument1 = commandParts[1];
            }
            else
            {
                command = commandParts[0];
                argument1 = commandParts[1];
                argument2 = commandParts[2];
            }

            //match user input to proper engine action
            switch (command)
            {
                case "n":
                case "e":
                case "s":
                case "w":
                    ChangeLocationHandler(command);
                    break;
                case "t":
                case "trade":
                    TradeHandler(argument1);
                    break;
                case "buy":
                    BuyHandler(argument1, argument2);
                    break;
                case "sell":
                    SellHandler(argument1, argument2);
                    break;
                case "look":
                case "l":
                    LookHandler(argument1);
                    break;
                case "use":
                case "u":
                    UseHandler(argument1);
                    break;
                case "drop":
                case "d":
                    DropHandler(argument1, argument2);
                    break;
                case "inventory":
                case "i":
                    InventoryHandler(Data.Player!, false);
                    break;
                case "stats":
                    StatsHandler();
                    break;
                default:
                    PrintMessage("Pier%#$isz jak potłuczony..", MessageType.SystemFeedback);
                    return;
            }
        }




        //==============================================COMMAND HANDLERS=============================================

        //method moving player to next location
        private void ChangeLocationHandler(string direction)
        {
            string directionString = string.Empty;
            bool passage = Data.Player!.CurrentLocation!.GetPassage(direction);
            Location nextLocation = new Location();

            //if player is in combat state
            if (Data.Player!.CurrentState! == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            //if player is in trade state
            if (Data.Player!.CurrentState! == Player.State.Trade)
            {
                BreakTradeState();
            }

            switch (direction)
            {
                case "n":
                    directionString = "północ";
                    break;
                case "e":
                    directionString = "wschód";
                    break;
                case "s":
                    directionString = "południe";
                    break;
                case "w":
                    directionString = "zachód";
                    break;
            }

            if (GetNextLocation(direction, out nextLocation))
            {

                //if the passage is open
                if (passage)
                {
                    PrintMessage("Idziesz na " + directionString, MessageType.Action);

                    //change player's current location
                    Data.Player!.CurrentLocation = nextLocation;

                    //remove player from previous location
                    Data.Player.CurrentLocation!.Characters!.Remove(Data.Player);

                    //add player to the list of new location entities
                    Data.Player.CurrentLocation!.AddCharacter(Data.Player);

                    //display location info to user
                    LocationInfo();
                }
                else
                {
                    PrintMessage("Nie potrafisz otworzyć tego przejścia", MessageType.SystemFeedback);
                }
            }
            else
            {
                PrintMessage("Nic nie ma w tamtym kierunku", MessageType.SystemFeedback);
            }
        }

        //method handling 'look' command
        private void LookHandler(string entityName)
        {
            int index = -1;
            string description = string.Empty;
            entityName = entityName.ToLower();

            //if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            if (entityName == string.Empty || entityName == "around")
            {

                //if player is in trade state
                if (Data.Player!.CurrentState! == Player.State.Trade)
                {
                    BreakTradeState();
                }

                //if command "look" was used without argument, print location description
                LocationInfo();
            }
            else
            {

                //search location for items on the ground
                index = Data.Player!.CurrentLocation!.Items!.FindIndex(item => item.Name!.ToLower() == entityName);
                if (index != -1)
                {
                    ItemInfo(entityName);
                    return;
                }


                //else search characters of current location and player's inventory for entity with name matching the argument
                index = Data.Player!.CurrentLocation!.Characters!.FindIndex(character => character.Name!.ToLower() == entityName);
                if (index != -1)
                {

                    //if player is in trade state
                    if (Data.Player!.CurrentState! == Player.State.Trade)
                    {
                        BreakTradeState();
                    }
                    CharacterInfo(Data.Player!.CurrentLocation!.Characters[index]);
                }
                else
                {
                    //else search player's inventory for item with name matching the argument
                    index = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == entityName.ToLower());
                    if (index != -1)
                    {
                        ItemInfo(Data.Player!.Inventory[index].Name!);
                    }
                    else if (Data.Player!.CurrentState == Player.State.Trade)
                    {
                        index = Data.Player!.InteractsWith!.Inventory!.FindIndex(item => item.Name!.ToLower() == entityName.ToLower());
                        if (index != -1)
                        {
                            ItemInfo(Data.Player!.InteractsWith!.Inventory![index].Name!);
                        }
                    }
                }

                //if any entity matched the argument, print it's description to user
                if (index == -1)
                {
                    PrintMessage("Nie ma tu niczego o nazwie \"" + entityName + "\"", MessageType.SystemFeedback);
                }
            }
        }

        //method handling 'trade' command
        private void TradeHandler(string characterName)
        {
            int index = -1;
            Character tradingCharacter = new Character();

            //if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            //check if player is trading with someone already
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                BreakTradeState();
            }

            //check if the character of specified name exists in player's current location
            index = Data.Player!.CurrentLocation!.Characters!.FindIndex(character => character.Name!.ToLower() == characterName.ToLower());
            if (index != -1)
            {
                tradingCharacter = Data.Player!.CurrentLocation!.Characters![index];

                //check if chosen character is trader type or not
                if (tradingCharacter.GetType() != typeof(Trader))
                {
                    PrintMessage("Nie możesz handlować z tą postacią", MessageType.SystemFeedback);
                    return;
                }

                //set player's interaction character
                Data.Player!.InteractsWith = tradingCharacter;

                //set player's state to trade
                Data.Player!.CurrentState = Player.State.Trade;

                PrintMessage("Handlujesz z: " + tradingCharacter.Name, MessageType.Action);
                InventoryInfo(tradingCharacter, true);
                InventoryInfo(Data.Player!, true);
            }
            else
            {
                PrintMessage("Nie ma tu takiej postaci", MessageType.SystemFeedback);
            }
        }

        //method handling 'inventory' command
        private void InventoryHandler(Player player, bool withPrice)
        {

            //if player is trading, print trader's and player's inventory (trading interface)
            if (Data.Player!.CurrentState! == Player.State.Trade)
            {
                InventoryInfo(Data.Player!.InteractsWith!, true);
                InventoryInfo(Data.Player!, true);
            }
            else
            {
                InventoryInfo(player, withPrice);
            }
        }

        //method handling 'buy' command
        private void BuyHandler(string itemName, string quantity)
        {

            //if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            //if the player is trading with someone
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                Trader trader = trader = (Data.Player!.InteractsWith as Trader)!;
                int itemIndex = -1;
                int itemQuantity = 1;
                int buyingPrice;

                itemIndex = trader.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());

                //check if the item exists in trader's inventory and if trader has enough of it
                if (itemIndex == -1)
                {
                    PrintMessage(trader.Name + " nie posiada tego przedmiotu", MessageType.SystemFeedback);
                    return;
                }
                else if (trader.Inventory[itemIndex].Quantity < itemQuantity)
                {
                    PrintMessage(trader.Name + " nie posiada przedmiotu w tej ilości", MessageType.SystemFeedback);
                    return;
                }

                //set item quantity depedning on 2nd argument
                if (quantity != string.Empty)
                {
                    if (!int.TryParse(quantity, out itemQuantity))
                    {
                        PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                        return;
                    }
                    if (itemQuantity == 0)
                    {
                        PrintMessage("Powietrze chcesz kupić?", MessageType.SystemFeedback);
                        return;
                    }
                }

                //set buying price depending on quantity
                buyingPrice = CalculateTraderPrice(itemName) * itemQuantity;

                //if total buying price of the item is lesser than amount of gold possesed by player
                if (Data.Player!.Gold! >= buyingPrice)
                {
                    //remove item from traders inventory and gold from player's inventory
                    trader.RemoveItem(itemName, itemQuantity);
                    RemoveGoldFromPlayer(buyingPrice);

                    //add item to player's inventory 
                    AddItemToPlayer(itemName, itemQuantity);

                    //add gold amount to trader's pool
                    trader.Gold += buyingPrice;

                    //display trader's/player's inventories once again
                    InventoryInfo(trader, true);
                    InventoryInfo(Data.Player!, true);
                }
                else
                {
                    PrintMessage("Nie stać Cię", MessageType.SystemFeedback);
                }
            }
            else
            {
                PrintMessage("Obecnie z nikim nie handlujesz", MessageType.SystemFeedback);
            }
        }

        //method handling 'sell' command
        private void SellHandler(string itemName, string quantity)
        {
            //if the player is trading with someone
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                Trader trader = trader = (Data.Player!.InteractsWith as Trader)!;
                int itemIndex = -1;
                int itemQuantity = 1;
                int sellingPrice = 0;

                itemIndex = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());

                //check if the item exists in player's inventory and if he has enough of it
                if (itemIndex == -1)
                {
                    PrintMessage("Nie posiadasz wybranego przedmiotu", MessageType.SystemFeedback);
                    return;
                }
                else if (Data.Player!.Inventory[itemIndex].Quantity < itemQuantity)
                {
                    PrintMessage("Nie posiadasz wybranego przedmiotu w tej ilości", MessageType.SystemFeedback);
                    return;
                }

                //set item quantity depedning on 2nd argument
                if (quantity != string.Empty)
                {
                    if (!int.TryParse(quantity, out itemQuantity))
                    {
                        PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                        return;
                    }
                    if (itemQuantity == 0)
                    {
                        PrintMessage("Kogo chcesz oszukać?", MessageType.SystemFeedback);
                        return;
                    }
                }

                //set buying price depending on quantity
                sellingPrice = Data.Player!.Inventory[itemIndex].Price * itemQuantity;

                //if total buying price of the item is lesser than amount of gold possesed by player
                if (trader.Gold >= sellingPrice)
                {
                    //remove item from player's inventory and put it into trader's inventory
                    RemoveItemFromPlayer(itemName, itemQuantity);
                    trader.AddItem(Data.Player!.Inventory[itemIndex], itemQuantity);

                    //swap gold from player to trader 
                    AddGoldToPlayer(sellingPrice);
                    trader.Gold -= sellingPrice;
   
                    //display trader's/player's inventories once again
                    InventoryInfo(trader, true);
                    InventoryInfo(Data.Player!, true);
                }
                else
                {
                    PrintMessage("Handlarza nie stać na taki zakup", MessageType.SystemFeedback);
                }
            }
            else
            {
                PrintMessage("Obecnie z nikim nie handlujesz", MessageType.SystemFeedback);
            }
        }

        //method handling 'use' command
        private void UseHandler(string itemName)
        {
            Item itemToUse;

            //if 'use' was typed without any argument
            if (itemName == string.Empty)
            {
                PrintMessage("Co chcesz użyć?", MessageType.SystemFeedback);
                return;
            }

            if (Data.Player!.Inventory!.Exists(item => item.Name!.ToLower() == itemName.ToLower()))
            {
                itemToUse = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;

                if (itemToUse.GetType() == typeof(Consumable))
                {
                    UseConsumable((itemToUse as Consumable)!);
                }
                else
                {
                    PrintMessage("Nie możesz użyć tego przedmiotu", MessageType.SystemFeedback);
                }
            }
            else
            {
                PrintMessage("Nie posiadasz przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            

        }

        //method handling 'stats' command
        private void StatsHandler()
        {
            StatsInfo();
        }

        //method handling 'drop' command
        private void DropHandler(string itemName, string quantity)
        {
            int itemIndex = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());
            int itemQuantity = 1;
            Item itemToRemove;

            if (itemIndex != -1)
            {
                itemToRemove = Data.Player!.Inventory[itemIndex];

                //set item quantity depedning on 2nd argument if it's not empty, otherwise leave it as 1
                if (quantity != string.Empty)
                {
                    if (!int.TryParse(quantity, out itemQuantity))
                    {
                        PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                        return;
                    }
                    if (itemQuantity == 0)
                    {
                        PrintMessage("Powietrze chcesz wyrzucić?", MessageType.SystemFeedback);
                        return;
                    }
                }

                if (itemToRemove.Quantity >= itemQuantity)
                {
                    PrintMessage("Upuszczasz " + itemQuantity + " " + itemToRemove.Name, MessageType.Action);
                    RemoveItemFromPlayer(itemName, itemQuantity);
                    Data.Player!.CurrentLocation!.AddItem(itemToRemove, itemQuantity);
                }
                else
                {
                    PrintMessage("Nie możesz wyrzucić więcej niż posiadasz", MessageType.SystemFeedback);
                }
            }

            //if the item name is 'zloto' drop gold
            else if (itemName == "złoto")
            {
                if (itemQuantity <= Data.Player.Gold)
                {
                    PrintMessage("Upuszczasz " + itemQuantity + " złota", MessageType.Action);
                    RemoveGoldFromPlayer(itemQuantity);
                }
                else
                {
                    PrintMessage("Nie możesz wyrzucić więcej niż posiadasz", MessageType.SystemFeedback);
                }
                return;
            }
            else
            {
                PrintMessage("Nie posiadasz przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
            }
        }




        //==============================================DESCRIPTION METHODS=============================================

        //method describing location to user
        private void LocationInfo()
        {
            Location nextLocation = new Location();
            string itemsInfo = "Przedmioty: ";
            string charactersInfo = "Postacie: ";
            string exitsInfo = "Wyjścia: ";
            string[] directionsLetters = { "n", "e", "s", "w" };
            string[] directionsStrings = { " północ,", " wschód,", " południe,", " zachód," };
            int currentX = Data.Player!.CurrentLocation!.X;
            int currentY = Data.Player!.CurrentLocation!.Y;

            //print location name and description
            PrintMessage("[ " + Data.Player!.CurrentLocation!.Name + " ]");
            PrintMessage("Widzisz " + Data.Player!.CurrentLocation!.Description!);

            //describe exits for each direction
            for (int i = 0; i < 4; i++)
            {

                //if the location exists
                if (GetNextLocation(directionsLetters[i], out nextLocation))
                {
                    exitsInfo += directionsStrings[i];
                }
            }

            //remove the last comma 
            exitsInfo = Regex.Replace(exitsInfo, @",$", "");

            PrintMessage(exitsInfo);

            //add character names to their info strings for each character of specific type present in player's current location
            Data.Player.CurrentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() != typeof(Player))
                {
                    charactersInfo += " " + character.Name + ",";
                }
            });

            //add items names for each item present in the location
            Data.Player.CurrentLocation.Items!.ForEach((item) =>
            {
                itemsInfo += " " + item.Name;
                if (item.Quantity > 1)
                {
                    itemsInfo += "(" + item.Quantity + ")";
                }
                itemsInfo += ",";
            });

            //remove the last comma
            charactersInfo = Regex.Replace(charactersInfo, @",$", "");
            itemsInfo = Regex.Replace(itemsInfo, @",$", "");


            //if any characters found, print them to outputBox
            if (charactersInfo.Length > 13)
            {
                PrintMessage(charactersInfo);
            }

            //if any items found, print them to outputBox
            if (itemsInfo.Length > 12)
            {
                PrintMessage(itemsInfo);
            }

        }

        //method printing character's inventory
        private void InventoryInfo(Character character, bool withPrice = true)
        {
            string spaceAfterName = string.Empty;
            string spaceAfterQuantity = string.Empty;
            string spaceAfterPrice = string.Empty;
            string descriptionTable = string.Empty;
            string tableRow = string.Empty;
            string horizontalBorder = string.Empty;
            string delimeter = "||-----------------------------------------------------------||";
            string descriptionRow = "|| Przedmiot:                              | Ilość: || Cena: ||\n" + delimeter;
            int nameColumnSize = 0;
            int quantityColumnSize = 0;
            int priceColumnSize = 0;
            int price = 0;
            int borderSize = 0;

            //set delimeter, borderSize and descriptionRow depending on withPrice parameter
            if (withPrice)
            {
                delimeter = "||-----------------------------------------------------------||";
                descriptionRow = "|| Przedmiot:                              | Ilość: || Cena: ||\n" + delimeter;
                borderSize = 63;
            }
            else
            {
                delimeter = "||--------------------------------------------------||";
                descriptionRow = "|| Przedmiot:                              | Ilość: ||\n" + delimeter;
                borderSize = 54;
            }

            //create string representing top/bottom table borders
            for (int i = 0; i < borderSize; i++)
            {
                horizontalBorder += "=";
            }

            if (!withPrice)
            {
                descriptionTable = "********************** EKWIPUNEK *********************";
            }
            else if (character.GetType() == typeof(Player))
            {
                descriptionTable = "*********************** TWÓJ EKWIPUNEK ************************";
            }
            else
            {
                descriptionTable = "********************* EKWIPUNEK HANDLARZA *********************";
            }

            //print talbe description and top table border
            PrintMessage(descriptionTable);
            PrintMessage(descriptionRow);

            foreach (var item in character.Inventory!)
            {
                //calculate sizes of spaces in table rows to mantain neat table layout
                nameColumnSize = 40 - item.Name!.Length;
                quantityColumnSize = 7 - Convert.ToString(item.Quantity).Length;
                spaceAfterName = string.Empty;
                spaceAfterQuantity = string.Empty;

                //only if it's trade mode and price is needed
                if (withPrice)
                {
                    priceColumnSize = 7 - Convert.ToString(item.Price).Length;
                    spaceAfterPrice = string.Empty;
                    for (int i = 0; i < priceColumnSize; i++)
                    {
                        spaceAfterPrice += " ";
                    }
                }

                //create strings representing spaces with calculated lenghts
                for (int i = 0; i < nameColumnSize; i++)
                {
                    spaceAfterName += " ";
                }
                for (int i = 0; i < quantityColumnSize; i++)
                {
                    spaceAfterQuantity += " ";
                }


                //set the price depending on character type (higher price for traders)
                //only if it's in trade mode
                if (withPrice)
                {
                    if (character.GetType() == typeof(Player))
                    {
                        price = item.Price;
                    }
                    else
                    {
                        price = CalculateTraderPrice(item.Name);
                    }
                }

                //create a string representing table row (with price or without depending on withPrice parameter)
                if (withPrice)
                {
                    tableRow = "|| " + item.Name + spaceAfterName + "| " + Convert.ToString(item.Quantity) + spaceAfterQuantity + "| "
                        + price + spaceAfterPrice + "||";
                }
                else
                {
                    tableRow = "|| " + item.Name + spaceAfterName + "| " + Convert.ToString(item.Quantity) + spaceAfterQuantity + "||";
                }

                PrintMessage(tableRow);
            }

            //print bottom table border
            PrintMessage(horizontalBorder);

            //print player's gold pool
            if (character.GetType() == typeof(Player))
            {
                PrintMessage("Twoje złoto: " + Convert.ToString(Data.Player!.Gold!));
            }
        }

        //method describing character
        private void CharacterInfo(Character character)
        {
            PrintMessage("[ " + character.Name + " ]");
            PrintMessage(character.Description!);
        }

        //method describing item
        private void ItemInfo(string itemName)
        {
            string description = string.Empty;
            string weight = string.Empty;
            string itemType = string.Empty;
            string modifiers = string.Empty;
            string effect = string.Empty;
            string attack = string.Empty;
            string defense = string.Empty;
            string range = string.Empty;
            string sign = string.Empty;
            Item itemToDescribe = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;

            //set item's weight and type
            weight = "Waga: " + Convert.ToString(itemToDescribe.Weight);
            itemType = "Typ: ";

            //depending on item type, add info to description and set itemType string
            if (itemToDescribe.GetType() == typeof(Consumable))
            {
                itemType += "używalne";
                effect = "Działanie: ";

                //add modifiers descriptions to effect description for every modifier present in the item
                if (itemToDescribe.Modifiers!.Count > 0)
                {
                    itemToDescribe.Modifiers.ForEach(mod =>
                    {
                        effect += GetModDescription(mod) + ", ";
                    });

                    //remove trailing comma and add duration
                    effect = Regex.Replace(effect, @",\s$", "");
                    effect += " {" + itemToDescribe.Modifiers[0].Duration + " sekund}";
                }
                else
                {
                    effect += "brak";
                }
            }
            else if (itemToDescribe.GetType() == typeof(Armor))
            {

                //set string for polish armor type
                itemType += GetPolishArmorType((itemToDescribe as Armor)!.Type);

                defense = "Obrona: " + (itemToDescribe as Armor)!.Defense;
            }
            else if (itemToDescribe.GetType() == typeof(Weapon))
            {
                attack = "Atak: " + (itemToDescribe as Weapon)!.Attack;
                itemType += "Broń biała";
            }
            else if (itemToDescribe.GetType() == typeof(Ranged))
            {
                attack = "Atak: " + (itemToDescribe as Ranged)!.Attack;
                range += "Zasięg: " + (itemToDescribe as Ranged)!.Range;
                itemType += "Broń dystansowa";
            }

            //set modifiers string
            itemToDescribe.Modifiers!.ForEach(mod =>
            {
                //only if the mod is not temporary
                if (mod.Duration == 0)
                {
                    modifiers += GetModDescription(mod) + ", ";
                }
            });

            //remove trailing comma
            modifiers = Regex.Replace(modifiers, @",\s$", "");

            //print basic item info
            PrintMessage("[ " + itemToDescribe.Name + " ]");
            PrintMessage(itemToDescribe.Description!);
            PrintMessage(weight);
            PrintMessage(itemType);
            if (effect != string.Empty)
            {
                PrintMessage(effect);
            }
            if (defense != string.Empty)
            {
                PrintMessage(defense);
            }
            if (attack != string.Empty)
            {
                PrintMessage(attack);
            }
            if (range != string.Empty)
            {
                PrintMessage(range);
            }
            if (modifiers != string.Empty)
            {
                PrintMessage("Modyfikatory: " + modifiers);
            }
        }

        //method printing player's statistics
        private void StatsInfo()
        {
            const int halfSize = 42;
            const int rowsSize = 13;
            const int numberOfAttributes = 3;
            int remainingSpace = 0;
            int i = 1;
            int j = 0;
            int[] diffs = new int[numberOfAttributes];
            string[] rows = new string[rowsSize];
            string[] attributes = new string[numberOfAttributes];
            Player player = Data.Player!;

            //set string for displaying attributes base values along with modifiers bonus
            attributes[0] = Convert.ToString(player.Strength) + "(";
            attributes[1] = Convert.ToString(player.Agility) + "(";
            attributes[2] = Convert.ToString(player.Intelligence) + "(";
            diffs[0] = player.GetEffectiveStrength() - player.Strength;
            diffs[1] = player.GetEffectiveAgility() - player.Agility;
            diffs[2] = player.GetEffectiveIntelligence() - player.Intelligence;

            for (i = 0; i < numberOfAttributes; i++)
            {
                if (diffs[i] >= 0)
                {
                    attributes[i] += "+";
                }

                attributes[i] += diffs[i] + ")";
            }
    


            //format left side of the table
            rows[0] = "**********************************STATYSTYKI POSTACI**********************************";
            rows[1] = "||     Poziom: " + player.Level;
            rows[2] = "||     Doświadczenie: " + player.Experience;
            rows[3] = "||     Pkt. Atrybutów: " + player.AttributePoints;
            rows[4] = "||-----------------------------------";
            rows[5] = "||     Siła: " + attributes[0];
            rows[6] = "||     Zręczność: " + attributes[1] ;
            rows[7] = "||     Inteligencja: " + attributes[2];
            rows[8] = "||";
            rows[9] = "||-----------------------------------";
            rows[10] = "||     Maks. HP: " + player.EffectiveMaxHp;
            rows[11] = "||     Maks. MP: " + player.EffectiveMaxMp;
            rows[12] = "======================================================================================";

            //fill remaining space with space-characters
            for (i = 1; i < rowsSize - 1; i++)
            {
                remainingSpace = halfSize - rows[i].Length;

                //fill with spaces
                for (j = 0; j < remainingSpace; j++)
                {
                    rows[i] += " ";
                }

                //add middle vertical border
                rows[i] += "|     ";
            }

            //add right side (combat statistics)
            rows[1] += "Szybkość: " + Math.Floor(player.GetEffectiveSpeed());
            rows[2] += "Atak: " + Math.Floor(player.GetEffectiveAttack());
            rows[3] += "Szybkość Ataku: " + Math.Floor(player.GetEffectiveAtkSpeed());
            rows[4] += "Celność: " + Math.Floor(player.GetEffectiveAccuracy());
            rows[5] += "Obrona: " + Math.Floor(player.GetEffectiveDefense());
            rows[6] += "Uniki: " + Math.Floor(player.GetEffectiveEvasion());
            rows[7] += "Trafienia krytyczne: " + Math.Floor(player.GetEffectiveCritical());
            rows[8] += "Odporność na magię: " + Math.Floor(player.GetEffectiveMagicResistance());
            rows[9] += "Regeneracja HP " + Math.Floor(player.GetEffectiveHpRegen());
            rows[10] += "Regeneracja HP: " + Math.Floor(player.GetEffectiveMpRegen());

            //fill remaining space
            for (i = 1; i < rowsSize - 1; i++)
            {
                remainingSpace = halfSize * 2 - rows[i].Length;

                //fill with spaces
                for (j = 0; j < remainingSpace; j++)
                {
                    rows[i] += " ";
                }

                //add middle vertical border
                rows[i] += "||";
            }

            foreach (string row in rows)
            {
                PrintMessage(row);
            }
        }


        //==============================================HELPER METHODS=============================================

        //method returning effect description string
        //DEFINE THE METHOD NOW ===========================================

        //method returning formatted string representing modifier and it's value with sign
        private string GetModDescription(Modifier modifier)
        {
            string description = string.Empty;
            string modType = GetPolishModType(modifier.Type);
            string valueSign = string.Empty;

            //set sign of modifier to + if its positive number (for negative, minus sign is displayed automatically)
            if (modifier.Value > 0)
            {
                valueSign = "+";
            }

            description = modType + "(" + valueSign + modifier.Value + ")";
            return description;
        }

        //method returning polish string representing specified type of ArmorType type
        private string GetPolishArmorType(Armor.ArmorType type)
        {
            string armorType = string.Empty;
            switch (type)
            {
                case Armor.ArmorType.FullBody:
                    armorType = "Korpus";
                    break;
                case Armor.ArmorType.Helmet:
                    armorType = "Hełm";
                    break;
                case Armor.ArmorType.Shoes:
                    armorType = "Buty";
                    break;
                case Armor.ArmorType.Gloves:
                    armorType = "Rękawice";
                    break;
            }
            return armorType;
        }

        //method returning polish string representing specified type of CombatCharacter statistic 
        private string GetPolishModType(CombatCharacter.StatType type)
        {
            string modType = string.Empty;

            switch (type)
            {
                case (CombatCharacter.StatType.HpRegen):
                    modType = "Regeneracja Hp";
                    break;
                case (CombatCharacter.StatType.MpRegen):
                    modType = "Regeneracja Mp";
                    break;
                case (CombatCharacter.StatType.MaxHp):
                    modType = "Maks. Hp";
                    break;
                case (CombatCharacter.StatType.MaxMp):
                    modType = "Maks. Mp";
                    break;
                case (CombatCharacter.StatType.Strength):
                    modType = "Siła";
                    break;
                case (CombatCharacter.StatType.Intelligence):
                    modType = "Inteligencja";
                    break;
                case (CombatCharacter.StatType.Agility):
                    modType = "Zręczność";
                    break;
                case (CombatCharacter.StatType.Speed):
                    modType = "Szybkość";
                    break;
                case (CombatCharacter.StatType.Attack):
                    modType = "Atak";
                    break;
                case (CombatCharacter.StatType.AtkSpeed):
                    modType = "SzybkośćAtaku";
                    break;
                case (CombatCharacter.StatType.Accuracy):
                    modType = "Celność";
                    break;
                case (CombatCharacter.StatType.Critical):
                    modType = "Trafienia krytyczne";
                    break;
                case (CombatCharacter.StatType.Defense):
                    modType = "Obrona";
                    break;
                case (CombatCharacter.StatType.Evasion):
                    modType = "Uniki";
                    break;
                case (CombatCharacter.StatType.MagicResistance):
                    modType = "Odporność na magię";
                    break;
            }
            return modType;
        }

        /// <summary>
        /// finds location in the direction specified by 'direction' argument and returns true if found, false otherwise
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// 
        private bool GetNextLocation(string direction, out Location nextLocation)
        {
            nextLocation = Data.Player!.CurrentLocation!;
            int currentX = nextLocation.X;
            int currentY = nextLocation.Y;
            int locationIndex = -1;
            bool isFound = false;

            switch (direction)
            {
                case "n":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Y == currentY + 1);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "e":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX + 1);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "s":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Y == currentY - 1);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "w":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX - 1);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
            }

            return isFound;
        }

        //helper method for calculating selling price (trader price) of the item
        private int CalculateTraderPrice(string itemName)
        {
            Item itemToEvaluate = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;

            double doublePrice = Convert.ToDouble(itemToEvaluate.Price);
            int roundedPrice = Convert.ToInt32(Math.Round(doublePrice * Data!.PriceMultiplier));
            return roundedPrice;
        }

        //method displaying communicates in outputBox of the gui
        private void PrintMessage(string msg, MessageType type = MessageType.Default)
        {

            //color text of the message before displaying
            TextRange tr = new(this.Window.outputBox.Document.ContentEnd, this.Window.outputBox.Document.ContentEnd);
            tr.Text = "\n" + msg;

            switch (type)
            {
                case (MessageType.Default):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightGray);
                    break;
                case (MessageType.UserCommand):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Aqua);
                    break;
                case (MessageType.SystemFeedback):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkSalmon);
                    break;
                case (MessageType.Action):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightSkyBlue);
                    break;
                case (MessageType.Gain):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                    break;
                case (MessageType.Loss):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Goldenrod);
                    break;
                case (MessageType.EffectOn):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.MediumSpringGreen);
                    break;
                case (MessageType.EffectOff):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.SeaGreen);
                    break;
            }

            Window.outputBox.ScrollToEnd();
        }






        //==============================================MANIPULATION METHODS=============================================

        //method handling adding items to player's inventory
        private void AddItemToPlayer(string itemName, int quantity)
        {
            Item itemToAdd = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;
            Data.Player!.AddItem(itemToAdd, quantity);
            PrintMessage("Zdobyłeś " + Convert.ToString(quantity) + " " + itemToAdd.Name, MessageType.Gain);
        }

        //method handling removing items from player's inventory
        private void RemoveItemFromPlayer(string itemName, int quantity = 1)
        {

            //find item in data to have it's proper (first letter capitalized) name string to display in message for player
            Item itemToRemove = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;

            if (Data.Player!.RemoveItem(itemName, quantity))
            {
                PrintMessage("Straciłeś " + Convert.ToString(quantity) + " " + itemToRemove.Name, MessageType.Loss);
            }
            else
            {
                PrintMessage("Coś poszło nie tak..", MessageType.SystemFeedback);
            }
        }

        //method handling adding gold to player's pool
        private void AddGoldToPlayer(int gold)
        {
            Data.Player!.Gold += gold;
            PrintMessage("Zyskałeś " + Convert.ToString(gold) + " złota", MessageType.Gain);
        }

        //method handling removing gold from player's pool
        private void RemoveGoldFromPlayer(int gold)
        {
            if (Data.Player!.Gold >= gold)
            {
                PrintMessage("Straciłeś " + Convert.ToString(gold) + " złota", MessageType.Loss);
                Data.Player!.Gold -= gold;
            }
            else
            {
                PrintMessage("Straciłeś " + Convert.ToString(Data.Player!.Gold) + " złota", MessageType.Loss);
                Data.Player!.Gold = 0;
            }

        }

        //method for using consumable item
        public void UseConsumable(Consumable item)
        {
            RemoveItemFromPlayer(item.Name!);
            ApplyEffect(item.Modifiers!, item.Name!);
        }


        //method applying effects to player
        private void ApplyEffect(List<Modifier> modifiers, string objectName)
        {
            Player player = Data.Player!;
            EffectOnPlayer itemEffect;
            string description = objectName + ":";
            int durationInTicks = 0;

            //if there are any modifiers in the item
            if (modifiers.Count > 0)
            {

                //get duration for new effect from first modifier in the list
                durationInTicks = modifiers[0].DurationInTicks;

                //set ParentEffect for each mod and add it's description to description string
                modifiers!.ForEach(mod =>
                {
                    if (mod.Duration != 0)
                    {
                        mod.ParentEffect = objectName;

                        //add modifiers descriptors in form of 'modifier(+/-[value])' to description string 
                        description += " " + GetModDescription(mod) + ",";
                    }
                });

                //clear description from trailing comma and prepare item effect object
                description = Regex.Replace(description, @",$", "");
                itemEffect = new EffectOnPlayer(objectName, description, durationInTicks);

                //if effect is supposed to stack
                if (Data.StackingEffects!.Contains(objectName.ToLower()))
                {
                    player.Effects!.Add(itemEffect);
                    modifiers.ForEach(mod =>
                    {
                        player.AddModifier(mod);
                    });
                }
                else
                {
                    
                    //if player is already affected by the same effect, reset effect and mods durations
                    if (player.Modifiers!.Exists(mod => mod.ParentEffect!.ToLower() == objectName.ToLower()))
                    {
                        player.Modifiers.ForEach(mod =>
                        {
                            if (mod.ParentEffect == objectName)
                            {
                                mod.ResetDuration();
                            }
                            
                        });

                        //reset effect's durationInTicks value
                        player.Effects!.Find(effect => effect.Name!.ToLower() == objectName.ToLower())!.DurationInTicks = durationInTicks;
                    }
                    else
                    {
                        player.Effects!.Add(itemEffect);
                        modifiers.ForEach(mod =>
                        {
                            player.AddModifier(mod);
                        });
                    }
                }


                PrintMessage("Czujesz efekt działania " + itemEffect.Description, MessageType.EffectOn);
            }
        }

        //method breaking trade state and printing proper message
        private void BreakTradeState()

        {
             PrintMessage("Przestajesz handlować z: " + Data.Player.InteractsWith!.Name, MessageType.Action);
             Data.Player.InteractsWith = new Character();
             Data.Player!.CurrentState = Player.State.Idle;
        }






        //==============================================EVENT HANDLERS=============================================

        //handler for tick event of GameClock
        private void GameClockTick(object sender, EventArgs e)
        {
            int numberOfEffects;
            List<EffectOnPlayer> playerEffects;

            //handle all characters regeneration/duration-decrease of modifiers etc
            Data.Characters!.ForEach(character =>
            {
                if (character is CombatCharacter)
                {
                    (character as CombatCharacter)!.HandleTick();
                }
            });

            playerEffects = Data.Player!.Effects!;
            numberOfEffects = playerEffects.Count;
            
            //handle duration-decrease and wearing off of effects affecting player
            for (int i = 0; i < numberOfEffects; i++)
            {
                
                //if duration is greater than 1 - decrement it.
                //otherwise, if it equals 1 - effect has ended so remove it
                if (playerEffects[i].DurationInTicks > 1)
                {
                    playerEffects[i].DurationInTicks--;
                }
                else if (playerEffects[i].DurationInTicks == 1)
                {
                    PrintMessage("Skończyło się działanie " + playerEffects[i].Description, MessageType.EffectOff);
                    Data.Player!.Effects!.Remove(playerEffects[i]);
                    
                    //after removing effect, the list count has decremented, so number of effects also needs
                    //to decrement to avoid OutOfRange exception
                    numberOfEffects--;
                }
            }


            //PrintMessage(Convert.ToString((Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp));
        }
    }
}
