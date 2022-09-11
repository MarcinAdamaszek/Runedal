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
            Engine = new MainEngine(this);
            Player = Engine.Data.Player!;
            CommandHistory = new List<string>();
            WasDownPressed = false;
            WasUpPressed = false;
            CommandIndex = -1;

            DataContext = Player;
        }

        public string Input { get; set; } = string.Empty;
        public MainEngine Engine { get; set; }
        public Player Player { get; set; }
        public List<string> CommandHistory { get; set; }
        public int CommandIndex { get; set; }
        public bool WasDownPressed { get; set; }
        public bool WasUpPressed { get; set; }


        //handling user input on pressing and releasing Enter key
        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        
        {

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

        
    }
}
