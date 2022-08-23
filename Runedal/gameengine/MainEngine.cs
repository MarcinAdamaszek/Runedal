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
            GameClock.Tick += GameClock_Tick!;

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
            Info,
            SystemFeedback,
            Gain,
            Loss
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
                case "inventory":
                case "i":
                    InventoryHandler(Data.Player!, false);
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
            if (IsPlayerInCombat())
            {
                return;
            }

            //if player is in trade state
            IsPlayerInTrade();

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

            ////if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            if (entityName == string.Empty || entityName == "around")
            {
                //if player is in trade state, break the state and print proper message
                IsPlayerInTrade();

                //if command "look" was used without argument, print location description
                LocationInfo();
            }
            else
            {
                //else search characters of current location and player's inventory for entity with name matching the argument
                index = Data.Player!.CurrentLocation!.Characters!.FindIndex(character => character.Name!.ToLower() == entityName);
                if (index != -1)
                {

                    //if player is in trade state, break the state and print proper message
                    IsPlayerInTrade();
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

            ////if player isn't in idle state
            if (IsPlayerInCombat())
            {
                return;
            }

            //check if player is trading with someone already
            IsPlayerInTrade();

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

                PrintMessage("[ " + tradingCharacter.Name + " ]", MessageType.Info);
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
            InventoryInfo(player, withPrice);
        }

        //method handling 'buy' command
        private void BuyHandler(string itemName, string quantity)
        {

            //if the player is trading with someone
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                Trader trader = trader = (Data.Player!.InteractsWith as Trader)!;
                Item itemPrototype;
                int itemIndex = -1;
                int itemQuantity = 1;
                int buyingPrice;

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

                itemIndex = trader.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());

                //check if the item exists in trader's inventory and if trader has enough of it
                if (itemIndex == -1)
                {
                    PrintMessage(trader.Name + " nie posiada wybranego przedmiotu", MessageType.SystemFeedback);
                    return;
                }
                else if (trader.Inventory[itemIndex].Quantity < itemQuantity)
                {
                    PrintMessage(trader.Name + " nie posiada wybranego przedmiotu w tej ilości", MessageType.SystemFeedback);
                    return;
                }

                //set boughtItem variable to proper item in Items list in Data
                itemPrototype = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;

                //set buying price depending on quantity
                buyingPrice = CalculateTraderPrice(itemPrototype) * itemQuantity;

                //if total buying price of the item is lesser than amount of gold possesed by player
                if (Data.Player!.Gold! >= buyingPrice)
                {
                    //remove item from traders inventory and gold from player's inventory
                    trader.RemoveItem(itemName, itemQuantity);
                    RemoveGoldFromPlayer(buyingPrice);

                    //add item to player's inventory 
                    AddItemToPlayer(itemPrototype, itemQuantity);

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
                Item itemPrototype;
                int itemIndex = -1;
                int itemQuantity = 1;
                int sellingPrice = 0;

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
                        PrintMessage("Nie możesz sprzedać Madiego (chyba że studentom ;E)", MessageType.SystemFeedback);
                        return;
                    }
                }

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

                //set boughtItem variable to proper item in Items list in Data
                itemPrototype = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;

                //set buying price depending on quantity
                sellingPrice = itemPrototype.Price * itemQuantity;

                //if total buying price of the item is lesser than amount of gold possesed by player
                if (trader.Gold >= sellingPrice)
                {
                    //remove item from player's inventory and put it into trader's inventory
                    RemoveItemFromPlayer(itemPrototype, itemQuantity);
                    trader.AddItem(itemPrototype, itemQuantity);

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
            int itemIndex = -1;
            Item itemToUse = new Item();

            //if 'use' was typed without any argument
            if (itemName == string.Empty)
            {
                PrintMessage("Co chcesz użyć?", MessageType.SystemFeedback);
                return;
            }

            itemIndex = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());
            if (itemIndex != -1)
            {
                itemToUse = Data.Player!.Inventory[itemIndex];
            }
            else
            {
                PrintMessage("Nie posiadasz przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            if (itemToUse.GetType() == typeof(Consumable))
            {
                UseConsumable((itemToUse as Consumable)!);
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

            //print player's gold pool
            if (character.GetType() == typeof(Player))
            {
                PrintMessage("Twoje złoto: " + Convert.ToString(Data.Player!.Gold!), MessageType.Info);
            }
        }

        //method describing character
        private void CharacterInfo(Character character)
        {
            PrintMessage("[ " + character.Name + " ]", MessageType.Info);
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
                        effect += GetModDescription(mod);
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
            PrintMessage("[ " + itemToDescribe.Name + " ]", MessageType.Info);
            PrintMessage(itemToDescribe.Description!);
            PrintMessage(weight, MessageType.Info);
            PrintMessage(itemType, MessageType.Info);
            if (effect != string.Empty)
            {
                PrintMessage(effect, MessageType.Info);
            }
            if (defense != string.Empty)
            {
                PrintMessage(defense, MessageType.Info);
            }
            if (attack != string.Empty)
            {
                PrintMessage(attack, MessageType.Info);
            }
            if (range != string.Empty)
            {
                PrintMessage(range, MessageType.Info);
            }
            if (modifiers != string.Empty)
            {
                PrintMessage("Modyfikatory: " + modifiers, MessageType.Info);
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

        //method checking if player is in combat state, and printing proper message if so
        private bool IsPlayerInCombat()
        {
            if (Data.Player!.CurrentState! == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return true;
            }
            else
            {
                return false;
            }
        }

        //method breaking trade state and printing proper message
        private bool IsPlayerInTrade()

        {
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                PrintMessage("Przestajesz handlować z: " + Data.Player.InteractsWith!.Name, MessageType.SystemFeedback);
                Data.Player.InteractsWith = new Character();
                Data.Player!.CurrentState = Player.State.Idle;
                return true;
            }
            else
            {
                return false;
            }
        }






        //==============================================PLAYER MANIPULATION METHODS=============================================

        //method handling adding items to player's inventory
        private void AddItemToPlayer(Item item, int quantity)
        {
            Data.Player!.AddItem(item, quantity);
            PrintMessage("Zdobyłeś " + item.Name + " w ilości " + Convert.ToString(quantity) + " sztuk", MessageType.Gain);
        }

        //method handling removing items from player's inventory
        private void RemoveItemFromPlayer(Item item, int quantity = 1)
        {
            if (Data.Player!.RemoveItem(item.Name!, quantity))
            {
                PrintMessage("Straciłeś " + item.Name! + "w ilości " + Convert.ToString(quantity) + " sztuk", MessageType.Loss);
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
            Player player = Data.Player!;
            int itemIndex = -1;

            itemIndex = player.Inventory!.IndexOf(item);

            if (itemIndex != -1)
            {
                RemoveItemFromPlayer(item);

                player.Inventory[itemIndex].Modifiers!.ForEach(mod =>
                {
                    
                    //apply only temporary modifiers
                    if (mod.Duration != 0)
                    {
                        player.AddModifier(mod);
                    }
                });

                ApplyEffect(item);
            }
        }


        //method applying consumable items effects to player
        private void ApplyEffect(Consumable item)
        {
            Effect itemEffect;
            string description = item.Name! + ":";
            int durationInTicks = 0;

            if (item.Modifiers!.Count > 0)
            {
                durationInTicks = item.Modifiers[0].DurationInTicks;
            }
            //add modifiers descriptors in form of 'modifier(+/-[value])' to
            //description string for each modifier the item has
            item.Modifiers!.ForEach(mod =>
            {
                description += " " + GetModDescription(mod) + ",";
            });

            itemEffect = new Effect(description, durationInTicks);

            PrintMessage("Czujesz efekt działania " + description, MessageType.SystemFeedback);
            Data.Player!.Effects!.Add(itemEffect);
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
                case (MessageType.Gain):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                    break;
                case (MessageType.Loss):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Tan);
                    break;
            }

            Window.outputBox.ScrollToEnd();
        }




        //==============================================EVENT HANDLERS=============================================

        //handler for tick event of GameClock
        private void GameClock_Tick(object sender, EventArgs e)
        {
            Data.Characters!.ForEach(character =>
            {
                if (character is CombatCharacter)
                {
                    (character as CombatCharacter)!.HandleTick();
                }
            });

            //PrintMessage(Convert.ToString((Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp));
        }
    }
}
