using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;
using TexturePackerLoader;

namespace BugHunter
{
    class Android
    {
        public Vector2 Position;
        public float Speed { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public Texture2D OriginTexture;
        public bool ShowPlayerOrigin = false;
        public float attackDamage = 1;
        public Player player;
        

        public bool IsDead = false;

        // Potentielle Neue Position
        public Vector2 PotNewEnemyPosition;

        // Textures
        public SpriteSheet spriteSheet;
        public SpriteRender spriteRender;


        Settings settings;


        public double LastCollisionCheck = 0;

        public Game1 game;

        public void Init(Game1 game, Settings settings, Player player)
        {
            this.game = game;
            this.settings = settings;
            this.player = player;
        }

        /// <summary>
        /// Konstruktorfür Klasse Android
        /// </summary>
        /// <param name="Speed">Bewegungsgeschwindigkeit</param>
        /// <param name="MaxHealth">Maximales Leben (= Startleben)</param>
        public Android(float Speed, int MaxHealth)
        {
            this.Speed = Speed;
            this.MaxHealth = MaxHealth;
            this.Health = MaxHealth;
        }

         /// <summary>
         /// Update für Android
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
            SpriteFrame sp = spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android1);
            
            // Mit sp.Size erhält man einen Vektor mit der größe einer einzelnen Textur. Ist einfach und zudem noch effizienter
            if (
                ((PotNewEnemyPosition.X + sp.Size.X / 2 >= player.Position.X - player.Texture.Width / 2 && PotNewEnemyPosition.X - sp.Size.X / 2 <= player.Position.X + player.Texture.Width / 2)
                && (PotNewEnemyPosition.Y + sp.Size.Y / 2 >= player.Position.Y - player.Texture.Height / 2 && PotNewEnemyPosition.Y - sp.Size.Y / 2 <= player.Position.Y + player.Texture.Height / 2))
                )
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - LastCollisionCheck >= 500)
                {
                    player.Health = (int)(player.Health - attackDamage);
                    LastCollisionCheck = gameTime.TotalGameTime.TotalMilliseconds;
                    player.hitmarkerTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                    player.GotHit(this, gameTime);
                }
            }
            else
            {
                this.Position = PotNewEnemyPosition;
            }

            foreach (Projectile p in player.projectiles)
            {
                if (p.IsActive)
                {
                    if (p.CheckForHit(this))
                    {
                        this.Health -= Weapon.getDamageAktWeapon(player.aktWeapon);


                        // Falls Gegner 0 leben hat, soll dieser despawnen und der Score erhöht werden.
                        if (this.Health <= 0)
                        {
                            this.attackDamage = attackDamage * 1.1f;
                            game.Score += 100;
                            this.IsDead = true;
                        }
                    }
                }
            }
        }

        // Sucht nach Spawn Tile im Maparray und setzt den Spieler darauf.
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
        /// Draw Funktion für Android
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteRender = new SpriteRender(spriteBatch);
            if (this.MaxHealth >= this.Health && this.Health > this.MaxHealth * 0.75f)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android4),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.75f >= this.Health && this.Health > this.MaxHealth * 0.50f)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android3),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.50f >= this.Health && this.Health > this.MaxHealth * 0.25f)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android2),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.25f >= this.Health && this.Health > 0)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android1),
                    this.Position,
                    Color.White);
            }
            
            if (ShowPlayerOrigin)
            {
                spriteBatch.Draw(
                    OriginTexture,
                    Position,
                    null,
                    Color.White,
                0f,
                new Vector2(spriteSheet.Sprite("android1").Texture.Width / 2, spriteSheet.Sprite("android1").Texture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f
                );
            }
        }
    }
}
