﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BugHunter
{
    class Settings
    {
        // Öffentliche Variablen welche geändet werden können
        public int resolutionWidth { get; set; }
        public int resolutionHeight;
        public bool IsFullscreen;
        public bool IsMouseVisible;
        public bool AreDebugInformationsVisible = false;

        /// <summary>
        /// Erstellt ein Object mit den Einstellungen
        /// </summary>
        /// <param name="resolutionWidth">Auflösung - Breite</param>
        /// <param name="resolutionHeight">Auflösung - Höhe</param>
        /// <param name="IsFullscreen"> Einstellung für Vollbild</param>
        /// <param name="IsMouseVisible">Einstellung ob Maus sichtbar ist</param>
        public Settings(int resolutionWidth, int resolutionHeight, bool IsFullscreen, bool IsMouseVisible)
        {
            this.resolutionWidth = resolutionWidth;
            this.resolutionHeight = resolutionHeight;
            this.IsFullscreen = IsFullscreen;
            this.IsMouseVisible = IsMouseVisible;
        }

        public void UpdateSettings(GraphicsDeviceManager gdm)
        {
            gdm.IsFullScreen = IsFullscreen;
            gdm.ApplyChanges();
        }
        

        // Programm Interne Einstellungen / Constanten
        public int MapSizeHeight { get; set; }
        public int MapSizeWidth { get; set; }
        public const int TilePixelSize = 64;
        public const byte HitBoxTileNumber = 18;
        public const byte PlayerSpawnTileId = 21;
        public const byte EnemeySpawnTileId = 19;
        public const byte ReloadTileId = 20;

        public Texture2D EmptyTexture { get; set; }
    }
}