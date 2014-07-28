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

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(loginscreen, usernameinput);
        }

        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            String username = usernameinput.Text;
            String password = passwordinput.Password;
            int chosenIndex = indexinput.SelectedIndex;

            // database connection and check login info
        }

        // need this function to close the hidden login form
        private void MainWindowClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Closing loginwindow");
            this.Close();
        }
    }
}
