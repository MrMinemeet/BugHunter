using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using TexturePackerLoader;

namespace BugHunter
{
    class Android
    {
        public Vector2 Position;
        public float Speed { get; set; }
        public int MaxHealth { get; }
        public int Health { get; set; }
        public Texture2D OriginTexture;
        public bool ShowPlayerOrigin = false;

        // Potentielle Neue Position
        public Vector2 PotNewEnemyPosition;

        // Textures
        public SpriteSheet spriteSheet;
        public SpriteRender spriteRender;

        public bool IsActive = true;


        public double LastCollisionCheck = 0;

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
        public void Update(GameTime gameTime, int[][] CollisionMapArray, TiledMap map, Player player)
        {
            if (IsActive)
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

                if (
                    ((PotNewEnemyPosition.X + spriteSheet.Sprite("sprites/android1").Texture.Width / 4 / 2 >= player.Position.X - player.Texture.Width / 2 && PotNewEnemyPosition.X - spriteSheet.Sprite("sprites/android1").Texture.Width / 4 / 2 <= player.Position.X + player.Texture.Width / 2)
                    && (PotNewEnemyPosition.Y + spriteSheet.Sprite("sprites/android1").Texture.Height / 2 >= player.Position.Y - player.Texture.Height / 2 && PotNewEnemyPosition.Y - spriteSheet.Sprite("sprites/android1").Texture.Height / 2 <= player.Position.Y + player.Texture.Height / 2))
                    )
                {
                    if (gameTime.TotalGameTime.TotalMilliseconds - LastCollisionCheck >= 500)
                    {
                        player.Health = player.Health - 1;
                        LastCollisionCheck = gameTime.TotalGameTime.TotalMilliseconds;
                        player.GotHit(this);
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
                            this.Health--;
                        }
                    }
                }
            }
        }
        


        /// <summary>
        /// Setzt Enemy zurück auf starteinstellung
        /// </summary>
        /// <param name="MapArray"></param>
        public void Reset(int[][] MapArray)
        {
            this.Health = 100;
            SetSpawnFromMap(MapArray);
        }

        /// <summary>
        /// Sucht nach Spawn Tile im Maparray und setzt den Spieler darauf.
        /// </summary>
        /// <param name="MapArray"></param>        
        public void SetSpawnFromMap(int[][] MapArray)
        {
            int x, y;

            for (y = 0; y < MapArray.Length; y++)
            {
                for (x = 0; x < MapArray[y].Length; x++)
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
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.android_packed.Sprites_android4),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.75f >= this.Health && this.Health > this.MaxHealth * 0.50f)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.android_packed.Sprites_android3),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.50f >= this.Health && this.Health > this.MaxHealth * 0.25f)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.android_packed.Sprites_android2),
                    this.Position,
                    Color.White);
            }
            if (this.MaxHealth * 0.25f >= this.Health && this.Health > 0)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.android_packed.Sprites_android1),
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
                new Vector2(spriteSheet.Sprite("sprites/android1").Texture.Width / 2, spriteSheet.Sprite("sprites/android1").Texture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f
                );
            }
        }
    }
}
