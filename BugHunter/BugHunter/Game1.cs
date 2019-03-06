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
using System.Diagnostics;
using System.Threading;
using MySql.Data.MySqlClient;

namespace BugHunter
{
    // This is the main type for your game.
    public class Game1 : Game
    {
        private int StatsBoostGiven = 1;

        public Logger logger = null;
        public Texture2D pauseScreen;

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
        public SpriteFont font;
        public SpriteFont MenuFont;
        public Weapon weapon;
        public Player player;
                     
        public Random random = new Random();

        // Threads
        public Thread updateThread;
        public Thread RankingListUpdateThread;
        public Thread GlobalScoreListUpdateThread;
        Thread CheckDatabaseConnectionThread;


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

            Thread.CurrentThread.Name = "MainThread";
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

            weapon = new Weapon();

            settingsMenu = new SettingsMenu(this);

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
            gui.spriteSheet = spriteSheetLoader.Load("gui_packed.png");
            gui.CustomCurserTexture = Content.Load<Texture2D>("sprites/mauszeiger");

            // Animation

            this.PoofSpriteSheet = spriteSheetLoader.Load("effects_packed.png");
            InitialiseAnimationManager();

            // Threads werden nur erstellt, wenn Datenbankstatistiken erlaubt sind um Ressourcen zu sparen
            if (settings.IsSendStatisticsAllowed)
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

                // Threads Starten
                updateThread.Start();
                RankingListUpdateThread.Start();
                GlobalScoreListUpdateThread.Start();
                CheckDatabaseConnectionThread.Start();
            }

            pauseScreen = new Texture2D(graphics.GraphicsDevice, settings.resolutionWidth, settings.resolutionHeight);
            Color[] data = new Color[settings.resolutionHeight * settings.resolutionWidth];
            for (int i = 0; i < data.Length; ++i) data[i] = new Color(0,0,0,128);
            pauseScreen.SetData(data);
        }

        // UnloadContent will be called once per game and is the place to unload game-specific content.
        protected override void UnloadContent()
        {
            updateThread.Interrupt();
            RankingListUpdateThread.Interrupt();
            GlobalScoreListUpdateThread.Interrupt();
            CheckDatabaseConnectionThread.Interrupt();

            UpdateGlobalScore();

            // Speichern von Einstellungen
            settings.SaveSettings();

            // Speichern von Spielkritischen Daten
            settings.SaveGamedata();

            // Gamepadvibrationen ausschalten
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);

            updateThread.Join();
            RankingListUpdateThread.Join();
            GlobalScoreListUpdateThread.Join();
            CheckDatabaseConnectionThread.Join();

            // Spiel beenden
            logger.Log("Spiel beenden", Thread.CurrentThread.Name);
            logger.WriteLog();
        }

        // Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
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
                            this.CurrentGameState = GameState.Settings;
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
            if(CurrentGameState == GameState.Ingame)
            {
                if(this.Score >= (5000 + StatsBoostGiven) && Score != 0)
                {
                    player.MaxHealth += 25;
                    player.Health += 25;
                    player.Damageboost += 10;
                    this.StatsBoostGiven += 5000;
                }

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

                // Debug Feature
                if (Keyboard.GetState().IsKeyDown(Keys.F4) && Keyboard.GetState().IsKeyDown(Keys.LeftControl) && settings.IsDebugEnabled)
                {
                    player.Health = 0;
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
                UpdateGlobalScore();
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
                        spriteBatch.DrawString(MenuFont, "Highscore: " + gameStats.HighScore,
                            new Vector2(player.camera.Position.X + 750, player.camera.Position.Y), Color.White);
                    }

                    if (CurrentGameState == GameState.DeathScreen)
                    {
                        spriteBatch.Draw(pauseScreen, new Vector2(player.camera.Position.X, player.camera.Position.Y));
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

            var idleAnimation = new Animation(new Vector2(0, 0), timePerFrame, SpriteEffects.None, idle);


            player.IdleAnimations = new[]
            {
               idleAnimation
            };

            player.IdleAM = new AnimationManager(this.spriteSheet, player.Position, player.IdleAnimations);

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

        private void UpdateGlobalScore()
        {
            if (!settings.IsSendStatisticsAllowed)
                return;


            String connString = "Server=" + Settings.host + ";Database=" + Settings.database
                 + ";port=" + Settings.port + ";User Id=" + Settings.username + ";password=" + Settings.password;

            MySqlConnection connection = new MySqlConnection(connString);
            MySqlCommand command;
            MySqlDataReader reader;
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    // Verbindung muss erst aufgebaut werden
                    this.logger.Log("Datenbankverbindung wird aufgebaut", "Debug");
                    connection.Open();

                }

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    // Datenbankverbindung steht
                    this.logger.Log("Datenbankverbindung steht", "Debug");

                    // select rückgabe auslesen

                    command = new MySqlCommand();
                    command.CommandText = "SELECT * FROM `GlobalScore`";
                    command.Connection = connection;

                    reader = command.ExecuteReader();
                    reader.Read();

                    uint GlobalKilledEnemies = reader.GetUInt32(1);
                    uint GlobalCollectedPowerups = reader.GetUInt32(2);
                    UInt64 GlobalAnzahlSchuesse = reader.GetUInt64(3);
                    UInt64 GlobalAnzahlHits = reader.GetUInt64(4);
                    uint GlobalDeathCount = reader.GetUInt32(5);


                    reader.Close();

                    // Datenbankeintrag wird upgedated
                    command.CommandText = "UPDATE `GlobalScore` SET `KilledEnemies` = '" + (GlobalKilledEnemies + this.gameStats.KilledEnemies - this.gameStats.KilledEnemiesOld) + "', `CollectedPowerups` = '" + (GlobalCollectedPowerups + this.gameStats.CollectedPowerups - this.gameStats.CollectedPowerupsOld)+ "', `Shots` = '" + (GlobalAnzahlSchuesse + this.gameStats.AnzahlSchuesse - this.gameStats.AnzahlSchuesseOld) + "', `Hits` = '" + (GlobalAnzahlHits + this.gameStats.AnzahlTreffer - this.gameStats.AnzahlTrefferOld) + "', `Deaths` = '" + (GlobalDeathCount + this.gameStats.AnzahlTode - this.gameStats.AnzahlTodeOld) + "' WHERE `GlobalScore`.`ID` = 1;";

                    command.ExecuteNonQuery();

                    this.gameStats.KilledEnemiesOld = this.gameStats.KilledEnemies;
                    this.gameStats.CollectedPowerupsOld = this.gameStats.CollectedPowerups;
                    this.gameStats.AnzahlSchuesseOld = this.gameStats.AnzahlSchuesse;
                    this.gameStats.AnzahlTrefferOld = this.gameStats.AnzahlTreffer;
                    this.gameStats.AnzahlTodeOld = this.gameStats.AnzahlTode;
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                this.logger.Log(e.Message, "Error");
            }
            finally
            {
                connection.Close();
                this.logger.Log("Datenbankverbindung geschlossen",Thread.CurrentThread.Name);
            }
        }
    }
}
