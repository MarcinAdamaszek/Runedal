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
using System.Windows.Navigation;
using Microsoft.Win32.SafeHandles;
using System.Windows.Input;
using System.Numerics;

namespace Runedal.GameEngine
{
    public class MainEngine
    {
        public MainEngine(MainWindow window)
        { 
            this.Window = window;
            this.Data = new Data();
            this.Rand = new Random();
            this.AttackInstances = new List<AttackInstance>();

            //set game clock for game time
            GameClock = new DispatcherTimer(DispatcherPriority.Send);
            GameClock.Interval = TimeSpan.FromMilliseconds(100);
            GameClock.Tick += GameClockTick!;

            Data.LoadLocations();
            Data.LoadCharacters();
            Data.LoadItems();
            Data.LoadSpells();
            Data.InitializeEverything();

            GameClock.Start();


            //Data.Locations!.Find(loc => loc.Name == "Karczma").Characters.ForEach(ch =>
            //{
            //    if (ch.GetType() == typeof(Monster))
            //    {
            //        PrintMessage(Convert.ToString((ch as Monster).Id));
            //    }
            //});
            //double szczurAtkSpeed = (Data.Locations!.Find(loc => loc.Name == "Piwnica").Characters.
            //    Find(character => character.Name == "Szczur") as CombatCharacter).GetEffectiveAtkSpeed();
            //double playerAtkSpeed = (Data.Locations!.Find(loc => loc.Name == "Karczma").Characters.
            //    Find(character => character.Name == "Czesiek") as CombatCharacter).GetEffectiveAtkSpeed();

            //PrintMessage("atk speed szczura: " + szczurAtkSpeed);
            //PrintMessage("atk speed gracza: " + playerAtkSpeed);

            //Location karczma = Data.Locations.Find(loc => loc.Name.ToLower() == "karczma");
            //Monster skeleton = karczma.Characters.Find(ch => ch.Name.ToLower() == "dziki_pies") as Monster;
            //AttackInstances.Add(new AttackInstance(Data.Player!, skeleton));
            //AttackInstances.Add(new AttackInstance(skeleton, Data.Player!));
            //Data.Player.Hp -= 500;
            //Data.Player.Mp -= 300;
            //(Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp -= 10;
            //PrintMessage(Convert.ToString((Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp));
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
            EffectOff,
            Speech,
            DealDmg,
            ReceiveDmg,
            CriticalHit,
            Miss,
            Avoid
        }

        public MainWindow Window { get; set; }
        public Data Data { get; set; }
        public DispatcherTimer GameClock { get; set; }
        public Random Rand { get; set; }
        public List<AttackInstance> AttackInstances { get; set; }

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
                case "u":
                case "d":
                    ChangeLocationHandler(command);
                    break;
                case "attack":
                case "a":
                    AttackHandler(argument1);
                    break;
                case "trade":
                    TradeHandler(argument1);
                    break;
                case "talk":
                    TalkHandler(argument1);
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
                    UseHandler(argument1);
                    break;
                case "wear":
                    WearHandler(argument1);
                    break;
                case "takeoff":
                case "off":
                    TakeoffHandler(argument1);
                    break;
                case "drop":
                    DropHandler(argument1, argument2);
                    break;
                case "pickup":
                case "pick":
                case "p":
                    PickupHandler(argument1, argument2);
                    break;
                case "inventory":
                case "i":
                    InventoryHandler(Data.Player!, false);
                    break;
                case "stats":
                    StatsHandler();
                    break;
                case "spells":
                    SpellsHandler();
                    break;
                case "point":
                    PointHandler(argument1);
                    break;
                case "combine":
                    CombineHandler(argument1, argument2);
                    break;
                case "stop":
                    StopHandler();
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "10":
                    OptionHandler(command);
                    break;
                default:
                    PrintMessage("Że co?", MessageType.SystemFeedback);
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

            ResetPlayerState();

            switch (direction)
            {
                case "n":
                    directionString = "na północ";
                    break;
                case "e":
                    directionString = "na wschód";
                    break;
                case "s":
                    directionString = "na południe";
                    break;
                case "w":
                    directionString = "na zachód";
                    break;
                case "u":
                    directionString = "do góry";
                    break;
                case "d":
                    directionString = "w dół";
                    break;
            }

            if (GetNextLocation(direction, out nextLocation))
            {

                //if the passage is open
                if (passage)
                {
                    PrintMessage("Idziesz " + directionString, MessageType.Action);

                    //remove player from previous location
                    Data.Player.CurrentLocation!.Characters!.Remove(Data.Player);

                    //change player's current location
                    AddCharacterToLocation(nextLocation, Data.Player!);
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

            //if command "look" was used without argument, print location description
            if (entityName == string.Empty || entityName == "around")
            {
                ResetPlayerState();
                LocationInfo();
            }
            else
            {

                //search player's remembered spells
                index = Data.Player!.RememberedSpells.FindIndex(spell => spell.Name.ToLower() == entityName);
                if (index != -1)
                {
                    SpellInfo(Data.Player!.RememberedSpells[index]);
                    return;
                }

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
                    ResetPlayerState();
                    CharacterInfo(Data.Player!.CurrentLocation!.Characters[index]);
                    return;
                }
                else
                {
                    //else search player's inventory for item with name matching the argument
                    index = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == entityName.ToLower());
                    if (index != -1)
                    {
                        ItemInfo(Data.Player!.Inventory[index].Name!);
                        return;
                    }
                    else if (Data.Player!.CurrentState == Player.State.Trade)
                    {
                        index = Data.Player!.InteractsWith!.Inventory!.FindIndex(item => item.Name!.ToLower() == entityName.ToLower());
                        if (index != -1)
                        {
                            ItemInfo(Data.Player!.InteractsWith!.Inventory![index].Name!);
                            return;
                        }
                    }
                }

                //search for the item in player worn items
                string wornItemName = "placeholder";

                if (Data.Player!.Weapon!.Name!.ToLower() == entityName.ToLower())
                {
                    wornItemName = Data.Player!.Weapon!.Name;
                }
                else if (Data.Player!.Helmet!.Name!.ToLower() == entityName.ToLower())
                {
                    wornItemName = Data.Player!.Helmet!.Name;
                }
                else if (Data.Player!.Torso!.Name!.ToLower() == entityName.ToLower())
                {
                    wornItemName = Data.Player!.Torso!.Name;
                }
                else if (Data.Player!.Pants!.Name!.ToLower() == entityName.ToLower())
                {
                    wornItemName = Data.Player!.Pants!.Name;
                }
                else if (Data.Player!.Gloves!.Name!.ToLower() == entityName.ToLower())
                {
                    wornItemName = Data.Player!.Gloves!.Name;
                }
                else if (Data.Player!.Shoes!.Name!.ToLower() == entityName.ToLower())
                {
                    wornItemName = Data.Player!.Shoes!.Name;
                }

                if (wornItemName != "placeholder")
                {
                    ItemInfo(wornItemName);
                    return;
                }

                //if if nothing's matched, print appropriate message to user
                PrintMessage("Nie ma tu niczego o nazwie \"" + entityName + "\"", MessageType.SystemFeedback);
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

            ResetPlayerState();

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

        //method handling 'talk' command
        private void TalkHandler(string characterName)
        {

            //if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            int index = Data.Player!.CurrentLocation!.Characters!.FindIndex(
                character => character.Name!.ToLower() == characterName.ToLower());


            ResetPlayerState();

            //if character not found in current location
            if (index == -1)
            {
                PrintMessage("Nie ma tu takiej postaci", MessageType.SystemFeedback);
                return;
            }

            Character talkingCharacter = Data.Player!.CurrentLocation!.Characters[index];

            //if there is no talking option with character
            if (talkingCharacter.Questions!.Length == 0)
            {
                PrintMessage(talkingCharacter.Name + ": " + 
                    talkingCharacter.PassiveResponses![Rand.Next(talkingCharacter.PassiveResponses.Length)], MessageType.Speech);
                return;
            }

            //begin conversation
            Data.Player!.CurrentState = Player.State.Talk;
            Data.Player!.InteractsWith = talkingCharacter;
            PrintMessage("Rozmawiasz z " + talkingCharacter.Name, MessageType.Action);
            PrintMessage(talkingCharacter.Name + ": " + 
                talkingCharacter.PassiveResponses![Rand.Next(talkingCharacter.PassiveResponses.Length)], MessageType.Speech);

            PrintMessage("Wybierz co chcesz powiedzieć:");
            for (int i = 0; i < talkingCharacter.Questions.Length; i++)
            {
                PrintMessage(i + 1 + ". " + talkingCharacter.Questions[i]);
            }
            PrintMessage(Data.Player!.InteractsWith!.Questions.Length + 1 + ": Bywaj");
        }

        //method handling chosen option (1, 2, 3 etc.)
        private void OptionHandler(string option)
        {
            if (Data.Player!.CurrentState != Player.State.Talk)
            {
                PrintMessage("Nie rozmawiasz z nikim", MessageType.SystemFeedback);
                return;
            }

            int optionNumber = Int32.Parse(option);
            
            if (optionNumber > Data.Player!.InteractsWith!.Questions!.Length)
            {
                PrintSpeech(Data.Player!, "Bywaj");
                ResetPlayerState();
                return;
            }

            PrintSpeech(Data.Player!, Data.Player!.InteractsWith!.Questions![optionNumber - 1]);
            PrintSpeech(Data.Player!.InteractsWith, Data.Player!.InteractsWith!.Answers![optionNumber - 1]);

            PrintMessage("Wybierz co chcesz powiedzieć: ");
            for (int i = 0; i < Data.Player!.InteractsWith!.Questions.Length; i++)
            {
                PrintMessage(i + 1 + ". " + Data.Player!.InteractsWith!.Questions[i]);
            }
            PrintMessage(Data.Player!.InteractsWith!.Questions.Length + 1 + ": Bywaj");
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

            //if the player is trading with someone
            if (Data.Player!.CurrentState != Player.State.Trade)
            {
                PrintMessage("Obecnie z nikim nie handlujesz", MessageType.SystemFeedback);
                return;
            }

            Trader trader = trader = (Data.Player!.InteractsWith as Trader)!;
            int itemIndex = -1;
            int itemQuantity = 1;
            int buyingPrice;

            itemIndex = trader.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());

            //check if the item exists in trader's inventory
            if (itemIndex == -1)
            {
                PrintMessage(trader.Name + " nie posiada tego przedmiotu", MessageType.SystemFeedback);
                return;
            }

            //if player typed 'all' as second argument, set quantity to maximum
            if (quantity == "all" || quantity == "a")
            {
                itemQuantity = trader.Inventory[itemIndex].Quantity;
            }

            //else set quantity depending on value parsed from second argument
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //prevent player from buying '0' items
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }
            
            //if player set quantity to more than trader has, set it to
            //all trader has (just buy all)
            if (trader.Inventory[itemIndex].Quantity < itemQuantity)
            {
                itemQuantity = trader.Inventory[itemIndex].Quantity;
            }

            //set buying price depending on quantity
            buyingPrice = CalculateTraderPrice(itemName) * itemQuantity;

            //if total buying price of the item is lesser than amount of gold possesed by player
            if (Data.Player!.Gold! >= buyingPrice)
            {
                //inform player about action
                PrintMessage("Kupujesz " + itemQuantity + " " + itemName, MessageType.Action);

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

        //method handling 'sell' command
        private void SellHandler(string itemName, string quantity)
        {

            //if the player is trading with someone
            if (Data.Player!.CurrentState != Player.State.Trade)
            {
                PrintMessage("Obecnie z nikim nie handlujesz", MessageType.SystemFeedback);
                return;
            }

            Trader trader = trader = (Data.Player!.InteractsWith as Trader)!;
            int itemIndex = -1;
            int itemQuantity = 1;
            int sellingPrice = 0;

            itemIndex = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());

            //check if the item exists in player's inventory
            if (itemIndex == -1)
            {
                PrintMessage("Nie posiadasz wybranego przedmiotu", MessageType.SystemFeedback);
                return;
            }

            //if player typed 'all' as second argument, set quantity to maximum
            if (quantity == "all" || quantity == "a")
            {
                itemQuantity = Data.Player!.Inventory[itemIndex].Quantity;
            }

            //else set quantity depending on value parsed from second argument
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //prevent player from selling 0 items
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }

            //if player set quantity to more than he has, set it to
            //all he has (just sell all)
            if (Data.Player!.Inventory[itemIndex].Quantity < itemQuantity)
            {
                itemQuantity = Data.Player!.Inventory[itemIndex].Quantity;
            }

            //set buying price depending on quantity
            sellingPrice = Data.Player!.Inventory[itemIndex].Price * itemQuantity;

            //if total buying price of the item is lesser than amount of gold possesed by player
            if (trader.Gold >= sellingPrice)
            {
                //inform player about action
                PrintMessage("Sprzedajesz " + itemQuantity + " " + itemName, MessageType.Action);

                //remove item from player's inventory and put it into trader's inventory
                RemoveItemFromPlayer(itemName, itemQuantity);
                AddItemToNpc(trader, itemName, itemQuantity);

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

        //method handling 'use' command
        private void UseHandler(string itemName)
        {
            Item itemToUse;

            ResetPlayerState();

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
            int itemQuantity;
            Item itemToRemove = new Item();

            ResetPlayerState();

            if (itemIndex == -1 && itemName != "złoto")
            {
                PrintMessage("Nie posiadasz przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            if (itemName != "złoto")
            {
                itemToRemove = Data.Player!.Inventory[itemIndex];
            }

            //set item quantity depedning on 2nd argument if it's not empty
            if (quantity == "all" || quantity == "a")
            {

                //depending if it's item or gold
                if (itemName != "złoto")
                {
                    itemQuantity = itemToRemove.Quantity;
                }
                else
                {
                    itemQuantity = Data.Player.Gold;
                }
            }
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //if player typed drop without quantity argument (ConvertQuantityString method
            //assigns 0 to it's out parameter) - set quantity to 1
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }

            //if the item name is 'zloto' drop gold
            if (itemName == "złoto")
            {

                //if player wants to drop more quantity of gold than he has,
                //set quantity to equal to what he possesses
                if (Data.Player!.Gold < itemQuantity)
                {
                    itemQuantity = Data.Player!.Gold;
                }

                PrintMessage("Upuszczasz " + itemQuantity + " złota", MessageType.Action);
                RemoveGoldFromPlayer(itemQuantity);
                AddGoldToLocation(Data.Player.CurrentLocation!, itemQuantity);

                return;
            }

            //if player wants to drop more quantity of items than he actually has
            //set quantity to equal actual item quantity and drop it all
            if (itemToRemove.Quantity < itemQuantity)
            {
                itemQuantity = itemToRemove.Quantity;
            }

            PrintMessage("Upuszczasz " + itemQuantity + " " + itemToRemove.Name, MessageType.Action);
            RemoveItemFromPlayer(itemName, itemQuantity);
            AddItemToLocation(Data.Player!.CurrentLocation!, itemName, itemQuantity);
        }

        //method handling 'pickup' command
        private void PickupHandler(string itemName, string quantity)
        {
            int itemIndex = Data.Player!.CurrentLocation!.Items!.FindIndex(item => item.Name!.ToLower() == itemName);
            int itemQuantity;
            Item itemToPickup = new Item();

            ResetPlayerState();

            if (itemIndex == -1 && itemName != "złoto")
            {
                PrintMessage("Nie ma tu przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            if (itemName != "złoto")
            {
                itemToPickup = Data.Player!.CurrentLocation!.Items[itemIndex];
            }

            //set item quantity depedning on 2nd argument if it's not empty
            if (quantity == "all" || quantity == "a")
            {

                //depending if it's item or gold
                if (itemName != "złoto")
                {
                    itemQuantity = itemToPickup.Quantity;
                }
                else
                {
                    itemQuantity = Data.Player.Gold;
                }
            }
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }            

            //if the item name is 'zloto' pickup gold
            if (itemName == "złoto")
            {
                //if player wants to pick up more quantity of gold than there is in his current
                //location, set quantity to the amount of all gold lying on the ground
                if (Data.Player!.CurrentLocation.Gold < itemQuantity || itemQuantity == 0)
                {
                    itemQuantity = Data.Player!.CurrentLocation.Gold;
                }

                //if there is any gold on the ground..
                if (itemQuantity != 0)
                {
                    PrintMessage("Podnosisz " + itemQuantity + " złota", MessageType.Action);
                    Data.Player!.CurrentLocation.Gold -= itemQuantity;
                    AddGoldToPlayer(itemQuantity);
                }
                else
                {
                    PrintMessage("Nie ma tu żadnego złota", MessageType.SystemFeedback);
                }

                return;
            }

            //if player wants to pick up more quantity of items than there are in his current
            //location, set quantity to equal actual item quantity and pick up them all
            if (itemToPickup.Quantity < itemQuantity)
            {
                itemQuantity = itemToPickup.Quantity;
            }

            //if player typed pickup without quantity argument (ConvertQuantityString method
            //assigns 0 to it's out parameter) - set quantity to 1
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }

            PrintMessage("Podnosisz " + itemQuantity + " " + itemToPickup.Name, MessageType.Action);
            AddItemToPlayer(itemName, itemQuantity);
            Data.Player!.CurrentLocation!.RemoveItem(itemName, itemQuantity);
        }

        //method handling 'wear' command
        private void WearHandler(string itemName)
        {
            int itemIndex = Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == itemName.ToLower());

            if (itemIndex == -1)
            {
                PrintMessage("Nie posiadasz takiego przedmiotu", MessageType.SystemFeedback);
                return;
            }

            Item itemToWear = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;
            
            if (itemToWear.GetType() == typeof(Armor))
            {
                WearArmorOnPlayer(itemName);
            }
            else if (itemToWear.GetType() == typeof(Weapon))
            {
                WearWeaponOnPlayer(itemName);
            }
            else
            {
                PrintMessage("Nie możesz założyć tego przedmiotu", MessageType.SystemFeedback);
                return;
            }

        }

        //method for handling 'takeoff' command
        private void TakeoffHandler(string slotName)
        {
            switch (slotName)
            {
                case "weapon":
                    if (!TakeOffWeaponFromPlayer())
                    {
                        PrintMessage("Nie jesteś uzbrojony", MessageType.SystemFeedback);
                    }
                    break;
                case "helm":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Helmet))
                    {
                        PrintMessage("Nie nosisz żadnego hełmu", MessageType.SystemFeedback);
                    }
                    break;
                case "torso":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Torso))
                    {
                        PrintMessage("Nie nosisz żadnego korpusu", MessageType.SystemFeedback);
                    }
                    break;
                case "pants":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Pants))
                    {
                        PrintMessage("Nie nosisz żadnych spodni (Trochę wstyd..)", MessageType.SystemFeedback);
                    }
                    break;
                case "gloves":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Gloves))
                    {
                        PrintMessage("Nie nosisz żadnych rękawic", MessageType.SystemFeedback);
                    }
                    break;
                case "shoes":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Shoes))
                    {
                        PrintMessage("Nie nosisz żadnych butów (Uważaj po czym stąpasz..)", MessageType.SystemFeedback);
                    }
                    break;
                default:
                    PrintMessage("Nie możesz zdjąć " + slotName + ". Prawidłowe nazwy to: helmet, torso, pants, gloves, shoes", MessageType.SystemFeedback);
                    break;
            }
        }

        //method for stopping actions/states
        private void StopHandler()
        {
            ResetPlayerState();
        }

        //method for handling 'attack' command
        private void AttackHandler(string characterName)
        {

            //if user typed 'attack' without argument
            if (characterName == string.Empty)
            {

                //if there are no opponents currently fighting with player
                if (Data.Player!.CurrentState != CombatCharacter.State.Combat)
                {
                    PrintMessage("Obecnie z nikim nie walczysz", MessageType.SystemFeedback);
                    return;
                }

                //if player is fighting only with single opponent, attack that opponent
                else if (Data.Player.Opponents.Count == 1)
                {
                    AttackCharacter(Data.Player, Data.Player.Opponents[0]);
                }

                //if player if fighting with multiple opponents
                else
                {
                    CombatCharacter weakestOpponent = new CombatCharacter(9999999);
                    bool opponentFound = false;

                    //find the opponent with the lowest level
                    Data.Player!.Opponents!.ForEach(op =>
                    {
                        if (op.Level < weakestOpponent.Level)
                        {
                            weakestOpponent = op;
                            opponentFound = true;
                        }
                    });

                    if (opponentFound)
                    {
                        AttackCharacter(Data.Player, weakestOpponent);
                    }
                }
                return;
            }

            //else if player wanted to attack specific character (typed attack with 1 argument)
            int characterIndex = Data.Player!.CurrentLocation!.Characters!
                .FindIndex(character => character.Name!.ToLower() == characterName.ToLower());

            if (characterIndex == -1)
            {
                PrintMessage("Nie ma tu takiej postaci", MessageType.SystemFeedback);
                return;
            }

            Character characterToAttack = Data.Player!.CurrentLocation!.Characters[characterIndex];

            if (!(characterToAttack is CombatCharacter))
            {
                PrintMessage("Nie możesz zaatakować tej postaci", MessageType.SystemFeedback);
                return;
            }

            AttackCharacter(Data.Player, (characterToAttack as CombatCharacter)!);

        }

        //method for spending attribute pointsd
        private void PointHandler(string attribute)
        {
            Player player = Data.Player!;
            string attributeWord = string.Empty;

            if (player.AttributePoints < 1)
            {
                PrintMessage("Nie masz żadnych punktów atrybutów!", MessageType.SystemFeedback);
                return;
            }

            switch (attribute)
            {
                case "strength":
                case "str":
                    attributeWord = "siła";
                    player.Strength++;
                    break;
                case "agility":
                case "agi":
                    attributeWord = "zręczność";
                    player.Agility++;
                    break;
                case "intelligence":
                case "int":
                    attributeWord = "inteligencja";
                    player.Intelligence++;
                    break;
                default:
                    PrintMessage("\"" + attribute + "\" nie jest poprawną nazwą atrybutu. Poprawne nazwy" +
                        "to: strength, agility, intelligence (lub str, agi, int)", MessageType.SystemFeedback);
                    return;
            }

            player.AttributePoints--;
            PrintMessage("Twoja " + attributeWord + " zwiększa się o 1!");
        }

        //method for crafting spells from runes combinations
        private void CombineHandler(string firstRune, string secondRune)
        {
            bool isCombinationDouble = false;
            string spellName = string.Empty;
            string[] runeNames = new string[5] { "zjarrit", "akull", "verde", "xitan", "dara" };
            Spell craftedSpell = new Spell();
            Spell returnedSpell = new Spell();

            //choose spell depending on single rune choice
            if (secondRune == string.Empty)
            {
                switch (firstRune)
                {
                    case "zjarrit":
                        spellName = "kula_ognia";
                        break;
                    case "akull":
                        spellName = "zamrożenie";
                        break;
                    case "verde":
                        spellName = "zdrewniała_skóra";
                        break;
                    case "xitan":
                        spellName = "pocisk_zagłady";
                        break;
                    case "dara":
                        spellName = "niebiański_dotyk";
                        break;

                        //if the first rune name is incorrect
                    default:
                        PrintMessage("Nie istnieje runa o nazwie \"" + firstRune + "\"", MessageType.SystemFeedback);
                        return;
                }
            }

            //choose spell depending on double runes combination
            else
            {
                isCombinationDouble = true;

                //if the second rune name is incorrect
                if (!runeNames.Contains(secondRune))
                {
                    PrintMessage("Nie istnieje runa o nazwie \"" + secondRune + "\"", MessageType.SystemFeedback);
                    return;
                }

                //zjarrit-akull combination
                if (firstRune == runeNames[0] && secondRune == runeNames[1] || firstRune == runeNames[1] && secondRune == runeNames[0])
                {
                    spellName = "podmuch_pary";
                }
            }

            //check if player possesses required runes
            if (!Data.Player!.Inventory!.Exists(item => item.Name!.ToLower() == firstRune))
            {
                PrintMessage("Nie posiadasz runy " + firstRune, MessageType.SystemFeedback);
                return;
            }
            if (isCombinationDouble)
            {
                if (!Data.Player!.Inventory!.Exists(item => item.Name!.ToLower() == secondRune))
                {
                    PrintMessage("Nie posiadasz runy " + secondRune, MessageType.SystemFeedback);
                    return;
                }
            }

            //craft spell and add it to player's remembered spells
            craftedSpell = Data.Spells!.Find(spell => spell.Name!.ToLower() == spellName)!;

            if (!Data.Player.SpendMana(100))
            {
                PrintMessage("Nie masz wystarczającej ilości many aby to zrobić");
                return;
            }

            PrintMessage("Tworzysz czar " + craftedSpell.Name, MessageType.Action);
            PrintMessage("Czujesz jak nowe zaklęcie wypełnia Twój umysł", MessageType.Action);

            returnedSpell = Data.Player.AddSpell(craftedSpell);
            if (returnedSpell.Name != "placeholder")
            {
                PrintMessage("Zapominasz czar " + returnedSpell.Name, MessageType.Action);
            }

        }

        //method showing player's remembered spells
        private void SpellsHandler()
        {
            SpellsInfo(Data.Player!);
        }




        //==============================================DESCRIPTION METHODS=============================================

        //method describing location to user
        private void LocationInfo()
        {
            Location currentLocation = Data.Player!.CurrentLocation!;
            Location nextLocation = new Location();
            string goldInfo = String.Empty;
            string itemsInfo = "Przedmioty: ";
            string charactersInfo = "Postacie: ";
            string exitsInfo = "Wyjścia: ";
            string[] directionsLetters = { "n", "e", "s", "w", "u", "d" };
            string[] directionsStrings = { " północ,", " wschód,", " południe,", " zachód,", " góra,", " dół," };
            int currentX = currentLocation.X;
            int currentY = currentLocation.Y;

            //print location name and description
            PrintMessage("[ " + currentLocation.Name + " ]");
            PrintMessage(currentLocation.Description!);

            //describe exits for each direction
            for (int i = 0; i < directionsLetters.Length; i++)
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
            currentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() != typeof(Player))
                {
                    charactersInfo += " " + character.Name + ",";
                }
            });

            //add items names for each item present in the location
            currentLocation.Items!.ForEach((item) =>
            {
                itemsInfo += " " + item.Name;
                if (item.Quantity > 1)
                {
                    itemsInfo += "(" + item.Quantity + ")";
                }
                itemsInfo += ",";
            });

            //set the gold description string
            if (currentLocation.Gold > 0)
            {
                goldInfo = "Złoto: " + currentLocation.Gold;
            }

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

            //if there is gold on the ground
            if (goldInfo != String.Empty)
            {
                PrintMessage(goldInfo);
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
            string descriptionRow = "|| Przedmiot:                              | Ilość: | Cena:  ||\n" + delimeter;
            int nameColumnSize = 0;
            int quantityColumnSize = 0;
            int priceColumnSize = 0;
            int price = 0;
            int borderSize = 0;

            //set delimeter, borderSize and descriptionRow depending on withPrice parameter
            if (withPrice)
            {
                delimeter = "||-----------------------------------------------------------||";
                descriptionRow = "|| Przedmiot:                              | Ilość: | Cena:  ||\n" + delimeter;
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
                PrintMessage("Złoto: " + Convert.ToString(Data.Player!.Gold!));
            }

            //separate gold display from worn items display
            PrintMessage(horizontalBorder);

            //prepare worn items description
            if (!withPrice)
            {
                string[] itemSlots = new string[6];
                int i;

                itemSlots[0] = "Broń: " + (character as Player)!.Weapon!.Name!;
                itemSlots[1] = "Hełm: " + (character as Player)!.Helmet!.Name!;
                itemSlots[2] = "Tors: " + (character as Player)!.Torso!.Name!;
                itemSlots[3] = "Spodnie: " + (character as Player)!.Pants!.Name!;
                itemSlots[4] = "Rękawice: " + (character as Player)!.Gloves!.Name!;
                itemSlots[5] = "Buty: " + (character as Player)!.Shoes!.Name!;

                //if item slot was empty (meaning was filled with placeholder)
                //swap 'placeholder' string to 'brak'
                for (i = 0; i < itemSlots.Length; i++)
                {
                    itemSlots[i] = Regex.Replace(itemSlots[i], @"\bplaceholder\b", "brak");
                    PrintMessage(itemSlots[i]);
                }
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
                effect += GetEffectDescription(itemToDescribe.Modifiers!);
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
            else if (itemToDescribe.GetType() == typeof(RuneStone))
            {
                itemType += "Runa";
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
            rows[3] = "||     Następny poziom: " + player.NextLvlExpCap; 
            rows[4] = "||     Pkt. Atrybutów: " + player.AttributePoints;
            rows[5] = "||-----------------------------------";
            rows[6] = "||     Siła: " + attributes[0];
            rows[7] = "||     Zręczność: " + attributes[1];
            rows[8] = "||     Inteligencja: " + attributes[2]; ;
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
            rows[10] += "Regeneracja MP: " + Math.Floor(player.GetEffectiveMpRegen());

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

        //method printing info about player's remembered spells
        private void SpellsInfo(CombatCharacter character)
        {
            int i, j;
            int remainingSpace;
            int tableWidth = 53;
            int numberOfRows = character.RememberedSpells!.Count + 2;
            string[] tableRows = new string[numberOfRows];

            tableRows[0] = "******************* TWOJE CZARY *********************";

            //bottom border of the table
            for (i = 0; i < tableWidth; i++)
            {
                tableRows[numberOfRows - 1] += "*";
            }

            //fill interior of the table with character's spells
            for (i = 1; i < numberOfRows - 1; i++)
            {
                tableRows[i] += "||   " + i + ". " + character.RememberedSpells[i - 1].Name;

                //fill remaining space in every row with white space characters
                remainingSpace = tableWidth - tableRows[i].Length - 2;
                for (j = 0; j < remainingSpace; j++)
                {
                    tableRows[i] += " ";
                }

                tableRows[i] += "||";
            }

            for (i = 0; i < numberOfRows; i++)
            {
                PrintMessage(tableRows[i]);
            }

        }

        //method printing detailed info about single spell
        private void SpellInfo(Spell spell)
        {
            string name = spell.Name!;
            string description = spell.Description!;
            string defaultTarget = "Cel domyślny: ";
            string manaCost = "Koszt many: " + spell.ManaCost;
            string dmgDealt = "Obrażenia: " + spell.Damage;
            string effect = "Działanie: ";

            //assign proper default target name
            if (spell.DefaultTarget == Spell.Target.Self)
            {
                defaultTarget += "Rzucający";
            }
            else
            {
                defaultTarget += "Przeciwnik";
            }

            effect += GetEffectDescription(spell.Modifiers!);

            //display info
            PrintMessage("[ " + name + " ]");
            PrintMessage(description);
            PrintMessage(defaultTarget);
            PrintMessage(manaCost);
            PrintMessage(dmgDealt);
            PrintMessage(effect);
        }






        //==============================================HELPER METHODS=============================================

        //method returning formatted string representing effect description (it's modifiers and duration)
        private string GetEffectDescription(List<Modifier> modifiers)
        {
            string effect = string.Empty;

            if (modifiers.Count > 0)
            {
                modifiers.ForEach(mod =>
                {
                    effect += GetModDescription(mod) + ", ";
                });

                //remove trailing comma and add duration
                effect = Regex.Replace(effect, @",\s$", "");
                effect += " {" + modifiers[0].Duration + " sekund}";
            }
            else
            {
                effect += "brak";
            }

            return effect;
        }

        //method returning formatted string representing modifier and it's value with sign
        private string GetModDescription(Modifier modifier)
        {
            string description = string.Empty;
            string modType = GetPolishModType(modifier.Type);
            string valueSign = string.Empty;
            string percentSign = string.Empty;

            //add percent sign if the modifier is percentage
            if (modifier.IsPercentage)
            {
                percentSign = "_%";
            }

            //set sign of modifier to + if its positive number (for negative, minus sign is displayed automatically)
            if (modifier.Value > 0)
            {
                valueSign = "+";
            }

            description = modType + "(" + valueSign + modifier.Value + percentSign + ")";
            return description;
        }

        //method returning polish string representing specified type of ArmorType type
        private string GetPolishArmorType(Armor.ArmorType type)
        {
            string armorType = string.Empty;
            switch (type)
            {
                case Armor.ArmorType.Torso:
                    armorType = "Korpus";
                    break;
                case Armor.ArmorType.Pants:
                    armorType = "Spodnie";
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
                    modType = "Szybkość ataku";
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
            int currentZ = nextLocation.Z;
            int locationIndex = -1;
            bool isFound = false;

            switch (direction)
            {
                case "n":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Y == currentY + 1 && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "e":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX + 1 && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "s":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Y == currentY - 1 && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "w":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX - 1 && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "u":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Z == currentZ + 1 && loc.X == currentX && loc.Y == currentY);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "d":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Z == currentZ - 1 && loc.X == currentX && loc.Y == currentY);
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

        //method converting quantity string to number and returning true
        //if conversion succeded and value is > 0  (returns false otherwise)
        private bool ConvertQuantityString(string quantityString, out int quantityValue)
        {
            int parsedQuantity = 0;

            if (quantityString != string.Empty)
            {
                if (!int.TryParse(quantityString, out parsedQuantity))
                {
                    quantityValue = parsedQuantity;
                    return false;
                }
                if (parsedQuantity <= 0)
                {
                    quantityValue = parsedQuantity;
                    return false;
                }
            }

            quantityValue = parsedQuantity;
            return true;
        }

        //method printing characters line in form of speech
        private void PrintSpeech(Character character, string line)
        {
            string characterLine = character.Name + ": " + line;
            PrintMessage(characterLine, MessageType.Speech);
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
                case (MessageType.Speech):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkKhaki);
                    break;
                case (MessageType.DealDmg):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Lime);
                    break;
                case (MessageType.ReceiveDmg):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Crimson);
                    break;
                case (MessageType.CriticalHit):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Magenta);
                    break;
                case (MessageType.Miss):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.FloralWhite);
                    break;
                case (MessageType.Avoid):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.BurlyWood);
                    break;
            }

            Window.outputBox.ScrollToEnd();
        }

        /// <summary>
        /// /// method taking chance parameter (as double 1-999 value, indicating promile
        /// number) returns true if succeded and false if not
        /// </summary>
        /// <param name="chance"></param>
        /// <returns></returns>
        private bool TryOutChance(double chance)
        {
            double randShot = Rand.NextDouble();
            if (chance < randShot)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// /// method determining if attack reached the target on basis of two 
        /// parameters: accuracy and evasion. If attack is a success - returns true
        /// if missed - returns false
        /// </summary>
        /// <param name="accuracy"></param>
        /// <param name="evasion"></param>
        /// <returns></returns>
        private bool IsAttackHit(double accuracy, double evasion)
        {
            double hitChance = Rand.NextDouble() * accuracy;
            double missChance = Rand.NextDouble() * evasion;

            if (hitChance > missChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsHitCritical(double critical)
        {
            double chance = Math.Sqrt(critical) / 100;
            bool isCritical = TryOutChance(chance);
            return isCritical;
        }

        //method calculating dmg from attack and defense values
        private double CalculateDmg(double attack, double defense)
        {
            double reductionMultiplier = 1 / (Math.Sqrt(defense) / 10);
            double dmg = attack / 5 * reductionMultiplier;
            return dmg;
        }

        //method randomizing dmg
        private double RandomizeDmg(double staticDmg)
        {
            double randomDmgMultiplier = Rand.Next(70, 131) * 0.01;
            double randomizedDmg = staticDmg * randomDmgMultiplier;
            return randomizedDmg;
        }







        //==============================================MANIPULATION METHODS=============================================

        //method for attacking character by another character
        private void AttackCharacter(CombatCharacter attacker, CombatCharacter attacked)
        {
            //make sure to remove previous attack instance, so attacker doesn't attack
            //2 (or more) characters simultaneously
            int instanceIndex = AttackInstances.FindIndex(ins => ins.Attacker == attacker);
            if (instanceIndex != -1)
            {
                AttackInstances.RemoveAt(instanceIndex);
            }
            AttackInstances.Add(new AttackInstance(attacker, attacked));

            //print npcs aggressive response when it attacks
            if (attacker != Data.Player)
            {
                int randomIndex = Rand.Next(0, attacker.AggressiveResponses!.Length);
                PrintSpeech(attacker, attacker.AggressiveResponses[randomIndex]);
            }

            //print appropriate message depending on player's position in attacker/attacked configuration
            if (attacker == Data.Player)
            {
                PrintMessage("Atakujesz postać: " + attacked.Name + "!", MessageType.Action);
            }
            else if (attacked == Data.Player)
            {
                PrintMessage("Zostałeś zaatakowany przez: " + attacker.Name + "!", MessageType.Action);
            }
            else if (attacked.CurrentLocation! == Data.Player!.CurrentLocation)
            {
                PrintMessage(attacker.Name! + " atakuje: " + attacked.Name);
            }
            
            //if attacked character doesn't exist in attacker's opponents list
            if (!(attacker.Opponents.Exists(op => op == attacked)))
            {
                attacker.InteractsWith = attacked;
                attacker.AddOpponent(attacked);
                attacked.AddOpponent(attacker);
            }
        }

        //method killing non-player combat character
        private void KillCharacter(CombatCharacter character)
        {
            int i;

            //erase character from it's current location
            character.CurrentLocation!.RemoveCharacter(character);

            //remove all attack instances related to dying character
            List<AttackInstance> instancesToRemove = new List<AttackInstance>();
            for (i = 0; i < AttackInstances.Count; i++)
            {
                if (AttackInstances[i].Attacker == character || AttackInstances[i].Receiver == character)
                {
                    instancesToRemove.Add(AttackInstances[i]);
                }
            }
            instancesToRemove.ForEach(ins =>
            {
                AttackInstances.Remove(ins);
            });

            //remove all modifiers from character
            List<Modifier> modsToRemove = new List<Modifier>();
            for (i = 0; i < character.Modifiers!.Count; i++)
            {
                modsToRemove.Add(character.Modifiers[i]);
            }
            modsToRemove.ForEach(mod => character.RemoveModifier(mod));

            //if it's player dying
            if (character == Data.Player!)
            {
                PrintMessage("Nogi odmawiają Ci posłuszeństwa, wzrok traci ostrość a dźwięki dochodzą jakby z oddali. " +
                "Upadasz na kolana, a potem na twarz. Czujesz, że to koniec i powoli odpływasz w nicość.. umierasz.", MessageType.Action);

                //remove all effects from player
                List<EffectOnPlayer> effectsToRemove = new List<EffectOnPlayer>();
                for (i = 0; i < (character as Player)!.Effects!.Count; i++)
                {
                    effectsToRemove.Add((character as Player)!.Effects![i]);
                }
                effectsToRemove.ForEach(eff => RemoveEffect(eff));

                //respawn player
                PrintMessage("Odradzasz się..", MessageType.Action);
                AddCharacterToLocation(Data.Locations!.Find(loc => loc.Name == "Karczma")!, Data.Player!);
                Data.Player!.Hp = Data.Player.MaxHp * 0.4;
                Data.Player!.Mp = 0;
            }

            //else if it's npc dying
            else
            {

                //if it's dying in location occupied by the player
                if (character.CurrentLocation == Data.Player!.CurrentLocation)
                {
                    PrintMessage(character.Name + " ginie");
                }

                //drop character's items
                character.Inventory!.ForEach(item =>
                {
                    AddItemToLocation(character.CurrentLocation!, item.Name!, item.Quantity);
                });
                AddGoldToLocation(character.CurrentLocation!, character.Gold);
            }
        }

        /// <summary>
        /// method dealing dmg to combat-character. Returns true if the dmg is lethal,
        /// otherwise - returns false;
        /// </summary>
        /// <param name="dealer"></param>
        /// <param name="receiver"></param>
        /// <param name="dmg"></param>
        /// <returns></returns>
        private bool DealDmgToCharacter(CombatCharacter dealer, CombatCharacter receiver, int dmg)
        {
            bool isDealerPlayer = dealer.GetType() == typeof(Player);
            bool isReceiverPlayer = receiver.GetType() == typeof(Player);
            bool isDmgLethal = receiver.DealDamage(dmg);


            if (isDmgLethal)
            {
                KillCharacter(receiver);

                //end combat, remove opponents etc.
                if (dealer.Opponents.Exists(opponent => opponent == receiver))
                {
                    dealer.RemoveOpponent(receiver);
                }
                if (receiver.Opponents.Exists(opponent => opponent == dealer))
                {
                    receiver.RemoveOpponent(dealer);
                }
                dealer.InteractsWith = new Character();
                receiver.InteractsWith = new Character();

                if (isDealerPlayer)
                {
                    GivePlayerExperience(receiver.Level);
                }
            }

            return isDmgLethal;
        }

        //method adding certain amount to player's experience pool
        private void GivePlayerExperience(int lvl)
        {
            ulong experience = Convert.ToUInt64(lvl * 100);
            int previousLevel = Data.Player!.Level;

            PrintMessage("Zdobywasz " + experience + " doświadczenia", MessageType.Action);
            if (Data.Player!.GainExperience(experience))
            {
                PrintMessage("***Zdobywasz nowy poziom!***", MessageType.Action);
                Data.Player!.AddAttributePoints((Data.Player!.Level - previousLevel) * 3);
            }
        }

        //method putting character into location
        private void AddCharacterToLocation(Location location, Character character)
        {
            location.AddCharacter(character);
            character.CurrentLocation = location;

            if (character.GetType() == typeof(Player))
            {
                LocationInfo();

                location.Characters!.ForEach(ch =>
                {
                    if (ch.GetType() == typeof(Monster))
                    {
                        if ((ch as Monster)!.isAggressive)
                        {
                            AttackCharacter((ch as CombatCharacter)!, (character as CombatCharacter)!);
                        }
                    }
                });
            }
            else if (Data.Player!.CurrentLocation == location)
            {
                PrintMessage("W lokacji pojawia się postać: " + character.Name);
            }
        }

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

        //method for wearing a weapon-type items by player
        private void WearWeaponOnPlayer(string itemName)
        {
            Weapon weaponToWear = (Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower()) as Weapon)!;

            TakeOffWeaponFromPlayer();

            PrintMessage("Uzbrajasz się w " + weaponToWear.Name, MessageType.Action);
            Data.Player!.WearWeapon(weaponToWear);
        }

        //method for wearing armor type items by player
        private void WearArmorOnPlayer(string itemName)
        {
            Armor armorToWear = (Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower()) as Armor)!;
            Armor.ArmorType armorType = armorToWear.Type;
            string wornArmorName = string.Empty;

            switch (armorType)
            {
                case Armor.ArmorType.Helmet:
                    wornArmorName = Data.Player!.Helmet!.Name!;
                    break;
                case Armor.ArmorType.Torso:
                    wornArmorName = Data.Player!.Torso!.Name!;
                    break;
                case Armor.ArmorType.Pants:
                    wornArmorName = Data.Player!.Pants!.Name!;
                    break;
                case Armor.ArmorType.Gloves:
                    wornArmorName = Data.Player!.Gloves!.Name!;
                    break;
                case Armor.ArmorType.Shoes:
                    wornArmorName = Data.Player!.Shoes!.Name!;
                    break;
            }

            //if there is something in the slot already, take it off
            if (wornArmorName != "placeholder")
            {
                TakeOffArmorFromPlayer(armorType);
            }

            PrintMessage("Zakładasz " + armorToWear.Name, MessageType.Action);
            Data.Player!.WearArmor(armorToWear);
        }

        /// <summary>
        /// Takes off weapon from player's weapon slot.
        /// Returns true if weapon is taken off, false if slot is empty
        /// </summary>
        /// <returns></returns>
        private bool TakeOffWeaponFromPlayer()
        {
            string weaponName = Data.Player!.TakeOffWeapon();
            if (weaponName != "placeholder")
            {
                PrintMessage("Odkładasz " + weaponName, MessageType.Action);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Takes off armor from player's armor slot.
        /// Returns true if armor is taken off, false if slot is empty
        /// </summary>
        /// <returns></returns>
        private bool TakeOffArmorFromPlayer(Armor.ArmorType type)
        {
            string wornArmorName = Data.Player!.TakeOffArmor(type);
            if (wornArmorName != "placeholder")
            {
                PrintMessage("Zdejmujesz " + wornArmorName, MessageType.Action);
                return true;
            }
            return false;
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
            PrintMessage(item.UseActivityName! + " " + item.Name!, MessageType.Action);
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
                        mod.Parent = objectName;

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
                    if (player.Modifiers!.Exists(mod => mod.Parent!.ToLower() == objectName.ToLower()))
                    {
                        player.Modifiers.ForEach(mod =>
                        {
                            if (mod.Parent == objectName)
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

        //method removing effects from player
        private void RemoveEffect(EffectOnPlayer effect)
        {
            PrintMessage("Skończyło się działanie " + effect.Description, MessageType.EffectOff);
            Data.Player!.Effects!.Remove(effect);
        }

        //method breaking trade state and printing proper message
        private void BreakTradeState()

        {
             PrintMessage("Przestajesz handlować z: " + Data.Player!.InteractsWith!.Name, MessageType.Action);
             Data.Player.InteractsWith = new Character();
             Data.Player!.CurrentState = Player.State.Idle;
        }

        //method breaking talk state and printing proper message
        private void BreakTalkState()
        {
            PrintMessage("Przestajesz rozmawiać z: " + Data.Player!.InteractsWith!.Name, MessageType.Action);
            Data.Player.InteractsWith = new Character();
            Data.Player!.CurrentState = Player.State.Idle;
        }

        //method checking if player is trading/talking and breaking the state if so
        private void ResetPlayerState()
        {
            //check if player is trading with someone already
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                BreakTradeState();
            }
            else if (Data.Player!.CurrentState == Player.State.Talk)
            {
                BreakTalkState();
            }
        }

        //method adding items to non-player characters
        private void AddItemToNpc(Character character, string itemName, int quantity)
        {
            Item itemToAdd = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;
            character.AddItem(itemToAdd, quantity);
        }

        //method adding items to location
        private void AddItemToLocation(Location location, string itemName, int quantity)
        {
            Item itemToAdd = Data.Items!.Find(item => item.Name!.ToLower() == itemName.ToLower())!;
            location.AddItem(itemToAdd, quantity);
            if (location == Data.Player!.CurrentLocation)
            {
                PrintMessage("W lokacji pojawia się przedmiot: " + itemToAdd.Name + "(" + quantity + ")");
            }
        }
        
        //method adding gold to location
        private void AddGoldToLocation(Location location, int amount)
        {
            location.Gold += amount;
            
            if (location == Data.Player!.CurrentLocation)
            {
                PrintMessage("W lokacji pojawia się złoto!");
            }
        }





        //==============================================EVENT HANDLERS=============================================

        //handler for tick event of GameClock
        private void GameClockTick(object sender, EventArgs e)
        {
            CharactersTick();
            PlayerEffectsTick();
            AttacksTick();
        }

        //method launching HandleTick method for every character in game
        private void CharactersTick()
        {
            //handle all characters regeneration/duration-decrease of modifiers etc
            Data.Locations!.ForEach(loc =>
            {
                loc.Characters!.ForEach(character =>
                {
                    if (character is CombatCharacter)
                    {
                        (character as CombatCharacter)!.HandleTick();
                    }
                });
            });
            
        }

        //method handling player effects tick
        private void PlayerEffectsTick()
        {
            List<EffectOnPlayer> playerEffects;
            List<EffectOnPlayer> effectsToRemove = new List<EffectOnPlayer>();
            playerEffects = Data.Player!.Effects!;

            //handle duration-decrease and wearing off of effects affecting player
            for (int i = 0; i < playerEffects.Count; i++)
            {

                //if duration is greater than 1 - decrement it.
                //otherwise, if it equals 1 - effect has ended so remove it
                if (playerEffects[i].DurationInTicks > 1)
                {
                    playerEffects[i].DurationInTicks--;
                }
                else if (playerEffects[i].DurationInTicks == 1)
                {
                    effectsToRemove.Add(playerEffects[i]);
                }
            }
            effectsToRemove.ForEach(eff =>
            {
                RemoveEffect(eff);
            });
        }

        //method handling attacks for every attack instance
        private void AttacksTick()
        {
            bool isAttackerPlayer;
            bool isReceiverPlayer;
            bool isDmgLethal = false;
            int i;
            double staticDmg;
            double rawDmg;
            double dealtDmg;
            int dmgAsInt;
            
            CombatCharacter attacker = new CombatCharacter();
            CombatCharacter receiver = new CombatCharacter();

            for (i = 0; i < AttackInstances.Count; i++)
            {
                attacker = AttackInstances[i].Attacker;
                receiver = AttackInstances[i].Receiver;

                //skip attack if attackers attack is on cooldown
                if (attacker.ActionCounter > 0)
                {
                    continue;
                }

                isAttackerPlayer = attacker.GetType() == typeof(Player);
                isReceiverPlayer = receiver.GetType() == typeof(Player);

                attacker.PerformAttack();

                //try if attack actually hits or misses
                if (!IsAttackHit(attacker.GetEffectiveAccuracy(), receiver.GetEffectiveEvasion()))
                {
                    //display appropriate message if missed
                    if (isAttackerPlayer)
                    {
                        PrintMessage("Chybiłeś!", MessageType.Miss);
                    }
                    if (isReceiverPlayer)
                    {
                        PrintMessage("Unikasz ataku " + attacker.Name, MessageType.Miss);
                    }
                }
                else
                {

                    staticDmg = CalculateDmg(attacker.GetEffectiveAttack(), receiver.GetEffectiveDefense());
                    rawDmg = RandomizeDmg(staticDmg);

                    if (IsHitCritical(attacker.GetEffectiveCritical()))
                    {
                        if (isAttackerPlayer)
                        {
                            PrintMessage("Trafienie krytyczne!", MessageType.CriticalHit);
                        }
                        dealtDmg = rawDmg * 4;
                    }
                    else
                    {
                        dealtDmg = rawDmg;
                    }

                    dmgAsInt = Convert.ToInt32(dealtDmg);

                    //print appropriate messages to user about dmg dealing
                    if (isAttackerPlayer)
                    {
                        PrintMessage("Zadajesz " + dmgAsInt + " obrażeń", MessageType.DealDmg);
                    }
                    else if (isReceiverPlayer)
                    {
                        PrintMessage(attacker.Name! + " zadaje Ci " + dmgAsInt + " obrażeń", MessageType.ReceiveDmg);
                    }

                    isDmgLethal = DealDmgToCharacter(attacker, receiver, dmgAsInt);
                }

                //if receiver is an npc character - respond with counterattack
                if (receiver != Data.Player && !isDmgLethal)
                {

                    //check if attacked isn't already attacking the attacker
                    if (!AttackInstances.Exists(ins => ins.Attacker == receiver && ins.Receiver == attacker))
                    {
                        AttackCharacter(receiver, attacker);
                    }
                }
            }

            
        }
    }
}
