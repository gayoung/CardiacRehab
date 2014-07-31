using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

namespace CardiacRehab
{
    class DatabaseClass
    {
        private MySqlConnection db_connection;
        private String db_name;
        private String db_server;
        private String db_username;
        private String db_password;
        
        public DatabaseClass()
        {
            InitializeDB();
        }

        private void InitializeDB()
        {
            db_server = "localhost";
            db_name = "cardiac";
            db_username = "root";
            db_password = "";

            String connectionString = "SERVER=" + db_server + ";DATABASE="
                + db_name + ";UID=" + db_username + ";PASSWORD=" + db_password + ";";

            db_connection = new MySqlConnection(connectionString);
        }

        private bool OpenDBConnection()
        {
            try 
            {
                db_connection.Open();
                return true;
            } 
            catch(MySqlException ex)
            {
                switch(ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to the server.");
                        break;
                    case 1045:
                        MessageBox.Show("Invalid username/password. Please try again.");
                        break;
                }
                return false;
            }
        }
        private bool CloseDBConnection()
        {
            try 
            {
                db_connection.Close();
                return true;
            }
            catch(MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        public void InsertRecord()
        {

        }
        public void DeleteRecord()
        {

        }
        public void UpdateRecord()
        {

        }
        public List<String>[] SelectRecords(String fields, String table, String condition)
        {
            String db_query = "";
            if(condition == "")
            {
                db_query = "SELECT " + fields + " FROM " + table;

            }
            else
            {
                db_query = "SELECT " + fields + " FROM " + table + " WHERE " + condition;
            }

            String[] diffFields = fields.Split(',');

            List<String>[] queryResults = new List<String>[diffFields.Length];

            for (int i = 0; i < diffFields.Length; i++)
            {
                queryResults[i] = new List<String>();
            }
            
            if(this.OpenDBConnection() == true)
            {
                MySqlCommand command = new MySqlCommand(db_query, db_connection);
                MySqlDataReader dataReader = command.ExecuteReader();

                while(dataReader.Read())
                {
                    for (int i = 0; i < diffFields.Length; i++)
                    {
                        queryResults[i].Add(dataReader[diffFields[i].Trim()] + "");
                    }
                }

                dataReader.Close();
                this.CloseDBConnection();

                return queryResults;

            }
            else
            {
                Console.WriteLine("error at SelectRecords");
                return queryResults;
            }

        }
        
    }
    

}
