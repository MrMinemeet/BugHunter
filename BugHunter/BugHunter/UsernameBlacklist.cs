using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugHunter
{
    class UsernameBlacklist
    {
        public static void GetUsernameBlacklist(Game1 game)
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


            BinaryReader br = null;

            try
            {
                // Versuchen Username Blacklist zu öffnen
                br = new BinaryReader(new FileStream(path, FileMode.Open));

                ValuesInFile = br.ReadInt32();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                br?.Close();
            }

            MySqlConnection mySqlConnection = null;
            MySqlCommand mySqlCommand = null;
            MySqlDataReader reader = null;

            string connString = "Server=" + Settings.host + ";Database=" + Settings.database + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            if (game.settings.HasInternetConnection)
            {
                try
                {
                    if (mySqlConnection.State != System.Data.ConnectionState.Open)
                    {
                        // Verbindung muss erst aufgebaut werden
                        game.logger.Log("Datenbankverbindung wird aufgebaut", "GetUsernameBlacklist", "Debug");
                        mySqlConnection.Open();

                    }

                    if (mySqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        // Größe der Blacklist abfragen
                        string query = "SELECT GlobalHighscore.UserID FROM GlobalHighscore WHERE GlobalHighscore.UserID = '" + game.settings.GUID + "'";

                        mySqlCommand = new MySqlCommand(query);
                        mySqlCommand.Connection = mySqlConnection;
                        reader = mySqlCommand.ExecuteReader();

                        reader.Read();

                        reader.Close();
                    }
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
