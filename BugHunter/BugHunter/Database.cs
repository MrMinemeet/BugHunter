using BugHunter;
using MySql.Data.MySqlClient;
using System;

namespace ProjectWhitespace
{
    public class Database
    {
        public MySqlConnection mySqlConnection = null;
        private Game1 game = null;

        public Database(Game1 game)
        {
            this.game = game;
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            mySqlConnection = new MySqlConnection(connString);
            try
            {
                this.mySqlConnection.Open();
            }
            catch (MySqlException e)
            {
                game.logger.Log("Fehler beim Verbinden zu der Datenbank: " + e.Message);
            }
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
                
                // Überprüfen ob Datenbankverbindung steht
                if(mySqlConnection.State == System.Data.ConnectionState.Open){

                    game.logger.Log("Connection active. Sending Query.");
                    mySqlCommand.Connection = this.mySqlConnection;

                    mySqlCommand.ExecuteNonQueryAsync();

                    Console.WriteLine("Insetion Done");
                }
            }
            catch (Exception e)
            {
                game.logger.Log(e.Message);
            }
        }

        public void Dispose()
        {
            this.mySqlConnection.Close();
            this.mySqlConnection.Dispose();
        }
    }
}
