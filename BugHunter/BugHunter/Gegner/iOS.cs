﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;
using TexturePackerLoader;

namespace BugHunter
{
    public class iOS
    {
        public Vector2 Position;
        public float Speed { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int attackDamage = 1;
        public Player player;


        public bool IsDead = false;

        // Potentielle Neue Position
        public Vector2 PotNewEnemyPosition;

        // Textures
        public SpriteRender spriteRender;


        Settings settings;


        public double LastCollisionCheck = 0;

        public Game1 game;


        /// <summary>
        /// Konstruktorfür Klasse iOS
        /// </summary>
        /// <param name="Speed">Bewegungsgeschwindigkeit</param>
        /// <param name="MaxHealth">Maximales Leben (= Startleben)</param>
        public iOS(float Speed, int MaxHealth, int attackDamage, Game1 game, Settings settings, Player player)
        {
            this.Speed = Speed;
            this.MaxHealth = MaxHealth;
            this.Health = MaxHealth;
            this.attackDamage = attackDamage;
            this.game = game;
            this.settings = settings;
            this.player = player;
        }

        /// <summary>
        /// Update für iOS
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="CollisionMapArray"></param>
        /// <param name="map"></param>
        /// <param name="player"></param>
        public void Update(GameTime gameTime, int[][] CollisionMapArray, TiledMap map)
        {
            this.PotNewEnemyPosition = this.Position;

            if (this.Position.X > player.Position.X)
            {
                this.PotNewEnemyPosition.X -= this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (this.Position.X < player.Position.X)
            {
                this.PotNewEnemyPosition.X += this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (this.Position.Y > player.Position.Y)
            {
                this.PotNewEnemyPosition.Y -= this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (this.Position.Y < player.Position.Y)
            {
                this.PotNewEnemyPosition.Y += this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Bekommt einen Frame
            SpriteFrame sp = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.IOS_10);

            // Mit sp.Size erhält man einen Vektor mit der größe einer einzelnen Textur. Ist einfach und zudem noch effizienter
            if (
                ((PotNewEnemyPosition.X + sp.Size.X / 2 >= player.Position.X - player.Frame.Size.X / 2 && PotNewEnemyPosition.X - sp.Size.X / 2 <= player.Position.X + player.Frame.Size.X / 2)
                && (PotNewEnemyPosition.Y + sp.Size.Y / 2 >= player.Position.Y - player.Frame.Size.Y / 2 && PotNewEnemyPosition.Y - sp.Size.Y / 2 <= player.Position.Y + player.Frame.Size.Y / 2))
                )
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - LastCollisionCheck >= 500)
                {
                    player.Health = (int)(player.Health - attackDamage);
                    LastCollisionCheck = gameTime.TotalGameTime.TotalMilliseconds;
                    player.GotHit(gameTime);

                    // Zufälligen Damagesound abspielen
                    player.PlayPlayerDamage(gameTime);
                }
            }
            else
            {
                this.Position = PotNewEnemyPosition;
            }

            // Überprüft ob von Projektil getroffen wurde
            for (int i = 0; i < player.projectiles.Count; i++)
            {
                if (player.projectiles[i].CheckForHitiOS(this))
                {
                    this.Health -= (game.weapon.GetDamageAktWeapon(player.aktWeapon) + player.Damageboost);

                    player.projectiles.Remove(player.projectiles[i]);
                    game.gameStats.AnzahlTreffer++;

                    // Falls Gegner 0 leben hat, soll dieser despawnen und der Score erhöht werden.
                    if (this.Health <= 0)
                    {
                        game.Score += 100;
                        this.IsDead = true;
                    }
                }
            }
        }

        // Sucht nach Spawn Tile im Maparray und setzt den iOS Gegner darauf.
        public void SetSpawnFromMap(int[][] MapArray)
        {
            Random random = new Random();

            int x, y;
            while (true)
            {
                for (y = random.Next(settings.MapSizeHeight); y < MapArray.Length; y++)
                {
                    for (x = random.Next(settings.MapSizeWidth); x < MapArray[y].Length; x++)
                    {
                        if (MapArray[y][x].Equals(Settings.EnemeySpawnTileId))
                        {
                            this.Position.Y = y * Settings.TilePixelSize;
                            this.Position.X = x * Settings.TilePixelSize;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw Funktion für iOS
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteRender = new SpriteRender(spriteBatch);

            if (this.MaxHealth >= this.Health && this.Health > this.MaxHealth * 0.75f)
            {
                spriteRender.Draw(
                    game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.IOS_7),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.75f >= this.Health && this.Health > this.MaxHealth * 0.50f)
            {
                spriteRender.Draw(
                    game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.IOS_8),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.50f >= this.Health && this.Health > this.MaxHealth * 0.25f)
            {
                spriteRender.Draw(
                    game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.IOS_9),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.25f >= this.Health && this.Health > 0)
            {
                spriteRender.Draw(
                    game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.IOS_10),
                    this.Position,
                    Color.White);
            }
        }
    }
}
