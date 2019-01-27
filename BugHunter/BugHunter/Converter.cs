using MonoGame.Extended.Tiled;

namespace BugHunter
{
    class Converter
    {
        public static int[][] MapToIntArray(TiledMap map, Settings _settings)
        {
            TiledMapTileLayer tml = map.GetLayer<TiledMapTileLayer>("Collision/Trigger");
            TiledMapTile? tmt;

            Settings settings = _settings;
            int[][] MapArray = new int[settings.MapSizeHeight][];

            // TiledMapLayer wie 2D Array Durchlaufen
            for (int y = 0; y < settings.MapSizeHeight; y++)
            {
                MapArray[y] = new int[settings.MapSizeWidth];

                for(int x = 0; x < MapArray[y].Length; x++)
                {
                    // Inhalt von TiledMapLayer Tile auf MapArray übertragen
                    tmt = tml.GetTile((ushort)x, (ushort)y);
                    if(tmt != null)
                    {
                        MapArray[y][x] = tmt.Value.GlobalIdentifier;

                        // Console.Write(MapArray[y][x] + "\t");
                    }
                }
                // Console.WriteLine();
            }
            
            return MapArray;
        }
    }
}
