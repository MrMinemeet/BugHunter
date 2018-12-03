using Microsoft.Xna.Framework;

namespace BugHunter
{
    class Settings
    {

        // Öffentliche Variablen welche geändet werden können
        private int resolutionWidth;
        private int resolutionHeight;
        private bool IsFullscreen;
        private bool IsMouseVisible;
        private bool AreDebugInformationsVisible = false;

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

        public void setResolutionWidth(int value)
        {
            this.resolutionWidth = value;
        }
        public int getResolutionWidth()
        {
            return this.resolutionWidth;
        }

        public void setResolutionHeight(int value)
        {
            this.resolutionHeight = value;
        }
        public int getResolutionHeight()
        {
            return this.resolutionHeight;
        }

        public void setIsFullScreen(bool value)
        {
            this.IsFullscreen = value;
        }
        public bool getIsFullScreen()
        {
            return this.IsFullscreen;
        }

        public void setIsMouseVisible(bool value)
        {
            this.IsMouseVisible = value;
        }
        public bool getIsMouseVisible()
        {
            return this.IsMouseVisible;
        }

        public void setAreDebugInformationsVisible(bool value)
        {
            this.AreDebugInformationsVisible = value;
        }
        public bool getAreDebugInformationsVisible()
        {
            return this.AreDebugInformationsVisible;
        }


        // Programm Interne Einstellungen / Constanten
        private int MapSizeHeight;
        private int MapSizeWidth;
        public const int TilePixelSize = 64;
        public const int HitBoxTileNumber = 18;
        public const int PlayerSpawnTileId = 21;
        public const int EnemeySpawnTileId = 19;


        // Getter / Setter
        public void setMapSizeHeight(int value)
        {
            this.MapSizeHeight = value;
        }
        public int getMapSizeHeight()
        {
            return this.MapSizeHeight;
        }

        public void setMapSizeWidth(int value)
        {
            this.MapSizeWidth = value;
        }
        public int getMapSizeWidth()
        {
            return this.MapSizeWidth;
        }
    }
}