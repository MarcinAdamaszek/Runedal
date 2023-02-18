using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Runedal.GameEngine;
using Runedal.GameData.Characters;

namespace Runedal
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        const int CommandHistorySize = 10;
        public MainWindow()
        {
            InitializeComponent();

            //ImageBrush myBrush = new ImageBrush();
            //myBrush.ImageSource =
            //    new BitmapImage(new Uri("GameData//Resources//Images//wall-3341768.jpg",
            //    UriKind.Relative));
            //this.Background = myBrush;

            Engine = new MainEngine(this);
            //Player = Engine.Data.Player!;
            CommandHistory = new List<string>();
            WasDownPressed = false;
            WasUpPressed = false;
            IsWelcomeScreenOn = true;
            CommandIndex = -1;
            inputBox.IsReadOnly = true;
            IsFullScreen = true;

            //DataContext = Player;
        }

        public string Input { get; set; } = string.Empty;
        public MainEngine Engine { get; set; }
        public Player? Player { get; set; }
        public List<string> CommandHistory { get; set; }
        public int CommandIndex { get; set; }
        public bool WasDownPressed { get; set; }
        public bool WasUpPressed { get; set; }
        public bool IsWelcomeScreenOn { get; set; }
        public bool IsFullScreen { get; set; }


        //handling user input on pressing and releasing Enter key
        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        {
            //testing area
            //if (e.Key == Key.Q)
            //{
            //    Engine.PrintMessage("IsInMenu: " + Engine.IsInMenu);
            //    Engine.PrintMessage("IsInGame: " + Engine.IsInGame + "\n");
                
            //    Engine.PrintMessage("IsLoading: " + Engine.IsLoading);
            //    Engine.PrintMessage("IsSaving: " + Engine.IsSaving);
            //    Engine.PrintMessage("IsNewSave: " + Engine.IsNewSave);
            //    Engine.PrintMessage("IsInManual: " + Engine.IsInManual + "\n");

            //    Engine.PrintMessage("IsPlayerChoosingAName: " + Engine.IsPlayerChoosingAName);
            //    Engine.PrintMessage("IsConfirmationScreen: " + Engine.IsSaveConfirmation);
            //}

            //handle fullscreen on/off
            if (e.Key == Key.F11)
            {
                SwitchFullScreenMode();
            }

            //handle welcome screen
            if (IsWelcomeScreenOn)
            {
                Engine.PrintMainMenu();
                inputBox.IsReadOnly = false;
                inputBox.Focus();
                IsWelcomeScreenOn = false;
                return;
            }

            //handle saving error
            if (Engine.IsErrorSaving)
            {
                Engine.PrintMainMenu();
                Engine.IsErrorSaving = false;
                inputBox.Text = "";
            }

            
            if (e.Key == Key.Escape)
            {
                if (Engine.IsSaveOverwriteConfirmation)
                {
                    Engine.IsSaveOverwriteConfirmation = false;
                    Engine.IsInMenu = true;
                    Engine.PrintMainMenu();
                    return;
                }

                if (Engine.IsInMenu && Engine.IsInGame)
                {
                    Engine.IsInMenu = false;
                    Engine.ClearOutputBox();
                    Engine.LocationInfo(Engine.Data.Player!.CurrentLocation!);
                    if (!Engine.IsPaused)
                    {
                        Engine.GameClock.Start();
                    }
                    return;
                }

                if (Engine.IsInManual)
                {
                    Engine.IsInManual = false;
                    Engine.IsInMenu = true;
                    Engine.ClearOutputBox();
                    Engine.PrintMainMenu();
                    return;
                }
                if (Engine.IsLoading)
                {
                    Engine.IsLoading = false;
                    Engine.IsInMenu = true;
                    Engine.ClearOutputBox();
                    Engine.PrintMainMenu();
                    return;
                }
                if (Engine.IsSaving)
                {
                    Engine.IsSaving = false;
                    Engine.IsNewSave = false;
                    Engine.IsSaveConfirmation = false;
                    Engine.IsInMenu = true;
                    Engine.ClearOutputBox();
                    Engine.PrintMainMenu();
                    return;
                }
                if (Engine.IsPlayerChoosingAName)
                {
                    Engine.IsPlayerChoosingAName = false;
                    Engine.IsInMenu = true;
                    Engine.ClearOutputBox();
                    Engine.PrintMainMenu();
                    return;
                }
                if (Engine.IsExitConfirmation)
                {
                    Engine.IsExitConfirmation = false;

                    Engine.IsInMenu = true;
                    Engine.ClearOutputBox();
                    Engine.PrintMainMenu();
                    return;
                }

                if (Engine.IsDeleting)
                {
                    Engine.IsDeleting = false;
                    Engine.IsInMenu = true;
                    Engine.PrintMainMenu();
                    return;
                }


                if (Engine.IsInGame)
                {
                    Engine.IsInMenu = true;
                    if (!Engine.IsPaused)
                    {
                        Engine.GameClock.Stop();
                    }
                    Engine.ClearOutputBox();
                    Engine.PrintMainMenu();
                    return;
                }
            }

            if (e.Key == Key.Enter && inputBox.Text != String.Empty)
            {

                if (inputBox.Text != String.Empty)
                {
                    CommandIndex = 0;
                }
                CommandHistory.Insert(CommandIndex, inputBox.Text);

                this.Engine.ProcessCommand();

                if (CommandHistory.Count > CommandHistorySize)
                {
                     CommandHistory.RemoveAt(CommandHistorySize - 1);
                }

                //set command index below first element so it becomes 1st element (0)
                //when player presses up key
                CommandIndex = -1;
                return;
            }
            
            if (e.Key == Key.Up)
            {

                //up command only if there are any commands in command history
                //and commandIndex isn't pointing to last element of command history
                if (CommandHistory.Count > 0 && CommandIndex < CommandHistory.Count - 1)
                {
                    CommandIndex++;
                }
            }

            if (e.Key == Key.Down && CommandHistory.Count > 0 && CommandIndex > 0)
            {
                    CommandIndex--;
            }


            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                if (CommandIndex > -1)
                {
                    inputBox.Text = CommandHistory[CommandIndex];
                    inputBox.CaretIndex = inputBox.Text.Length;
                }
            }

        }

        public void InitializePlayerDataContext(Player player)
        {
            Player = player;
            DataContext = player;
        }
        public void SwitchFullScreenMode()
        {
            if (IsFullScreen)
            {
                IsFullScreen = false;
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                IsFullScreen = true;
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }
        }
        
    }
}
