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
using System.Windows.Shapes;

namespace CardiacRehab
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        int sessionID;

        public PopupWindow(int session)
        {
            sessionID = session;

            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            String note = patientNote.Text;

            // save the note into DB

            DatabaseClass db = new DatabaseClass();
            String recordValue = "'" + note + "', " + "NOW(), " + sessionID.ToString();
            db.InsertRecord("note_data", "note, date_stamped, session_id", recordValue);

            this.Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
