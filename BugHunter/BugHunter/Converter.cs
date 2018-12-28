using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.IO;

/*
 * Dear Programmer,
 * 
 * When I wrote this code, only god and I knew how it worked
 * 
 * Now, only god knows it!
 * 
 * therefore if you are trying to optimize
 * this code and fail (most surely),
 * please increase this counter as a
 * warning for the next person:
 * 
 * total_hours_wasted_here = 21
*/

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
