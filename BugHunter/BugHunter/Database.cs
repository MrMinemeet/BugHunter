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
            this.game = game;
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";UserId=" + Settings.username + ";password=" + Settings.password;

            mySqlConnection = new MySqlConnection(connString);
            try
            {
                // this.mySqlConnection.Open();
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



        public static void UpdateDatabase(Game1 game)
        {

            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;

            while (true)
            {
                // Datenbank wird alle 15 Sekunden upgedated
                Thread.Sleep(15000);

                try
                {
                    if(connection.State != System.Data.ConnectionState.Open)
                    {
                        // Verbindung muss erst aufgebaut werden
                        connection.Open();
                    }

                    if(connection.State == System.Data.ConnectionState.Open)
                    {
                        // Datenbankverbindung steht

                        string query = "select `globalscore`.`userid`,`globalscore`.`score` from `globalscore`";
                        command = new MySqlCommand(query);
                        command.Connection = connection;

                        // select rückgabe auslesen
                        reader = command.ExecuteReader();

                        bool GuidExists = false;

                        while (reader.Read())
                        {
                            // guid in datenbank gefunden
                            if (reader.GetString(0).Equals(game.settings.GUID))
                            {
                                GuidExists = true;
                                break;
                            }
                        }

                        reader.Close();

                        if (GuidExists)
                        {
                            // Datenbankeintrag wird upgedated
                            command.CommandText =
                                "UPDATE `globalscore` SET `Name` = '" + game.settings.UserName +
                                "', `Score` = '" + game.settings.HighScore +
                                "', `DateTime` = '" +
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                "' WHERE `globalscore`.`UserID` = '" + game.settings.GUID + "'";
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            // Kein Eintrag gefunden, wodurch ein neuer erstellt wird
                            command = new MySqlCommand("INSERT INTO `globalscore` (`UserID`, `Name`, `Score`, `DateTime`, `IPAddress`) VALUES('" + game.settings.GUID + "', '" + game.settings.UserName + "', '" + game.settings.HighScore + "', '" +
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'UNUSED');");

                            command.Connection = connection;

                            command.ExecuteNonQuery();
                        }

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

        }
    }
}
