using BugHunter;
using MySql.Data.MySqlClient;
using System;
using System.Threading;

namespace ProjectWhitespace
{
    public class Database
    {
        public MySqlConnection mySqlConnection = null;
        private Game1 game = null;

        public Database(Game1 game)
        {
            /*
            this.game = game;
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            mySqlConnection = new MySqlConnection(connString);
            try
            {
               //  mySqlConnection.Open();
            }
            catch (MySqlException e)
            {
                game.logger.Log("Fehler beim Verbinden zu der Datenbank: " + e.Message);
            }
            */
        }

        /// <summary>
        /// Sendet query an SQL-Server
        /// </summary>
        /// <param name="query">SQL-Query als String</param>
        public void SendQueryCommand(string query)
        {
            /*
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
            */
        }

        public void Dispose()
        {
            this.mySqlConnection?.Close();
            this.mySqlConnection?.Dispose();
        }
    }

    public class ThreadWork
    {
        public static void DoWork(Game1 game)
        {

            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection mySqlConnection;
            MySqlCommand mySqlCommand;

            while (true)
            {
                Console.WriteLine("THREAD");
                mySqlConnection = new MySqlConnection(connString);
                mySqlConnection.Open();

                // Überprüft ob Datenbank läuft
                if (mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    game.logger.Log("Datenbank nicht aktiv.");
                    return;
                }
                game.logger.Log("Datenbank aktiv.");

                // Stats an Datenbank senden

                string myInsertQuery = "SELECT `globalscore`.`UserID`,`globalscore`.`Score` FROM `globalscore`";
                mySqlCommand = new MySqlCommand(myInsertQuery)
                {
                    Connection = mySqlConnection
                };

                // SELECT rückgabe auslesen
                MySqlDataReader reader = mySqlCommand.ExecuteReader();

                bool GuidExists = false;

                while (reader.Read())
                {
                    // GUID in Datenbank gefunden
                    if (reader.GetString(0).Equals(game.settings.GUID))
                    {
                        game.logger.Log("GUID in Datenbank gefunden." + reader.GetString(0));
                        GuidExists = true;
                        break;
                    }
                }

                reader.Close();
                reader.Dispose();

                if (GuidExists)
                {
                    // Datenbankeintrag wird upgedated
                    mySqlCommand.CommandText =
                        "UPDATE `globalscore` SET `Name` = '" + game.settings.UserName +
                        "', `Score` = '" + game.settings.HighScore +
                        "', `DateTime` = '" +
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                        "' WHERE `globalscore`.`UserID` = '" + game.settings.GUID + "'";
                    mySqlCommand.ExecuteNonQuery();
                    game.logger.Log("Datenbankeintrag für " + game.settings.GUID + " upgedated.");
                }
                else
                {
                    // Kein Eintrag gefunden, wodurch ein neuer erstellt wird
                    string query = "INSERT INTO `globalscore` (`UserID`, `Name`, `Score`, `DateTime`, `IPAddress`) VALUES('" + game.settings.GUID + "', '" + game.settings.UserName + "', '" + game.settings.HighScore + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'UNUSED');";

                    mySqlCommand = new MySqlCommand(query);
                    mySqlCommand.Connection = mySqlConnection;
                    mySqlCommand.ExecuteNonQueryAsync();

                    game.logger.Log("Datenbankeintrag für " + game.settings.GUID + " erstellt.");
                }

                mySqlConnection.CloseAsync();

                Thread.Sleep(60);
            }
            mySqlConnection.Dispose();
            mySqlCommand.Dispose();
        }
    }
}
