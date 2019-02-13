using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace BugHunter
{
    public class Map
    {
        // The tile map
        public TiledMap maplevel;
        // The renderer for the map
        public TiledMapRenderer mapRenderer;

        public void SetTiledMap(TiledMap value)
        {
            this.maplevel = value;
        }
        public TiledMap GetTiledMap()
        {
            return this.maplevel;
        }
    }
}