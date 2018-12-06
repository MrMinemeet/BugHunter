using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Player.Weapons ProjectileType;

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
                case Player.Weapons.cpp:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.Cpp), this.ProjectilePosition);
                    break;
                case Player.Weapons.c:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.C), this.ProjectilePosition);
                    break;
                case Player.Weapons.java:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.Cpp), this.ProjectilePosition);
                    break;
                case Player.Weapons.maschinensprache:
                    spriteRender.Draw(spriteSheet.Sprite(TexturePackerMonoGameDefinitions.weapons.Cpp), this.ProjectilePosition);
                    break;
            }
        }
    }
}
