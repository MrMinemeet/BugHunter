using BugHunter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectWhitespace
{
    class Konsole
    {
        private Color ConsoleColor { get; set; }
        private SpriteFont ConsoleFont;

        public bool IsActive { get; set; } = false;
        private Game1 game;

        private List<string> CommandList = new List<string>();

        private StringBuilder aktCommand = new StringBuilder("");

        private double LastKeyStroke = 0;
        private bool CommandSubmitted = false;
        private LinkedList<string> KonsoleLog = new LinkedList<string>();


        public Konsole(Game1 game)
        {
            this.game = game;
            this.ConsoleFont = game.DebugFont;

            ConsoleColor = new Color(0, 0, 0, 128);
        }

        public void Update(GameTime gameTime)
        {
            // Konsole (de-)aktivieren wenn F3 und TAB gedrückt werden
            if(Keyboard.GetState().IsKeyDown(Keys.F3) && Keyboard.GetState().IsKeyDown(Keys.Tab) && (gameTime.TotalGameTime.TotalMilliseconds - LastKeyStroke >= 250) && game.settings.IsDebugEnabled)
            {
                LastKeyStroke = gameTime.TotalGameTime.TotalMilliseconds;

                IsActive = !IsActive;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && (gameTime.TotalGameTime.TotalMilliseconds - LastKeyStroke >= 250))
            {
                LastKeyStroke = gameTime.TotalGameTime.TotalMilliseconds;

                IsActive = false;
            }

            // Wenn nicht aktiv, Funktion beenden
            if (!this.IsActive)
                return;

            // Tasteninput erhalten und an aktuellen Kommand anhängen
            string KeyInput = GetKeyboardInput(gameTime);
            if (!KeyInput.Equals("none"))    // Bei none ist keine Taste gedrückt worden
            {
                switch(KeyInput)
                {
                    case "rem":
                        if (aktCommand.Length > 0)
                            aktCommand.Length--;
                        break;
                    case "enter":
                        CommandSubmitted = true;
                        break;

                    default:
                        aktCommand.Append(KeyInput);
                        break;
                }
            }

            // Bestätigten Befehl abarbeiten
            if (CommandSubmitted && aktCommand.Length > 0)
            {
                string Befehl = string.Empty;
                string Argument = string.Empty;
                CommandSubmitted = false;

                // Befehl vorbereiten
                try
                {
                    string[] temp = aktCommand.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Befehl = temp[0];
                    Argument = string.Empty;

                    if (temp.Length > 1)
                        Argument = temp[1];
                }
                catch(Exception e)
                {
                    game.logger.Log(e.Message, "Konsole", "Error");
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    aktCommand.Clear();
                }

                if (Befehl.Length > 0)
                {
                    Befehl = Befehl.ToLower();

                    try
                    {
                        // Schauen ob der eingegebene Befehl bekannt ist
                        switch (Befehl)
                        {
                            case "help":
                                KonsoleLog.AddFirst("Godmode <true/false>");
                                KonsoleLog.AddFirst("Refill-All");
                                KonsoleLog.AddFirst("UnlimitedAmmo <true/false>");
                                KonsoleLog.AddFirst("SedDamageBoost <number>");
                                KonsoleLog.AddFirst("KillAll");
                                KonsoleLog.AddFirst("Fastshoot <true/false>");
                                break;

                            case "godmode":
                                bool GodmodeArg = bool.Parse(Argument);
                                game.player.IsGodmode = GodmodeArg;

                                KonsoleLog.AddFirst("Godmode: " + GodmodeArg);
                                break;

                            case "refill-all":
                                Player player = game.player;

                                player.AmmunitionAmmountList[Weapon.WeaponTypes.c] = game.weapon.CAmmoAmount;
                                player.AmmunitionAmmountList[Weapon.WeaponTypes.cpp] = game.weapon.CppAmmoAmount;
                                player.AmmunitionAmmountList[Weapon.WeaponTypes.csharp] = game.weapon.CsharpAmmoAmount;
                                player.AmmunitionAmmountList[Weapon.WeaponTypes.java] = game.weapon.JavaAmmoAmount;
                                player.AmmunitionAmmountList[Weapon.WeaponTypes.maschinensprache] = game.weapon.MaschinenspracheAmmoAmount;

                                KonsoleLog.AddFirst("Munition aufgefuellt");
                                break;

                            case "unlimitedammo":
                                bool UnlimitedAmmoArg = bool.Parse(Argument);
                                game.player.HasUnlimitedAmmo = UnlimitedAmmoArg;

                                KonsoleLog.AddFirst("Unlimited Ammo: " + UnlimitedAmmoArg);
                                break;

                            case "setdamageboost":
                                int boost = int.Parse(Argument);
                                game.player.Damageboost = boost;

                                KonsoleLog.AddFirst("Damageboost auf " + boost + " gesetzt");
                                break;

                            case "killall":
                                game.AndroidsList.Clear();
                                game.iOSList.Clear();
                                game.WindowsList.Clear();

                                KonsoleLog.AddFirst("Alle gegner getoetet");
                                break;

                            case "fastshoot":
                                bool FastshootArg = bool.Parse(Argument);
                                game.player.FastshootEnabled = FastshootArg;

                                KonsoleLog.AddFirst("Fastshoot: " + FastshootArg);
                                break;

                            default:

                                KonsoleLog.AddFirst("Befehl nicht gefunden!");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        game.logger.Log(e.Message, "Konsole", "Error");
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Konsole beenden, falls nicht aktiv.
            if (!IsActive)
            {
                return;
            }

            // <== Konsole AKTIV ==>
            
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;
            int ConsoleHeight = (int)(graphicsDevice.PresentationParameters.BackBufferHeight * 0.33f);
            int ConsoleWidth = graphicsDevice.PresentationParameters.BackBufferWidth;

             // Transparenter Hintergrund für Konsole zeichnen
            Texture2D texture;
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new Color[] { Color.DarkSlateGray });
            spriteBatch.Draw(texture, new Rectangle(0, 0, ConsoleWidth, ConsoleHeight), new Color(0, 0, 0, 128));

            spriteBatch.DrawString(this.ConsoleFont, aktCommand, new Vector2(10, ConsoleHeight - 20), Color.GhostWhite);

            for(int i = KonsoleLog.Count; i > 0; i--)
            {
                if(ConsoleHeight - 40 - (25*i) <= 0)
                {
                    KonsoleLog.RemoveLast();
                }
            }

            int linecount = 0;
            // Konsolenlog ausgeben
            foreach(string line in KonsoleLog)
            {
                spriteBatch.DrawString(this.ConsoleFont, line, new Vector2(10, ConsoleHeight - 40 - (25 * linecount)), Color.GhostWhite);
                linecount++;
            }
        }

        private string GetKeyboardInput(GameTime gameTime)
        {
            StringBuilder sb = new StringBuilder();

            if (gameTime.TotalGameTime.TotalMilliseconds - game.LastKeyStrokeInput >= 150)
            {
                bool ShiftIsPressed = false;

                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                    ShiftIsPressed = true;
                else
                    ShiftIsPressed = false;

                // Löschen des letzen Zeichens
                if (Keyboard.GetState().IsKeyDown(Keys.Back))
                {
                    game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    return "rem";
                }

                // Bestätigen des Befehls
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    game.LastKeyStrokeInput = gameTime.TotalGameTime.TotalMilliseconds;
                    return "enter";
                }

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
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    sb.Append(' ');
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
                if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && !ShiftIsPressed)
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
                return sb.ToString();
            }
            return "none";

        }
    }
}