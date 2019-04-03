using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace BugHunter
{
    class SettingsMenu
    {
        enum Einstellungen : byte { MusikLautstaerke, SoundLautstaerke, Statistiken, AnonymeStatistiken, Nutzername}
        Einstellungen aktEinstellung = Einstellungen.MusikLautstaerke;

        Game1 game;

        bool WasKeyPressed = false;

        public SettingsMenu(Game1 game)
        {
            this.game = game;
        }

        public void Update(GameTime gameTime)
        {

            //  <== NAVIGATION ==>
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                game.CurrentGameState = Game1.GameState.Hauptmenu;

            // Hoch im Menü
            if(Keyboard.GetState().IsKeyDown(Keys.Up) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 500)
            {
                game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;

                switch (aktEinstellung)
                {
                    case Einstellungen.MusikLautstaerke:
                        aktEinstellung = Einstellungen.Nutzername;
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
                    case Einstellungen.Nutzername:
                        aktEinstellung = Einstellungen.AnonymeStatistiken;
                        break;

                }
            }
            // Runter im Menü
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 500)
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
                        aktEinstellung = Einstellungen.Nutzername;
                        break;
                        
                    case Einstellungen.Nutzername:
                        aktEinstellung = Einstellungen.MusikLautstaerke;
                        break;
                }
            }    
            
            //  <== EINSTELLUNGEN ÄNDERN ==>

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

            if (aktEinstellung.Equals(Einstellungen.Nutzername) && gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 150)
            {
                bool ShiftIsPressed = false;

                StringBuilder sb = new StringBuilder(game.settings.UserName);

                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                    ShiftIsPressed = true;
                else
                    ShiftIsPressed = false;

                // Löschen des letzen Zeichens
                if (Keyboard.GetState().IsKeyDown(Keys.Back))
                {
                    if(sb.Length > 0)
                        sb.Length--;

                    game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                }

                if (sb.Length < 15)
                {
                    // Überprüfen welche Taste gedrückt wurde

                    // Buchstaben
                    if (Keyboard.GetState().IsKeyDown(Keys.A))
                    {
                        if (ShiftIsPressed)
                            sb.Append('A');
                        else
                            sb.Append('a');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.B))
                    {
                        if (ShiftIsPressed)
                            sb.Append('B');
                        else
                            sb.Append('b');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.C))
                    {
                        if (ShiftIsPressed)
                            sb.Append('C');
                        else
                            sb.Append('c');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D))
                    {
                        if (ShiftIsPressed)
                            sb.Append('D');
                        else
                            sb.Append('d');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.E))
                    {
                        if (ShiftIsPressed)
                            sb.Append('E');
                        else
                            sb.Append('e');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.F))
                    {
                        if (ShiftIsPressed)
                            sb.Append('F');
                        else
                            sb.Append('f');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.G))
                    {
                        if (ShiftIsPressed)
                            sb.Append('G');
                        else
                            sb.Append('g');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.H))
                    {
                        if (ShiftIsPressed)
                            sb.Append('H');
                        else
                            sb.Append('h');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.I))
                    {
                        if (ShiftIsPressed)
                            sb.Append('I');
                        else
                            sb.Append('i');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.J))
                    {
                        if (ShiftIsPressed)
                            sb.Append('J');
                        else
                            sb.Append('j');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.K))
                    {
                        if (ShiftIsPressed)
                            sb.Append('K');
                        else
                            sb.Append('k');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.L))
                    {
                        if (ShiftIsPressed)
                            sb.Append('L');
                        else
                            sb.Append('l');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.M))
                    {
                        if (ShiftIsPressed)
                            sb.Append('M');
                        else
                            sb.Append('m');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.N))
                    {
                        if (ShiftIsPressed)
                            sb.Append('N');
                        else
                            sb.Append('n');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.O))
                    {
                        if (ShiftIsPressed)
                            sb.Append('O');
                        else
                            sb.Append('o');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.P))
                    {
                        if (ShiftIsPressed)
                            sb.Append('P');
                        else
                            sb.Append('p');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Q))
                    {
                        if (ShiftIsPressed)
                            sb.Append('Q');
                        else
                            sb.Append('q');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.R))
                    {
                        if (ShiftIsPressed)
                            sb.Append('R');
                        else
                            sb.Append('r');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.S))
                    {
                        if (ShiftIsPressed)
                            sb.Append('S');
                        else
                            sb.Append('s');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.T))
                    {
                        if (ShiftIsPressed)
                            sb.Append('T');
                        else
                            sb.Append('t');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.U))
                    {
                        if (ShiftIsPressed)
                            sb.Append('U');
                        else
                            sb.Append('u');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.V))
                    {
                        if (ShiftIsPressed)
                            sb.Append('V');
                        else
                            sb.Append('v');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.W))
                    {
                        if (ShiftIsPressed)
                            sb.Append('W');
                        else
                            sb.Append('w');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.X))
                    {
                        if (ShiftIsPressed)
                            sb.Append('X');
                        else
                            sb.Append('x');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Y))
                    {
                        if (ShiftIsPressed)
                            sb.Append('Y');
                        else
                            sb.Append('y');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.Z))
                    {
                        if (ShiftIsPressed)
                            sb.Append('Z');
                        else
                            sb.Append('z');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }

                    // Zahlen + Ein paar Sonderzeichen
                    if (Keyboard.GetState().IsKeyDown(Keys.D1))
                    {
                        if (!ShiftIsPressed)
                            sb.Append('l');
                        else
                            sb.Append('!');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D2))
                    {
                        sb.Append('2');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D3))
                    {
                        sb.Append('3');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D4))
                    {
                        sb.Append('4');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D5))
                    {
                        sb.Append('5');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D6))
                    {
                        sb.Append('6');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D7))
                    {
                        if (!ShiftIsPressed)
                            sb.Append('7');
                        else
                            sb.Append('/');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D8))
                    {
                        if (!ShiftIsPressed)
                            sb.Append('8');
                        else
                            sb.Append('(');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D9))
                    {
                        if (!ShiftIsPressed)
                            sb.Append('9');
                        else
                            sb.Append(')');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.D0))
                    {
                        if (!ShiftIsPressed)
                            sb.Append('0');
                        else
                            sb.Append('=');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }

                    // Zeichen
                    if(Keyboard.GetState().IsKeyDown(Keys.OemMinus) && !ShiftIsPressed)
                    {
                        sb.Append('-');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && ShiftIsPressed)
                    {
                        sb.Append('_');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }

                    if (Keyboard.GetState().IsKeyDown(Keys.OemComma) && !ShiftIsPressed)
                    {
                        sb.Append(',');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.OemComma) && ShiftIsPressed)
                    {
                        sb.Append(';');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }

                    if (Keyboard.GetState().IsKeyDown(Keys.OemPeriod) && !ShiftIsPressed)
                    {
                        sb.Append('.');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    if (Keyboard.GetState().IsKeyDown(Keys.OemPeriod) && ShiftIsPressed)
                    {
                        sb.Append(':');

                        game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                }
                

                game.settings.UserName = sb.ToString();
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


            spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Username, new Vector2(100, 600), Color.White);
            spriteBatch.DrawString(game.MenuFont, game.settings.UserName, new Vector2(1200, 600), Color.White);


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

                case Einstellungen.Nutzername:
                    spriteBatch.DrawString(game.MenuFont, Texttable_DE.Settings_Username, new Vector2(100, 600),Color.YellowGreen);
                    spriteBatch.DrawString(game.MenuFont, game.settings.UserName, new Vector2(1200, 600), Color.YellowGreen);
                    break;
            }
        }
    }
}
