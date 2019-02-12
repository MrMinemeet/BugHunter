using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using TexturePackerLoader;

namespace BugHunter
{
    public class Projectile
    {
        public bool IsActive = false;
        public float ProjectileSpeed = 400f;
        public Vector2 ProjectilePosition;

        public double TimeSinceShot = 0;
        public enum Directions : byte { Up, Down, Left, Right }
        public Directions aktDirection;
        public Weapon.WeaponTypes ProjectileType;

        public byte textureVersion;

        public Game1 game;
        public Player player;

        public Projectile(Game1 game)
        {
            this.game = game;
            this.player = game.player;
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
            
        }

        /// <summary>
        /// Überprüft ob der Schuss abgelaufen ist
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public bool IsProjectileTimeOver(GameTime gameTime)
        {
            if(gameTime.TotalGameTime.TotalSeconds - TimeSinceShot >= 3)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Überprüft ob der Schuss eine Wand getroffen hat
        /// </summary>
        /// <param name="mapArray"></param>
        /// <returns></returns>
        public bool DidProjectileHitCollision(int[][] CollisionMapArray, TiledMap map)
        {
            SpriteFrame projectileFrame = null;

            switch (ProjectileType)
            {
                case Weapon.WeaponTypes.cpp:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Cpp); break;
                case Weapon.WeaponTypes.c:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.C); break;
                case Weapon.WeaponTypes.java:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Java); break;
                case Weapon.WeaponTypes.maschinensprache:
                    switch (this.textureVersion)
                    {
                        case 0:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss000); break;
                        case 1:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss001); break;
                        case 2:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss010); break;
                        case 3:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss011); break;
                        case 4:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss100); break;
                        case 5:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss101); break;
                        case 6:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss110); break;
                        case 7:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss111); break;
                    }
                    break;
                case Weapon.WeaponTypes.csharp:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Csharp); break;
            }
            if (projectileFrame == null)
                return false;

            Rectangle MapCollisionRectangle;
            Rectangle ProjectileCollision;

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
                        ProjectileCollision = new Rectangle((int)(this.ProjectilePosition.X - projectileFrame.Size.X / 2), (int)(ProjectilePosition.Y - projectileFrame.Size.Y / 2), (int)projectileFrame.Size.X, (int)projectileFrame.Size.Y);

                        // Überprüfen ob sich die beiden Rechtecke überschneiden
                        if (ProjectileCollision.Intersects(MapCollisionRectangle))
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
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Cpp), this.ProjectilePosition);
                    break;
                case Weapon.WeaponTypes.c:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.C), this.ProjectilePosition);
                    break;
                case Weapon.WeaponTypes.java:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Java), this.ProjectilePosition);
                    break;
                case Weapon.WeaponTypes.maschinensprache:
                    switch (this.textureVersion)
                    {
                        case 0:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss000), this.ProjectilePosition); break;
                        case 1:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss001), this.ProjectilePosition); break;
                        case 2:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss010), this.ProjectilePosition); break;
                        case 3:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss011), this.ProjectilePosition); break;
                        case 4:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss100), this.ProjectilePosition); break;
                        case 5:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss101), this.ProjectilePosition); break;
                        case 6:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss110), this.ProjectilePosition); break;
                        case 7:
                            spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss111), this.ProjectilePosition); break;
                    }
                    break;
                case Weapon.WeaponTypes.csharp:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Csharp), this.ProjectilePosition);
                    break;
            }
        }

        /// <summary>
        /// Funktion für das Überprüfen auf dreffern bei Androids
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public bool CheckForHitAndroid(Android enemy)
        {
            SpriteFrame enemySpriteFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android1);

            SpriteFrame projectileFrame = null;
            switch (ProjectileType)
            {
                case Weapon.WeaponTypes.cpp:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Cpp); break;
                case Weapon.WeaponTypes.c:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.C); break;
                case Weapon.WeaponTypes.java:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Java); break;
                case Weapon.WeaponTypes.maschinensprache:
                    switch (this.textureVersion)
                    {
                        case 0:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss000); break;
                        case 1:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss001); break;
                        case 2:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss010); break;
                        case 3:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss011); break;
                        case 4:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss100); break;
                        case 5:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss101); break;
                        case 6:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss110); break;
                        case 7:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss111); break;
                    }
                    break;
                case Weapon.WeaponTypes.csharp:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Csharp); break;
            }

            if (projectileFrame == null)
                return false;

            if (
                ((ProjectilePosition.X + projectileFrame.Size.X / 2 >= enemy.Position.X - enemySpriteFrame.Size.X / 2 && ProjectilePosition.X - projectileFrame.Size.X / 2 <= enemy.Position.X + enemySpriteFrame.Size.X / 2)
                && (ProjectilePosition.Y + projectileFrame.Size.Y / 2 >= enemy.Position.Y - enemySpriteFrame.Size.Y / 2 && ProjectilePosition.Y - projectileFrame.Size.Y / 2 <= enemy.Position.Y + enemySpriteFrame.Size.Y / 2))
                )
            {
                // Projektil weg schalten
                this.IsActive = false;

                return true;
            }

            return false;
            
        }


        /// <summary>
        /// Funktion für das Überprüfen auf dreffern bei Windows
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public bool CheckForHitWindows(Windows enemy)
        {
            SpriteFrame enemySpriteFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Android1);

            SpriteFrame projectileFrame = null;
            switch (ProjectileType)
            {
                case Weapon.WeaponTypes.cpp:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Cpp); break;
                case Weapon.WeaponTypes.c:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.C); break;
                case Weapon.WeaponTypes.java:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Java); break;
                case Weapon.WeaponTypes.maschinensprache:
                    switch (this.textureVersion)
                    {
                        case 0:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss000); break;
                        case 1:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss001); break;
                        case 2:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss010); break;
                        case 3:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss011); break;
                        case 4:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss100); break;
                        case 5:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss101); break;
                        case 6:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss110); break;
                        case 7:
                            projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Mss111); break;
                    }
                    break;
                case Weapon.WeaponTypes.csharp:
                    projectileFrame = game.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Csharp); break;
            }

            if (projectileFrame == null)
                return false;

            if (
                ((ProjectilePosition.X + projectileFrame.Size.X / 2 >= enemy.Position.X - enemySpriteFrame.Size.X / 2 && ProjectilePosition.X - projectileFrame.Size.X / 2 <= enemy.Position.X + enemySpriteFrame.Size.X / 2)
                && (ProjectilePosition.Y + projectileFrame.Size.Y / 2 >= enemy.Position.Y - enemySpriteFrame.Size.Y / 2 && ProjectilePosition.Y - projectileFrame.Size.Y / 2 <= enemy.Position.Y + enemySpriteFrame.Size.Y / 2))
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
