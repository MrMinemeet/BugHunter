using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        
        public bool CheckForHit(Android enemy)
        {
            SpriteFrame sp = enemy.spriteSheet.Sprite(TexturePackerMonoGameDefinitions.android_packed.Sprites_android1);
            Texture2D EnemyTexture = sp.Texture;


            if (
                ((ProjectilePosition.X + texture.Width / 2 >= enemy.Position.X - EnemyTexture.Width /4 / 2 && ProjectilePosition.X - texture.Width / 2 <= enemy.Position.X + EnemyTexture.Width /4 / 2)
                && (ProjectilePosition.Y + texture.Height / 2 >= enemy.Position.Y - EnemyTexture.Height / 2 && ProjectilePosition.Y - texture.Height / 2 <= enemy.Position.Y + EnemyTexture.Height / 2))
                )
            {
                this.IsActive = false;
                if(enemy.Health <= 0)
                {
                    enemy.IsActive = false;
                }
                return true;
            }

            return false;
            
        }
    }
}
