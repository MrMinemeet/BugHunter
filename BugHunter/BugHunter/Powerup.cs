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
        private Game1 game;

        public enum PowerupTypes { Medipack, Ammoboost, DamageUp, ShootSpeedUp }

        PowerupTypes PowerupType = PowerupTypes.ShootSpeedUp;
        

        public Powerup(Game1 game,SpriteSheet spriteSheet, SpriteRender spriteRender, Settings settings, int[][] MapArray)
        {
            this.game = game;
            this.spriteSheet = spriteSheet;
            this.spriteRender = spriteRender;
            this.settings = settings;
            this.position = this.SetSpawnFromMap(MapArray, this.settings);

            // Weist einen zufälligen Powerup Typ hinzu
            Random random = new Random();

            switch (random.Next(System.Enum.GetNames(typeof(PowerupTypes)).Length))
            {
                case 0:
                    this.PowerupType = PowerupTypes.Medipack;
                    break;
                case 1:
                    this.PowerupType = PowerupTypes.Ammoboost;
                    break;
                case 2:
                    this.PowerupType = PowerupTypes.DamageUp;
                    break;
                case 3:
                    this.PowerupType = PowerupTypes.ShootSpeedUp;
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

        public void Update(GameTime gameTime, Player player)
        {
        }

        public bool WasCollected(Player player)
        {
            Rectangle PlayerCollision;
            Rectangle PowerupCollision;
            SpriteFrame sp = null;

            // TODO: Player Powerup collision
            // Bekommt einen Frame


            sp = spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_kaffee);


            switch (PowerupType)
            {
                case PowerupTypes.ShootSpeedUp:
                    sp = spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_kaffee);
                    break;
            }

            // Rechtecke über Spieler und Powerup ziehen
            PlayerCollision = new Rectangle((int)(player.Position.X - player.Texture.Width / 2), (int)(player.Position.Y - player.Texture.Height / 2), player.Texture.Width, player.Texture.Height);

            PowerupCollision = new Rectangle((int)(this.position.X - sp.Size.X / 2), (int)(this.position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);

            if (PowerupCollision.Intersects(PlayerCollision))
            {

                switch (PowerupType)
                {
                    // Macht alle Sprachen um 10ms schneller
                    case PowerupTypes.ShootSpeedUp:
                        if(this.game.weapon.CDelayMs >= 50) { 
}
                            this.game.weapon.CDelayMs -= 10;


                        if (this.game.weapon.CppDelayMs >= 50)
                            this.game.weapon.CppDelayMs -= 10;

                        if (this.game.weapon.JavaDelayMs >= 50)
                            this.game.weapon.JavaDelayMs -= 10;

                        if (this.game.weapon.MaschinenspracheDelayMs >= 50)
                            this.game.weapon.MaschinenspracheDelayMs -= 10;

                        if (this.game.weapon.CsharpDelayMs >= 50)
                            this.game.weapon.CsharpDelayMs -= 10;


                        Console.WriteLine(this.game.weapon.CsharpDelayMs);

                        break;
                }

                return true;
            }

            return false;
        }
    }
    
}
