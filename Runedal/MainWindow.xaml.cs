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
            this.Engine = new MainEngine(this);
        }

        public string Input { get; set; } = string.Empty;
        public MainEngine Engine { get; set; }


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
