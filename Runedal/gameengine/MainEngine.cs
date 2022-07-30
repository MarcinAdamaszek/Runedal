using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Runedal.GameData;
using Runedal.GameData.Locations;


namespace Runedal.GameEngine
{
    public class MainEngine
    {
        public MainEngine(MainWindow window)
        { 
            this.Window = window;
            this.Data = new Data();
        }

        public MainWindow Window { get; set; }
        public string? UserCommand { get; set; }
        public Data Data { get; set; }


        //method processing user input commands
        public void ProcessCommand()
        {
            //get user input from inputBox
            UserCommand = Window.inputBox.Text;
            Window.inputBox.Text = string.Empty;

            //clear the input from extra spaces
            UserCommand = Regex.Replace(UserCommand, @"\s+", " ");

            //format to lowercase
            UserCommand = UserCommand.ToLower();

            //match user input to proper engine command
            switch (UserCommand)
            {
                case "n":
                case "e":
                case "s":
                case "w":
                    PrintMessage(UserCommand);
                    ChangeLocation(UserCommand);
                    break;
                default:
                    PrintMessage("\"" + UserCommand + "\"" + "Nie ma takiej komendy");
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
        private void PrintMessage(string msg)
        {
            Window.outputBox.AppendText("\n" + msg);
            Window.outputBox.ScrollToEnd();
        }
    }
}
