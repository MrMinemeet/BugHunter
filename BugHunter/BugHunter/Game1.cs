/*
 * Entwickler: Alexander Voglsperger (4AHELS 2018/2019)
 * Softwarename: BugHunter
 * Entwicklungszeitraum:  18.11.2018 - JETZT
 * 
 * Kurzbeschreibung:
 * Der Spieler muss gegen immer Stärker werdende Wellen von Bugs(Gegnern) kämpfen indem er mit Code auf sie schießt.
 * Neue Munition kann dadurch erhalten werden, wenn der Spieler an einen PC geht.
 * Jede Sprache hat zudem eine unterschiedliche Schussfrequenz und stärke.
 * zB.: Maschinencode schießt langsam aber macht viel Schaden, JS schießt schnell aber macht geringen Schaden
 * 
 * Verwendete Software:
 * Tiled-Editor
 * Visual Studio Code
 * Visual Studio 2017
 * MonoGame FrameWork
 * MonoGame.Extended Framework
 * Photoshop CC 2019
 * TexturePackerGUI 
 */


 // TODO: Hitmarker bei Schaden
 // TODO: MacOS Gegner
 // TODO: Windows Gegner
 // TODO: iOS Gegner
 // TODO: JavaScript Waffe
 // TODO: Gegner gegen Gegner Collision
 // TODO: Tastatur als Waffe in die Hand
 // TODO: Powerup: Medipack - PC Teile
 // TODO: Powerup: Mehr Ammo - USB Stick
 // TODO: Ammopack: Lädt komplette Munition nach
 // TODO: Powerup: Erhöhter Schaden - Bücher
 // TODO: Powerup: Erhöhte Schussgeschwindigkeit - Kaffee


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
using System.Timers;

namespace BugHunter
{
    // This is the main type for your game.
    public class Game1 : Game
    {
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

        Timer timer;



        private readonly TimeSpan timePerFrame = TimeSpan.FromSeconds(3f / 30f);

        Player player = new Player(200f,100);

        // Android gegner
        IDictionary<int, Android> Androids = new Dictionary<int, Android>();
        int maxAndroids = 1;
        int AndroidHealth = 30;


        IDictionary<int, Powerup> Powerups = new Dictionary<int, Powerup>();
        int maxPowerups = 4;


        public Map[] map = new Map[1];
        GUI gui = new GUI();
        public SoundFX sound = new SoundFX();

        int AktuelleMap = 0;

        // Standardeinstellungen setzen
        Settings settings = new Settings();
         
        public int[][] MapArray;

        private double LastKeyStrokeInput = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteRender spriteRender;
        SpriteFont font;
        public SpriteFont MenuFont;


        // DEBUG Featurese
        FpsCounter fps = new FpsCounter();
        public SpriteFont DebugFont;

        enum GameState : Byte { Ingame, Paused, DeathScreen, Hauptmenu };
        GameState CurrentGameState = GameState.Hauptmenu;

        private SpriteSheetLoader spriteSheetLoader;
        public GraphicsDevice graphicsDevice;

        // Score
        public int Score { get; set; }

        // Animation

        // POOF Animation
        private SpriteSheet PoofSpriteSheet;
        private Animation[] PoofAnimations;
        private AnimationManager poofAM;
        private bool PoofIsActive = false;
        private Vector2 PoofPosition;


        // HAUPTMENÜ
        enum Menubuttons : Byte { Spielen, Einstellungen, Stats, Beenden };
        Menubuttons aktuellerMenupunkt = Menubuttons.Spielen;
        double lastMenuButtonSwitch = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = settings.resolutionWidth,
                PreferredBackBufferHeight = settings.resolutionHeight,
                IsFullScreen = settings.IsFullscreen
            };
            IsMouseVisible = settings.IsMouseVisible;
            Content.RootDirectory = "Content";
        }

        // Allows the game to perform any initialization it needs to before starting to run.
        // This is where it can query for any required services and load any non-graphic
        // related content.  Calling base.Initialize will enumerate through any components and initialize them as well.
        protected override void Initialize()
        {
            this.graphicsDevice = GraphicsDevice;

            this.Score = 0;

            player.camera = new OrthographicCamera(GraphicsDevice);

            map[0] = new Map();

            spriteSheetLoader = new SpriteSheetLoader(Content, GraphicsDevice);

            gui.Init(this);


            //Create a new client
            client = new DiscordRpcClient(ClientID);

            //Create the logger
            client.Logger = new DiscordRPC.Logging.ConsoleLogger() { Level = DiscordLogLevel, Coloured = true };

            //Create some events so we know things are happening
            client.OnReady += (sender, msg) => { Console.WriteLine("Connected to discord with user {0}", msg.User.Username); };
            client.OnPresenceUpdate += (sender, msg) => { Console.WriteLine("Presence has been updated!"); };

            //Create a timer that will regularly call invoke
            timer = new System.Timers.Timer(150);
            timer.Elapsed += (sender, evt) => { client.Invoke(); };
            timer.Start();


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

            base.Initialize();
        }
        
        // LoadContent will be called once per game and is the place to load all of your content.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteRender = new SpriteRender(spriteBatch);

            // Verarbeitete Map aus Pipeline laden
            map[AktuelleMap].setTiledMap(Content.Load<TiledMap>("map1"));
            // MapRenderer für die Map erstellen
            map[AktuelleMap].mapRenderer = new TiledMapRenderer(GraphicsDevice, map[AktuelleMap].getTiledMap());

            settings.MapSizeHeight = map[AktuelleMap].getTiledMap().Height;
            settings.MapSizeWidth = map[AktuelleMap].getTiledMap().Width;
            settings.EmptyTexture = Content.Load<Texture2D>("sprites/empty");
            settings.Init(this);
            settings.LoadSettings();

            // TMX (wie CSV) Map in 2D Array wandeln
            MapArray = Converter.MapToIntArray(map[AktuelleMap].maplevel, settings);

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
            player.Texture = Content.Load<Texture2D>("sprites/player/afk_0001");
            player.OriginTexture = Content.Load<Texture2D>("sprites/originSpot");
            player.Hitmarker = Content.Load<Texture2D>("sprites/gui/hitmarker");
            player.WeaponSpriteSheet = spriteSheetLoader.Load("sprites/entities/entities.png");
            player.Init(this.settings, this, this.sound);
            
            // Setze Spielerposition auf SpawnTilekoordinaten
            player.SetSpawnFromMap(MapArray);

            // GUI Init
            gui.PausedBackground = Content.Load<Texture2D>("paused_background");
            gui.spriteSheet = spriteSheetLoader.Load("gui_packed.png");
            gui.CustomCurserTexture = Content.Load<Texture2D>("sprites/mauszeiger");

            // Animation
            PoofSpriteSheet = spriteSheetLoader.Load("effects_packed.png");

            InitialiseAnimationManager();
        }

        // UnloadContent will be called once per game and is the place to unload game-specific content.
        protected override void UnloadContent()
        {
        }

        // Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            presence.Details = "Score: " + this.Score;
            presence.Assets.LargeImageKey = "icon";


            // Spiel schließen
            if (Keyboard.GetState().IsKeyDown(Keys.Delete) && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                settings.SaveSettings();
                Exit();
            }

            // Spiel in Vollbild machen
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
                presence.State = "Im Hauptmenü";
                if ((Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A)) && gameTime.TotalGameTime.TotalMilliseconds - lastMenuButtonSwitch >= 150)
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

                if ((Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Y)) && gameTime.TotalGameTime.TotalMilliseconds - lastMenuButtonSwitch >= 150)
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

                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    switch (aktuellerMenupunkt)
                    {
                        case Menubuttons.Spielen:
                            this.CurrentGameState = GameState.Ingame;
                            break;
                        case Menubuttons.Stats:
                            // TODO: Stats
                            break;
                        case Menubuttons.Einstellungen:
                            // TODO: Einstellungen
                            break;
                        case Menubuttons.Beenden:
                            settings.SaveSettings();
                            Exit();
                            break;
                    }
                }
            }

            // Ingame
            if(CurrentGameState == GameState.Ingame)
            {
                presence.State = "Am Debuggen";
                if (Keyboard.GetState().IsKeyDown(Keys.F3) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                {
                    settings.AreDebugInformationsVisible = !settings.AreDebugInformationsVisible;
                    LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }

                // Updaten
                for(int i = 0; i < Androids.Count; i++)
                {
                    Androids[i].Update(gameTime, MapArray, map[AktuelleMap].maplevel);

                    if (Androids[i].IsDead)
                    {
                        PoofPosition = Androids[i].Position;
                        PoofIsActive = true;
                        Androids.Remove(i);
                    }
                }
                player.Update(gameTime, MapArray, map[AktuelleMap].getTiledMap());
                gui.Update(gameTime, player);

                // MapRenderer  updaten
                map[AktuelleMap].mapRenderer.Update(gameTime);


                // Update the fps
                fps.Update(gameTime);

                if (this.Score > settings.HighScore)
                {
                    settings.HighScore = Score;
                    sound.ScoreSound.Play(0.5f,0,0);
                }

                sound.HintergrundMusikEffect.Volume = 0.1f;

                if(sound.HintergrundMusikEffect.State == SoundState.Stopped)
                {
                    sound.HintergrundMusikEffect.Play();
                }

                maxAndroids = (int)(this.Score / 1000) + 1;

                // Generiert neue Einträge im Dictionary wenn weniger Gegner da sind als max. zulässig sind
                // Generiert immer dann einen Eintrag wenn der Key nicht verwendet wird
                for (int i = 0; i < maxAndroids; i++)
                {
                    if (!Androids.ContainsKey(i))
                    {
                        // Neues Leben für Android berechen
                        this.AndroidHealth = (int)(AndroidHealth * 1.08f);


                        // Neuen Android in Liste erstellen
                        Androids.Add(i, new Android(50f, AndroidHealth));

                        // Android Initialisieren
                        Androids[i].Init(this, this.settings, this.player);
                        Androids[i].OriginTexture = Content.Load<Texture2D>("sprites/originSpot");
                        Androids[i].spriteSheet = spriteSheetLoader.Load("sprites/entities/entities.png");
                        Androids[i].SetSpawnFromMap(MapArray);
                    }
                }

                // Generiert neue Einträge im Dictionary wenn weniger Powerup da sind als max. zulässig sind
                // Generiert immer dann einen Eintrag wenn der Key nicht verwendet wird
                
                for (int i = 0; i < maxPowerups; i++)
                {
                    if (!Powerups.ContainsKey(i))
                    {
                        Powerups.Add(i, new Powerup(spriteSheetLoader.Load("sprites/entities/entities.png"), new SpriteRender(spriteBatch), this.settings, this.MapArray));
                    }

                    // Setzt solage eine neue Position bis alle Powerups auf einer unterschiedlichen Position sind
                    int j = 0;
                    while (true) { 

                        if(j >= Powerups.Count)
                        {
                            break;
                        }

                        if (Powerups[i].position.Equals(Powerups[j].position) && j != i)
                        {
                            Powerups[i].ResetPosition(this.MapArray);
                            j = 0;
                        }

                        j++;
                    }
                }
                

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
            if (player.Health <= 0)
            {
                CurrentGameState = GameState.DeathScreen;
            }

            // Respawn wenn in Deathscreen
            if((Keyboard.GetState().IsKeyDown(Keys.R) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftStick)) && CurrentGameState == GameState.DeathScreen)
            {
                player.Reset(MapArray);
                this.CurrentGameState = GameState.Ingame;
                this.Score = 0;
                this.maxAndroids = 1;
                this.AndroidHealth = 30;
            }

            if(CurrentGameState == GameState.Paused)
            {
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



            client.SetPresence(presence);

            base.Update(gameTime);
        }

        // This is called when the game should draw itself.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        { 
            // Schwarzer Hintergrund
            GraphicsDevice.Clear(Color.TransparentBlack);
            
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

            if(CurrentGameState != GameState.Hauptmenu)
            {
                // MapRenderer zum Zeichnen der aktuell sichtbaren Map
                map[AktuelleMap].mapRenderer.Draw(player.camera.GetViewMatrix());

                // Sprite ausgeben
                player.Draw(spriteBatch, font);

                for (int i = 0; i < Androids.Count; i++)
                {
                    Androids[i].Draw(spriteBatch, font);
                }

                for (int i = 0; i < Powerups.Count; i++)
                {
                    Powerups[i].Draw(spriteBatch);
                }

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
                    spriteBatch.Draw(gui.PausedBackground, new Vector2(player.Position.X - 960, player.Position.Y - 540), Color.White);
                    spriteBatch.DrawString(MenuFont, "PAUSE", new Vector2(player.Position.X - 100, player.Position.Y - 64), Color.White);
                    spriteBatch.DrawString(MenuFont, "Highscore: " + settings.HighScore,
                        new Vector2(player.camera.Position.X + 750, player.camera.Position.Y), Color.White);
                }

                if (CurrentGameState == GameState.DeathScreen)
                {
                    spriteBatch.Draw(gui.PausedBackground, new Vector2(player.Position.X - 960, player.Position.Y - 540), Color.White);
                    spriteBatch.DrawString(MenuFont, Texttable.Text_Died, new Vector2(player.Position.X - 300, player.Position.Y - 64), Color.White);
                }
            }
            
            // Hauptmenü
            if(CurrentGameState == GameState.Hauptmenu)
            {
                // Zeichnet alle Menüpunkte
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Start, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y - 200), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Stats, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y - 100), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Einstellungen, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Menu_Ende, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y + 100), Color.White);

                // Überzeichnet den Menüpunkt der ausgewählt ist.
                switch (aktuellerMenupunkt)
                {
                    case Menubuttons.Spielen:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Start, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y - 200), Color.Gray);
                        break;
                    case Menubuttons.Stats:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Stats, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y - 100), Color.Gray);
                        break;
                    case Menubuttons.Einstellungen:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Einstellungen, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y), Color.Gray);
                        break;
                    case Menubuttons.Beenden:
                        spriteBatch.DrawString(MenuFont, Texttable.Menu_Ende, new Vector2(player.camera.Origin.X - 400, player.camera.Origin.Y + 100), Color.Gray);
                        break;
                }
                
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void InitialiseAnimationManager()
        {
            var poof = new[] {
                TexturePackerMonoGameDefinitions.effect_packed.Poof_001,
                TexturePackerMonoGameDefinitions.effect_packed.Poof_002,
                TexturePackerMonoGameDefinitions.effect_packed.Poof_003,
                TexturePackerMonoGameDefinitions.effect_packed.Poof_004,
                TexturePackerMonoGameDefinitions.effect_packed.Poof_005,
                TexturePackerMonoGameDefinitions.effect_packed.Poof_006,
                TexturePackerMonoGameDefinitions.effect_packed.Poof_007
            };
            
            var poofAnimation = new Animation(new Vector2(1, 0), timePerFrame, SpriteEffects.None, poof);

            PoofAnimations = new[]
            {
               poofAnimation
            };

            poofAM = new AnimationManager(PoofSpriteSheet, player.Position, PoofAnimations);
        }


        // Wird beim Schließen aufgerufen
        protected override void OnExiting(Object sender, EventArgs args)
        {
            settings.SaveSettings();
            GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
            base.OnExiting(sender, args);


            //At the very end we need to dispose of it
            timer.Close();
            client.Dispose();
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
