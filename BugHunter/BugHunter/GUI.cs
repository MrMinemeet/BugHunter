using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TexturePackerLoader;

namespace BugHunter
{
    class GUI
    {
        public Vector2 HeartPosition;
        public Vector2 HeartStatusTextPosition;
        public Texture2D CustomCurserTexture;
        public Texture2D PausedBackground;

        // Textures
        public SpriteSheet spriteSheet;
        public SpriteRender spriteRender;

        public Game1 game;

        private int previousScrollValue = 0;
        private double lastWeaponChangeTime = 0;
        private int AmmunitionAmmount = -1;

        public void Init(Game1 game)
        {
            this.game = game;
        }

        /// <summary>
        /// Update-Funktion für GUI
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="PlayerPosition"></param>
        public void Update(GameTime gameTime, Player player)
        {

            this.HeartPosition.X = player.camera.Position.X + 50;
            this.HeartPosition.Y = player.camera.Position.Y + 1020;


            this.HeartStatusTextPosition.X = player.camera.Position.X + 120;
            this.HeartStatusTextPosition.Y = player.camera.Position.Y + 1010;
            
            if((gameTime.TotalGameTime.TotalMilliseconds - lastWeaponChangeTime) >= 250)
            {
                if (Mouse.GetState().ScrollWheelValue > previousScrollValue)
                {
                    GoWeaponUpByOne(player);
                }
                if (Mouse.GetState().ScrollWheelValue < previousScrollValue)
                {
                    GoWeaponDownByOne(player);
                }


                previousScrollValue = Mouse.GetState().ScrollWheelValue;
                lastWeaponChangeTime = gameTime.TotalGameTime.TotalMilliseconds;
            }

            this.AmmunitionAmmount = player.AmmunitionAmmountList[player.aktWeapon];
        }

        private void GoWeaponUpByOne(Player player)
        {
            switch (player.aktWeapon)
            {
                case Weapon.WeaponTypes.c:
                    player.aktWeapon = Weapon.WeaponTypes.cpp; break;
                case Weapon.WeaponTypes.cpp:
                    player.aktWeapon = Weapon.WeaponTypes.java; break;
                case Weapon.WeaponTypes.java:
                    player.aktWeapon = Weapon.WeaponTypes.maschinensprache; break;
                case Weapon.WeaponTypes.maschinensprache:
                    player.aktWeapon = Weapon.WeaponTypes.csharp; break;
                case Weapon.WeaponTypes.csharp:
                    player.aktWeapon = Weapon.WeaponTypes.c; break;
            }
        }
        private void GoWeaponDownByOne(Player player)
        {
            switch (player.aktWeapon)
            {
                case Weapon.WeaponTypes.c:
                    player.aktWeapon = Weapon.WeaponTypes.csharp; break;
                case Weapon.WeaponTypes.cpp:
                    player.aktWeapon = Weapon.WeaponTypes.c; break;
                case Weapon.WeaponTypes.java:
                    player.aktWeapon = Weapon.WeaponTypes.cpp; break;
                case Weapon.WeaponTypes.maschinensprache:
                    player.aktWeapon = Weapon.WeaponTypes.java; break;
                case Weapon.WeaponTypes.csharp:
                    player.aktWeapon = Weapon.WeaponTypes.maschinensprache; break;
            }
        }

        /// <summary>
        /// Draw-Funktion für Gui
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="font"></param>
        /// <param name="player"></param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Player player)
        {
            this.spriteRender = new SpriteRender(spriteBatch);

            // Aktuelle Waffe in der rechten unteren Ecke zeichnen
            switch (player.aktWeapon)
            {
                case Weapon.WeaponTypes.cpp:

                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.Cpp),
                        new Vector2(player.camera.Position.X + 1800, player.camera.Position.Y + 1010),
                        Color.White);
                    break;
                case Weapon.WeaponTypes.c:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.C),
                        new Vector2(player.camera.Position.X + 1800, player.camera.Position.Y + 1010),
                        Color.White);
                    break;
                case Weapon.WeaponTypes.java:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.Java),
                        new Vector2(player.camera.Position.X + 1800, player.camera.Position.Y + 1010),
                        Color.White);
                    break;
                case Weapon.WeaponTypes.maschinensprache:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.Maschinensprache),
                        new Vector2(player.camera.Position.X + 1800, player.camera.Position.Y + 1010),
                        Color.White);
                    break;
                case Weapon.WeaponTypes.csharp:
                    spriteRender.Draw(
                        spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.Csharp),
                        new Vector2(player.camera.Position.X + 1800, player.camera.Position.Y + 1010),
                        Color.White);
                    break;

            }

            if (player.Health * 0.5f > player.MaxHealth)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.Heart_full),
                    this.HeartPosition,
                    Color.White);
            }
            if (player.Health * 0.5f <= player.MaxHealth)
            {
                spriteRender.Draw(
                    spriteSheet.Sprite(TexturePackerMonoGameDefinitions.gui.Heart_half),
                    this.HeartPosition,
                    Color.White);
            }

            spriteBatch.DrawString(font, game.Score.ToString(), new Vector2(player.Position.X, player.camera.Position.Y), Color.White);

            spriteBatch.DrawString(font, player.Health.ToString(),HeartStatusTextPosition, Color.White);

            spriteBatch.DrawString(font,
                this.AmmunitionAmmount.ToString() + " / " + Weapon.getMaxAmmoAmountAktWeapon(player.aktWeapon),
                new Vector2(player.camera.Position.X + 1750, player.camera.Position.Y + 930),
                Color.White);

            if (player.IsReloading)
            {
                spriteBatch.DrawString(game.MenuFont, "NACHLADEN", new Vector2(player.Position.X - 200, player.Position.Y - 30), Color.WhiteSmoke);
            }
        }
    }
}
