using Microsoft.Xna.Framework;
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
        public const int HitBoxTileNumber = 18;
        public const int PlayerSpawnTileId = 21;
        public const int EnemeySpawnTileId = 19;

        public Texture2D EmptyTexture { get; set; }



    }
}