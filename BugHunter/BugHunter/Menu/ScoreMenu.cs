using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace BugHunter
{
    class ScoreMenu
    {
        public static void ShowScoreMenu(SpriteBatch spriteBatch, Game1 game)
        {
            List<string> StatsText = new List<string>();
            List<string> GlobalStatsText = new List<string>();

            TimeSpan timeSpan = TimeSpan.FromMilliseconds(game.gameStats.PlayTime);

            // Textliste für Stats generieren
            StatsText.Add(Texttable_DE.Stats_Spielzeit + timeSpan.ToString("dd\\.hh\\:mm"));
            StatsText.Add(Texttable_DE.Stats_Highscore + game.gameStats.HighScore);
            StatsText.Add(Texttable_DE.Stats_Getötete_Gegner + game.gameStats.KilledEnemies);
            StatsText.Add(Texttable_DE.Stats_Gesammelte_Powerups + game.gameStats.CollectedPowerups);
            StatsText.Add(Texttable_DE.Stats_Anzahl_Geschossen + game.gameStats.AnzahlSchuesse);
            StatsText.Add(Texttable_DE.Stats_Anzahl_Treffer + game.gameStats.AnzahlTreffer);
            StatsText.Add(Texttable_DE.Stats_Trefferrate + ((float)game.gameStats.AnzahlTreffer / (float)game.gameStats.AnzahlSchuesse).ToString("P"));
            StatsText.Add(Texttable_DE.Stats_Tode + game.gameStats.AnzahlTode);

            // Textliste für GlobalStats generieren
            GlobalStatsText.Add(Texttable_DE.Stats_Global_Player_Amount + game.gameStats.GlobalPlayerAmount);
            GlobalStatsText.Add(Texttable_DE.Stats_Getötete_Gegner + game.gameStats.GlobalKilledEnemies);
            GlobalStatsText.Add(Texttable_DE.Stats_Gesammelte_Powerups + game.gameStats.GlobalCollectedPowerups);
            GlobalStatsText.Add(Texttable_DE.Stats_Anzahl_Geschossen + game.gameStats.GlobalAnzahlSchuesse);
            GlobalStatsText.Add(Texttable_DE.Stats_Anzahl_Treffer + game.gameStats.GlobalAnzahlTreffer);
            GlobalStatsText.Add(Texttable_DE.Stats_Trefferrate + ((float)game.gameStats.GlobalAnzahlTreffer / (float)game.gameStats.GlobalAnzahlSchuesse).ToString("P"));
            GlobalStatsText.Add(Texttable_DE.Stats_Tode + game.gameStats.GlobalAnzahlTode);

            // Playerstats
            spriteBatch.DrawString(game.MenuFont, game.settings.UserName, new Vector2(game.player.camera.Origin.X - 900, game.player.camera.Origin.Y - 500), Color.White);

            for (int i = 0; i < StatsText.Count; i++)
            {
                spriteBatch.DrawString(game.font, StatsText[i], new Vector2(game.player.camera.Origin.X - 900, game.player.camera.Origin.Y - 400 + (i * 50)), Color.White);
            }


            
            // Global Ranking Liste
            spriteBatch.DrawString(game.MenuFont, "Top 10 Spieler", new Vector2(game.player.camera.Origin.X - 300, game.player.camera.Origin.Y - 500), Color.White);

            if (game.settings.HasInternetConnection)
            {
                if (game.gameStats.Top10Names.Count == 0)
                {
                    spriteBatch.DrawString(game.font, Texttable_DE.General_Not_Avilable, new Vector2(game.player.camera.Origin.X - 300, game.player.camera.Origin.Y - 400), Color.MonoGameOrange);
                }
                for (int i = 0; i < game.gameStats.Top10Names.Count; i++)
                {
                    spriteBatch.DrawString(game.font, game.gameStats.Top10Names[i] + ":  " + game.gameStats.Top10Score[i], new Vector2(game.player.camera.Origin.X - 300, game.player.camera.Origin.Y + (50 * i) - 400), Color.White);
                }
            }
            else
            {
                spriteBatch.DrawString(game.font, Texttable_DE.General_No_Internet_Connection, new Vector2(game.player.camera.Origin.X - 200, game.player.camera.Origin.Y - 400), Color.OrangeRed);
            }



            // Global Score
            spriteBatch.DrawString(game.MenuFont, Texttable_DE.Stats_Global_Stats, new Vector2(game.player.camera.Origin.X + 350, game.player.camera.Origin.Y - 500), Color.White);


            if (game.settings.HasInternetConnection)
            {
                if (game.gameStats.GlobalPlayerAmount == 0)
                {
                    spriteBatch.DrawString(game.font, Texttable_DE.General_Not_Avilable, new Vector2(game.player.camera.Origin.X + 350, game.player.camera.Origin.Y - 400), Color.MonoGameOrange);
                }
                else {
                    for (int i = 0; i < GlobalStatsText.Count; i++)
                    {
                        spriteBatch.DrawString(game.font, GlobalStatsText[i], new Vector2(game.player.camera.Origin.X + 350, game.player.camera.Origin.Y - 400 + (i * 50)), Color.White);
                    }
                }
            }
            else
            {
                spriteBatch.DrawString(game.font, Texttable_DE.General_No_Internet_Connection, new Vector2(game.player.camera.Origin.X + 400, game.player.camera.Origin.Y - 400), Color.OrangeRed);

            }
        }
    }
}
