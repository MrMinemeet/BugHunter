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
        public uint KilledEnemies = 0;
        public uint CollectedPowerups = 0;
        public uint AnzahlSchuesse = 0;
        public uint AnzahlTreffer = 0;
        public uint AnzahlTode = 0;

        public uint KilledEnemiesOld = 0;
        public uint CollectedPowerupsOld = 0;
        public uint AnzahlSchuesseOld = 0;
        public uint AnzahlTrefferOld = 0;
        public uint AnzahlTodeOld = 0;

        // Variablen für Globale Stats Liste
        public uint GlobalPlayerAmount = 0;
        public uint GlobalKilledEnemies = 0;
        public uint GlobalCollectedPowerups = 0;
        public UInt64 GlobalAnzahlSchuesse = 0;
        public UInt64 GlobalAnzahlTreffer = 0;
        public uint GlobalAnzahlTode = 0;
    }
}
