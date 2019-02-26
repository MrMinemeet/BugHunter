using Microsoft.Xna.Framework.Graphics;
using BugHunter;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ProjectWhitespace.Menu
{
    class ScoreMenu
    {
        public static void ShowScoreMenu(SpriteBatch spriteBatch, Game1 game)
        {
            List<string> StatsText = new List<string>();

            StatsText.Add(Texttable.Stats_Getötete_Gegner + game.gameStats.KilledEnemies);
            StatsText.Add(Texttable.Stats_Gesammelte_Powerups + game.gameStats.CollectedPowerups);
            StatsText.Add(Texttable.Stats_Anzahl_Geschossen + game.gameStats.AnzahlSchuesse);
            StatsText.Add(Texttable.Stats_Anzahl_Treffer + game.gameStats.AnzahlTreffer);
            StatsText.Add(Texttable.Stats_Trefferrate + ((float)game.gameStats.AnzahlTreffer / (float)game.gameStats.AnzahlSchuesse).ToString("P"));
            StatsText.Add(Texttable.Stats_Tode + game.gameStats.AnzahlTode);

            // Playerstats
            spriteBatch.DrawString(game.MenuFont, game.settings.UserName, new Vector2(game.player.camera.Origin.X - 900, game.player.camera.Origin.Y - 500), Color.White);

            for (int i = 0; i < StatsText.Count; i++)
            {
                spriteBatch.DrawString(game.font, StatsText[i], new Vector2(game.player.camera.Origin.X - 900, game.player.camera.Origin.Y - 400 + (i * 50)), Color.White);
            }
            
            // Global Ranking Liste
            spriteBatch.DrawString(game.MenuFont, "Top 10 Spieler", new Vector2(game.player.camera.Origin.X - 300, game.player.camera.Origin.Y - 500), Color.White);

            for (int i = 0; i < game.gameStats.Top10Names.Count; i++)
            {
                spriteBatch.DrawString(game.font, game.gameStats.Top10Names[i] + ":  " + game.gameStats.Top10Score[i], new Vector2(game.player.camera.Origin.X - 300, game.player.camera.Origin.Y + (50 * i) - 400), Color.White);
            }
        }
    }
}
