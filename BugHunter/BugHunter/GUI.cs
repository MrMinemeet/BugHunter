using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            spriteBatch.DrawString(font, player.Health.ToString(),HeartStatusTextPosition, Color.White);
        }
    }
}
