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
 * Discord RPC
 * MySQL Connect
 * 
 * CopyRight:
 * Alle Rechte der Bilder, Spiellogik und Spielidee gehören den rechtmäßigen Eigentümern.
 * Das unerlaubte Kopieren, Veröffentlichen, Verleihen und öffentliches Vorführen ist verboten!
 * 
 * 3809 Zeilen an Code
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
using DiscordRPC.Message;
using DiscordRPC;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Threading;

namespace BugHunter
{
    // This is the main type for your game.
    public class Game1 : Game
    {
        // Discord RPC
        /// <summary>
        /// ID of the client
        /// </summary>
        private static string ClientID = "534449793288765450";

        /// <summary>
        /// The level of logging to use.
        /// </summary>
        private static DiscordRPC.Logging.LogLevel DiscordLogLevel = DiscordRPC.Logging.LogLevel.Warning;

        /// <summary>
        /// The current presence to send to discord.
        /// </summary>
        private static RichPresence presence = new RichPresence()
        {
            Details = "Example Project",
            State = "csharp example",
            Assets = new Assets()
            {
                LargeImageKey = "image_large",
                LargeImageText = "Lachee's Discord IPC Library",
                SmallImageKey = "image_small"
            }
        };

        /// <summary>
        /// The discord client
        /// </summary>
        private static DiscordRpcClient client;

        public bool IsDiscordRunning = false;

        private int StatsBoostGiven = 1;

        public Logger logger = null;
        Texture2D pauseScreen;

        private readonly TimeSpan timePerFrame = TimeSpan.FromSeconds(3f / 30f);

        // Gegner
        int MaxEnemies = 1;

        List<Android> AndroidsList = new List<Android>();
        int AndroidHealth = 30;
        int AndroidDamage = 1;
        List<Windows> WindowsList = new List<Windows>();
        int WindowsHealth = 30;
        int WindowsDamage = 1;



        public List<Powerup> Powerups = new List<Powerup>();


        public List<Map> map = new List<Map>();
        GUI gui;
        public SoundFX sound = new SoundFX();

        int AktuelleMap = 0;

        // Standardeinstellungen setzen
        public Settings settings;
         
        public int[][] MapArray;
        public int[][] EnemySpawnPointsArray;

        private double LastKeyStrokeInput = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteRender spriteRender;
        SpriteFont font;
        public SpriteFont MenuFont;
        public Weapon weapon;
        public Player player;
                     
        public Random random = new Random();

        // Threads
        Thread updateThread;
        Thread RankingListUpdateThread;


        // DEBUG Featurese
        FpsCounter fps = new FpsCounter();
        public SpriteFont DebugFont;

        enum GameState : Byte { Ingame, Paused, DeathScreen, Hauptmenu, Stats };
        GameState CurrentGameState = GameState.Hauptmenu;

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
        
        // HAUPTMENÜ
        enum Menubuttons : Byte { Spielen, Einstellungen, Stats, Beenden };
        Menubuttons aktuellerMenupunkt = Menubuttons.Spielen;
        double lastMenuButtonSwitch = 0;

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
            IsMouseVisible = settings.IsMouseVisible;
            
            graphics.SynchronizeWithVerticalRetrace = true;
            // IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
        }

        // Allows the game to perform any initialization it needs to before starting to run.
        // This is where it can query for any required services and load any non-graphic
        // related content.  Calling base.Initialize will enumerate through any components and initialize them as well.
        protected override void Initialize()
        {
            // IsDiscordRunning = IsProcessRunning("Discord");
            // if(!IsDiscordRunning)
                // IsDiscordRunning = IsProcessRunning("discord");

            this.graphicsDevice = GraphicsDevice;

            logger = new Logger(this.settings.LoggingPath);

            Stopwatch sw = new Stopwatch();

            this.Score = 0;

            map.Add(new Map());

            spriteSheetLoader = new SpriteSheetLoader(Content, GraphicsDevice);

            updateThread = new Thread(() => Database.UpdateDatabaseThread(this));
            RankingListUpdateThread = new Thread(() => Database.GetRankingListThread(this));


            if (IsDiscordRunning){

                //Create a new client
                client = new DiscordRpcClient(ClientID)
                {
                    //Create the logger
                    Logger = new DiscordRPC.Logging.ConsoleLogger() { Level = DiscordLogLevel, Coloured = true }
                };

                //Create some events so we know things are happening
                client.OnReady += (sender, msg) => { Console.WriteLine("Connected to discord with user {0}", msg.User.Username); };
                client.OnPresenceUpdate += (sender, msg) => { Console.WriteLine("Presence has been updated!"); };
                

                //Register to the events we care about. We are registering to everyone just to show off the events
                client.OnReady += OnReady;
                client.OnClose += OnClose;
                client.OnError += OnError;

                presence.Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                };
            
                client.SetPresence(presence);
                
                //Connect
                client.Initialize();
            }

            weapon = new Weapon();

            // Generiergt Pause Screen
            pauseScreen = new Texture2D(graphics.GraphicsDevice, settings.resolutionWidth, settings.resolutionHeight);
            Color[] data = new Color[settings.resolutionWidth * settings.resolutionHeight];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            pauseScreen.SetData(data);

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

            // Schriften
            font = Content.Load<SpriteFont>("Font");
            DebugFont = Content.Load<SpriteFont>("Debug");
            MenuFont = Content.Load<SpriteFont>("MenuFont");

            // Spieler Init
            player = new Player(this,200f,100);
            player.camera = new OrthographicCamera(GraphicsDevice);
            player.Texture = Content.Load<Texture2D>("sprites/entities/player/idle");
            player.OriginTexture = Content.Load<Texture2D>("sprites/originSpot");
            player.Init(this.settings, this, this.sound);

            // Setze Spielerposition auf SpawnTilekoordinaten
            player.SetSpawnFromMap(MapArray);

            // GUI Init
            gui.PausedBackground = Content.Load<Texture2D>("paused_background");
            gui.spriteSheet = spriteSheetLoader.Load("gui_packed.png");
            gui.CustomCurserTexture = Content.Load<Texture2D>("sprites/mauszeiger");

            // Animation

            this.PoofSpriteSheet = spriteSheetLoader.Load("effects_packed.png");
            InitialiseAnimationManager();

            // Threads Starten
            updateThread.Start();
            RankingListUpdateThread.Start();
        }

        // UnloadContent will be called once per game and is the place to unload game-specific content.
        protected override void UnloadContent()
        {
            updateThread.Abort();
            RankingListUpdateThread.Abort();

            // Speichern von Einstellungen
            settings.SaveSettings();

            // Speichern von Spielkritischen Daten
            settings.SaveGamedata();

            // Gamepadvibrationen ausschalten
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);

            // Discord Client freigeben
            client?.Dispose();

            // Spiel beenden
            logger.Log("Spiel beenden");
        }

        // Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (IsDiscordRunning)
            {
                presence.Details = "Score: " + this.Score;
                presence.Assets.LargeImageKey = "icon";
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
                if (IsDiscordRunning)
                    presence.State = "Im Hauptmenü";
                if ((Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp)) && gameTime.TotalGameTime.TotalMilliseconds - lastMenuButtonSwitch >= 150)
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

                if ((Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp)) && gameTime.TotalGameTime.TotalMilliseconds - lastMenuButtonSwitch >= 150)
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
                            ResetGame();
                            this.CurrentGameState = GameState.Ingame;
                            break;
                        case Menubuttons.Stats:
                            this.CurrentGameState = GameState.Stats;
                            break;
                        case Menubuttons.Einstellungen:
                            // TODO: Einstellungen
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

            // Ingame
            if(CurrentGameState == GameState.Ingame)
            {
                if(this.Score % (5000 + StatsBoostGiven) == 0 && Score != 0)
                {
                    player.MaxHealth += 25;
                    player.Health += 25;
                    player.Damageboost += 10;
                    this.StatsBoostGiven += 5000;
                }

                if (IsDiscordRunning)
                    presence.State = "Am Debuggen";

                if (Keyboard.GetState().IsKeyDown(Keys.F3) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                {
                    settings.AreDebugInformationsVisible = !settings.AreDebugInformationsVisible;
                    LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }
                
                if (this.Score > int.Parse(gameStats.HighScore))
                {
                    gameStats.HighScore = Score.ToString();
                    sound.ScoreSound.Play(0.5f, 0, 0);
                }
                
                MaxEnemies = (int)(this.Score / 1000) + 1;

                // Falls weniger Gegner Aktiv sind als Maximal zugelassen sind, dann werden neue gespawnt
                if(WindowsList.Count + AndroidsList.Count < MaxEnemies)
                {
                    Rectangle enemyHitbox;
                    Rectangle playersBox = new Rectangle((int)(player.Position.X - Settings.TilePixelSize * 2), (int)(player.Position.Y - Settings.TilePixelSize * 2),Settings.TilePixelSize * 4, Settings.TilePixelSize * 4);

                    SpriteFrame sp = null;

                    switch (random.Next(2))
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
                    }
                }

                // Updaten
                for (int i = 0; i < AndroidsList.Count; i++)
                {
                    AndroidsList[i].Update(gameTime, EnemySpawnPointsArray, map[AktuelleMap].maplevel);

                    if (AndroidsList[i].IsDead)
                    {
                        gameStats.KilledEnemies++;
                        PoofPosition = AndroidsList[i].Position;
                        PoofIsActive = true;

                        // 10% Chance das sich der Schaden 
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
                        gameStats.KilledEnemies++;
                        PoofPosition = WindowsList[i].Position;
                        PoofIsActive = true;
                        // 10% Chance das sich der Schaden 
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

                if (Keyboard.GetState().IsKeyDown(Keys.F4) && Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    player.Health = 5;
                }

                player.Update(gameTime, MapArray, map[AktuelleMap].GetTiledMap());
                gui.Update(gameTime, player);

                // MapRenderer  updaten
                map[AktuelleMap].mapRenderer.Update(gameTime);


                // Update the fps
                fps.Update(gameTime);

                sound.HintergrundMusikEffect.Volume = 0.1f;

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
                
                // Updated poof wenn Aktiv
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
                gameStats.AnzahlTode++;
                CurrentGameState = GameState.DeathScreen;
            }

            // Respawn wenn in Deathscreen
            if((Keyboard.GetState().IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A)) && CurrentGameState == GameState.DeathScreen)
            {
                player.Health = player.MaxHealth;
                this.CurrentGameState = GameState.Hauptmenu;
            }

            if(CurrentGameState == GameState.Paused)
            {
                if (IsDiscordRunning)
                    presence.State = "Im Pausemenü";
            }

            // PAUSE
            if((Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start)) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
            {
                if (CurrentGameState == GameState.Ingame)
                {
                    // Hintergrundmusik pausieren
                    if (sound.HintergrundMusikEffect.State == SoundState.Playing)
                    {
                        sound.HintergrundMusikEffect.Pause();
                    }
                    CurrentGameState = GameState.Paused;
                }
                else if (CurrentGameState == GameState.Paused)
                {
                    // Hintergrundmusik pausieren
                    if (sound.HintergrundMusikEffect.State == SoundState.Paused)
                    {
                        sound.HintergrundMusikEffect.Resume();
                    }
                    CurrentGameState = GameState.Ingame;
                }

                LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (IsDiscordRunning)
                if (!client.Disposed)
                    client.SetPresence(presence);

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
                spriteBatch.DrawString(font, Texttable.Stats_Getötete_Gegner + gameStats.KilledEnemies, new Vector2(player.camera.Origin.X - 900, player.camera.Origin.Y - 500), Color.White);
                spriteBatch.DrawString(font, Texttable.Stats_Gesammelte_Powerups + gameStats.CollectedPowerups, new Vector2(player.camera.Origin.X - 900, player.camera.Origin.Y - 450), Color.White);
                spriteBatch.DrawString(font, Texttable.Stats_Anzahl_Geschossen + gameStats.AnzahlSchuesse, new Vector2(player.camera.Origin.X - 900, player.camera.Origin.Y - 400), Color.White);
                spriteBatch.DrawString(font, Texttable.Stats_Anzahl_Treffer + gameStats.AnzahlTreffer, new Vector2(player.camera.Origin.X - 900, player.camera.Origin.Y - 350), Color.White);
                spriteBatch.DrawString(font, Texttable.Stats_Trefferrate + ((float)gameStats.AnzahlTreffer / (float)gameStats.AnzahlSchuesse).ToString("P"), new Vector2(player.camera.Origin.X - 900, player.camera.Origin.Y - 300), Color.White);
                spriteBatch.DrawString(font, Texttable.Stats_Tode + gameStats.AnzahlTode, new Vector2(player.camera.Origin.X - 900, player.camera.Origin.Y - 250), Color.White);


                // Global Ranking Liste
                spriteBatch.DrawString(MenuFont, "Top 10 Spieler", new Vector2(player.camera.Origin.X - 100, player.camera.Origin.Y - 500), Color.White);

                for (int i = 0; i < gameStats.Top10Names.Count; i++)
                {
                    spriteBatch.DrawString(font, gameStats.Top10Names[i] + ":  " + gameStats.Top10Score[i], new Vector2(player.camera.Origin.X - 100, player.camera.Origin.Y + (50 * i) - 400), Color.White);
                }
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
                }

                if (CurrentGameState == GameState.Ingame || CurrentGameState == GameState.Paused || CurrentGameState == GameState.DeathScreen)
                {
                    // MapRenderer zum Zeichnen der aktuell sichtbaren Map
                    map[AktuelleMap].mapRenderer.Draw(player.camera.GetViewMatrix());

                    // Sprite ausgeben
                    player.Draw(spriteBatch, font);

                    foreach (Android android in AndroidsList)
                    {
                        android.Draw(spriteBatch, font);
                    }

                    foreach (Windows windows in WindowsList)
                    {
                        windows.Draw(spriteBatch, font);
                    }

                    foreach (Powerup powerup in Powerups)
                        powerup.Draw(spriteBatch);

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
                        spriteBatch.Draw(pauseScreen, new Vector2(player.camera.Position.X, player.camera.Position.Y), new Color(0, 0, 0, 128));


                        spriteBatch.DrawString(MenuFont, "PAUSE", new Vector2(player.Position.X - 100, player.Position.Y - 64), Color.White);
                        spriteBatch.DrawString(MenuFont, "Highscore: " + gameStats.HighScore,
                            new Vector2(player.camera.Position.X + 750, player.camera.Position.Y), Color.White);
                    }

                    if (CurrentGameState == GameState.DeathScreen)
                    {
                        spriteBatch.Draw(gui.PausedBackground, new Vector2(player.Position.X - 960, player.Position.Y - 540), Color.White);
                        spriteBatch.DrawString(MenuFont, Texttable.Text_Died, new Vector2(player.Position.X - 300, player.Position.Y - 64), Color.White);
                    }
                }
                spriteBatch.End();
            }
            // Hauptmenü
            if (CurrentGameState == GameState.Hauptmenu)
            {
                spriteBatch.Begin();
                // Zeichnet alle Menüpunkte
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Start, new Vector2(400, 300), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Stats, new Vector2(400, 400), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Einstellungen, new Vector2(400, 500), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Ende, new Vector2(400, 600), Color.White);

                // Überzeichnet den Menüpunkt der ausgewählt ist.
                switch (aktuellerMenupunkt)
                {
                    case Menubuttons.Spielen:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Start, new Vector2(400,300), Color.YellowGreen);
                        break;
                    case Menubuttons.Stats:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Stats, new Vector2(400,400), Color.YellowGreen);
                        break;
                    case Menubuttons.Einstellungen:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Einstellungen, new Vector2(400,500), Color.YellowGreen);
                        break;
                    case Menubuttons.Beenden:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Ende, new Vector2(400,600), Color.YellowGreen);
                        break;
                }
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
        private void InitialiseAnimationManager()
        {
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
        }


        private void ResetGame()
        {
            AndroidHealth = 30;
            AndroidDamage = 1;
            WindowsHealth = 30;
            WindowsDamage = 1;
            Score = 0;

            weapon = new Weapon();
            player.Reset(MapArray);

            AndroidsList.RemoveRange(0, AndroidsList.Count);
            WindowsList.RemoveRange(0, WindowsList.Count);
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

        private static void OnReady(object sender, ReadyMessage args)
		{
			//This is called when we are all ready to start receiving and sending discord events. 
			// It will give us some basic information about discord to use in the future.
			
			//It can be a good idea to send a inital presence update on this event too, just to setup the inital game state.
			Console.WriteLine("On Ready. RPC Version: {0}", args.Version);

		}
		private static void OnClose(object sender, CloseMessage args)
		{
			//This is called when our client has closed. The client can no longer send or receive events after this message.
			// Connection will automatically try to re-establish and another OnReady will be called (unless it was disposed).
			Console.WriteLine("Lost Connection with client because of '{0}'", args.Reason);
		}
		private static void OnError(object sender, ErrorMessage args)
		{
			//Some error has occured from one of our messages. Could be a malformed presence for example.
			// Discord will give us one of these events and its upto us to handle it
			Console.WriteLine("Error occured within discord. ({1}) {0}", args.Message, args.Code);
		}
    }
}
