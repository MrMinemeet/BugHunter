using BugHunter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public bool DidSpawn { get; }

        public enum PowerupTypes { Medipack, MoreAmmo, DamageUp, ShootSpeedUp, AmmoPack }

        PowerupTypes PowerupType;

        private int type;
        

        public Powerup(Game1 game,SpriteSheet spriteSheet, SpriteRender spriteRender, Settings settings, Vector2 Position)
        {
            this.game = game;
            this.spriteSheet = spriteSheet;
            this.spriteRender = spriteRender;
            this.settings = settings;
            this.position = Position;
            DidSpawn = false;

            while (!DidSpawn)
            {
                switch (game.random.Next(5))
                {
                    case 0:
                        this.PowerupType = PowerupTypes.ShootSpeedUp;
                        DidSpawn = true;
                        break;
                    case 1:
                        this.PowerupType = PowerupTypes.MoreAmmo;
                        DidSpawn = true;
                        break;
                    case 2:
                        if (game.player.Health < game.player.MaxHealth)
                        {
                            this.PowerupType = PowerupTypes.Medipack;
                            DidSpawn = true;

                            this.type = game.random.Next(2);
                        }
                        break;
                    case 3:
                        this.PowerupType = PowerupTypes.DamageUp;
                        this.type = game.random.Next(5);
                        DidSpawn = true;
                        break;
                    case 4:
                        this.PowerupType = PowerupTypes.AmmoPack;
                        DidSpawn = true;
                        break;
                }
            }
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

                case PowerupTypes.DamageUp:
                    switch (type)
                    {
                        case 0:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_Eclipse),
                                this.position,
                                Color.White);
                            break;
                        case 1:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_IntelliJ),
                                this.position,
                                Color.White);
                            break;
                        case 2:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_NetBeans),
                                this.position,
                                Color.White);
                            break;
                        case 3:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_VSCode),
                                this.position,
                                Color.White);
                            break;

                        case 4:
                            spriteRender.Draw(
                                spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_AndroidStudio),
                                this.position,
                                Color.White);
                            break;
                    }
                    break;

                case PowerupTypes.AmmoPack:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.entities.Powerup_Book),
                        this.position,
                        Color.White);
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
            PlayerCollision = new Rectangle((int)(player.Position.X - player.Frame.Size.X / 2), (int)(player.Position.Y - player.Frame.Size.Y / 2), (int)player.Frame.Size.X, (int)player.Frame.Size.Y);

            PowerupCollision = new Rectangle((int)(this.position.X - sp.Size.X / 2), (int)(this.position.Y - sp.Size.Y / 2), (int)sp.Size.X, (int)sp.Size.Y);

            if (PowerupCollision.Intersects(PlayerCollision))
            {

                switch (PowerupType)
                {
                    // Macht alle Sprachen um 10ms schneller
                    case PowerupTypes.ShootSpeedUp:
                        if(this.game.weapon.CDelayMs > 50)
                            this.game.weapon.CDelayMs -= 15;


                        if (this.game.weapon.CppDelayMs > 50)
                            this.game.weapon.CppDelayMs -= 15;

                        if (this.game.weapon.JavaDelayMs > 50)
                            this.game.weapon.JavaDelayMs -= 15;

                        if (this.game.weapon.MaschinenspracheDelayMs > 50)
                            this.game.weapon.MaschinenspracheDelayMs -= 15;

                        if (this.game.weapon.CsharpDelayMs > 50)
                            this.game.weapon.CsharpDelayMs -= 15;

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
                    case PowerupTypes.DamageUp:
                        game.player.Damageboost += 2;
                        break;
                    case PowerupTypes.AmmoPack:
                        if (this.game.weapon.CAmmoAmount - 25 > this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.c])
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.c] += 25;
                        else
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.c] = this.game.weapon.CAmmoAmount;

                        if (this.game.weapon.CppAmmoAmount - 25 > this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.cpp])
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.cpp] += 25;
                        else
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.cpp] = this.game.weapon.CppAmmoAmount;

                        if (this.game.weapon.JavaAmmoAmount - 25 > this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.java])
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.java] += 25;
                        else
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.java] = this.game.weapon.JavaAmmoAmount;

                        if (this.game.weapon.MaschinenspracheAmmoAmount - 25 > this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache])
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] += 25;
                        else
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] = this.game.weapon.MaschinenspracheAmmoAmount;

                        if (this.game.weapon.CsharpAmmoAmount - 25 > this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.csharp])
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.csharp] += 25;
                        else
                            this.game.player.AmmunitionAmmountList[Weapon.WeaponTypes.csharp] = this.game.weapon.CsharpAmmoAmount;

                        break;
                }
                return true;
            }
            return false;
        }
    }
}
