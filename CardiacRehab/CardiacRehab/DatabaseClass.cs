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
        /// <summary>
        /// Set all fields for the database connection.  Currently
        /// it's set to connect to the local MySQL database.
        /// </summary>
        private void InitializeDB()
        {
            db_server = "localhost";
            db_name = "cardiac";
            db_username = "root";
            db_password = "";

            String connectionString = "SERVER=" + db_server + ";DATABASE="
                + db_name + ";UID=" + db_username + ";PASSWORD=" + db_password + ";charset=utf8;";

            db_connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Opens the connection to the MySQL database.
        /// </summary>
        /// <returns> Returns true if the database connection was successfully made. Otherwise can return
        /// error code 0 or 1045.</returns>
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

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        /// <returns>Return true if successfully closed. Otherwise, false.</returns>
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
        /// <summary>
        /// Insert the specified record into the specified database table
        /// </summary>
        /// <param name="tablename">database table name</param>
        /// <param name="fields">all fields in this table</param>
        /// <param name="values">Values to be inserted into db. Strings needs to have '' around...etc</param>
        public int InsertRecord(String tablename, String fields, String values)
        {
            int insert_id = 0;
            String db_query = "INSERT INTO " + tablename + " (" + fields + ") VALUES(" + values + ")";
            if(this.OpenDBConnection())
            {
                MySqlCommand insertCmd = new MySqlCommand(db_query, db_connection);
                insertCmd.ExecuteNonQuery();
                insert_id = (int)insertCmd.LastInsertedId;
                this.CloseDBConnection();
            }
            else
            {
                Console.WriteLine("Connection errror at InsertRecord.");
            }

            return insert_id;

        }

        /// <summary>
        /// Delete the specified record(s) from specified database table.
        /// </summary>
        /// <param name="tablename">database table name</param>
        /// <param name="condition">Condition given to find the existing record(s)</param>
        public void DeleteRecord(String tablename, String condition)
        {
            String deleteQuery = "DELETE FROM " + tablename + " WHERE " + condition;

            if(this.OpenDBConnection())
            {
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, db_connection);
                deleteCmd.ExecuteNonQuery();
                this.CloseDBConnection();
            }
        }

        /// <summary>
        /// Update the record in the specified database table with specified new values.
        /// </summary>
        /// <param name="tablename">database table name</param>
        /// <param name="updateInfo">Information in the table field(s) to be updated. (eg. name='Bob'...etc)</param>
        /// <param name="condition">Condition given to find the existing record. (eg. id=2...etc)</param>
        public void UpdateRecord(String tablename, String updateInfo, String condition)
        {
            String updateQuery = "UPDATE " + tablename + " SET " + updateInfo + " WHERE " + condition;

            if(this.OpenDBConnection())
            {
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, db_connection);
                updateCmd.ExecuteNonQuery();
                this.CloseDBConnection();
            }
            else
            {
                Console.WriteLine("Connection error at UpdateRecord.");
            }

        }

        /// <summary>
        /// Select specified record(s) from the specified database table.
        /// </summary>
        /// <param name="fields">need to list all fields in db (* character does not work in this syntax)</param>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
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
            
            if(this.OpenDBConnection())
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
                Console.WriteLine("Connection error at SelectRecords.");
                return queryResults;
            }

        }
        
    }
    

}
