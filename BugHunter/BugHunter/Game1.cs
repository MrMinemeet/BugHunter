/*
 * Entwickler: Alexander Voglsperger aka. MrMinemeet(4AHELS 2018/2019)
 * Gamestudio: Project-Whitespace (ProjectWhitespace.net)
 * Softwarename: BugHunter
 * Entwicklungszeitraum:  18.11.2018 - JETZT
 * 
 * 
 * Kurzbeschreibung:
 * Der Spieler muss gegen immer Stärker werdende Wellen von Bugs(Gegnern) kämpfen indem er mit Code auf sie schießt.
 * Neue Munition kann dadurch erhalten werden, wenn der Spieler an einen PC geht.
 * Jede Sprache hat zudem eine unterschiedliche Schussfrequenz und stärke.
 * zB.: Maschinencode schießt langsam aber macht viel Schaden, JS schießt schnell aber macht geringen Schaden.
 * 
 * Verwendete Software:
 * Tiled-Editor
 * Visual Studio Code
 * Visual Studio 2017
 * MonoGame FrameWork
 * MonoGame.Extended Framework
 * Photoshop CC 2019
 * TexturePackerGUI 
 * 
 * Datenbank:
 * MariaDB
 * NaviCat
 * phpMyAdmin
 * 
 * APIs:
 * MySQL Connect
 * 
 * CopyRight:
 * Alle Rechte der Bilder, Spiellogik und Spielidee gehören den rechtmäßigen Eigentümern.
 * Das unerlaubte Kopieren, Veröffentlichen, Verleihen und öffentliches Vorführen ist verboten!
 */


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using ProjectWhitespace;
using System;
using System.Collections.Generic;
using TexturePackerLoader;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Content;

namespace BugHunter
{
    // This is the main type for your game.
    public class Game1 : Game
    {
        private int StatsBoostGiven = 1;

        // Zeit für aktuellen Lauf
        public Stopwatch stopwatch = new Stopwatch();
        public long CurrentRunTime = 0;

        public Logger logger = null;
        public Texture2D pauseScreen;

        private readonly TimeSpan timePerFrame = TimeSpan.FromSeconds(3f / 30f);

        // Gegner
        int MaxEnemies = 1;

        public List<Android> AndroidsList = new List<Android>();
        int AndroidHealth = 30;
        int AndroidDamage = 1;

        public List<Windows> WindowsList = new List<Windows>();
        int WindowsHealth = 30;
        int WindowsDamage = 1;

        public List<iOS> iOSList = new List<iOS>();
        int iOSHealth = 30;
        int iOSDamage = 1;


        public List<Powerup> Powerups = new List<Powerup>();

        public List<Map> map = new List<Map>();
        GUI gui;
        public SoundFX sound = new SoundFX();

        int AktuelleMap = 0;

        // Standardeinstellungen setzen
        public Settings settings;
         
        public int[][] MapArray;
        public int[][] EnemySpawnPointsArray;

        public double LastKeyStrokeInput = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteRender spriteRender;
        public SpriteFont font;
        public SpriteFont MenuFont;
        public Weapon weapon;
        public Player player;
                     
        public Random random = new Random();

        // Threads
        bool ThreadsHaveStarted = false;
        public Thread updateThread;
        public Thread RankingListUpdateThread;
        public Thread GlobalScoreListUpdateThread;
        Thread CheckDatabaseConnectionThread;
        public Thread SendStatisticsThread;
        public Thread GetBadusernameListThread;
        public Thread CheckNameThread;

        // DEBUG Featurese
        FpsCounter fps = new FpsCounter();
        public SpriteFont DebugFont;

        public enum GameState : Byte { Ingame, Paused, DeathScreen, Hauptmenu, Stats, Settings };
        public GameState CurrentGameState = GameState.Hauptmenu;

        private SpriteSheetLoader spriteSheetLoader;
        public GraphicsDevice graphicsDevice;
        public Stats gameStats;

        // Score
        public int Score { get; set; }

        // Animation

        // POOF Animation
        private SpriteSheet PoofSpriteSheet;
        private Animation[] PoofAnimations;
        private AnimationManager poofAM;
        private bool PoofIsActive = false;
        private Vector2 PoofPosition;

        // Texturen
        public SpriteSheet spriteSheet;
        public Texture2D rect;

        // Menü
        enum Menubuttons : Byte { Spielen, Einstellungen, Stats, Beenden };
        Menubuttons aktuellerMenupunkt = Menubuttons.Spielen;
        double lastMenuButtonSwitch = 0;
        SettingsMenu settingsMenu;


        // Klassen
        public Requests requests;
        Konsole GameConsole;

        public Game1()
        {
            gameStats = new Stats();
            this.settings = new Settings(this);
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = settings.resolutionWidth,
                PreferredBackBufferHeight = settings.resolutionHeight,
                IsFullScreen = settings.IsFullscreen
            };

            graphics.SynchronizeWithVerticalRetrace = true;

            IsMouseVisible = settings.IsMouseVisible;
            Content.RootDirectory = "Content";

            Thread.CurrentThread.Name = "MainThread";
        }

        // Allows the game to perform any initialization it needs to before starting to run.
        // This is where it can query for any required services and load any non-graphic
        // related content.  Calling base.Initialize will enumerate through any components and initialize them as well.
        protected override void Initialize()
        {
            requests = new Requests(this);

            this.graphicsDevice = GraphicsDevice;

            logger = new Logger(this.settings.LoggingPath);

            Stopwatch sw = new Stopwatch();

            this.Score = 0;

            map.Add(new Map());

            spriteSheetLoader = new SpriteSheetLoader(Content, GraphicsDevice);

            weapon = new Weapon();

            settingsMenu = new SettingsMenu(this);

            settings.Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            base.Initialize();
        }
        
        // LoadContent will be called once per game and is the place to load all of your content.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteRender = new SpriteRender(spriteBatch);

            this.spriteSheet = spriteSheetLoader.Load("sprites/entities/entities.png");


            // Verarbeitete Map aus Pipeline laden
            map[AktuelleMap].maplevel = Content.Load<TiledMap>("map1");
            // MapRenderer für die Map erstellen
            map[AktuelleMap].mapRenderer = new TiledMapRenderer(GraphicsDevice, map[AktuelleMap].GetTiledMap());

            settings.MapSizeHeight = map[AktuelleMap].GetTiledMap().Height;
            settings.MapSizeWidth = map[AktuelleMap].GetTiledMap().Width;
            settings.EmptyTexture = Content.Load<Texture2D>("sprites/empty");
            settings.LoadSettings();
            settings.LoadGamedata();

            gui = new GUI(this);

            // TMX (wie CSV) Map in 2D Array wandeln
            MapArray = Converter.MapToIntArray(map[AktuelleMap].maplevel, settings, @"Collision/Trigger");
            EnemySpawnPointsArray = Converter.MapToIntArray(map[AktuelleMap].maplevel, settings, @"EnemySpawnPos");


            // <== PLAYER ==>
            try
            {
                // Audio
                sound.ScoreSound = Content.Load<SoundEffect>("audio/Score");
                sound.HintergrundMusik = Content.Load<SoundEffect>("audio/Musik");
                sound.HintergrundMusikEffect = sound.HintergrundMusik.CreateInstance();
                sound.Schuesse[0] = Content.Load<SoundEffect>("audio/Schuss/schuss_001");
                sound.Schuesse[1] = Content.Load<SoundEffect>("audio/Schuss/schuss_002");
                sound.Schuesse[2] = Content.Load<SoundEffect>("audio/Schuss/schuss_003");
                sound.Schuesse[3] = Content.Load<SoundEffect>("audio/Schuss/schuss_004");
                sound.Schuesse[4] = Content.Load<SoundEffect>("audio/Schuss/schuss_005");
                sound.Schuesse[5] = Content.Load<SoundEffect>("audio/Schuss/schuss_006");
            }
            catch (ContentLoadException e)
            {
                logger.Log("Beim Player Sounds laden ist ein Fehler aufgetreten", Thread.CurrentThread.Name, "Error");
                this.Exit();
            }

            try { 
                // Deathsounds laden
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_1"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_2"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_3"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_4"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_5"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_6"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_7"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_8"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_9"));
                sound.MaleDeathSound.Add(Content.Load<SoundEffect>("audio/Male/Death/death_10"));
            }
            catch (ContentLoadException e)
            {
                logger.Log("Beim Player Sounds laden ist ein Fehler aufgetreten", Thread.CurrentThread.Name, "Error");
                this.Exit();
            }

            try { 
                // Damagesounds laden
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_1"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_2"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_3"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_4"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_5"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_6"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_7"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_8"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_9"));
                sound.MaleDamageSound.Add(Content.Load<SoundEffect>("audio/Male/Damage/damage_10"));
            }
            catch (ContentLoadException e)
            {
                logger.Log("Beim Player Sounds laden ist ein Fehler aufgetreten", Thread.CurrentThread.Name, "Error");
                this.Exit();
            }

            // <== GEGNER ==>
            try { 
                sound.EnemieDeathSound.Add(Content.Load<SoundEffect>("audio/Enemies/Death/Enemie_Damage_1"));
                sound.EnemieDeathSound.Add(Content.Load<SoundEffect>("audio/Enemies/Death/Enemie_Damage_2"));
                sound.EnemieDeathSound.Add(Content.Load<SoundEffect>("audio/Enemies/Death/Enemie_Damage_3"));
                sound.EnemieDeathSound.Add(Content.Load<SoundEffect>("audio/Enemies/Death/Enemie_Damage_4"));
                sound.EnemieDeathSound.Add(Content.Load<SoundEffect>("audio/Enemies/Death/Enemie_Damage_5"));
                sound.EnemieDeathSound.Add(Content.Load<SoundEffect>("audio/Enemies/Death/Enemie_Damage_6"));
            }
            catch (ContentLoadException e)
            {
                logger.Log("Beim Gegner Sounds laden ist ein Fehler aufgetreten", Thread.CurrentThread.Name, "Error");
                this.Exit();
            }

            // Schriften
            try { 
                font = Content.Load<SpriteFont>("Font");
                DebugFont = Content.Load<SpriteFont>("Debug");
                MenuFont = Content.Load<SpriteFont>("MenuFont");
            }
            catch (ContentLoadException e)
            {
                logger.Log("Beim Schriftarten laden ist ein Fehler aufgetreten", Thread.CurrentThread.Name, "Error");
                this.Exit();
            }

            // Spieler Init
            player = new Player(this,200f,100);
            player.camera = new OrthographicCamera(GraphicsDevice);
            player.OriginTexture = Content.Load<Texture2D>("sprites/originSpot");
            player.Init(this.settings, this, this.sound);

            // Setze Spielerposition auf SpawnTilekoordinaten
            player.SetSpawnFromMap(MapArray);

            // GUI Init
            gui.spriteSheet = spriteSheetLoader.Load("gui_packed.png");
            gui.CustomCurserTexture = Content.Load<Texture2D>("sprites/mauszeiger");

            // Animation

            this.PoofSpriteSheet = spriteSheetLoader.Load("effects_packed.png");
            InitialiseAnimationManager();

            // Threads werden nur erstellt, wenn Datenbankstatistiken erlaubt sind um Ressourcen zu sparen
            if (settings.IsSendStatsAllowed)
            {
                // Threads zuweisen/erstellen
                updateThread = new Thread(() => Database.UpdateDatabaseThread(this));
                updateThread.Name = "updateThread";

                RankingListUpdateThread = new Thread(() => Database.GetRankingListThread(this));
                RankingListUpdateThread.Name = "RankingListUpdateThread";

                GlobalScoreListUpdateThread = new Thread(() => Database.GetGlobalScoreList(this));
                GlobalScoreListUpdateThread.Name = "GlobalScoreListUpdateThread";

                CheckDatabaseConnectionThread = new Thread(() => Settings.CheckInternetConnectionThread(this));
                CheckDatabaseConnectionThread.Name = "CheckDatabaseConnectionThread";

                SendStatisticsThread = new Thread(() => Database.SendAnonymStatistics(this));
                SendStatisticsThread.Name = "SendStatisticsThread";

                CheckNameThread = new Thread(() => settingsMenu.CheckNameThreadMethod(this));
                CheckNameThread.Name = "CheckNameThread";

                // Threads Starten
                CheckDatabaseConnectionThread.Start();
            }

            pauseScreen = new Texture2D(graphics.GraphicsDevice, settings.resolutionWidth, settings.resolutionHeight);
            Color[] data = new Color[settings.resolutionHeight * settings.resolutionWidth];
            for (int i = 0; i < data.Length; ++i) data[i] = new Color(0,0,0,128);
            pauseScreen.SetData(data);


            // GameConsole initialisieren
            GameConsole = new Konsole(this);
        }

        // UnloadContent will be called once per game and is the place to unload game-specific content.
        protected override void UnloadContent()
        {
            if (!SendStatisticsThread.ThreadState.Equals(System.Diagnostics.ThreadState.Wait))
            {
                updateThread?.Interrupt();
                RankingListUpdateThread?.Interrupt();
                GlobalScoreListUpdateThread?.Interrupt();
                CheckDatabaseConnectionThread?.Interrupt();
                SendStatisticsThread?.Interrupt();
                CheckNameThread?.Interrupt();
            }

            // Speichern von Einstellungen
            settings.SaveSettings();

            // Speichern von Spielkritischen Daten
            settings.SaveGamedata();

            // Gamepadvibrationen ausschalten
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);

            // Spiel beenden
            logger.Log("Spiel beenden", Thread.CurrentThread.Name);
            logger.WriteLog();
        }

        // Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // GameKonsole updaten
            GameConsole.Update(gameTime);

            // Falls eine Internetverbindung besteht und mehr als 120 seit dem letzen Request vergangen sind, wird geschaut ob eine neue Version verfügbar ist
            if (settings.HasInternetConnection && gameTime.TotalGameTime.TotalSeconds - requests.LastAvailibleVersionCheck >= 60)
            {
                requests.GetLatestAvailableVersion(gameTime);
            }

            if (settings.IsSendStatsAllowed && !ThreadsHaveStarted)
            {
                updateThread.Start();
                RankingListUpdateThread.Start();
                GlobalScoreListUpdateThread.Start();

                GetBadusernameListThread = new Thread(() => UsernameBlacklist.GetUsernameBlacklistFromDatabase(this));
                GetBadusernameListThread.Name = "GetBadUsernameListThread";
                GetBadusernameListThread.Start();

                CheckNameThread.Start();

                if (settings.SendAnonymStatistics)
                {
                    SendStatisticsThread.Start();
                }

                ThreadsHaveStarted = true;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                CurrentGameState = GameState.Paused;
                System.Diagnostics.Process.Start("https://discordapp.com/invite/rDzmQeC");
            }

            // Spiel schließen
            if (Keyboard.GetState().IsKeyDown(Keys.Delete) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                Exit();               
            }

            // Spiel in Vollbild machenx
            if (Keyboard.GetState().IsKeyDown(Keys.F11) || (Keyboard.GetState().IsKeyDown(Keys.LeftAlt) && Keyboard.GetState().IsKeyDown(Keys.Enter)))
            {
                if(gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                {
                    graphics.ToggleFullScreen();
                    LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }

            // Hauptmenü
            if(CurrentGameState == GameState.Hauptmenu)
            {
                if ((Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp)) && gameTime.TotalGameTime.TotalMilliseconds - lastMenuButtonSwitch >= 250)
                {
                    switch (aktuellerMenupunkt)
                    {
                        case Menubuttons.Spielen:
                            this.aktuellerMenupunkt = Menubuttons.Stats;
                            break;
                        case Menubuttons.Stats:
                            this.aktuellerMenupunkt = Menubuttons.Einstellungen;
                            break;
                        case Menubuttons.Einstellungen:
                            this.aktuellerMenupunkt = Menubuttons.Beenden;
                            break;
                        case Menubuttons.Beenden:
                            this.aktuellerMenupunkt = Menubuttons.Spielen;
                            break;
                    }

                    lastMenuButtonSwitch = gameTime.TotalGameTime.TotalMilliseconds;
                }

                if ((Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp)) && gameTime.TotalGameTime.TotalMilliseconds - lastMenuButtonSwitch >= 250)
                {
                    switch (aktuellerMenupunkt)
                    {
                        case Menubuttons.Spielen:
                            this.aktuellerMenupunkt = Menubuttons.Beenden;
                            break;
                        case Menubuttons.Stats:
                            this.aktuellerMenupunkt = Menubuttons.Spielen;
                            break;
                        case Menubuttons.Einstellungen:
                            this.aktuellerMenupunkt = Menubuttons.Stats;
                            break;
                        case Menubuttons.Beenden:
                            this.aktuellerMenupunkt = Menubuttons.Einstellungen;
                            break;
                    }

                    lastMenuButtonSwitch = gameTime.TotalGameTime.TotalMilliseconds;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
                {
                    switch (aktuellerMenupunkt)
                    {
                        case Menubuttons.Spielen:
                            stopwatch.Start();
                            ResetGame();
                            this.CurrentGameState = GameState.Ingame;
                            break;
                        case Menubuttons.Stats:
                            this.CurrentGameState = GameState.Stats;
                            break;
                        case Menubuttons.Einstellungen:
                            this.CurrentGameState = GameState.Settings;
                            LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                            break;
                        case Menubuttons.Beenden:
                            Exit();
                            break;
                    }
                }
            }

            // Stats
            if(CurrentGameState == GameState.Stats)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                    this.CurrentGameState = GameState.Hauptmenu;
            }

            // Einstellungen
            if(CurrentGameState == GameState.Settings){
                settingsMenu.Update(gameTime);
            }

            // Ingame
            if (CurrentGameState == GameState.Ingame)
            {
                // Musiklautstärke zuweisen
                if (sound.HintergrundMusikEffect.Volume != settings.Musiklautstaerke)
                    sound.HintergrundMusikEffect.Volume = (float)settings.Musiklautstaerke / 100f;


                this.CurrentRunTime = stopwatch.ElapsedMilliseconds;
                if(this.Score >= (5000 + StatsBoostGiven) && Score != 0)
                {
                    player.MaxHealth += 25;
                    player.Health += 25;
                    player.Damageboost += 10;
                    this.StatsBoostGiven += 5000;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.F3) && Keyboard.GetState().IsKeyUp(Keys.Tab) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                {
                    settings.AreDebugInformationsVisible = !settings.AreDebugInformationsVisible;
                    LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }
                
                if (this.Score > int.Parse(gameStats.HighScore))
                {
                    // Flag für neuen Highscore setzen
                    settings.GotNewHighscore = true;
                    gameStats.HighScore = Score.ToString();
                    sound.ScoreSound.Play((settings.Soundlautstaerke / 100), 0, 0);
                }
                
                MaxEnemies = (int)(this.Score / 1000) + 1;

                // Falls weniger Gegner Aktiv sind als Maximal zugelassen sind, dann werden neue gespawnt
                if(WindowsList.Count + AndroidsList.Count + iOSList.Count < MaxEnemies)
                {
                    Rectangle enemyHitbox;
                    Rectangle playersBox = new Rectangle((int)(player.Position.X - Settings.TilePixelSize * 2), (int)(player.Position.Y - Settings.TilePixelSize * 2),Settings.TilePixelSize * 4, Settings.TilePixelSize * 4);

                    SpriteFrame sp = null;

                    switch (random.Next(3))
                    {
                        // Spawn Android
                        case 0:
                            this.AndroidHealth += 5;
                            AndroidsList.Add(new Android(50f, AndroidHealth, AndroidDamage, this));
                            AndroidsList[AndroidsList.Count - 1].SetSpawnFromMap(EnemySpawnPointsArray);

                            sp = spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android1);

                            enemyHitbox = new Rectangle((int)(AndroidsList[AndroidsList.Count - 1].Position.X - sp.Size.X / 2), (int)(AndroidsList[AndroidsList.Count - 1].Position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);

                            while (enemyHitbox.Intersects(playersBox))
                            {
                                AndroidsList[AndroidsList.Count - 1].SetSpawnFromMap(EnemySpawnPointsArray);

                                enemyHitbox = new Rectangle((int)(AndroidsList[AndroidsList.Count - 1].Position.X - sp.Size.X / 2), (int)(AndroidsList[AndroidsList.Count - 1].Position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);
                            }
                            break;

                        // Spawn Windows
                        case 1:
                            this.WindowsHealth += 5;
                            WindowsList.Add(new Windows(50f, WindowsHealth, WindowsDamage, this, this.settings, this.player));
                            WindowsList[WindowsList.Count - 1].SetSpawnFromMap(EnemySpawnPointsArray);

                            sp = spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Windows1);

                            enemyHitbox = new Rectangle((int)(WindowsList[WindowsList.Count - 1].Position.X - sp.Size.X / 2), (int)(WindowsList[WindowsList.Count - 1].Position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);

                            while (enemyHitbox.Intersects(playersBox))
                            {
                                WindowsList[WindowsList.Count - 1].SetSpawnFromMap(EnemySpawnPointsArray);

                                enemyHitbox = new Rectangle((int)(WindowsList[WindowsList.Count - 1].Position.X - sp.Size.X / 2), (int)(WindowsList[WindowsList.Count - 1].Position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);
                            }
                            break;

                        // Spawn Windows
                        case 2:
                            this.iOSHealth += 5;
                            iOSList.Add(new iOS(50f, iOSHealth, iOSDamage, this, this.settings, this.player));
                            iOSList[iOSList.Count - 1].SetSpawnFromMap(EnemySpawnPointsArray);

                            sp = spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.IOS_10);

                            enemyHitbox = new Rectangle((int)(iOSList[iOSList.Count - 1].Position.X - sp.Size.X / 2), (int)(iOSList[iOSList.Count - 1].Position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);

                            while (enemyHitbox.Intersects(playersBox))
                            {
                                iOSList[iOSList.Count - 1].SetSpawnFromMap(EnemySpawnPointsArray);

                                enemyHitbox = new Rectangle((int)(iOSList[iOSList.Count - 1].Position.X - sp.Size.X / 2), (int)(iOSList[iOSList.Count - 1].Position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);
                            }
                            break;
                    }
                }


                // Updaten
                for (int i = 0; i < iOSList.Count; i++)
                {
                    iOSList[i].Update(gameTime, EnemySpawnPointsArray, map[AktuelleMap].maplevel);

                    if (iOSList[i].IsDead)
                    {
                        sound.EnemieDeathSound[random.Next(sound.EnemieDeathSound.Count - 1)].Play((float)settings.Soundlautstaerke / 100f, 0, 0);

                        gameStats.KilledEnemies++;
                        PoofPosition = iOSList[i].Position;
                        PoofIsActive = true;

                        // 10% Chance das sich der Schaden erhöht
                        if (random.Next(100) < 10)
                        {
                            iOSDamage += 1;
                        }
                        // 25% Chance dass ein Powerup spawnt
                        if (random.Next(100) < 25)
                        {
                            Powerups.Add(new Powerup(this, spriteSheetLoader.Load("sprites/entities/entities.png"), new SpriteRender(spriteBatch), settings, iOSList[i].Position));
                        }
                        iOSList.Remove(iOSList[i]);
                    }
                }

                for (int i = 0; i < AndroidsList.Count; i++)
                {
                    AndroidsList[i].Update(gameTime, EnemySpawnPointsArray, map[AktuelleMap].maplevel);

                    if (AndroidsList[i].IsDead)
                    {
                        sound.EnemieDeathSound[random.Next(sound.EnemieDeathSound.Count - 1)].Play((float)settings.Soundlautstaerke / 100f, 0, 0);
                    
                        gameStats.KilledEnemies++;
                        PoofPosition = AndroidsList[i].Position;
                        PoofIsActive = true;

                        // 10% Chance das sich der Schaden erhöht
                        if(random.Next(100) < 10)
                        {
                            AndroidDamage += 1;
                        }
                        // 25% Chance dass ein Powerup spawnt
                        if (random.Next(100) < 25)
                        {
                            Powerups.Add(new Powerup(this, spriteSheetLoader.Load("sprites/entities/entities.png"), new SpriteRender(spriteBatch), settings, AndroidsList[i].Position));
                        }
                        AndroidsList.Remove(AndroidsList[i]);                        
                    }
                }

                for (int i = 0; i < WindowsList.Count; i++)
                {
                    WindowsList[i].Update(gameTime, MapArray, map[AktuelleMap].maplevel);

                    if (WindowsList[i].IsDead)
                    {
                        sound.EnemieDeathSound[random.Next(sound.EnemieDeathSound.Count - 1)].Play((float)settings.Soundlautstaerke / 100f, 0, 0);

                        gameStats.KilledEnemies++;
                        PoofPosition = WindowsList[i].Position;
                        PoofIsActive = true;
                        // 10% Chance das sich der Schaden erhöht
                        if (random.Next(100) < 10)
                        {
                            WindowsDamage += 1;
                        }
                        // 25% Chance dass ein Powerup spawnt
                        if (random.Next(100) < 25)
                        {
                            Powerups.Add(new Powerup(this, spriteSheetLoader.Load("sprites/entities/entities.png"), new SpriteRender(spriteBatch), this.settings, WindowsList[i].Position));
                        }
                        WindowsList.Remove(WindowsList[i]);
                    }
                }

                // Debug Feature
                if (Keyboard.GetState().IsKeyDown(Keys.F4) && Keyboard.GetState().IsKeyDown(Keys.LeftControl) && settings.IsDebugEnabled)
                {
                    player.Health = 0;
                    // zufälligen Death sound abspielen
                    int randomValue = random.Next(0, sound.MaleDeathSound.Count);
                    sound.MaleDeathSound[randomValue].Play((settings.Soundlautstaerke / 100f),0,0);
                }

                player.Update(gameTime, MapArray, map[AktuelleMap].GetTiledMap());
                gui.Update(gameTime, player);

                // MapRenderer  updaten
                map[AktuelleMap].mapRenderer.Update(gameTime);


                // Update the fps
                fps.Update(gameTime);

                if(sound.HintergrundMusikEffect.State == SoundState.Stopped)
                {
                    sound.HintergrundMusikEffect.Play();
                }

                // Überprüft ob Powerup gelöscht wurde und löscht es falls ja
                for (int i = 0; i < Powerups.Count; i++)
                {
                    if (Powerups.Contains(Powerups[i]))
                    {
                        if (Powerups[i].WasCollected(this.player))
                        {
                            gameStats.CollectedPowerups++;
                            Powerups.Remove(Powerups[i]);
                            break;
                        }
                    }
                }                

                // Updated Powerups
                foreach(Powerup p in Powerups)
                    p.Update(gameTime, this.player);
                
                if (PoofIsActive)
                {
                    poofAM.Update(gameTime);

                    // Setzt poof auf inaktiv wenn abgelaufen
                    if (poofAM.CurrentFrame == PoofAnimations[0].Sprites.Length - 1)
                    {
                        PoofIsActive = false;
                    }
                }
            }

            // Deathscreen
            if (player.Health <= 0 && CurrentGameState == GameState.Ingame)
            {
                sound.HintergrundMusikEffect.Stop();
                stopwatch.Stop();
                gameStats.PlayTime += stopwatch.ElapsedMilliseconds;
                gameStats.AnzahlTode++;
                CurrentGameState = GameState.DeathScreen;
            }

            // Respawn wenn in Deathscreen
            if((Keyboard.GetState().IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A)) && CurrentGameState == GameState.DeathScreen)
            {
                settings.GotNewHighscore = false;
                player.Health = player.MaxHealth;
                this.CurrentGameState = GameState.Hauptmenu;
            }

            // PAUSE
            if((Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start)) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
            {
                if (CurrentGameState == GameState.Ingame)
                {
                    // Hintergrundmusik pausieren
                    if (sound.HintergrundMusikEffect.State == SoundState.Playing)
                    {
                        stopwatch.Stop();
                        sound.HintergrundMusikEffect.Stop();
                    }
                    CurrentGameState = GameState.Paused;
                }
                else if (CurrentGameState == GameState.Paused)
                {
                    // Hintergrundmusik pausieren
                    if (sound.HintergrundMusikEffect.State == SoundState.Paused || sound.HintergrundMusikEffect.State == SoundState.Stopped)
                    {
                        sound.HintergrundMusikEffect.Play();
                    }
                    stopwatch.Start();
                    CurrentGameState = GameState.Ingame;
                }

                LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

            logger.WriteLog();

            base.Update(gameTime);
        }

        // This is called when the game should draw itself.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        { 


            // Schwarzer Hintergrund
            GraphicsDevice.Clear(Color.TransparentBlack);
            
            if (CurrentGameState == GameState.Stats)
            {
                spriteBatch.Begin();
                ScoreMenu.ShowScoreMenu(spriteBatch, this);
                spriteBatch.End();
            }

            if(CurrentGameState == GameState.Settings)
            {
                spriteBatch.Begin();
                settingsMenu.Draw(spriteBatch);
                spriteBatch.End();
            }

            if (CurrentGameState == GameState.Ingame || CurrentGameState == GameState.Paused || CurrentGameState == GameState.DeathScreen)
            {
                var transformMatrix = player.camera.GetViewMatrix();
                spriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointClamp);

                // Display Debug-Information
                if (settings.AreDebugInformationsVisible)
                {
                    fps.DrawFps(spriteBatch, DebugFont,
                        player.camera.Position,
                        Color.White);

                    spriteBatch.DrawString(DebugFont,
                        "Player:\n" + " X: " + ((int)player.Position.X).ToString() + " Y: " + ((int)player.Position.Y).ToString(),
                        new Vector2(player.Position.X - (settings.resolutionWidth / 2), player.Position.Y - (settings.resolutionHeight / 2) + 125),
                        Color.White);

                    spriteBatch.DrawString(DebugFont, "Dmg-Boost: " + player.Damageboost.ToString(), new Vector2(player.Position.X - (settings.resolutionWidth / 2), player.Position.Y - (settings.resolutionHeight / 2) + 185), Color.White);
                }

                if (CurrentGameState == GameState.Ingame || CurrentGameState == GameState.Paused || CurrentGameState == GameState.DeathScreen)
                {
                    // MapRenderer zum Zeichnen der aktuell sichtbaren Map
                    map[AktuelleMap].mapRenderer.Draw(player.camera.GetViewMatrix());


                    foreach (Android android in AndroidsList)
                    {
                        android.Draw(spriteBatch, font);
                    }

                    foreach (Windows windows in WindowsList)
                    {
                        windows.Draw(spriteBatch, font);
                    }

                    foreach (iOS ios in iOSList)
                    {
                        ios.Draw(spriteBatch, font);
                    }

                    foreach (Powerup powerup in Powerups)
                        powerup.Draw(spriteBatch);

                    // Sprite ausgeben
                    player.Draw(spriteBatch, font);

                    if (PoofIsActive)
                    {
                        spriteRender.Draw(
                            poofAM.CurrentSprite,
                            PoofPosition,
                            Color.White, 0, 1,
                            poofAM.CurrentSpriteEffects);
                    }

                    gui.Draw(spriteBatch, font, player);

                    if (CurrentGameState == GameState.Paused)
                    {
                        spriteBatch.Draw(pauseScreen, new Vector2(player.camera.Position.X, player.camera.Position.Y));
                        
                        spriteBatch.DrawString(MenuFont, "PAUSE", new Vector2(player.Position.X - 100, player.Position.Y - 64), Color.White);
                        spriteBatch.DrawString(MenuFont, Texttable_DE.Stats_Highscore + gameStats.HighScore, new Vector2(player.camera.Position.X + 725, player.camera.Position.Y), Color.White);
                    }

                    if (CurrentGameState == GameState.DeathScreen)
                    {
                        // Schwarzer Blend Effekt
                        spriteBatch.Draw(pauseScreen, new Vector2(player.camera.Position.X, player.camera.Position.Y));

                        // Scores
                        if(settings.GotNewHighscore)
                        {
                            spriteBatch.DrawString(MenuFont, Texttable_DE.Stats_Neuer_Highscore + gameStats.HighScore, new Vector2(player.camera.Position.X + 700, player.camera.Position.Y), Color.GreenYellow);
                        }
                        else
                        {
                            spriteBatch.DrawString(MenuFont, Texttable_DE.Stats_Highscore + gameStats.HighScore, new Vector2(player.camera.Position.X + 725, player.camera.Position.Y), Color.White);
                        }

                        spriteBatch.DrawString(font, Texttable_DE.Stats_Score + Score, new Vector2(player.camera.Position.X + 950, player.camera.Position.Y + 100), Color.White);


                        spriteBatch.DrawString(MenuFont, Texttable_DE.Text_Died, new Vector2(player.Position.X - 300, player.Position.Y - 64), Color.White);
                    }
                    TimeSpan time = stopwatch.Elapsed;
                    spriteBatch.DrawString(font, time.ToString("mm\\:ss\\.ff"), new Vector2(player.Position.X + 700, player.Position.Y - 500), Color.White);
                }

                spriteBatch.End();
            }
            // Hauptmenü
            if (CurrentGameState == GameState.Hauptmenu)
            {
                spriteBatch.Begin();

                // Schreibt Build-Version in Ecke
                spriteBatch.DrawString(DebugFont, "Buildverion: " + settings.Version, new Vector2(0, 0), Color.White);

                if (!settings.AvailableVersion.Equals(settings.Version) && settings.AvailableVersion != "")
                {
                    spriteBatch.DrawString(DebugFont, Texttable_DE.General_NeueVersionVerfuegbar, new Vector2(0, 30), Color.Orange);
                }


                // Zeichnet alle Menüpunkte
                spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Start, new Vector2(400, 300), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Stats, new Vector2(400, 400), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Einstellungen, new Vector2(400, 500), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Ende, new Vector2(400, 600), Color.White);

                // Überzeichnet den Menüpunkt der ausgewählt ist.
                switch (aktuellerMenupunkt)
                {
                    case Menubuttons.Spielen:
                        spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Start, new Vector2(400,300), Color.YellowGreen);
                        break;
                    case Menubuttons.Stats:
                        spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Stats, new Vector2(400,400), Color.YellowGreen);
                        break;
                    case Menubuttons.Einstellungen:
                        spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Einstellungen, new Vector2(400,500), Color.YellowGreen);
                        break;
                    case Menubuttons.Beenden:
                        spriteBatch.DrawString(MenuFont, Texttable_DE.Menu_Ende, new Vector2(400,600), Color.YellowGreen);
                        break;
                }
                spriteBatch.End();
            }


            // GameConsole zeichnen
            spriteBatch.Begin();
            GameConsole.Draw(spriteBatch);
            spriteBatch.End();


            base.Draw(gameTime);
        }
        private void InitialiseAnimationManager()
        {
            // Poof
            var poof = new[] {
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_001,
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_002,
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_003,
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_004,
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_005,
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_006,
                TexturePackerMonoGameDefinitions.Effect_packed.Poof_007
            };
            
            var poofAnimation = new Animation(new Vector2(1, 0), timePerFrame, SpriteEffects.None, poof);

            PoofAnimations = new[]
            {
               poofAnimation
            };

            poofAM = new AnimationManager(PoofSpriteSheet, player.Position, PoofAnimations);

            // Player Idle
            var idle = new[]
            {
                TexturePackerMonoGameDefinitions.entities.Idle_000,
                TexturePackerMonoGameDefinitions.entities.Idle_001,
                TexturePackerMonoGameDefinitions.entities.Idle_002,
                TexturePackerMonoGameDefinitions.entities.Idle_003,
                TexturePackerMonoGameDefinitions.entities.Idle_004,
                TexturePackerMonoGameDefinitions.entities.Idle_005,
                TexturePackerMonoGameDefinitions.entities.Idle_006,
                TexturePackerMonoGameDefinitions.entities.Idle_007,
                TexturePackerMonoGameDefinitions.entities.Idle_008,
                TexturePackerMonoGameDefinitions.entities.Idle_009,
                TexturePackerMonoGameDefinitions.entities.Idle_010,
                TexturePackerMonoGameDefinitions.entities.Idle_011
            };

            // Idle Right
            var idleAnimation = new Animation(new Vector2(0, 0), timePerFrame, SpriteEffects.None, idle);
            player.IdleAnimations = new[]
            {
               idleAnimation
            };
            player.IdleAMRight = new AnimationManager(this.spriteSheet, player.Position, player.IdleAnimations);

            // Idle Left
            idleAnimation = new Animation(new Vector2(0, 0), timePerFrame, SpriteEffects.FlipHorizontally, idle);

            player.IdleAnimations = new[]
            {
               idleAnimation
            };
            player.IdleAMLeft = new AnimationManager(this.spriteSheet, player.Position, player.IdleAnimations);

            // Player Run Right
            var runRight = new[]
            {
                TexturePackerMonoGameDefinitions.entities.Run_000,
                TexturePackerMonoGameDefinitions.entities.Run_001,
                TexturePackerMonoGameDefinitions.entities.Run_002,
                TexturePackerMonoGameDefinitions.entities.Run_003,
                TexturePackerMonoGameDefinitions.entities.Run_004,
                TexturePackerMonoGameDefinitions.entities.Run_005,
                TexturePackerMonoGameDefinitions.entities.Run_006,
                TexturePackerMonoGameDefinitions.entities.Run_007
            };

            var runRightAnimation = new Animation(new Vector2(0, 0), timePerFrame, SpriteEffects.None, runRight);


            player.RunRightAnimations = new[]
            {
               runRightAnimation
            };

            player.RunRightAM = new AnimationManager(this.spriteSheet, player.Position, player.RunRightAnimations);

            // Player Run Left
            var runLeft = new Animation(new Vector2(0, 0), timePerFrame, SpriteEffects.FlipHorizontally, runRight);


            player.RunLeftAnimations = new[]
            {
               runLeft
            };

            player.RunLeftAM = new AnimationManager(this.spriteSheet, player.Position, player.RunLeftAnimations);
        }


        private void ResetGame()
        {
            AndroidHealth = 30;
            AndroidDamage = 1;
            WindowsHealth = 30;
            WindowsDamage = 1;
            iOSHealth = 30;
            iOSDamage = 1;
            Score = 0;

            weapon = new Weapon();
            player.Reset(MapArray);

            // Powerups löschen
            Powerups.RemoveRange(0,Powerups.Count);

            // Gegner löschen
            AndroidsList.RemoveRange(0, AndroidsList.Count);
            WindowsList.RemoveRange(0, WindowsList.Count);
            iOSList.RemoveRange(0, iOSList.Count);
        }

        /// <summary>
        /// Überprüft ob ein Prozess läuft
        /// </summary>
        /// <param name="name">Name der Anwendung. Richtig: cmd   Falsch: cmd.exe</param>
        /// <returns>Liefert true wenn die Anwendung läuft und false wenn nicht</returns>
        public bool IsProcessRunning(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
