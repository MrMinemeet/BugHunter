using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugHunter
{
    class SettingsMenu
    {
        enum Einstellungen : byte { MusikLautstaerke, SoundLautstaerke, Statistiken, AnonymeStatistiken}
        Einstellungen aktEinstellung = Einstellungen.MusikLautstaerke;

        double LastKeyStrokeInput = 0;

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
            if((Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up)) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
            {
                LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;

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
            if ((Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down)) && gameTime.TotalGameTime.TotalMilliseconds - LastKeyStrokeInput >= 500)
            {
                LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;

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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Zeichnet alle Menüpunkte
            spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Musik, new Vector2(100, 200), Color.White);
            spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Sounds, new Vector2(100, 300), Color.White);
            spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Statistiken, new Vector2(100, 400), Color.White);
            spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Anonyme_Statistiken, new Vector2(100, 500), Color.White);
            
            // Überzeichnet den Menüpunkt der ausgewählt ist.
            switch (aktEinstellung)
            {
                case Einstellungen.MusikLautstaerke:
                    spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Musik, new Vector2(100, 200), Color.YellowGreen);
                    break;
                case Einstellungen.SoundLautstaerke:
                    spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Sounds, new Vector2(100, 300), Color.YellowGreen);
                    break;
                case Einstellungen.Statistiken:
                    spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Statistiken, new Vector2(100, 400), Color.YellowGreen);
                    break;
                case Einstellungen.AnonymeStatistiken:
                    spriteBatch.DrawString(game.MenuFont, Texttable.Settings_Anonyme_Statistiken, new Vector2(100, 500), Color.YellowGreen);
                    break;
            }
        }
    }
}
