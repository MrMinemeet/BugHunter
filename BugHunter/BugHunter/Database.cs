using BugHunter;
using MySql.Data.MySqlClient;
using System;

namespace ProjectWhitespace
{
    public class Database
    {
        public MySqlConnection mySqlConnection = null;

        public Database()
        {
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            mySqlConnection = new MySqlConnection(connString);
            try
            {
                this.mySqlConnection.Open();
            } catch (Exception) { };
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
                
                // Überprüfen ob Datenbankverbindung steht
                if(mySqlConnection.State == System.Data.ConnectionState.Open){

                    mySqlCommand.Connection = this.mySqlConnection;

                    Console.WriteLine("Connection successful!");

                    mySqlCommand.ExecuteNonQueryAsync();

                    Console.WriteLine("Insetion Done");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public void Dispose()
        {
            this.mySqlConnection.Close();
            this.mySqlConnection.Dispose();
        }
    }
}
