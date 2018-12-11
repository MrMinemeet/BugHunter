using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using TexturePackerLoader;

namespace BugHunter
{
    class Projectile
    {
        public bool IsActive = false;
        public float ProjectileSpeed = 400f;
        public Vector2 ProjectilePosition;

        public double TimeSinceShot = 0;
        public enum Directions : byte { Up, Down, Left, Right }
        public Directions aktDirection;
        public Weapon.WeaponTypes ProjectileType;
        public Texture2D texture;

        public Game1 game;

        public void Init(Game1 game)
        {
            this.game = game;
        }

        public void UpdateShot(GameTime gameTime, Player player)
        {
            // Bewegt das Projektil in die vorgesehene Richtung
            switch (aktDirection)
            {
                case Directions.Right:
                    ProjectilePosition.X += ProjectileSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Directions.Left:
                    ProjectilePosition.X -= ProjectileSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Directions.Up:
                    ProjectilePosition.Y -= ProjectileSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case Directions.Down:
                    ProjectilePosition.Y += ProjectileSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
            }

            // Löscht den Schuss nach 3 Sekunden
            if (gameTime.TotalGameTime.TotalSeconds - TimeSinceShot >= 3)
            {
                this.IsActive = false;
            }
            
            // Überprüfen ob Projektil Hitbox der Map getroffen hat
            if(DidHitCollision(game.MapArray,game.map[0].getTiledMap()))
            {
                this.IsActive = false;
            }
            
        }

        /// <summary>
        /// Funktion zum Zeichnen des Projektiles
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteSheet">Spritesheet für Projektile</param>
        public void DrawShot(SpriteBatch spriteBatch, SpriteSheet spriteSheet)
        {
            SpriteRender spriteRender = new SpriteRender(spriteBatch);

            
            // Zeichnet Projektil je nach Projektilart
            switch (ProjectileType)
            {
                case Weapon.WeaponTypes.cpp:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.Cpp), this.ProjectilePosition);
                    break;
                case Weapon.WeaponTypes.c:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.C), this.ProjectilePosition);
                    break;
                case Weapon.WeaponTypes.java:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.Java), this.ProjectilePosition);
                    break;
                case Weapon.WeaponTypes.maschinensprache:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.Maschinensprache), this.ProjectilePosition);
                    break;
            }
        }

        private bool DidHitCollision(int[][] CollisionMapArray, TiledMap map)
        {
            for (int y = 0; y * Settings.TilePixelSize <= map.HeightInPixels; y++)
            {
                for (int x = 0; x < CollisionMapArray[y].Length; x++)
                {

                    if ((((ProjectilePosition.Y >= Settings.TilePixelSize * y) || (ProjectilePosition.Y + texture.Height >= Settings.TilePixelSize * y)) && ((ProjectilePosition.Y <= Settings.TilePixelSize * (y + 1)) || (ProjectilePosition.Y <= Settings.TilePixelSize * (y + 1))))
                        && (((ProjectilePosition.X >= Settings.TilePixelSize * x) || (ProjectilePosition.X + texture.Width >= Settings.TilePixelSize * x)) && ((ProjectilePosition.X <= Settings.TilePixelSize * (x + 1)) || (ProjectilePosition.X <= Settings.TilePixelSize * (x + 1)))))

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

        public bool CheckForHit(Android enemy)
        {
            SpriteFrame sp = enemy.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.android_packed.Sprites_android1);
            Texture2D EnemyTexture = sp.Texture;


            if (
                ((ProjectilePosition.X + texture.Width / 2 >= enemy.Position.X - EnemyTexture.Width /4 / 2 && ProjectilePosition.X - texture.Width / 2 <= enemy.Position.X + EnemyTexture.Width /4 / 2)
                && (ProjectilePosition.Y + texture.Height / 2 >= enemy.Position.Y - EnemyTexture.Height / 2 && ProjectilePosition.Y - texture.Height / 2 <= enemy.Position.Y + EnemyTexture.Height / 2))
                )
            {
                // Projektil weg schalten
                this.IsActive = false;

                return true;
            }

            return false;
            
        }
    }
}
