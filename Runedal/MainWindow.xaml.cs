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
        public MainWindow()
        {
            InitializeComponent();
            Engine = new MainEngine(this);
            Player = Engine.Data.Player!;

            DataContext = Player;
        }

        public string Input { get; set; } = string.Empty;
        public MainEngine Engine { get; set; }
        public Player Player { get; set; }


        //handling user input on pressing and releasing Enter key
        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        
        {
            if (e.Key == Key.Enter && inputBox.Text != String.Empty)
            {
                
                this.Engine.ProcessCommand();
            }
        }

        
    }
}
