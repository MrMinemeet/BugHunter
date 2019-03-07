using BugHunter;
using MySql.Data.MySqlClient;
using System;
using System.Net;
using System.Text.RegularExpressions;
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
            string connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;

            while (true)
            {
                if (game.settings.HasInternetConnection)
                {

                    try
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            // Verbindung muss erst aufgebaut werden
                            game.logger.Log("Datenbankverbindung wird aufgebaut", Thread.CurrentThread.Name, "Debug");
                            connection.Open();

                        }

                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            // Datenbankverbindung steht
                            game.logger.Log("Datenbankverbindung steht", Thread.CurrentThread.Name, "Debug");

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

                            string externalip = "private";
                            string Statistiken = "";
                            if (game.settings.SendAnonymStatistics)
                            {
                                string returnString = new WebClient().DownloadString("http://icanhazip.com");
                                externalip = Regex.Replace(returnString, @"\t|\n|\r", "");


                                OperatingSystem os_info = System.Environment.OSVersion;

                                TimeSpan timeSpan = TimeSpan.FromMilliseconds(game.gameStats.PlayTime);

                                Statistiken = "OS-Version:" + " " + os_info.VersionString +", Spielzeit:" + timeSpan.ToString("dd\\.hh\\:mm");                                
                            }

                            if (GuidExists)
                            {
                                game.logger.Log("GUID war vorhanden. Eintrag wird upgedated", Thread.CurrentThread.Name, "Debug");
                                // Datenbankeintrag wird upgedated
                                command.CommandText =
                                    "UPDATE `GlobalHighscore` SET `Name` = '" + game.settings.UserName +
                                    "', `Score` = '" + game.gameStats.HighScore +
                                    "', `DateTime` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                    "', `IPAddress` = '" + externalip +
                                    "', `Statistics` = '" + Statistiken +
                                    "' WHERE `GlobalHighscore`.`UserID` = '" + game.settings.GUID + "'";
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                game.logger.Log("GUID nicht gefunden. Neuer Eintrag wird erstellt", Thread.CurrentThread.Name, "Debug");
                                // Kein Eintrag gefunden, wodurch ein neuer erstellt wird
                                command = new MySqlCommand("INSERT INTO `GlobalHighscore` (`UserID`, `Name`, `Score`, `DateTime`, `IPAddress`) VALUES('" +
                                    game.settings.GUID + "', '" +
                                    game.settings.UserName + "', '" +
                                    game.gameStats.HighScore + "', '" +
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                "', '"+externalip+"');");

                                command.Connection = connection;

                                command.ExecuteNonQuery();
                            }

                        }
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.Message);
                        game.logger.Log(e.Message,Thread.CurrentThread.Name, "Error");
                    }
                    finally
                    {
                        connection.Close();
                        game.logger.Log("Datenbankverbindung geschlossen", Thread.CurrentThread.Name, "Debug");
                    }
                }

                // Datenbank wird alle 15 Sekunden upgedated
                try
                {
                    Thread.Sleep(30000);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }
            }
        }

        public static void GetRankingListThread(Game1 game)
        {
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;

            while (true)
            {
                if (game.settings.HasInternetConnection)
                {
                    try
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            // Verbindung muss erst aufgebaut werden
                            game.logger.Log("Datenbankverbindung wird aufgebaut", Thread.CurrentThread.Name, "Debug");
                            connection.Open();

                        }

                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            // Datenbankverbindung steht
                            game.logger.Log("Datenbankverbindung steht", Thread.CurrentThread.Name, "Debug");

                            string query = "SELECT* FROM `GlobalHighscore` ORDER BY `Score` DESC";
                            command = new MySqlCommand(query);
                            command.Connection = connection;

                            // select rückgabe auslesen
                            reader = command.ExecuteReader();

                            game.gameStats.Top10Names.RemoveRange(0, game.gameStats.Top10Names.Count);
                            game.gameStats.Top10Score.RemoveRange(0, game.gameStats.Top10Score.Count);

                            for (int i = 0; i <= 10 && reader.Read(); i++)
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
                        game.logger.Log(e.Message,Thread.CurrentThread.Name, "Error");
                    }
                    finally
                    {
                        connection?.Close();
                        game.logger.Log("Datenbankverbindung geschlossen", Thread.CurrentThread.Name, "Debug");
                    }
                }
                try
                {
                    Thread.Sleep(30000);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Rankinglist Update Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }
            }
        }

        public static void GetGlobalScoreList(Game1 game)
        {
            String connString = "Server=" + Settings.host + ";Database=" + Settings.database + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;

            while (true)
            {
                if (game.settings.HasInternetConnection)
                {
                    try
                    {
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            // Verbindung muss erst aufgebaut werden
                            game.logger.Log("Datenbankverbindung wird aufgebaut", Thread.CurrentThread.Name, "Debug");
                            connection.Open();

                        }

                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            // Datenbankverbindung steht
                            game.logger.Log("Datenbankverbindung steht", Thread.CurrentThread.Name, "Debug");

                            string query = "SELECT * FROM `GlobalScore`";
                            command = new MySqlCommand(query);
                            command.Connection = connection;

                            // select rückgabe auslesen
                            reader = command.ExecuteReader();

                            reader.Read();

                            game.gameStats.GlobalKilledEnemies = reader.GetUInt32(1);
                            game.gameStats.GlobalCollectedPowerups = reader.GetUInt32(2);
                            game.gameStats.GlobalAnzahlSchuesse = reader.GetUInt64(3);
                            game.gameStats.GlobalAnzahlTreffer = reader.GetUInt64(4);
                            game.gameStats.GlobalAnzahlTode = reader.GetUInt32(5);

                            reader.Close();

                            // Globale Anzahl an Spielern aus Datenbank lesen
                            query = "SELECT COUNT(*) FROM GlobalHighscore;";
                            command = new MySqlCommand(query);
                            command.Connection = connection;

                            // select rückgabe auslesen
                            reader = command.ExecuteReader();

                            reader.Read();

                            game.gameStats.GlobalPlayerAmount = reader.GetUInt32(0);

                            reader.Close();
                        }
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.Message);
                        game.logger.Log(e.Message,Thread.CurrentThread.Name, "Error");
                    }
                    finally
                    {
                        connection?.Close();
                        game.logger.Log("Datenbankverbindung geschlossen", Thread.CurrentThread.Name, "Debug");
                    }
                }

                try
                {
                    Thread.Sleep(30000);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Rankinglist Update Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }

            }
        }
    }
}
