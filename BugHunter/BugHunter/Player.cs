using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using TexturePackerLoader;

namespace BugHunter
{
    public class Player
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position;
        public Vector2 PotNewPosition;
        public float Speed { get; set; }
        public Texture2D OriginTexture { get; set; }
        public Texture2D DamageTexture { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsReloading = false;
        public int Damageboost = 0;

        public double ReloadTime = 0;

        // Munitionsanzahl
        public IDictionary<Weapon.WeaponTypes, int> AmmunitionAmmountList = new Dictionary<Weapon.WeaponTypes, int>();


        private int[][] CollisionMapArray;
        private TiledMap map;
        public OrthographicCamera camera;

        private bool ShowPlayerOrigin = false;

        // Waffen
        public List<Projectile> projectiles = new List<Projectile>();
        public SpriteSheet WeaponSpriteSheet;
        private double lastTimeShot = 0;
        public Weapon.WeaponTypes aktWeapon = Weapon.WeaponTypes.c;
        private double lastWeaponChangeLeft = 0;
        private double lastWeaponChangeRight = 0;

        Settings settings;
        SoundFX sound;

        Game1 game;

        // Vibration
        private bool IsVibrating = false;
        private float VibrationLeft = 0;
        private float VibrationRight = 0;
        private float VibrationTimeStart = 0;
        private int VibrationDuration = 0;

        /// <summary>
        /// Konstruktorfür Klasse Android
        /// </summary>
        /// <param name="Speed">Bewegungsgeschwindigkeit</param>
        /// <param name="MaxHealth">Maximales Leben (= Startleben)</param>
        public Player(Game1 game, float Speed, int MaxHealth)
        {
            this.game = game;
            this.Speed = Speed;
            this.MaxHealth = MaxHealth;
            this.Health = MaxHealth;

            AmmunitionAmmountList.Add(Weapon.WeaponTypes.c, game.weapon.CAmmoAmount);
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.cpp, game.weapon.CppAmmoAmout);
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.java, game.weapon.JavaAmmoAmount);
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.csharp, game.weapon.CsharpAmmoAmount);
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.maschinensprache, game.weapon.MaschinenspracheAmmoAmount);
        }


        public void Reset(int[][] MapArray)
        {
            this.Health = MaxHealth;
            SetSpawnFromMap(MapArray);

            this.Damageboost = 0;
            this.IsVibrating = false;

            AmmunitionAmmountList[Weapon.WeaponTypes.c] = game.weapon.CAmmoAmount;
            AmmunitionAmmountList[Weapon.WeaponTypes.cpp] = game.weapon.CppAmmoAmout;
            AmmunitionAmmountList[Weapon.WeaponTypes.java] = game.weapon.JavaAmmoAmount;
            AmmunitionAmmountList[Weapon.WeaponTypes.csharp] = game.weapon.CsharpAmmoAmount;
            AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] = game.weapon.MaschinenspracheAmmoAmount;
        }


        /// <summary>
        /// Updaten von Spieler
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="CollisionMapArray"></param>
        /// <param name="map"></param>
        public void Update(GameTime gameTime, int[][] CollisionMapArray, TiledMap map)
        {
            Rectangle MapTriggerRectangle;
            Rectangle PotNewPlayerCollision;

            this.CollisionMapArray = CollisionMapArray;
            this.map = map;
            PotNewPosition = Position;

            // Updaten der Player steuerung
            Movement(gameTime);

            // Updaten des Player schießen
            UpdatePlayerShooting(gameTime);

            // RELOAD ÜBERPRÜFUNG

            // Array durchlaufen
            for (int y = 0; y * Settings.TilePixelSize < map.HeightInPixels; y++)
            {
                for (int x = 0; x < CollisionMapArray[y].Length; x++)
                {                    
                    // Rechtecke über Spieler und aktuelles Tile ziehen
                    MapTriggerRectangle = new Rectangle((x * Settings.TilePixelSize), (y * Settings.TilePixelSize), Settings.TilePixelSize, Settings.TilePixelSize);
                    PotNewPlayerCollision = new Rectangle((int)(PotNewPosition.X - Texture.Width / 2), (int)(PotNewPosition.Y - Texture.Height / 2), Texture.Width, Texture.Height);
                    // Überprüfen ob sich die beiden Rechtecke überschneiden
                    if (PotNewPlayerCollision.Intersects(MapTriggerRectangle))
                    {
                        // Schauen ob aktuelles Tile ein Trigger Tile ist
                        if (CollisionMapArray[y][x].Equals(Settings.ReloadTileId))
                        {
                            this.IsReloading = true;
                            if (gameTime.TotalGameTime.TotalSeconds - ReloadTime > 0.5)
                            {
                                ReloadTime = gameTime.TotalGameTime.TotalSeconds;
                                if (AmmunitionAmmountList[Weapon.WeaponTypes.c] < game.weapon.CAmmoAmount)
                                    AmmunitionAmmountList[Weapon.WeaponTypes.c] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.cpp] < game.weapon.CppAmmoAmout)
                                    AmmunitionAmmountList[Weapon.WeaponTypes.cpp] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.csharp] < game.weapon.CsharpAmmoAmount)
                                    AmmunitionAmmountList[Weapon.WeaponTypes.csharp] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.java] < game.weapon.JavaAmmoAmount)
                                    AmmunitionAmmountList[Weapon.WeaponTypes.java] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] < game.weapon.MaschinenspracheAmmoAmount)
                                    AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] += 1;
                            }
                        }
                        else
                        {
                            this.IsReloading = false;
                            ReloadTime = gameTime.TotalGameTime.TotalSeconds;
                        }
                    }
                }
            }
            // Waffenart updaten
            WeaponUpdate(gameTime);            

            // Überprüfung und ausführung vom Vibrationen
            if (gameTime.TotalGameTime.TotalMilliseconds - this.VibrationTimeStart >= this.VibrationDuration)
                this.IsVibrating = false;
            
            if (this.IsVibrating)
            {
                GamePad.SetVibration(PlayerIndex.One, this.VibrationLeft, this.VibrationRight);
            }
            
            // Updated jedes aktive Projektil im Array
            foreach(Projectile p in projectiles)
            {
                p.UpdateShot(gameTime, this);
            }

            // Überprüft ob Projektile abgelaufen oder collided sind und löscht diese
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].DidProjectileHitCollision(CollisionMapArray, map) || projectiles[i].IsProjectileTimeOver(gameTime))
                {
                    projectiles.Remove(projectiles[i]);
                }
            }
        }

        /// <summary>
        /// Überprüft collision für den Spieler
        /// </summary>
        public void CheckCollisions()
        {
            if(DidHitCollision(this.CollisionMapArray, game.map[game.AktuelleMap].maplevel))
            {
                this.PotNewPosition = this.Position;
            }
        }

        /// <summary>
        /// Berechnet neue theoretische Position
        /// </summary>
        public void Movement(GameTime gameTime)
        {
            var kstate = Keyboard.GetState();
            var gamepadState = GamePad.GetState(PlayerIndex.One);

            // Überprüfe auf Sprint
            float Speed;

            if (kstate.IsKeyDown(Keys.LeftShift) || gamepadState.IsButtonDown(Buttons.LeftTrigger))
            {
                Speed = this.Speed * 2;
            }
            else
            {
                Speed = this.Speed;
            }


            if (kstate.IsKeyDown(Keys.W) || gamepadState.ThumbSticks.Left.Y > 0)
            {

                this.PotNewPosition.Y -= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.S) || gamepadState.ThumbSticks.Left.Y < 0)
            {
                this.PotNewPosition.Y += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.A) || gamepadState.ThumbSticks.Left.X < 0)
            {
                this.PotNewPosition.X -= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.D) || gamepadState.ThumbSticks.Left.X > 0)
            {
                this.PotNewPosition.X += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Bewegt Spieler auf neue Position
        /// </summary>
        public void Move()
        {
            this.Position = this.PotNewPosition;

            // Kamera über Spieler setzen
            camera.LookAt(Position);
        }

        // Method for checking if the player shoots
        private void UpdatePlayerShooting(GameTime gameTime)
        {
            var kstate = Keyboard.GetState();
            var gamepadState = GamePad.GetState(PlayerIndex.One);
            Random random = new Random();

            // Prüft ob noch Munition vorhanden ist
            if (AmmunitionAmmountList[aktWeapon] > 0)
            {
                // Initialisiert Projektil und stellt richtung, Position und Waffenart ein
                if (kstate.IsKeyDown(Keys.Right) || gamepadState.IsButtonDown(Buttons.B))
                {
                    if (gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= game.weapon.getDelayAktWeapon(aktWeapon))
                    {
                        projectiles.Add(new Projectile());


                        sound.Schuesse[random.Next(sound.Schuesse.Length - 1)].Play();
                        AmmunitionAmmountList[aktWeapon]--;

                        projectiles[projectiles.Count - 1].Init(this.game, this);
                        projectiles[projectiles.Count - 1].ProjectilePosition = this.Position;
                        projectiles[projectiles.Count - 1].TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                        projectiles[projectiles.Count - 1].aktDirection = Projectile.Directions.Right;
                        projectiles[projectiles.Count - 1].ProjectileType = aktWeapon;
                        projectiles[projectiles.Count - 1].textureVersion = (byte)random.Next(8);

                        lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
                else if (kstate.IsKeyDown(Keys.Left) || gamepadState.IsButtonDown(Buttons.X))
                {
                    if (gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= game.weapon.getDelayAktWeapon(aktWeapon))
                    {
                        projectiles.Add(new Projectile());
                        sound.Schuesse[random.Next(sound.Schuesse.Length - 1)].Play();
                        AmmunitionAmmountList[aktWeapon]--;

                        projectiles[projectiles.Count - 1].Init(this.game, this);
                        projectiles[projectiles.Count - 1].ProjectilePosition = this.Position;
                        projectiles[projectiles.Count - 1].TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                        projectiles[projectiles.Count - 1].aktDirection = Projectile.Directions.Left;
                        projectiles[projectiles.Count - 1].ProjectileType = aktWeapon;
                        projectiles[projectiles.Count - 1].textureVersion = (byte)random.Next(8);

                        lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
                else if (kstate.IsKeyDown(Keys.Up) || gamepadState.IsButtonDown(Buttons.Y))
                {
                    if (gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= game.weapon.getDelayAktWeapon(aktWeapon))
                    {
                        projectiles.Add(new Projectile());

                        sound.Schuesse[random.Next(sound.Schuesse.Length - 1)].Play();
                        AmmunitionAmmountList[aktWeapon]--;

                        projectiles[projectiles.Count - 1].Init(this.game, this);
                        projectiles[projectiles.Count - 1].ProjectilePosition = this.Position;
                        projectiles[projectiles.Count - 1].TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                        projectiles[projectiles.Count - 1].aktDirection = Projectile.Directions.Up;
                        projectiles[projectiles.Count - 1].ProjectileType = aktWeapon;
                        projectiles[projectiles.Count - 1].textureVersion = (byte)random.Next(8);

                        lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
                else if (kstate.IsKeyDown(Keys.Down) || gamepadState.IsButtonDown(Buttons.A))
                {
                    if (gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= game.weapon.getDelayAktWeapon(aktWeapon))
                    {
                        projectiles.Add(new Projectile());

                        sound.Schuesse[random.Next(sound.Schuesse.Length - 1)].Play();
                        AmmunitionAmmountList[aktWeapon]--;

                        projectiles[projectiles.Count - 1].Init(this.game, this);
                        projectiles[projectiles.Count - 1].ProjectilePosition = this.Position;
                        projectiles[projectiles.Count - 1].TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                        projectiles[projectiles.Count - 1].aktDirection = Projectile.Directions.Down;
                        projectiles[projectiles.Count - 1].ProjectileType = aktWeapon;
                        projectiles[projectiles.Count - 1].textureVersion = (byte)random.Next(8);

                        lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
            }
        }

        /// <summary>
        /// Erkennt von wo der Spieler getroffen wurde und platziert ihn etwas anders
        /// </summary>
        /// <param name="enemyPosition"></param>
        public void GotHit(GameTime gameTime)
        {
            this.SetVibration(0.1f, 0.1f, 250, gameTime);
        }

        private void SetVibration(float VibrationLeft, float VibrationRight, int VibrationDuration, GameTime gameTime)
        {
            this.IsVibrating = true;
            this.VibrationLeft = VibrationLeft;
            this.VibrationRight  = VibrationRight;
            this.VibrationDuration = VibrationDuration;
            this.VibrationTimeStart = (float)gameTime.TotalGameTime.TotalMilliseconds;
        }

        // Überprüft ob Waffe gewechselt wird und setzt die richtige aktiv
        private void WeaponUpdate(GameTime gameTime)
        {
            var gamepadState = GamePad.GetState(PlayerIndex.One);

            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                aktWeapon = Weapon.WeaponTypes.c;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                aktWeapon = Weapon.WeaponTypes.cpp;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                aktWeapon = Weapon.WeaponTypes.java;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D4))
            {
                aktWeapon = Weapon.WeaponTypes.maschinensprache;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D5))
            {
                aktWeapon = Weapon.WeaponTypes.csharp;
            }

            if (gamepadState.IsButtonDown(Buttons.LeftShoulder) && gameTime.TotalGameTime.TotalMilliseconds - this.lastWeaponChangeLeft >= 250)
            {
                // Waffen nach Links durchwechseln
                switch (aktWeapon)
                {
                    case Weapon.WeaponTypes.c:
                        aktWeapon = Weapon.WeaponTypes.csharp;
                        this.lastWeaponChangeLeft = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.cpp:
                        aktWeapon = Weapon.WeaponTypes.c;
                        this.lastWeaponChangeLeft = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.java:
                        aktWeapon = Weapon.WeaponTypes.cpp;
                        this.lastWeaponChangeLeft = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.maschinensprache:
                        aktWeapon = Weapon.WeaponTypes.java;
                        this.lastWeaponChangeLeft = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.csharp:
                        aktWeapon = Weapon.WeaponTypes.maschinensprache;
                        this.lastWeaponChangeLeft = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                }
            }

            if (gamepadState.IsButtonDown(Buttons.RightShoulder) && gameTime.TotalGameTime.TotalMilliseconds - this.lastWeaponChangeRight >= 250)
            {
                // Waffen nach Rechts durchwechseln
                switch (aktWeapon)
                {
                    case Weapon.WeaponTypes.c:
                        aktWeapon = Weapon.WeaponTypes.cpp;
                        this.lastWeaponChangeRight = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.cpp:
                        aktWeapon = Weapon.WeaponTypes.java;
                        this.lastWeaponChangeRight = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.java:
                        aktWeapon = Weapon.WeaponTypes.maschinensprache;
                        this.lastWeaponChangeRight = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.maschinensprache:
                        aktWeapon = Weapon.WeaponTypes.csharp;
                        this.lastWeaponChangeRight = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                    case Weapon.WeaponTypes.csharp:
                        aktWeapon = Weapon.WeaponTypes.c;
                        this.lastWeaponChangeRight = gameTime.TotalGameTime.TotalMilliseconds;
                        break;
                }
            }
        }



        /// <summary>
        /// Überprüft ob Spieler eine Collision der Map berührt
        /// </summary>
        /// <param name="CollisionMapArray"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private bool DidHitCollision(int[][] CollisionMapArray, TiledMap map)
        {
            Rectangle MapCollisionRectangle;
            Rectangle PotNewPlayerCollision;
            
            // Integer Map Array durchlaufen
            for (int y = 0; y * Settings.TilePixelSize < map.HeightInPixels; y++)
            {
                for (int x = 0; x < CollisionMapArray[y].Length; x++)
                {
                    // Schauen ob aktuelles Tile ein Hitbox Tile ist
                    if (CollisionMapArray[y][x].Equals(Settings.HitBoxTileNumber))
                    {
                        // Rechtecke über Spieler und aktuelles Tile ziehen
                        MapCollisionRectangle = new Rectangle((x * Settings.TilePixelSize), (y * Settings.TilePixelSize), Settings.TilePixelSize, Settings.TilePixelSize);
                        PotNewPlayerCollision = new Rectangle((int)(PotNewPosition.X - Texture.Width / 2), (int)(PotNewPosition.Y - Texture.Height / 2), Texture.Width, Texture.Height);

                        // Überprüfen ob sich die beiden Rechtecke überschneiden
                        if (PotNewPlayerCollision.Intersects(MapCollisionRectangle))
                        {
                            // Collision wurde ausgelöst
                            return true;
                        }
                    }
                }
            }

            // Keine Collision erkannt
            return false;
        }

        /// <summary>
        /// Initialisiert Dinge für Spieler
        /// </summary>
        public void Init(Settings settings, Game1 game, SoundFX sound)
        {
            this.game = game;   
            this.settings = settings;
            this.sound = sound;
        }

        /// <summary>
        /// Zeichnen für Spieler
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Zeichnet Spieler
            spriteBatch.Draw(
                Texture,
                Position,
                null,
                Color.White,
                0f,
                new Vector2(Texture.Width / 2, Texture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f
            );
            

            if (ShowPlayerOrigin)
            {
                spriteBatch.Draw(
                    OriginTexture,
                    Position,
                    null,
                    Color.White,
                0f,
                new Vector2(Texture.Width / 2, Texture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f
                );
            }

            // Zeichnet alle aktiven Projektile im Array
            foreach (Projectile p in projectiles)
            {
                p.DrawShot(spriteBatch, this.WeaponSpriteSheet);
            }
        }

        /// <summary>
        /// Sucht nach Spawn Tile im Maparray und setzt den Spieler darauf.
        /// </summary>
        /// <param name="MapArray"></param>
        public void SetSpawnFromMap(int[][] MapArray)
        {
            int x, y;

            for(y = 0; y < MapArray.Length; y++)
            {
                for(x = 0; x  < MapArray[y].Length; x++)
                {
                    if (MapArray[y][x].Equals(Settings.PlayerSpawnTileId))
                    {
                        this.Position.Y = y * Settings.TilePixelSize;
                        this.Position.X = x * Settings.TilePixelSize;
                        return;
                    }
                }
            }
        }
    }
}
