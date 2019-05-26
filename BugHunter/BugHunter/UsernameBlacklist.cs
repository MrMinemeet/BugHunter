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
        // Login-Daten für Filter
        static string Blacklist_Username = "BlacklistUser";
        static string Blacklist_Password = "sF9CpQlWZJcD5RC0aNkQ";


        public static string CheckUsernameForBadWords(Game1 game, string username)
        {

            List<string> BadUsernameList = new List<string>();
            // Bad Words Liste Laden
            BinaryReader br = null;
            string path;

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

            try
            {
                br = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));

                string input;
                int AmountInFile = br.ReadInt32();

                for (int i = 1; i <= AmountInFile; i++)
                {
                    input = br.ReadString();
                    if (!input.Equals(""))
                        BadUsernameList.Add(input);
                }
            }
            catch (Exception e)
            {
                game.logger.Log(e.Message, Thread.CurrentThread.Name, "Warning");
                Console.WriteLine(e.Message);
            }
            finally
            {
                br?.Close();
            }

            if (username.Length > 0)
            {

                // Liste mit nicht erlaubten Usernamen durchlaufen
                foreach (string s in BadUsernameList)
                {
                    // Name vorbereiten
                    string name = s.ToLower().Replace(" ", "");

                    // Falls Name Wort von Blacklist enthält wird der Systemname verwendet
                    if (username.Contains(name.ToLower().Replace(" ", "")))
                    {
                        username = Environment.UserName;
                        game.logger.Log("Username ist nicht erlaubt. '" + username + "'", Thread.CurrentThread.Name, "Warnung");

                        // Überprüfen ob Systemname ein nichterlaubtes Wort enthält
                        if (username.Contains(name.ToLower().Replace(" ", "")))
                        {
                            game.logger.Log("Systemname ist nicht erlaubt", Thread.CurrentThread.Name, "Warnung");
                            username = "NameNotOK";
                        }
                        return username;
                    }
                }
            }

            return username;
        }

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

            string connString = "Server=" + Settings.host + ";Database=Blacklists;port=" + Settings.port + ";User Id=" + Blacklist_Username + ";password=" + Blacklist_Password;

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
