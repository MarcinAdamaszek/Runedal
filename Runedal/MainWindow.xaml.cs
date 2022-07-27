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
using System.Text.RegularExpressions;

namespace Runedal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string input = string.Empty;

        //handling user input
        private void inputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && inputBox.Text != String.Empty)
            {
                input = inputBox.Text;
                inputBox.Text = string.Empty;

                //get rid of extra spaces
                input = Regex.Replace(input, @"\s+", " ");
                
                outputBox.AppendText(input + "\n");
                outputBox.ScrollToEnd();

                input = input.ToLower();
            }
        }

        
    }
}
