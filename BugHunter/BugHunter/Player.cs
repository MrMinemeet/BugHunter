using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

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


        // Potentielle Neue Position
        private Vector2 PotNewPlayerPosition;


        private int[][] CollisionMapArray;
        private TiledMap map;
        public OrthographicCamera camera;

        private bool ShowPlayerOrigin = false;

        public enum Weapons : byte { cpp, java, c, maschinensprache }

        public Weapons aktWeapon = Weapons.c;


        // Schuss
        public bool HasShot = false;
        public float ProjectileSpeed = 400f;
        public Vector2 ProjectilePosition;
        private double TimeSinceShot = 0;


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
        }


        public void Reset(int[][] MapArray)
        {
            this.Health = MaxHealth;
            SetSpawnFromMap(MapArray);
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
                        // Abfragen und co
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

            if (kstate.IsKeyDown(Keys.Right) && !HasShot)
            {
                shoot();
                TimeSinceShot = gameTime.TotalGameTime.TotalSeconds;
            }

            if (HasShot)
            {
                ProjectilePosition.X += ProjectileSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                
                if(gameTime.TotalGameTime.TotalSeconds - TimeSinceShot >= 2)
                {
                    HasShot = false;
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
            enemy.Health--;
        }

        public void shoot()
        {
            if (!HasShot)
            {
                HasShot = true;
                ProjectilePosition = this.Position;
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

            if (HasShot)
            {
                spriteBatch.Draw(
                    OriginTexture,
                    this.ProjectilePosition,
                    null,
                    Color.White,
                0f,
                new Vector2(Texture.Width / 2, Texture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f
                );
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
