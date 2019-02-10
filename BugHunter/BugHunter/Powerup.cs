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

        public enum PowerupTypes { Medipack, MoreAmmo, DamageUp, ShootSpeedUp }

        PowerupTypes PowerupType;

        private int type;
        

        public Powerup(Game1 game,SpriteSheet spriteSheet, SpriteRender spriteRender, Settings settings, Vector2 Position)
        {
            this.game = game;
            this.spriteSheet = spriteSheet;
            this.spriteRender = spriteRender;
            this.settings = settings;
            this.position = Position;

            switch(game.random.Next(3))
            {
                case 0:
                    this.PowerupType = PowerupTypes.ShootSpeedUp;
                    break;
                case 1:
                    this.PowerupType = PowerupTypes.MoreAmmo;
                    break;
                case 2:
                    if(game.player.Health < game.player.MaxHealth)
                    {
                        this.PowerupType = PowerupTypes.Medipack;
                    }
                    break;
                case 3:
                    this.PowerupType = PowerupTypes.DamageUp;
                    break;
            }

            this.type = game.random.Next(2);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (PowerupType)
            {
                case PowerupTypes.ShootSpeedUp:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_kaffee),
                        this.position,
                        Color.White);
                    break;
                case PowerupTypes.MoreAmmo:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_usb),
                        this.position,
                        Color.White);
                    break;
                case PowerupTypes.Medipack:
                    switch (type)
                    {
                        case 0:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_CPU),
                                this.position,
                                Color.White);
                            break;
                        case 1:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_Monitor),
                                this.position,
                                Color.White);
                            break;

                    }
                    break;
            }
        }

        public void Update(GameTime gameTime, Player player)
        {
        }

        public bool WasCollected(Player player)
        {
            Rectangle PlayerCollision;
            Rectangle PowerupCollision;
            SpriteFrame sp = null;
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
                        if(this.game.weapon.CDelayMs > 50)
                            this.game.weapon.CDelayMs -= 25;


                        if (this.game.weapon.CppDelayMs > 50)
                            this.game.weapon.CppDelayMs -= 25;

                        if (this.game.weapon.JavaDelayMs > 50)
                            this.game.weapon.JavaDelayMs -= 25;

                        if (this.game.weapon.MaschinenspracheDelayMs > 50)
                            this.game.weapon.MaschinenspracheDelayMs -= 25;

                        if (this.game.weapon.CsharpDelayMs > 50)
                            this.game.weapon.CsharpDelayMs -= 20;

                        break;

                    // Erhöht max. Munition um 10
                    case PowerupTypes.MoreAmmo:

                        if (this.game.weapon.CAmmoAmount < Weapon.GeneralMaxAmmo)
                            this.game.weapon.CAmmoAmount += 10;
                        if (this.game.weapon.CppAmmoAmount < Weapon.GeneralMaxAmmo)
                            this.game.weapon.CppAmmoAmount += 10;
                        if (this.game.weapon.JavaAmmoAmount < Weapon.GeneralMaxAmmo)
                            this.game.weapon.JavaAmmoAmount += 10;
                        if (this.game.weapon.MaschinenspracheAmmoAmount < Weapon.GeneralMaxAmmo)
                            this.game.weapon.MaschinenspracheAmmoAmount += 10;
                        if (this.game.weapon.CsharpAmmoAmount < Weapon.GeneralMaxAmmo)
                            this.game.weapon.CsharpAmmoAmount += 10;

                        break;
                    case PowerupTypes.Medipack:
                        if(player.Health + 25 > player.MaxHealth)
                        {
                            player.Health = player.MaxHealth;
                        }
                        else
                        {
                            player.Health += 25;
                        }
                        break;
                }
                return true;
            }
            return false;
        }
    }
}
