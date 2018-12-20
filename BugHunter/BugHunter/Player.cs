﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using TexturePackerLoader;

namespace BugHunter
{
    class Player
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position;
        public float Speed { get; set; }
        public Texture2D OriginTexture { get; set; }
        public Texture2D DamageTexture { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsReloading = false;

        public double ReloadTime = 0;

        // Munitionsanzahl
        public IDictionary<Weapon.WeaponTypes, int> AmmunitionAmmountList = new Dictionary<Weapon.WeaponTypes, int>();


        // Potentielle Neue Position
        private Vector2 PotNewPlayerPosition;


        private int[][] CollisionMapArray;
        private TiledMap map;
        public OrthographicCamera camera;

        private bool ShowPlayerOrigin = false;

        // Waffen
        public Projectile[] projectiles = new Projectile[100];
        public SpriteSheet WeaponSpriteSheet;
        private double lastTimeShot = 0;
        public Weapon.WeaponTypes aktWeapon = Weapon.WeaponTypes.c;


        Settings settings;

        Game1 game;

        /// <summary>
        /// Konstruktorfür Klasse Android
        /// </summary>
        /// <param name="Speed">Bewegungsgeschwindigkeit</param>
        /// <param name="MaxHealth">Maximales Leben (= Startleben)</param>
        public Player(float Speed, int MaxHealth)
        {
            this.Speed = Speed;
            this.MaxHealth = MaxHealth;
            this.Health = MaxHealth;

            AmmunitionAmmountList.Add(Weapon.WeaponTypes.c, Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.c));
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.cpp, Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.cpp));
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.java, Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.java));
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.csharp, Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.csharp));
            AmmunitionAmmountList.Add(Weapon.WeaponTypes.maschinensprache, Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.maschinensprache));
        }


        public void Reset(int[][] MapArray)
        {
            this.Health = MaxHealth;
            SetSpawnFromMap(MapArray);

            AmmunitionAmmountList[Weapon.WeaponTypes.c] = Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.c);
            AmmunitionAmmountList[Weapon.WeaponTypes.cpp] = Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.cpp);
            AmmunitionAmmountList[Weapon.WeaponTypes.java] = Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.java);
            AmmunitionAmmountList[Weapon.WeaponTypes.csharp] = Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.csharp);
            AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] = Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.maschinensprache);
        }


        /// <summary>
        /// Updaten von Spieler
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="CollisionMapArray"></param>
        /// <param name="map"></param>
        public void Update(GameTime gameTime, int[][] CollisionMapArray, TiledMap map)
        {
            var kstate = Keyboard.GetState();
            this.CollisionMapArray = CollisionMapArray;
            this.map = map;
            PotNewPlayerPosition = Position;

            for (int y = 0; y * Settings.TilePixelSize <= map.HeightInPixels; y++)
            {
                for (int x = 0; x < CollisionMapArray[y].Length; x++)
                {
                    if ((((Position.Y >= Settings.TilePixelSize * y) || (Position.Y + Texture.Height >= Settings.TilePixelSize * y)) && ((Position.Y <= Settings.TilePixelSize * (y + 1)) || (Position.Y <= Settings.TilePixelSize * (y + 1))))
                        && (((Position.X >= Settings.TilePixelSize * x + 32) || (Position.X + Texture.Width >= Settings.TilePixelSize * x + 32)) && ((Position.X <= Settings.TilePixelSize * (x + 1) + 32) || (Position.X <= Settings.TilePixelSize * (x + 1) + 32))))
                    {
                        if(CollisionMapArray[y][x] == Settings.ReloadTileId)
                        {
                            IsReloading = true;
                            if(gameTime.TotalGameTime.TotalSeconds - ReloadTime > 0.5)
                            {
                                ReloadTime = gameTime.TotalGameTime.TotalSeconds;
                                if (AmmunitionAmmountList[Weapon.WeaponTypes.c] < Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.c))
                                    AmmunitionAmmountList[Weapon.WeaponTypes.c] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.cpp] < Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.cpp))
                                    AmmunitionAmmountList[Weapon.WeaponTypes.cpp] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.csharp] < Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.csharp))
                                    AmmunitionAmmountList[Weapon.WeaponTypes.csharp] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.java] < Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.java))
                                    AmmunitionAmmountList[Weapon.WeaponTypes.java] += 1;

                                if (AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] < Weapon.getMaxAmmoAmountSpecificWeapon(Weapon.WeaponTypes.maschinensprache))
                                    AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] += 1;
                            }
                        }
                        else
                        {
                            IsReloading = false;
                            ReloadTime = gameTime.TotalGameTime.TotalSeconds;
                        }
                    }
                }
            }
            float Speed;
            if (kstate.IsKeyDown(Keys.LeftShift))
            {
                Speed = this.Speed * 2;
            }
            else
            {
                Speed = this.Speed;
            }


            if (kstate.IsKeyDown(Keys.W))
            {

                this.PotNewPlayerPosition.Y -= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.S))
            {
                this.PotNewPlayerPosition.Y += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.A))
            {
                this.PotNewPlayerPosition.X -= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.D))
            {
                this.PotNewPlayerPosition.X += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if(!DidHitCollision(CollisionMapArray,map))
            {
                this.Position = this.PotNewPlayerPosition;
            }
            Random random = new Random();
            // Waffenart updaten
            WeaponUpdate();

            // Prüft ob noch Munition vorhanden ist
            if(AmmunitionAmmountList[aktWeapon] > 0)
            {
                // Initialisiert Projektil und stellt richtung, Position und Waffenart ein
                if (kstate.IsKeyDown(Keys.Right))
                {

                    foreach (Projectile p in projectiles)
                    {
                        if (!p.IsActive && gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= Weapon.getDelayAktWeapon(aktWeapon))
                        {
                            AmmunitionAmmountList[aktWeapon]--;
                            p.IsActive = true;
                            p.ProjectilePosition = this.Position;
                            p.TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                            p.aktDirection = Projectile.Directions.Right;
                            p.ProjectileType = aktWeapon;
                            p.textureVersion = (byte)random.Next(8);

                            lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
                else if (kstate.IsKeyDown(Keys.Left))
                {
                    foreach (Projectile p in projectiles)
                    {
                        if (!p.IsActive && gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= Weapon.getDelayAktWeapon(aktWeapon))
                        {
                            AmmunitionAmmountList[aktWeapon]--;
                            p.IsActive = true;
                            p.ProjectilePosition = this.Position;
                            p.TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                            p.aktDirection = Projectile.Directions.Left;
                            p.ProjectileType = aktWeapon;
                            p.textureVersion = (byte)random.Next(8);

                            lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
                else if (kstate.IsKeyDown(Keys.Left))
                {
                    foreach (Projectile p in projectiles)
                    {
                        if (!p.IsActive && gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= Weapon.getDelayAktWeapon(aktWeapon))
                        {
                            AmmunitionAmmountList[aktWeapon]--;
                            p.IsActive = true;
                            p.ProjectilePosition = this.Position;
                            p.TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                            p.aktDirection = Projectile.Directions.Left;
                            p.ProjectileType = aktWeapon;
                            p.textureVersion = (byte)random.Next(8);

                            lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
                else if (kstate.IsKeyDown(Keys.Up))
                {
                    foreach (Projectile p in projectiles)
                    {
                        if (!p.IsActive && gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= Weapon.getDelayAktWeapon(aktWeapon))
                        {
                            AmmunitionAmmountList[aktWeapon]--;
                            p.IsActive = true;
                            p.ProjectilePosition = this.Position;
                            p.TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                            p.aktDirection = Projectile.Directions.Up;
                            p.ProjectileType = aktWeapon;
                            p.textureVersion = (byte)random.Next(8);

                            lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
                else if (kstate.IsKeyDown(Keys.Down))
                {
                    foreach (Projectile p in projectiles)
                    {
                        if (!p.IsActive && gameTime.TotalGameTime.TotalMilliseconds - lastTimeShot >= Weapon.getDelayAktWeapon(aktWeapon))
                        {
                            AmmunitionAmmountList[aktWeapon]--;
                            p.IsActive = true;
                            p.ProjectilePosition = this.Position;
                            p.TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
                            p.aktDirection = Projectile.Directions.Down;
                            p.ProjectileType = aktWeapon;
                            p.textureVersion = (byte)random.Next(8);

                            lastTimeShot = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
            }

            

            // Updated jedes aktive Projektil im Array
            foreach(Projectile p in projectiles)
            {
                if (p.IsActive)
                {
                    p.UpdateShot(gameTime, this);
                }
            }

            // Kamera über Spieler setzen
            camera.LookAt(Position);
        }

        /// <summary>
        /// Erkennt von wo der Spieler getroffen wurde und platziert ihn etwas anders
        /// </summary>
        /// <param name="enemyPosition"></param>
        public void GotHit(Android enemy)
        {
            
        }

        // Überprüft ob Waffe gewechselt wird und setzt die richtige aktiv
        private void WeaponUpdate()
        {
            for (int i = 0; i < 1; i++)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.D1))
                {
                    aktWeapon = Weapon.WeaponTypes.c;
                    break;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.D2))
                {
                    aktWeapon = Weapon.WeaponTypes.cpp;
                    break;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.D3))
                {
                    aktWeapon = Weapon.WeaponTypes.java;
                    break;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.D4))
                {
                    aktWeapon = Weapon.WeaponTypes.maschinensprache;
                    break;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.D5))
                {
                    aktWeapon = Weapon.WeaponTypes.csharp;
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
            for (int y = 0; y * Settings.TilePixelSize <= map.HeightInPixels; y++)
            {
                for (int x = 0; x < CollisionMapArray[y].Length; x++)
                {

                    if ((((PotNewPlayerPosition.Y >= Settings.TilePixelSize * y) || (PotNewPlayerPosition.Y + Texture.Height >= Settings.TilePixelSize * y)) && ((PotNewPlayerPosition.Y <= Settings.TilePixelSize * (y + 1)) || (PotNewPlayerPosition.Y <= Settings.TilePixelSize * (y + 1))))
                        && (((PotNewPlayerPosition.X >= Settings.TilePixelSize * x + 32) || (PotNewPlayerPosition.X + Texture.Width >= Settings.TilePixelSize * x + 32)) && ((PotNewPlayerPosition.X <= Settings.TilePixelSize * (x + 1) + 32) || (PotNewPlayerPosition.X <= Settings.TilePixelSize * (x + 1) + 32))))

                    {
                        if (CollisionMapArray[y][x] == Settings.HitBoxTileNumber)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Initialisiert Dinge für Spieler
        /// </summary>
        public void Init(Settings settings, Game1 game)
        {
            this.game = game;   
            this.settings = settings;
            for (int i = 0; i < projectiles.Length; i++)
            {
                projectiles[i] = new Projectile();

                projectiles[i].Init(game, this);
            }
        }

        /// <summary>
        /// Zeichnen für Spieler
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
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

            // Zeichnet alle aktiven Projektile im Arraye
            foreach (Projectile p in projectiles)
            {
                if (p.IsActive)
                {
                    p.DrawShot(spriteBatch, this.WeaponSpriteSheet);
                }
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
