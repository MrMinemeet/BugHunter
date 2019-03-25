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
            bool IsInterrupted = false;

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
                            bool GuidExists = false;



                            //  <=== HIGHSCORE ===>
                            string query = "SELECT GlobalHighscore.UserID FROM GlobalHighscore WHERE GlobalHighscore.UserID = '" + game.settings.GUID + "'";

                            command = new MySqlCommand(query);
                            command.Connection = connection;
                            reader = command.ExecuteReader();

                            reader.Read();

                            // Wenn eine Zeile zurück gekommen ist dann ist die GUID vorhanden
                            if(reader.HasRows)
                                if (reader.GetString(0).Equals(game.settings.GUID))
                                    GuidExists = true;

                            reader.Close();
                            
                            if (GuidExists)
                            {
                                game.logger.Log("GUID war vorhanden. Eintrag wird upgedated", Thread.CurrentThread.Name, "Debug");
                                // Datenbankeintrag wird upgedated
                                command.CommandText =
                                    "UPDATE `GlobalHighscore` SET `Name` = '" + game.settings.UserName +
                                    "', `Score` = '" + game.gameStats.HighScore +
                                    "', `DateTime` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                    "' WHERE `GlobalHighscore`.`UserID` = '" + game.settings.GUID + "'";
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                game.logger.Log("GUID nicht gefunden. Neuer Eintrag wird erstellt", Thread.CurrentThread.Name, "Debug");
                                // Kein Eintrag gefunden, wodurch ein neuer erstellt wird
                                command = new MySqlCommand("INSERT INTO `GlobalHighscore` (`UserID`, `Name`, `Score`, `DateTime`) VALUES('" +
                                    game.settings.GUID + "', '" +
                                    game.settings.UserName + "', '" +
                                    game.gameStats.HighScore + "', '" +
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                                "');");

                                command.Connection = connection;

                                command.ExecuteNonQuery();
                            }

                            //   <=== GLOBAL SCORE ===>

                            command = new MySqlCommand();
                            command.CommandText = "SELECT * FROM `GlobalScore`";
                            command.Connection = connection;

                            reader = command.ExecuteReader();
                            reader.Read();

                            // Aktuellen Wert speichern
                            uint GlobalKilledEnemies = reader.GetUInt32(1);
                            uint GlobalCollectedPowerups = reader.GetUInt32(2);
                            UInt64 GlobalAnzahlSchuesse = reader.GetUInt64(3);
                            UInt64 GlobalAnzahlHits = reader.GetUInt64(4);
                            uint GlobalDeathCount = reader.GetUInt32(5);

                            reader.Close();

                            // Datenbankeintrag wird upgedated
                            command.CommandText = "UPDATE `GlobalScore` SET `KilledEnemies` = '" + (GlobalKilledEnemies + game.gameStats.KilledEnemies - game.gameStats.KilledEnemiesOld) + "', `CollectedPowerups` = '" + (GlobalCollectedPowerups + game.gameStats.CollectedPowerups - game.gameStats.CollectedPowerupsOld) + "', `Shots` = '" + (GlobalAnzahlSchuesse + game.gameStats.AnzahlSchuesse - game.gameStats.AnzahlSchuesseOld) + "', `Hits` = '" + (GlobalAnzahlHits + game.gameStats.AnzahlTreffer - game.gameStats.AnzahlTrefferOld) + "', `Deaths` = '" + (GlobalDeathCount + game.gameStats.AnzahlTode - game.gameStats.AnzahlTodeOld) + "' WHERE `GlobalScore`.`ID` = 1;";

                            command.ExecuteNonQuery();

                            game.gameStats.KilledEnemiesOld = game.gameStats.KilledEnemies;
                            game.gameStats.CollectedPowerupsOld = game.gameStats.CollectedPowerups;
                            game.gameStats.AnzahlSchuesseOld = game.gameStats.AnzahlSchuesse;
                            game.gameStats.AnzahlTrefferOld = game.gameStats.AnzahlTreffer;
                            game.gameStats.AnzahlTodeOld = game.gameStats.AnzahlTode;

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

                // Wenn der Thread Interrupted wird läuft er noch einmal durch bis er sich beendet
                if (IsInterrupted)
                {
                    game.logger.Log("Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }

                // Datenbank wird alle 15 Sekunden upgedated
                try
                {
                    Thread.Sleep(Settings.DatabaseUpdateCycleTime);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    IsInterrupted = true;
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
                    Thread.Sleep(Settings.DatabaseUpdateCycleTime);
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
                    Thread.Sleep(Settings.DatabaseUpdateCycleTime);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Rankinglist Update Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }

            }
        }

        /// <summary>
        /// Thread zum senden anonymer Statistiken
        /// </summary>
        /// <param name="game"></param>
        public static void SendAnonymStatistics(Game1 game)
        {
            bool IsInterrupted = false;

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
                            bool GuidExists = false;

                            string query = "SELECT Statistiken.StatisticsID FROM Statistiken WHERE Statistiken.StatisticsID = '" + game.settings.StatisticsGUID + "'";

                            command = new MySqlCommand(query);
                            command.Connection = connection;
                            reader = command.ExecuteReader();

                            reader.Read();

                            // Wenn eine Zeile zurück gekommen ist dann ist die GUID vorhanden
                            if (reader.HasRows)
                                if (reader.GetString(0).Equals(game.settings.StatisticsGUID))
                                    GuidExists = true;

                            reader.Close();

                            string externalip = "non";

                            string returnString = new WebClient().DownloadString("http://icanhazip.com");
                            externalip = Regex.Replace(returnString, @"\t|\n|\r", "");

                            TimeSpan timeSpan = TimeSpan.FromMilliseconds(game.gameStats.PlayTime);

                            if (GuidExists)
                            {
                                game.logger.Log("Statistics GUID war vorhanden. Eintrag wird upgedated", Thread.CurrentThread.Name, "Debug");
                                // Datenbankeintrag wird upgedated
                                command.CommandText = "UPDATE `BugHunter`.`Statistiken` SET `IP Address` = '" + externalip + "', `OS` = '" + System.Environment.OSVersion.VersionString + "', `Playtime` = '" + timeSpan.ToString("dd\\.hh\\:mm") + "' WHERE `StatisticsID` = '" + game.settings.StatisticsGUID + "'";
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                game.logger.Log("Statistics GUID nicht gefunden. Neuer Eintrag wird erstellt", Thread.CurrentThread.Name, "Debug");
                                // Kein Eintrag gefunden, wodurch ein neuer erstellt wird
                                command = new MySqlCommand("INSERT INTO `BugHunter`.`Statistiken`(`StatisticsID`, `IP Address`, `OS`, `Playtime`) VALUES ('" +game.settings.StatisticsGUID + "', '" + externalip + "', '"+ System.Environment.OSVersion.VersionString +"', '" + timeSpan.ToString("dd\\.hh\\:mm") + "')");

                                command.Connection = connection;

                                command.ExecuteNonQuery();
                            }

                        }
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.Message);
                        game.logger.Log(e.Message, Thread.CurrentThread.Name, "Error");
                    }
                    finally
                    {
                        connection.Close();
                        game.logger.Log("Datenbankverbindung geschlossen", Thread.CurrentThread.Name, "Debug");
                    }
                }

                // Wenn der Thread Interrupted wird läuft er noch einmal durch bis er sich beendet
                if (IsInterrupted)
                {
                    game.logger.Log("Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }

                // Datenbank wird alle 15 Sekunden upgedated
                try
                {
                    Thread.Sleep(Settings.DatabaseUpdateCycleTime);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    IsInterrupted = true;
                }
            }
        }
    }
}
