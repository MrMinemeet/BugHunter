using BugHunter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexturePackerLoader;

namespace ProjectWhitespace
{
    public class Powerup
    {
        public SpriteSheet spriteSheet;
        public SpriteRender spriteRender;
        public Vector2 position;
        private Settings settings;

        public enum PowerupTypes { Medipack, Ammoboost, DamageUp, ShootSpeedUp }

        PowerupTypes PowerupTipe;


        public Powerup(SpriteSheet spriteSheet, SpriteRender spriteRender, Settings settings, int[][] MapArray)
        {
            this.spriteSheet = spriteSheet;
            this.spriteRender = spriteRender;
            this.settings = settings;
            this.position = this.SetSpawnFromMap(MapArray, this.settings);

            // Weist einen zufälligen Powerup Typ hinzu
            Random random = new Random();

            switch (random.Next(System.Enum.GetNames(typeof(PowerupTypes)).Length))
            {
                case 0:
                    this.PowerupTipe = PowerupTypes.Medipack;
                    break;
                case 1:
                    this.PowerupTipe = PowerupTypes.Ammoboost;
                    break;
                case 2:
                    this.PowerupTipe = PowerupTypes.DamageUp;
                    break;
                case 3:
                    this.PowerupTipe = PowerupTypes.ShootSpeedUp;
                    break;
            }

        }

        private Vector2 SetSpawnFromMap(int[][] MapArray, Settings settings)
        {

            Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            int x, y;
            while (true)
            {
                for (y = random.Next(settings.MapSizeHeight); y < MapArray.Length; y++)
                {
                    for (x = random.Next(settings.MapSizeWidth); x < MapArray[y].Length; x++)
                    {
                        if (MapArray[y][x].Equals(Settings.PowerupTileId))
                        {

                            return new Vector2(x * Settings.TilePixelSize, y * Settings.TilePixelSize);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Setzt Powerup auf neue Position
        /// </summary>
        /// <param name="MapArray"></param>
        /// <returns></returns>
        public void ResetPosition(int[][] MapArray)
        {
            this.position = SetSpawnFromMap(MapArray, this.settings);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteRender.Draw(
                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_kaffee),
                this.position,
                Color.White);
        }
    }
}
