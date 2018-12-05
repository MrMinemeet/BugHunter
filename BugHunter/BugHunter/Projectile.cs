using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        private void UpdateShot(GameTime gameTime, Player player)
        {
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
    }
}
