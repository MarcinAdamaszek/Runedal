using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Runedal.GameData;
using Runedal.GameData.Locations;
using System.Windows.Documents;
using System.Windows.Media;

namespace Runedal.GameEngine
{
    public class MainEngine
    {
        public MainEngine(MainWindow window)
        { 
            this.Window = window;
            this.Data = new Data();
        }

        enum MessageType
        {
            Default,
            UserCommand,
            DunnoYet
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

            //match user input to proper engine command
            switch (command)
            {
                case "n":
                case "e":
                case "s":
                case "w":
                    ChangeLocation(command);
                    break;
                default:
                    PrintMessage("Że co?");
                    return;
            }
        }

        //methods taking actions depending on user input command
        private void ChangeLocation(string direction)
        {
            int currentX = Data.Player.CurrentLocation!.X;
            int currentY = Data.Player.CurrentLocation!.Y;
            int destinationX = 0;
            int destinationY = 0;
            string directionString = string.Empty;
            Passage passage;

            switch (direction)
            {
                case "n":
                    destinationX = currentX;
                    destinationY = currentY + 1;
                    passage = Data.Player.CurrentLocation.NorthPassage;
                    directionString = "północ";
                    break;
                case "e":
                    destinationX = currentX + 1;
                    destinationY = currentY;
                    passage = Data.Player.CurrentLocation.EastPassage;
                    directionString = "wschód";
                    break;
                case "s":
                    destinationX = currentX;
                    destinationY = currentY - 1;
                    passage = Data.Player.CurrentLocation.SouthPassage;
                    directionString = "południe";
                    break;
                case "w":
                    destinationX = currentX - 1;
                    destinationY = currentY;
                    passage = Data.Player.CurrentLocation.WestPassage;
                    directionString = "zachód";
                    break;
            }


            //if there exists location in north direction to players current one
            if (Data.Locations.Exists(x => x.X == destinationX && x.Y == destinationY))
            {
                //if the passage is open
                if (Data.Player.CurrentLocation.NorthPassage.IsOpen)
                {
                    PrintMessage("Idziesz na " + directionString);

                    //change player's current location
                    Data.Player.CurrentLocation = Data.Locations.Find(x => x.X == destinationX && x.Y == destinationY);

                    //add player to the list of location entities
                    Data.Player.CurrentLocation!.AddEntity(Data.Player);
                    PrintMessage(Data.Player.CurrentLocation.Description!);
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
            TextRange tr = new TextRange(this.Window.outputBox.Document.ContentEnd, this.Window.outputBox.Document.ContentEnd);
            tr.Text = "\n" + msg;

            switch (type) {
                case (MessageType.Default):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightGray);
                    break;
                case (MessageType.UserCommand):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Aqua);
                    break;
                case (MessageType.DunnoYet):
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                    break;
            }

            Window.outputBox.ScrollToEnd();
        }
    }
}
