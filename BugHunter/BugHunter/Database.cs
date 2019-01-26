using BugHunter;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWhitespace
{
    class Database
    {
        MySqlConnection mySqlConnection = null;

        public Database()
        {
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            mySqlConnection = new MySqlConnection(connString);
        }

        /// <summary>
        /// Sendet query an SQL-Server
        /// </summary>
        /// <param name="query">SQL-Query als String</param>
        public void SendQueryCommand(string query)
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(query);

                Console.WriteLine("Openning Connection ...");

                this.mySqlConnection?.Open();

                // Überprüfen ob Datenbankverbindung steht
                if(mySqlConnection.State == System.Data.ConnectionState.Open){

                    mySqlCommand.Connection = this.mySqlConnection;

                    Console.WriteLine("Connection successful!");

                    mySqlCommand.ExecuteNonQuery();

                    Console.WriteLine("Insetion Done");

                    this.mySqlConnection.Close();
                    mySqlCommand.Connection.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public void Dispose()
        {
            this.mySqlConnection.Dispose();
        }
    }
}
