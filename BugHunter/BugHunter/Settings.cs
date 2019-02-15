using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectWhitespace;
using System;
using System.IO;
using System.Text;

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
        public const string host = "projectwhitespace.net";      // Domain von NoIP.com                 (projectwhitespace.net)
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
                    if (line.Contains("GUID="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 5);
                        this.GUID = sb.ToString();
                        DidLoad = true;
                    }
                    if (line.Contains("Username="))
                    {
                        sb = new StringBuilder(line);
                        sb = sb.Remove(0, 9);
                        this.UserName = sb.ToString();
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

                sw.WriteLine("Username=" + this.UserName);
                sw.WriteLine();
                sw.WriteLine(" -- Please do not edit anything below this line! Might harm/break your gaming experience. --");
                sw.WriteLine();
                sw.WriteLine("GUID=" + this.GUID); 
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
                game.gameStats.KilledEnemies = br.ReadInt32();
                game.gameStats.CollectedPowerups = br.ReadInt32();
                game.gameStats.AnzahlSchuesse = br.ReadInt32();
                game.gameStats.AnzahlTreffer = br.ReadInt32();
                game.gameStats.AnzahlTode = br.ReadInt32();
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
    }
}