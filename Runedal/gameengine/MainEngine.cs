using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using Runedal.GameData;
using Runedal.GameData.Locations;
using Runedal.GameData.Characters;
using Runedal.GameData.Items;


namespace Runedal.GameEngine
{
    public class MainEngine
    {
        public MainEngine(MainWindow window)
        { 
            this.Window = window;
            this.Data = new Data();

            Data.LoadLocations();
            Data.LoadCharacters();
            Data.LoadItems();
            
        }

        //enum type for type of message displayed in PrintMessage method for displaying messages in different colors
        enum MessageType
        {
            Default,
            UserCommand,
            Info,
            SystemFeedback,
            Gain,
            Loss
        }

        public MainWindow Window { get; set; }
        public Data Data { get; set; }


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
                    ChangeLocation(command);
                    break;
                case "t":
                case "trade":
                    HandleTrade(argument1);
                    break;
                case "b":
                case "buy":
                    HandleBuy(argument1, argument2);
                    break;
                case "look":
                case "l":
                    HandleLook(argument1);
                    break;
                case "inventory":
                case "i":
                    InventoryInfo(Data.Player!, false);
                    break;
                default:
                    PrintMessage("Pier%#$isz jak potłuczony..", MessageType.SystemFeedback);
                    return;
            }
        }



        //==============================================COMMAND HANDLERS=============================================

        //method moving player to next location
        private void ChangeLocation(string direction)
        {
            string directionString = string.Empty;
            bool passage = Data.Player!.CurrentLocation!.GetPassage(direction);
            Location nextLocation = new Location();

            //if player isn't in idle state
            if (!IsPlayerIdle())
            {
                return;
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
                    PrintMessage("Idziesz na " + directionString, MessageType.SystemFeedback);

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
                    PrintMessage("Nie potrafisz otworzyć tego przejścia");
                }
            }
            else
            {
                PrintMessage("Nic nie ma w tamtym kierunku");
            }
        }

        //method handling 'look' command
        private void HandleLook(string entityName)
        {
            int index = -1;
            string description = string.Empty;
            entityName = entityName.ToLower();

            ////if player isn't in idle state
            if (!IsPlayerIdle())
            {
                return;
            }

            if (entityName == string.Empty || entityName == "around")
            {

                //if command "look" was used without argument, print location description
                LocationInfo();
            }
            else
            {
                //else search characters of current location and player's inventory for entity with name matching the argument
                index = Data.Player!.CurrentLocation!.Characters!.FindIndex(character => character.Name!.ToLower() == entityName);
                if (index != -1)
                {
                    description = Data.Player!.CurrentLocation!.Characters[index].Description!;
                }
                else
                {
                    index = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == entityName);
                    if (index != -1)
                    {
                        description = Data.Player!.Inventory[index].Description!;
                    }
                }

                //if any entity matched the argument, print it's description to user
                if (description != string.Empty)
                {
                    PrintMessage(description);
                }
                else
                {
                    PrintMessage("Nie ma tu niczego o nazwie \"" + entityName + "\"");
                }
            }
        }

        //method handling 'trade' command
        private void HandleTrade(string characterName)
        {
            int index = -1;
            Character tradingCharacter = new Character();

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

                PrintMessage("Handlujesz z: " + tradingCharacter.Name, MessageType.SystemFeedback);
                InventoryInfo(tradingCharacter, true);
                InventoryInfo(Data.Player!, true);
            }
            else
            {
                PrintMessage("Nie ma tu takiej postaci", MessageType.SystemFeedback);
            }
        }

        //method handling 'buy' command
        private void HandleBuy(string itemName, string quantity)
        {
            Trader trader = new Trader();
            Item boughtItem = Data.Items!.Find(item => item.Name!.ToLower() == itemName)!;
            int itemQuantity = 1;
            int buyingPrice = CalculateTraderPrice(boughtItem);

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
                    PrintMessage("Nie możesz kupić Madiego", MessageType.SystemFeedback);
                    return;
                }
            }

            //if the player is trading with someone
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                trader = (Data.Player!.InteractsWith as Trader)!;

                //if the buying price doesn't exceed player's gold pool, remove the gold from it
                if (RemoveGoldFromPlayer(buyingPrice))
                {

                    //try to remove item from trader's inventory (will fail if the item is nonexistent)
                    if (trader.RemoveItem(itemName, itemQuantity))
                    {
                        //add item to player's inventory 
                        AddItemToPlayer(boughtItem, itemQuantity);
                        
                        //add gold amount to trader's pool
                        trader.Gold += buyingPrice;
                    }
                }
            }
        }




        //==============================================DESCRIPTION METHODS=============================================

        //method describing location to user
        private void LocationInfo()
        {
            Location nextLocation = new Location();
            string charactersInfo = "Postacie: ";
            string exitsInfo = "Wyjścia: ";
            string[] directionsLetters = { "n", "e", "s", "w" };
            string[] directionsStrings = { " północ,", " wschód,", " południe,", " zachód," };
            int currentX = Data.Player!.CurrentLocation!.X;
            int currentY = Data.Player!.CurrentLocation!.Y;

            //print location name and description
            PrintMessage("[ " + Data.Player!.CurrentLocation!.Name + " ]", MessageType.Info);
            PrintMessage(Data.Player!.CurrentLocation!.Description!);

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

            PrintMessage(exitsInfo, MessageType.Info);

            //add character names to their info strings for each character of specific type present in player's current location
            Data.Player.CurrentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() != typeof(Player))
                {
                    charactersInfo += " " + character.Name + ",";
                }
            });

            //remove the last comma
            charactersInfo = Regex.Replace(charactersInfo, @",$", "");


            //if any characters found, print them to outputBox
            if (charactersInfo.Length > 13)
            {
                PrintMessage(charactersInfo, MessageType.Info);
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
                        price = CalculateTraderPrice(item);
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
        }




        //==============================================HELPER METHODS=============================================

        //method checking if player is in idle state
        private bool IsPlayerIdle()
        {
            //if player isn't in idle state
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                PrintMessage("Przestajesz handlować z: " + Data.Player.InteractsWith!.Name, MessageType.SystemFeedback);
                Data.Player.InteractsWith = new Character();
                Data.Player!.CurrentState = Player.State.Idle;
                return true;
            }
            
            //or if he's in combat state
            else if (Data.Player.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return false;
            }
            return true;
        }

        //method handling adding items to player's inventory
        private void AddItemToPlayer(Item item, int quantity)
        {
            Data.Player!.AddItem(item, quantity);
            PrintMessage("Zdobyłeś " + Convert.ToString(quantity) + " " + item.Name);
        }

        //method handling removing items from player's inventory
        private bool RemoveItemFromPlayer(Item item, int quantity)
        {
            bool isRemoved = false;
            isRemoved = Data.Player!.RemoveItem(item.Name!, quantity);

            if (isRemoved)
            {
                PrintMessage("Straciłeś " + item.Name! + "w ilości " + Convert.ToString(quantity) + " sztuk");
                isRemoved = true;
            }
            else
            {
                PrintMessage("Nie posiadasz wymaganej ilości przedmiotu!", MessageType.SystemFeedback);
            }

            return isRemoved;
        }

        //method handling adding gold to player's pool
        private void AddGoldToPlayer(int gold)
        {
            Data.Player!.Gold += gold;
            PrintMessage("Zyskałeś " + Convert.ToString(gold) + " złota");
        }

        //method handling removing gold from player's pool
        private bool RemoveGoldFromPlayer(int gold)
        {
            if (gold <= Data.Player!.Gold)
            {
                Data.Player!.Gold -= gold;
                PrintMessage("Straciłeś " + Convert.ToString(gold) + " złota");
                return true;
            }
            else
            {
                PrintMessage("Nie stać Cię..", MessageType.SystemFeedback);
                return false;
            }
        }

        /// <summary>
        /// finds location in the direction specified by 'direction' argument and returns true if found, false otherwise
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
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
        private int CalculateTraderPrice(Item item)
        {
            double basePrice = Convert.ToDouble(item.Price);
            int price = Convert.ToInt32(Math.Round(basePrice * Data!.PriceMultiplier));
            return price;
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
                case (MessageType.Info):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.PaleGreen);
                    break;
            }

            Window.outputBox.ScrollToEnd();
        }
    }
}
