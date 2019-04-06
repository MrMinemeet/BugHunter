using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BugHunter
{
    class UsernameBlacklist
    {
        public static void GetUsernameBlacklistFromDatabase(Game1 game)
        {
            string path;
            int ValuesInFile = 0;

            if (Settings.IsLinux)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games/Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"/usernameBlacklist.bin";
            }
            else
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"\usernameBlacklist.bin";
            }

            // File öffnen um zu lesen, wieviele Inhalte drinnen sind
            BinaryReader br = null;

            try
            {
                // Versuchen Username Blacklist zu öffnen
                br = new BinaryReader(new FileStream(path, FileMode.Open));
                ValuesInFile = br.ReadInt32();
            }
            catch (Exception e)
            {
                // Kein Int als erstes oder kein File gefunden
                Console.WriteLine(e.Message);
            }
            finally
            {
                br?.Close();
            }

            // File zum schreiben öffnen
            BinaryWriter bw = null;

            try
            {
                // Versuchen Username Blacklist zu öffnen
                bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            MySqlConnection mySqlConnection = null;
            MySqlCommand mySqlCommand = null;
            MySqlDataReader reader = null;
            List<string> BadWordList = new List<string>();

            string connString = "Server=" + Settings.host + ";Database=Blacklists;port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            mySqlConnection = new MySqlConnection(connString);

            try
            {
                if (mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    // Verbindung muss erst aufgebaut werden
                    game.logger.Log("Datenbankverbindung wird aufgebaut", Thread.CurrentThread.Name, "Debug");
                    mySqlConnection.Open();

                }

                if (mySqlConnection.State == System.Data.ConnectionState.Open)
                {
                    // Größe der Blacklist abfragen
                    string query = "SELECT COUNT(*) FROM Username";

                    mySqlCommand = new MySqlCommand(query);
                    mySqlCommand.Connection = mySqlConnection;
                    reader = mySqlCommand.ExecuteReader();

                    reader.Read();

                    int ValuesInDB = reader.GetInt32(0);

                    reader.Close();

                    if (ValuesInDB > ValuesInFile)
                    {
                        // Daten abfragen
                        query = "SELECT `Username`.`Word` FROM `Username`";

                        mySqlCommand = new MySqlCommand(query);
                        mySqlCommand.Connection = mySqlConnection;
                        reader = mySqlCommand.ExecuteReader();

                        while (reader.Read())
                        {
                            BadWordList.Add(reader.GetString(0));
                        }

                        bw.Write(BadWordList.Count);

                        foreach (string word in BadWordList)
                            bw.Write(word);
                    }
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                bw?.Close();
                reader?.Close();
            }
        }
    }
}
