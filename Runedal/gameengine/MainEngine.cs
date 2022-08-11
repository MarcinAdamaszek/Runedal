﻿using System;
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
        }

        //enum type for type of message displayed in PrintMessage method for displaying messages in different colors
        enum MessageType
        {
            Default,
            UserCommand,
            Characters
        }

        public MainWindow Window { get; set; }
        public Data Data { get; set; }


        //method processing user input commands
        public void ProcessCommand()
        {
            string userCommand = string.Empty;
            string command = string.Empty;
            string argument1 = string.Empty;

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

            //split user input into command and it's argument
            command = Regex.Replace(userCommand, @"\s.+", "");
            argument1 = Regex.Replace(userCommand, @"^.+\s", "");

            //clear argument1 if there was none
            if (argument1 == command)
            {
                argument1 = string.Empty;
            }
            //match user input to proper engine command
            switch (command)
            {
                case "n":
                case "e":
                case "s":
                case "w":
                    ChangeLocation(command);
                    break;
                case "look":
                case "l":
                    DescribeEntity(argument1);
                    break;
                default:
                    PrintMessage("Że co?");
                    return;
            }
        }

        //methods taking actions depending on user input command
        private void ChangeLocation(string direction)
        {
            string directionString = string.Empty;
            bool passage = Data.Player!.CurrentLocation!.GetPassage(direction);
            Location nextLocation = new Location();

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
                    PrintMessage("Idziesz na " + directionString);

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

        //method displaying communicates in outputBox of the gui
        private void PrintMessage(string msg, MessageType type = MessageType.Default)
        {
            //color text of the message before displaying
            TextRange tr = new(this.Window.outputBox.Document.ContentEnd, this.Window.outputBox.Document.ContentEnd);
            tr.Text = "\n" + msg;

            switch (type) {
                case (MessageType.Default):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightGray);
                    break;
                case (MessageType.UserCommand):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Aqua);
                    break;
                case (MessageType.Characters):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightGreen);
                    break;
            }

            Window.outputBox.ScrollToEnd();
        }

        //method describing location to user
        private void LocationInfo()
        {
            Location nextLocation = new Location();
            string tradersInfo = "Handlarze: ";
            string heroesInfo = "Postacie: ";
            string monstersInfo = "Istoty: ";
            string exitsInfo = "Wyjścia:";
            string[] directionsLetters = { "n", "e", "s", "w" };
            string[] directionsStrings = { "\nPółnoc: ", "\nWschód: ", "\nPołudnie: ", "\nZachód: " };
            int currentX = Data.Player!.CurrentLocation!.X;
            int currentY = Data.Player!.CurrentLocation!.Y;

            //print location description
            //PrintMessage(delimeter);
            PrintMessage(Data.Player!.CurrentLocation!.Description!);

            //describe exits for each direction
            for (int i = 0; i < 4; i++)
            {

                //if the location exists
                if (GetNextLocation(directionsLetters[i], out nextLocation))
                {

                    //if the passage in specific direction is open add proper strings to exitsInfo
                    if (Data.Player!.CurrentLocation.GetPassage(directionsLetters[i]))
                    {
                        exitsInfo += directionsStrings[i] + nextLocation.Name;
                    }
                    else
                    {
                        exitsInfo += directionsStrings[i] + "Przejście zamknięte";
                    }
                }
            }

            //PrintMessage(delimeter);
            PrintMessage(exitsInfo);

            //add character names to their info strings for each character of specific type present in player's current location
            Data.Player.CurrentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() == typeof(Trader))
                {
                    tradersInfo += " " + character.Name + ",";
                }
            });
            Data.Player.CurrentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() == typeof(Hero))
                {
                    heroesInfo += " " + character.Name + ",";
                }
            });
            Data.Player.CurrentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() == typeof(Monster))
                {
                    monstersInfo += " " + character.Name + ",";
                }
            });

            //remove the last comma
            tradersInfo = Regex.Replace(tradersInfo, @",$", "");
            heroesInfo = Regex.Replace(heroesInfo, @",$", "");
            monstersInfo = Regex.Replace(monstersInfo, @",$", "");

            //PrintMessage(delimeter);
            PrintMessage(tradersInfo);
            PrintMessage(heroesInfo);
            PrintMessage(monstersInfo);
        }

        //method describing game entities to user (look command)
        private void DescribeEntity(string entityName) 
        {
            int index = -1;
            string description = string.Empty;
            entityName = entityName.ToLower();

            if (entityName == string.Empty || entityName == "around")
            {

                //if command "look" was used without argument, print location description
                LocationInfo();
                return;
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
                    if (index != -1)
                    {
                        Data.Player!.Inventory!.FindIndex(item => item.Name!.ToLower() == entityName);
                        description = Data.Player!.Inventory[index].Description!;
                    }
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

        /// <summary>
        /// finds location in the direction specified by 'direction' argument and returns true if found, false otherwise
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool GetNextLocation(string direction, out Location nextLocation)
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
    }
}
