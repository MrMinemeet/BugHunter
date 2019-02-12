using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWhitespace
{
    public class Stats
    {
        public string HighScore = "0";
        public List<string> Top10Names = new List<string>();
        public List<int> Top10Score = new List<int>();
        public int KilledEnemies = 0;
        public int CollectedPowerups = 0;
        public int AnzahlSchuesse = 0;
        public int AnzahlTreffer = 0;
    }
}
