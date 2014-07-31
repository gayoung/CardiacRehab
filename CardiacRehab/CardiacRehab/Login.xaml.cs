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
            String username = usernameinput.Text.Trim();
            String password = passwordinput.Password.Trim();
            int chosenIndex = indexinput.SelectedIndex;

            // database connection and check login info
            DatabaseClass db = new DatabaseClass();
            List<String>[] result = db.SelectRecords("id", "authentication", "username='" + username + "' AND password='" + password + "'");

            if (result[0].Count() > 0)
            {
                Console.WriteLine("Found you");
            }
            else
            {
                warning_label.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // need this function to close the hidden login form
        private void MainWindowClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Closing loginwindow");
            this.Close();
        }
    }
}
