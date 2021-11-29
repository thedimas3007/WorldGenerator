using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SimplexNoise;

namespace PerlinTest
{
    class Program
    {
        const float SEA_LEVEL = 100;

        static int width;
        static int height;
        static float[,] noise;
        static float[,] humidity;
        static float[,] temperature;
        
        enum biomeList {DESERT, PLAINS, TUNDRA, SAVANNAH, SHRUBLAND, TAIGA, SEASONAL_FOREST, FOREST, JUNGLE, SWAMP, ICE, UNKNOWN};
        static Pen[] biomeColors = {Pens.NavajoWhite, Pens.Green, Pens.MediumAquamarine, Pens.Olive, Pens.DarkGoldenrod, Pens.MediumSeaGreen, Pens.Green, Pens.ForestGreen, Pens.LimeGreen, Pens.OliveDrab, Pens.Snow, Pens.Black};

        static int CalcTemp(int x, int y, int height)
        {
            float noiseTemp = temperature[x, y] * 3.5f;
            return Convert.ToInt32((1 - Math.Abs(height/2.0 - y) / (height / 2.0)) * 100.0 * noiseTemp / 256);
        }

        static int CalcHumidity(int x, int y)
        {
            return Convert.ToInt32(humidity[x, y] / 256 * 100);
        }

        static biomeList GetBiome(int x, int y)
        {
            //noiseTemp = noiseTemp < 0.9 ? 0.9f : noiseTemp;
            int humidity = CalcHumidity(x, y);
            int temp = CalcTemp(x, y, height);
            temp = temp > 100 ? 100 : temp;

            if (temp <= 11 && humidity <= 100) { return biomeList.ICE; }
            else if (temp <= 25 && humidity <= 100) { return biomeList.TUNDRA; }
            else if (temp <= 60 && humidity <= 25) { return biomeList.PLAINS; }
            else if (temp <= 100 && humidity <= 25) { return biomeList.DESERT; }

            else if (temp > 25 && temp <= 50 && humidity <= 100) { return biomeList.TAIGA; }
            else if (temp <= 75 && humidity <= 50) { return biomeList.SHRUBLAND; }
            else if (temp <= 100 && humidity <= 50) { return biomeList.SAVANNAH; }

            else if (temp > 50 && temp <= 75 && humidity <= 75) { return biomeList.FOREST; }
            else if (temp <= 100 && humidity <= 75) { return biomeList.SEASONAL_FOREST; }

            else if (temp > 50 && temp <= 75 && humidity <= 100) { return biomeList.SWAMP; }
            else if (temp <= 100 && humidity <= 100) { return biomeList.JUNGLE; }

            else { Console.WriteLine($"ERROR | Temp: {temp} | Humidity: {humidity}"); return biomeList.UNKNOWN; }
        }
        static Pen GetBiomePen(biomeList biome)
        {
            return biomeColors[(int)biome];
        }

        static Pen GetPen(int x, int y, int level)
        {
            
            if (level < SEA_LEVEL/2)
            {
                return Pens.DarkBlue;
            }
            else if (level >= SEA_LEVEL/2 && level < SEA_LEVEL)
            {
                return Pens.Blue;
            }
            else if (level >= SEA_LEVEL && level < SEA_LEVEL+15)
            {
                return Pens.Yellow;
            }
            else if (level >= SEA_LEVEL+15 && level < SEA_LEVEL+130)
            {
                return GetBiomePen(GetBiome(x, y));
            }
            else if (level >= SEA_LEVEL+130 && level < SEA_LEVEL+150)
            {
                return Pens.Gray;
            }
            else
            {
                return Pens.White;
            }
        }
        static void Main(string[] args)
        {
            Console.Write("Enter seed (blank for random) > ");
            string seed = Console.ReadLine();
            Console.Write("Enter height > ");
            height = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter width > ");
            width = Convert.ToInt32(Console.ReadLine());

            //float mul

            if (seed.Trim() != "") { Noise.Seed = Convert.ToInt32(seed); }
            noise = Noise.Calc2D(width, height, 0.05f);
            humidity = Noise.Calc2D(width, height, 0.005f);
            temperature = Noise.Calc2D(width, height, 0.015f);

            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            MemoryStream ms = new MemoryStream();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //int perlin = Perlin.OctavePerlin
                    graphics.DrawRectangle(GetPen(x, y, (int)noise[x, y]), x, y, 1, 1);

                }
            }

            bitmap.Save("generated.png", ImageFormat.Png);
            Console.WriteLine("Generated image and saved into generated.png");
            Console.Write("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
