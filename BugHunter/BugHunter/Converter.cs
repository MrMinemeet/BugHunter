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
        [Obsolete()]
        public static int[][] TmxToIntArray(string path)
        {
            StreamReader sr = new StreamReader(path);
            var lines = new List<string[]>();
            int Row = 0;
            while (!sr.EndOfStream)
            {
                string[] Line = sr.ReadLine().Split(',');
                lines.Add(Line);
                Row++;
            }

            string[][] stringvar = lines.ToArray();

            int[][] intvar = new int[70 - 39][];

            for (int row = 39, i = 0; row < 70; row++, i++)
            {
                intvar[i] = new int[stringvar[row].Length];
                for (int col = 0; col < stringvar[row].Length - 1; col++)
                {
                    intvar[i][col] = int.Parse(stringvar[row][col]);
                    Console.Write(intvar[i][col] + " ");
                }
                Console.WriteLine();
            }

            return intvar;
        }

        public static int[][] MapToIntArray(TiledMap map, Settings _settings)
        {
            TiledMapTileLayer tml = map.GetLayer<TiledMapTileLayer>("Collision/Trigger");
            TiledMapTile? tmt;

            Settings settings = _settings;
            int[][] MapArray = new int[settings.MapSizeHeight][];

            for (int y = 0; y < settings.MapSizeHeight; y++)
            {
                MapArray[y] = new int[settings.MapSizeWidth];

                for(int x = 0; x < MapArray[y].Length; x++)
                {
                    tmt = tml.GetTile((ushort)x, (ushort)y);
                    if(tmt != null)
                    {
                        MapArray[y][x] = tmt.Value.GlobalIdentifier;
                        

                        Console.Write(MapArray[y][x] + "  ");
                    }
                }
                Console.WriteLine();
            }


            return MapArray;
        }
    }
}
