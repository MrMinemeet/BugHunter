using BugHunter;
using Microsoft.Xna.Framework;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace ProjectWhitespace
{
    public class Requests
    {
        Game1 game;

        private Thread latestAvailableVersionThread;

        public double LastAvailibleVersionCheck { get; set; }

        public Requests(Game1 game)
        {
            this.game = game;
        }

        public void GetLatestAvailableVersion(GameTime gameTime)
        {
            LastAvailibleVersionCheck = gameTime.TotalGameTime.TotalSeconds;

            latestAvailableVersionThread = new Thread(LatestAvailableVersionThread);
            latestAvailableVersionThread.Start();
        }

        private void LatestAvailableVersionThread()
        {
            try
            {
                string returnString = new WebClient().DownloadString("https://www.projectwhitespace.net/latestVersion.php");
                game.settings.AvailableVersion = Regex.Replace(returnString, @"\t|\n|\r", "");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
