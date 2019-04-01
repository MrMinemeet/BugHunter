using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BugHunter
{
    class SettingsMenu
    {
        enum Einstellungen : byte { MusikLautstaerke, SoundLautstaerke, Statistiken, AnonymeStatistiken}
        Einstellungen aktEinstellung = Einstellungen.MusikLautstaerke;

        Game1 game;

        public SettingsMenu(Game1 game)
        {
            this.game = game;
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                game.CurrentGameState = Game1.GameState.Hauptmenu;

            // Hoch im Menü
            if((Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up)) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 500)
            {
                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;

                switch (aktEinstellung)
                {
                    case Einstellungen.MusikLautstaerke:
                        aktEinstellung = Einstellungen.AnonymeStatistiken;
                        break;
                        
                    case Einstellungen.SoundLautstaerke:
                        aktEinstellung = Einstellungen.MusikLautstaerke;
                        break;
                        
                    case Einstellungen.Statistiken:
                        aktEinstellung = Einstellungen.SoundLautstaerke;
                        break;

                    case Einstellungen.AnonymeStatistiken:
                        aktEinstellung = Einstellungen.Statistiken;
                        break;
                }
            }
            // Runter im Menü
            if ((Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down)) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 500)
            {
                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;

                switch (aktEinstellung)
                {
                    case Einstellungen.MusikLautstaerke:
                        aktEinstellung = Einstellungen.SoundLautstaerke;
                        break;

                    case Einstellungen.SoundLautstaerke:
                        aktEinstellung = Einstellungen.Statistiken;
                        break;

                    case Einstellungen.Statistiken:
                        aktEinstellung = Einstellungen.AnonymeStatistiken;
                        break;

                    case Einstellungen.AnonymeStatistiken:
                        aktEinstellung = Einstellungen.MusikLautstaerke;
                        break;
                }
            }            

            // Senden von Anonymen Statistiken umschalten
            if(Keyboard.GetState().IsKeyDown(Keys.Enter) && aktEinstellung.Equals(Einstellungen.AnonymeStatistiken) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 250)
            {
                game.settings.SendAnonymStatistics = !game.settings.SendAnonymStatistics;

                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

            // Senden von Stats
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && aktEinstellung.Equals(Einstellungen.Statistiken) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 250)
            {
                game.settings.IsSendStatsAllowed = !game.settings.IsSendStatsAllowed;

                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

            // Musiklautstärke erhöhen
            if ((Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right)) && aktEinstellung.Equals(Einstellungen.MusikLautstaerke) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 250)
            {
                if (game.settings.Musiklautstaerke < 100)
                    game.settings.Musiklautstaerke++;

                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

            // Musiklautstärke reduzieren
            if ((Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left)) && aktEinstellung.Equals(Einstellungen.MusikLautstaerke) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 250)
            {
                if (game.settings.Musiklautstaerke > 0)
                    game.settings.Musiklautstaerke--;

                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

            // Soundlautstärke erhöhen
            if ((Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right)) && aktEinstellung.Equals(Einstellungen.SoundLautstaerke) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 250)
            {
                if (game.settings.Soundlautstaerke < 100)
                    game.settings.Soundlautstaerke++;

                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }
            // Soundlautstärke reduzieren
            if ((Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left)) && aktEinstellung.Equals(Einstellungen.SoundLautstaerke) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 250)
            {
                if (game.settings.Soundlautstaerke > 0)
                    game.settings.Soundlautstaerke--;

                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Zeichnet alle Menüpunkte
            spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Musik, new Vector2(100, 200), Color.White);
            spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Sounds, new Vector2(100, 300), Color.White);
            spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Statistiken, new Vector2(100, 400), Color.White);
            spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Anonyme_Statistiken, new Vector2(100, 500), Color.White);

            spriteBatch.DrawString(game.MenuFont, game.settings.Musiklautstaerke.ToString(), new Vector2(1200, 200), Color.White);
            spriteBatch.DrawString(game.MenuFont, game.settings.Soundlautstaerke.ToString(), new Vector2(1200, 300), Color.White);

            if (!game.settings.IsSendStatsAllowed)
                spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_Off, new Vector2(1200, 400), Color.White);
            if (game.settings.IsSendStatsAllowed)
                spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_On, new Vector2(1200, 400), Color.White);

            if (!game.settings.SendAnonymStatistics)
                spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_Off, new Vector2(1200, 500), Color.White);
            if (game.settings.SendAnonymStatistics)
                spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_On, new Vector2(1200, 500), Color.White);


            // Überzeichnet den Menüpunkt der ausgewählt ist mit anderer Farbe
            switch (aktEinstellung)
            {
                case Einstellungen.MusikLautstaerke:
                    spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Musik, new Vector2(100, 200), Color.YellowGreen);
                    spriteBatch.DrawString(game.MenuFont, game.settings.Musiklautstaerke.ToString(), new Vector2(1200, 200), Color.YellowGreen);
                    break;
                case Einstellungen.SoundLautstaerke:
                    spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Sounds, new Vector2(100, 300), Color.YellowGreen);
                    spriteBatch.DrawString(game.MenuFont, game.settings.Soundlautstaerke.ToString(), new Vector2(1200, 300), Color.YellowGreen);
                    break;
                case Einstellungen.Statistiken:
                    spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Statistiken, new Vector2(100, 400), Color.YellowGreen);
                    if (!game.settings.IsSendStatsAllowed)
                        spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_Off, new Vector2(1200, 400), Color.YellowGreen);
                    if (game.settings.IsSendStatsAllowed)
                        spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_On, new Vector2(1200, 400), Color.YellowGreen);
                    break;
                case Einstellungen.AnonymeStatistiken:
                    spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Anonyme_Statistiken, new Vector2(100, 500), Color.YellowGreen);
                    if (!game.settings.SendAnonymStatistics)
                        spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_Off, new Vector2(1200, 500), Color.YellowGreen);
                    if (game.settings.SendAnonymStatistics)
                        spriteBatch.DrawString(game.MenuFont, Texttable_DE.General_On, new Vector2(1200, 500), Color.YellowGreen);
                    break;
            }

            // Info für Namensänderung
            spriteBatch.DrawString(game.font, Texttable_DE.Settings_Change_Name, new Vector2(100, 900), Color.GreenYellow);
        }
    }
}
