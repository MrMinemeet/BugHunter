using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MySql.Data.MySqlClient;
using ProjectWhitespace;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace BugHunter
{
    public class Settings
    {
        // Öffentliche Variablen welche geändet werden können
        public bool IsActive = false;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool IsFullscreen = false;
        public bool IsMouseVisible = true;
        public bool AreDebugInformationsVisible = false;
        public bool IsDebugEnabled = false;
        public bool IsSendStatisticsAllowed = true;
        public bool HasInternetConnection = false;

        public string GUID= "";
        public string UserName = "";

        public void UpdateSettings(GraphicsDeviceManager gdm)
        {
            gdm.IsFullScreen = IsFullscreen;
            gdm.ApplyChanges();
        }

        // Programm Interne Einstellungen / Constanten
        public int MapSizeHeight { get; set; }
        public int MapSizeWidth { get; set; }
        public const int TilePixelSize = 64;
        public const byte HitBoxTileNumber = 18;
        public const byte PlayerSpawnTileId = 19;
        public const byte EnemeySpawnTileId = 21;
        public const byte ReloadTileId = 24;
        public const byte PowerupTileId = 20;
        public const byte generalMaxPowerUps = 6;
        public readonly string LoggingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter\log.txt");

        // DATABASE LOGIN
        public const string host = "projectwhitespace.net";      // Domain                              (projectwhitespace.net)
        public const int port = 60457;                           // MySQL Port von Portweiterleitung    (60457)
        public const string database = "BugHunter";              // Datenbankname                       (BugHunter)
        public const string username = "user";                   // Username                            (user)
        public const string password = "Z0pLFsZcviP1eXyK";       // Passwort                            (Z0pLFsZcviP1eXyK)

        private Game1 game = null;

        public Settings(Game1 game)
        {
            this.game = game;
        }

        public Texture2D EmptyTexture { get; set; }
        
        /// <summary>
        /// Lädt Einstellungen aus MyGames\Bughunter
        /// </summary>
        /// <returns>Liefet False wenn nichts geladen wurde</returns>
        public bool LoadSettings()
        {
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");

            StreamReader sr = null;
            bool DidLoad = false;

            try
            {
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);


                path = path + @"\Configuration.config";
                sr = new StreamReader(path);

                string line;
                StringBuilder sb;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("Username="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 9);
                        this.UserName = sb.ToString();
                        DidLoad = true;
                    }
                    if (line.Contains("Send-Stats="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 11);
                        if (sb.ToString().Equals("True") || sb.ToString().Equals("true"))
                            IsDebugEnabled = true;
                        else
                            IsDebugEnabled = false;
                        DidLoad = true;
                    }


                    if (line.Contains("GUID="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 5);
                        this.GUID = sb.ToString();
                        DidLoad = true;
                    }
                    if (line.Contains("Debug-Mode="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 11);
                        if (sb.ToString().Equals("True") || sb.ToString().Equals("true"))
                            IsDebugEnabled = true;
                        else
                            IsDebugEnabled = false;
                        DidLoad = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

            if(this.GUID.Length == 0)
            {
                this.GUID = Guid.NewGuid().ToString();
            }
            if(this.UserName.Length == 0)
            {
                this.UserName = Environment.UserName;
            }
            return DidLoad;
        }

        public void SaveSettings()
        {
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");

            StreamWriter sw = null;

            try
            {
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                
                path = path + @"\Configuration.config";
                sw = new StreamWriter(path);

                sw.WriteLine("// Nutzername für Statistiken");
                sw.WriteLine("Username=" + this.UserName);
                sw.WriteLine("// Sendet Statistiken an Globale Rankingliste und Globale Statistiken");
                sw.WriteLine("Send-Stats=" + this.IsSendStatisticsAllowed);
                sw.WriteLine();
                sw.WriteLine(" -- Please do not edit anything below this line! Maybe, most likely, definitly harm/break your gaming experience or game files. --");
                sw.WriteLine();
                sw.WriteLine("GUID=" + this.GUID);
                sw.WriteLine("Debug-Mode=" + this.IsDebugEnabled);

                sw.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        /// <summary>
        /// Lädt Informationen Spiel
        /// </summary>
        /// <returns>Liefet False wenn nichts geladen wurde</returns>
        public bool LoadGamedata()
        {
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");

            BinaryReader br = null;

            bool DidLoad = false;

            try
            {
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);

                path = path + @"\Game.data";
                br = new BinaryReader(new FileStream(path, FileMode.Open));

                string input;

                // Highscore einlesen und entschlüsseln
                input = br.ReadString();
                game.gameStats.HighScore = Encrypt.DecryptString(input, this.GUID);

                // Killed Enemies einlesen
                game.gameStats.KilledEnemies = br.ReadUInt32();
                game.gameStats.CollectedPowerups = br.ReadUInt32();
                game.gameStats.AnzahlSchuesse = br.ReadUInt32();
                game.gameStats.AnzahlTreffer = br.ReadUInt32();
                game.gameStats.AnzahlTode = br.ReadUInt32();


                game.gameStats.KilledEnemiesOld = game.gameStats.KilledEnemies;
                game.gameStats.CollectedPowerupsOld = game.gameStats.CollectedPowerups;
                game.gameStats.AnzahlSchuesseOld = game.gameStats.AnzahlSchuesse;
                game.gameStats.AnzahlTrefferOld = game.gameStats.AnzahlTreffer;
                game.gameStats.AnzahlTodeOld = game.gameStats.AnzahlTode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                DidLoad = false;
            }
            finally
            {
                if (br != null)
                    br.Close();
            }

            return DidLoad;
        }

        /// <summary>
        /// Speichert Spieldaten
        /// </summary>
        public void SaveGamedata()
        {
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");

            BinaryWriter bw = null;

            try
            {
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);

                path = path + @"\Game.data";
                bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));

                // Wert als String einlesen
                bw.Write(Encrypt.EncryptString(game.gameStats.HighScore.ToString(),this.GUID));


                bw.Write(game.gameStats.KilledEnemies);
                bw.Write(game.gameStats.CollectedPowerups);
                bw.Write(game.gameStats.AnzahlSchuesse);
                bw.Write(game.gameStats.AnzahlTreffer);
                bw.Write(game.gameStats.AnzahlTode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (bw != null)
                    bw.Close();
            }
        }

        public static void CheckInternetConnectionThread(Game1 game)
        {

            string connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            int WaitTime = 15;  // Wartezeit des Threads in Sekunden

            MySqlConnection connection = new MySqlConnection(connString);

            while (true)
            {
                try
                {
                    connection.Open();
                    game.settings.HasInternetConnection = true;
                    WaitTime = 15;
                }
                catch (MySqlException)
                {
                    game.settings.HasInternetConnection = false;

                    WaitTime *= 2;
                }
                finally
                {
                    if (connection.State.Equals(System.Data.ConnectionState.Open))
                        connection.Close();
                }
                

                if (WaitTime >= 3600)
                {
                    game.logger.Log("Seit " + (WaitTime / 2) + " keine Datenbankverbindung möglich. Versuche werden beendet", Thread.CurrentThread.Name);
                    Thread.CurrentThread.Abort();
                }

                game.logger.Log("Nächster Test in " + WaitTime + " Sekunden", Thread.CurrentThread.Name);
                try
                {
                    Thread.Sleep(WaitTime * 1000);
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e.Message);
                    game.logger.Log("Thread beendet", Thread.CurrentThread.Name, "Debug");
                    break;
                }
            }
        }
    }
}