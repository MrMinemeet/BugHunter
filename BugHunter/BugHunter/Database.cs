using BugHunter;
using MySql.Data.MySqlClient;
using System;
using System.Threading;

namespace ProjectWhitespace
{
    public class Database
    {
        /// <summary>
        /// Threadmethode um Highscore in Datenbank zu aktualisieren
        /// </summary>
        /// <param name="game">Game Objekt um auf Einstellungen und Highscore zugreifen zu können</param>
        public static void UpdateDatabaseThread(Game1 game)
        {

            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;
           
            while (true)
            {
                try
                {
                    if(connection.State != System.Data.ConnectionState.Open)
                    {
                        // Verbindung muss erst aufgebaut werden
                        game.logger.Log("Datenbankverbindung wird aufgebaut","Debug");
                        connection.Open();

                    }

                    if(connection.State == System.Data.ConnectionState.Open)
                    {
                        // Datenbankverbindung steht
                        game.logger.Log("Datenbankverbindung steht","Debug");

                        string query = "select `GlobalHighscore`.`userid`,`GlobalHighscore`.`score` from `GlobalHighscore`";
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
                            game.logger.Log("GUID war vorhanden. Eintrag wird upgedated", "Debug");
                            // Datenbankeintrag wird upgedated
                            command.CommandText =
                                "UPDATE `GlobalHighscore` SET `Name` = '" + game.settings.UserName +
                                "', `Score` = '" + game.gameStats.HighScore +
                                "', `DateTime` = '" +
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                "' WHERE `GlobalHighscore`.`UserID` = '" +
                                game.settings.GUID + "'";
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            game.logger.Log("GUID nicht gefunden. Neuer Eintrag wird erstellt", "Debug");
                            // Kein Eintrag gefunden, wodurch ein neuer erstellt wird
                            command = new MySqlCommand("INSERT INTO `GlobalHighscore` (`UserID`, `Name`, `Score`, `DateTime`, `IPAddress`) VALUES('" +
                                game.settings.GUID + "', '" +
                                game.settings.UserName + "', '" +
                                game.gameStats.HighScore + "', '" +
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                            "', 'UNUSED');");

                            command.Connection = connection;

                            command.ExecuteNonQuery();
                        }

                    }
                }
                catch(MySqlException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log(e.Message, "Error");
                }
                finally
                {
                    connection.Close();
                    game.logger.Log("Datenbankverbindung geschlossen");
                }

                // Datenbank wird alle 15 Sekunden upgedated
                try
                {
                    Thread.Sleep(30000);
                } catch(ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Database Update Thread beendet");
                    break;
                }
            }
        }

        public static void GetRankingListThread(Game1 game)
        {
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                    + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;

            while (true)
            {
                try
                {
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        // Verbindung muss erst aufgebaut werden
                        game.logger.Log("Datenbankverbindung wird aufgebaut", "Debug");
                        connection.Open();

                    }

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        // Datenbankverbindung steht
                        game.logger.Log("Datenbankverbindung steht", "Debug");

                        string query = "SELECT* FROM `GlobalHighscore` ORDER BY `Score` DESC";
                        command = new MySqlCommand(query);
                        command.Connection = connection;

                        // select rückgabe auslesen
                        reader = command.ExecuteReader();

                        game.gameStats.Top10Names.RemoveRange(0, game.gameStats.Top10Names.Count);
                        game.gameStats.Top10Score.RemoveRange(0, game.gameStats.Top10Score.Count);

                        for (int i = 0; i < 10 && reader.Read(); i++)
                        {
                            // guid in datenbank gefunden
                            if (!string.IsNullOrEmpty(reader.GetString(0)))
                            {
                                game.gameStats.Top10Names.Add(reader.GetString(1));
                                game.gameStats.Top10Score.Add(reader.GetInt32(2));
                            }
                        }

                        reader.Close();
                    }
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log(e.Message, "Error");
                }
                finally
                {
                    connection?.Close();
                    game.logger.Log("Datenbankverbindung geschlossen");
                }

                try
                {
                    Thread.Sleep(60000);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Rankinglist Update Thread beendet");
                    break;
                }
            }
        }
    }
}
