using Microsoft.Win32;
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
        public bool IsSendStatsAllowed = true;
        public bool HasInternetConnection = false;
        public bool SendAnonymStatistics = true;
        public Byte Musiklautstaerke = 50;
        public Byte Soundlautstaerke = 50;
        public readonly string LoggingPath;
        public string NetVersion;
        public string Version;
        public bool GotNewHighscore = false;


        public string GUID= "";
        public string StatisticsGUID = "";
        public string UserName = "";

        public void UpdateSettings(GraphicsDeviceManager gdm)
        {
            gdm.IsFullScreen = IsFullscreen;
            gdm.ApplyChanges();
        }

        // Programm Interne Einstellungen / Constanten
        public static int DatabaseUpdateCycleTime = 15000;  // Wartezeit für Datenbank Updates.

        public int MapSizeHeight { get; set; }
        public int MapSizeWidth { get; set; }
        public const int TilePixelSize = 64;
        public const byte HitBoxTileNumber = 18;
        public const byte PlayerSpawnTileId = 19;
        public const byte EnemeySpawnTileId = 21;
        public const byte ReloadTileId = 24;
        public const byte PowerupTileId = 20;
        public const byte generalMaxPowerUps = 6;


        // DATABASE LOGIN
        public const string host = "projectwhitespace.net";         // Domain                              (projectwhitespace.net)
        public const int port = 60457;                              // MySQL Port von Portweiterleitung    (60457)
        public const string database = "BugHunter";                 // Datenbankname                       (BugHunter)
        public const string username = "GameClient";                // Username                            (user)
        public const string password = "VEYZQpx75ndBkBVqaHNQ";      // Passwort                            (Z0pLFsZcviP1eXyK)

        private Game1 game = null;

        public Settings(Game1 game)
        {
            this.game = game;
            if (IsLinux)
            {
                 LoggingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games/Bug Hunter/log.txt");
            }
            else
            {
                 LoggingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter\log.txt");
            }
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public Texture2D EmptyTexture { get; set; }
        
        /// <summary>
        /// Lädt Einstellungen aus MyGames\Bughunter
        /// </summary>
        /// <returns>Liefet False wenn nichts geladen wurde</returns>
        public bool LoadSettings()
        {
            string path;

            if (IsLinux)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games/Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"/Configuration.config";
            }
            else
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"\Configuration.config";
            }

            StreamReader sr = null;
            bool DidLoad = false;

            try
            {
                sr = new StreamReader(path);

                string line;
                StringBuilder sb;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("Username="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 9);
                        if (sb.Length > 15)
                            sb.Length = 15;
                        this.UserName = sb.ToString();
                        DidLoad = true;
                    }

                    if (line.Contains("Musiklautstärke="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 16);
                        this.Musiklautstaerke = Byte.Parse(sb.ToString());
                        DidLoad = true;
                    }
                    if (line.Contains("Soundlautstärke="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 16);
                        this.Soundlautstaerke = Byte.Parse(sb.ToString());
                        DidLoad = true;
                    }

                    if (line.Contains("Send-Stats="))
                    {
                        if (line.ToLower().Contains("true"))
                            SendAnonymStatistics = true;
                        else
                            SendAnonymStatistics = false;
                        DidLoad = true;
                    }
                    if (line.Contains("Send-Anonym-Statistics="))
                    {
                        if(line.ToLower().Contains("true"))
                            SendAnonymStatistics = true;
                        else
                            SendAnonymStatistics = false;
                        DidLoad = true;
                    }

                    if (line.Contains("GUID="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 5);
                        this.GUID = sb.ToString();
                        DidLoad = true;
                    }
                    
                    if (line.Contains("Statistics-ID="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 14);
                        this.StatisticsGUID = sb.ToString();
                        DidLoad = true;
                    }

                    if (line.Contains("Debug-Mode="))
                    {
                        if (line.ToLower().Contains("true"))
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

            if(this.StatisticsGUID.Length == 0)
            {

                this.StatisticsGUID = Guid.NewGuid().ToString();
            }

            if (this.UserName.Length == 0)
            {
                this.UserName = Environment.UserName;
            }

            // Überprüfen das Lautstärke zwischen 0% und 100% liegt
            if (Musiklautstaerke > 100)
                Musiklautstaerke = 100;
            else if (Musiklautstaerke < 0)
                Musiklautstaerke = 0;
            
            if (Soundlautstaerke > 100)
                Soundlautstaerke = 100;
            else if (Soundlautstaerke < 0)
                Soundlautstaerke = 0;

            return DidLoad;
        }

        public void SaveSettings()
        {
            string path;

            if (IsLinux)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games/Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"/Configuration.config";
            }
            else
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"\Configuration.config";
            }

            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(path);

                sw.WriteLine("// Nutzername (Max. 15 Zeichen)");
                sw.WriteLine("Username=" + this.UserName);
                sw.WriteLine();

                sw.WriteLine("// Lautstärken");
                sw.WriteLine("Musiklautstärke=" + this.Musiklautstaerke);
                sw.WriteLine("Soundlautstärke=" + this.Soundlautstaerke);
                sw.WriteLine();

                sw.WriteLine("// Sendet Statistiken an Globale Rankingliste und Globale Statistiken");
                sw.WriteLine("Send-Stats=" + this.IsSendStatsAllowed);
                sw.WriteLine();

                sw.WriteLine("// Senden von anonyme Statistiken");
                sw.WriteLine("Send-Anonym-Statistics=" + this.SendAnonymStatistics);
                sw.WriteLine();
                sw.WriteLine(" -- Please do not edit anything below this line! Maybe, most likely, definitly harm/break your gaming experience or game files. --");
                sw.WriteLine();
                sw.WriteLine("GUID=" + this.GUID);
                sw.WriteLine("Statistics-ID=" + this.StatisticsGUID);
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
            string path;

            if (IsLinux)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games/Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"/Game.data";
            }
            else
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"\Game.data";
            }

            BinaryReader br = null;

            bool DidLoad = false;

            try
            {
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
                game.gameStats.PlayTime = (long)br.ReadInt64(); // Long ist 64-Bit daher geht es auch mit Int64


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
            string path;

            if (IsLinux)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games/Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"/Game.data";
            }
            else
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My games\Bug Hunter");
                // Ordner Verzeichniss erstellen
                Directory.CreateDirectory(path);
                path = path + @"\Game.data";
            }

            BinaryWriter bw = null;

            try
            {
                bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));

                // Wert als String einlesen
                bw.Write(Encrypt.EncryptString(game.gameStats.HighScore.ToString(),this.GUID));


                bw.Write(game.gameStats.KilledEnemies);
                bw.Write(game.gameStats.CollectedPowerups);
                bw.Write(game.gameStats.AnzahlSchuesse);
                bw.Write(game.gameStats.AnzahlTreffer);
                bw.Write(game.gameStats.AnzahlTode);
                bw.Write(game.gameStats.PlayTime);
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
                    game.updateThread.Interrupt();
                    game.RankingListUpdateThread.Interrupt();
                    game.GlobalScoreListUpdateThread.Interrupt();
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
        public static string Get45PlusFromRegistry()
        {
            if (!IsLinux)
            {
                const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
                {
                    if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    {
                        return CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                    }
                    else
                    {
                        return "Below 4.5";
                    }
                }

                // Checking the version using >= enables forward compatibility.
                string CheckFor45PlusVersion(int releaseKey)
                {
                    if (releaseKey >= 461808)
                        return "4.7.2 or later";
                    if (releaseKey >= 461308)
                        return "4.7.1";
                    if (releaseKey >= 460798)
                        return "4.7";
                    if (releaseKey >= 394802)
                        return "4.6.2";
                    if (releaseKey >= 394254)
                        return "4.6.1";
                    if (releaseKey >= 393295)
                        return "4.6";
                    if (releaseKey >= 379893)
                        return "4.5.2";
                    if (releaseKey >= 378675)
                        return "4.5.1";
                    if (releaseKey >= 378389)
                        return "4.5";
                    // This code should never execute. A non-null release key should mean
                    // that 4.5 or later is installed.
                    return "No 4.5 or later version detected";
                }
            }
            else
            {
                return "Failed to optain .NET version";
            }
        }
    }
}