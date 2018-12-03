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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;
using TexturePackerLoader;

namespace BugHunter
{
    // This is the main type for your game.
    public class Game1 : Game
    {
        Player player = new Player(200f,100);
        Android android = new Android(30f, 10);
        Map[] map = new Map[1];
        GUI gui = new GUI();

        int AktuelleMap = 0;

        // Standardeinstellungen setzen
        Settings settings = new Settings(1920, 1080, false, false);
         
        
        int[][] CollisionMapArray;

        private double LastKeyStrokeInput = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        SpriteFont MenuFont;


        // DEBUG Featurese
        FpsCounter fps = new FpsCounter();
        public SpriteFont DebugFont;

        Vector2 projectile;
        double grad;


        enum GameState : Byte { Ingame, Paused, DeathScreen };
        GameState CurrentGameState = GameState.Ingame;


        private SpriteSheetLoader spriteSheetLoader;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = settings.getResolutionWidth(),
                PreferredBackBufferHeight = settings.getResolutionHeight(),
                IsFullScreen = settings.getIsFullScreen()
            };
            IsMouseVisible = settings.getIsMouseVisible();
            Content.RootDirectory = "Content";
        }

        // Allows the game to perform any initialization it needs to before starting to run.
        // This is where it can query for any required services and load any non-graphic
        // related content.  Calling base.Initialize will enumerate through any components and initialize them as well.
        protected override void Initialize()
        {
            player.camera = new OrthographicCamera(GraphicsDevice);

            map[0] = new Map();

            spriteSheetLoader = new SpriteSheetLoader(Content, GraphicsDevice);

            base.Initialize();
        }
        
        // LoadContent will be called once per game and is the place to load all of your content.
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Verarbeitete Map aus Pipeline laden
            map[AktuelleMap].setTiledMap(Content.Load<TiledMap>("map1"));
            // MapRenderer für die Map erstellen
            map[AktuelleMap].mapRenderer = new TiledMapRenderer(GraphicsDevice, map[AktuelleMap].getTiledMap());

            settings.setMapSizeHeight(map[AktuelleMap].getTiledMap().Height);
            settings.setMapSizeWidth(map[AktuelleMap].getTiledMap().Width);

            // TMX (wie CSV) Map in 2D Array wandeln
            CollisionMapArray = Converter.TmxToIntArray(@"C:\Users\Alexa\Google Drive\Schule\4AHELS\Werkstätte\BugHunter\V1\BugHunter\BugHunter\Content\map1.tmx");

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("Font");
            DebugFont = Content.Load<SpriteFont>("Debug");
            MenuFont = Content.Load<SpriteFont>("MenuFont");

            // Spieler Sprite laden
            player.Texture = Content.Load<Texture2D>("sprites/player/afk_0001");
            player.OriginTexture = Content.Load<Texture2D>("sprites/originSpot");
            player.DamageTexture = Content.Load<Texture2D>("damaged");
            gui.PausedBackground = Content.Load<Texture2D>("paused_background");

            // Setze Spielerposition auf SpawnTilekoordinaten
            player.SetSpawnFromMap(CollisionMapArray);

            android.OriginTexture = Content.Load<Texture2D>("sprites/originSpot");
            android.spriteSheet = spriteSheetLoader.Load("android_packed.png");
            android.SetSpawnFromMap(CollisionMapArray);

            

            gui.spriteSheet = spriteSheetLoader.Load("gui_packed.png");
            gui.CustomCurserTexture = Content.Load<Texture2D>("sprites/mauszeiger");

        }

        // UnloadContent will be called once per game and is the place to unload game-specific content.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        // Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.Delete))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.F11) || (Keyboard.GetState().IsKeyDown(Keys.LeftAlt) && Keyboard.GetState().IsKeyDown(Keys.Enter)))
            {
                if(gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                {
                    graphics.ToggleFullScreen();
                    LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }

            if(CurrentGameState == GameState.Ingame)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.F3) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
                {
                    settings.setAreDebugInformationsVisible(!settings.getAreDebugInformationsVisible());
                    LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }

                // Spieler Updaten
                android.Update(gameTime, CollisionMapArray, map[AktuelleMap].getTiledMap(), player);
                player.Update(gameTime, CollisionMapArray, map[AktuelleMap].getTiledMap());
                gui.Update(gameTime, player);

                // MapRenderer  updaten
                map[AktuelleMap].mapRenderer.Update(gameTime);


                // Update the fps
                fps.Update(gameTime);
            }
            
            if(player.Health == 0)
            {
                CurrentGameState = GameState.DeathScreen;
            }

            if(Keyboard.GetState().IsKeyDown(Keys.R) && CurrentGameState == GameState.DeathScreen)
            {
                player.Reset(CollisionMapArray);
                android.Reset(CollisionMapArray);
                this.CurrentGameState = GameState.Ingame;
            }

            if(Keyboard.GetState().IsKeyDown(Keys.Escape) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
            {
                switch(CurrentGameState)
                {
                    case GameState.Ingame:
                        CurrentGameState = GameState.Paused;
                        break;
                    case GameState.Paused:
                        CurrentGameState = GameState.Ingame;
                        break;
                }

                LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }


            grad = (Math.Atan((player.Position.Y - android.Position.Y) / (player.Position.X - android.Position.X)));

            base.Update(gameTime);
        }

        // This is called when the game should draw itself.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        { 
            GraphicsDevice.Clear(Color.TransparentBlack);

            // TODO: Add your drawing code here
            
            var transformMatrix = player.camera.GetViewMatrix();
            spriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointClamp);


            // Display Debug-Information
            if (settings.getAreDebugInformationsVisible())
            {
                fps.DrawFps(spriteBatch, DebugFont,
                    player.camera.Position,
                    Color.White);

                spriteBatch.DrawString(DebugFont,
                    "Player:\n" + " X: " + ((int)player.Position.X).ToString() + " Y: " + ((int)player.Position.Y).ToString(),
                    new Vector2(player.Position.X - (settings.getResolutionWidth() / 2), player.Position.Y - (settings.getResolutionHeight() / 2) + 125),
                    Color.White);
            }
                
            // MapRenderer zum Zeichnen der aktuell sichtbaren Map
            map[AktuelleMap].mapRenderer.Draw(player.camera.GetViewMatrix());

            // Spieler Sprite ausgeben
            player.Draw(spriteBatch,font);
            android.Draw(spriteBatch, font);
            gui.Draw(spriteBatch, font, player);

            if(CurrentGameState == GameState.Paused)
            {
                spriteBatch.Draw(gui.PausedBackground, new Vector2(player.Position.X - 960, player.Position.Y - 540), Color.White);
                spriteBatch.DrawString(MenuFont, "PAUSE",new Vector2(player.Position.X - 100, player.Position.Y - 64),Color.White);
            }

            if(CurrentGameState == GameState.DeathScreen)
            {

                spriteBatch.Draw(gui.PausedBackground, new Vector2(player.Position.X - 960, player.Position.Y - 540), Color.White);
                spriteBatch.DrawString(MenuFont, Texttable.Text_Died, new Vector2(player.Position.X - 300, player.Position.Y - 64), Color.White);
            }

            spriteBatch.Draw(gui.CustomCurserTexture, new Vector2(player.camera.Position.X + Mouse.GetState().X, player.camera.Position.Y + Mouse.GetState().Y), Color.White);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
