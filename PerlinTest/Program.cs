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

        static int seed = Convert.ToInt32(new Random().NextDouble() * Int32.MaxValue);

        static int width;
        static int height;
        static Perlin noise = new Perlin(1, 0.125, 255, 1, seed);
        static Perlin humidity = new Perlin(1, 0.025, 255, 1, seed);
        static Perlin temperature = new Perlin(1, 0.0125, 255, 1, seed);
        static Perlin rivers = new Perlin(1, 0.1, 255, 1, seed);

        static Perlin perlin = new Perlin(1, 0.125, 255, 1, seed);

        // TODO: less biomes
        enum biomeList {DESERT, PLAINS, TUNDRA, SAVANNA, SHRUBLAND, TAIGA, SEASONAL_FOREST, FOREST, JUNGLE, SWAMP, ICE, UNKNOWN};
        static Pen[] biomeColors = {Pens.NavajoWhite, Pens.Green, Pens.MediumAquamarine, Pens.Olive, Pens.DarkGoldenrod, Pens.MediumSeaGreen, Pens.Green, Pens.ForestGreen, Pens.LimeGreen, Pens.OliveDrab, Pens.Snow, Pens.Black}; // TODO: use minecraft colormap
        static Pen[] waterColors = {PFH("#32A598"), PFH("#44AFF5"), PFH("#14559B"), PFH("#2C8B9C"), PFH("#44AFF5"), PFH("#007BF7"), PFH("#20A3CC"), PFH("#1E97F2"), PFH("#14A2C5"), PFH("#617B64"), PFH("#2570B5")};

        static int Get2D(int x, int y, Perlin perlin)
        {
            int noise = Convert.ToInt32(perlin.Get2D(x, y)) + 127;

            noise = noise > 255 ? 255 : noise;
            noise = noise < 0 ? 0 : noise;
            return noise;
        }

        static int CalcTemp(int x, int y, int height)
        {
            float noiseTemp = Get2D(x, y, temperature) / 255f * 100 * 4;
            return Convert.ToInt32((1 - Math.Abs(height / 2.0 - y) / (height / 2.0)) * 100.0 * noiseTemp / 256);
        }

        static Pen PFH(string color) // PenFromHtml
        { 
            return new Pen(ColorTranslator.FromHtml(color));
        }

        static int CalcHumidity(int x, int y)
        {
            return Convert.ToInt32(Get2D(x, y, humidity) / 255f * 100);
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
            else if (temp <= 100 && humidity <= 50) { return biomeList.SAVANNA; }

            else if (temp > 50 && temp <= 75 && humidity <= 75) { return biomeList.FOREST; }
            else if (temp <= 100 && humidity <= 75) { return biomeList.SEASONAL_FOREST; }

            else if (temp > 50 && temp <= 75 && humidity <= 100) { return biomeList.SWAMP; }
            else if (temp <= 100 && humidity <= 100) { return biomeList.JUNGLE; }

            else { Console.WriteLine($"ERROR | Temp: {temp} | Humidity: {humidity}"); return biomeList.UNKNOWN; }
        }

        static Pen GetWaterPen(biomeList biome)
        {
            return waterColors[(int)biome];
        }

        static Pen GetBiomePen(biomeList biome)
        {
            return biomeColors[(int)biome];
        }

        static Pen GetPen(int x, int y, int level)
        {
            bool isRiver = Get2D(x, y, rivers) >= 125 && Get2D(x, y, rivers) <= 145;
            if (level < SEA_LEVEL/2)
            {
                biomeList biome = GetBiome(x, y);
                if (biome == biomeList.ICE || biome == biomeList.TUNDRA) { return PFH("#2080C9"); }
                else { return PFH("#1787D4"); }
            }
            else if (level >= SEA_LEVEL/2 && level < SEA_LEVEL || isRiver)
            {
                return GetWaterPen(GetBiome(x, y));
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
            /*Console.Write("Enter seed (blank for random) > ");
            string seed = Console.ReadLine();*/
            Console.Write("Enter height > ");
            height = Convert.ToInt32(Console.ReadLine());
            Console.Write("Enter width > ");
            width = Convert.ToInt32(Console.ReadLine());

            //float mul
            DateTime start;
            //if (seed.Trim() != "") { Noise.Seed = Convert.ToInt32(seed); }
            /*Console.WriteLine("Generating noise...");
            start = DateTime.Now;
            noise = Noise.Calc2D(width, height, 0.05f);
            humidity = Noise.Calc2D(width, height, 0.005f);
            temperature = Noise.Calc2D(width, height, 0.015f);
            rivers = Noise.Calc2D(width, height, 0.011f);
            Console.WriteLine($"Noise generated {(DateTime.Now - start).TotalSeconds}");*/

            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);

            int max = 0, min = 0;

            Console.WriteLine("Starting render");
            start = DateTime.Now;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //graphics.DrawRectangle(GetPen(x, y, (int)noise[x, y]), x, y, 1, 1);
                    int clr = Convert.ToInt32(noise.Get2D(x, y)) + 127;

                    max = clr > max ? clr : max;
                    min = clr < min ? clr : min;


                    clr = clr > 255 ? 255 : clr;
                    clr = clr < 0 ? 0 : clr;

                    graphics.DrawRectangle(GetPen(x, y, clr), x, y, 1, 1);

                    /*Pen pen = PFH("#0000" + Math.Abs(clr).ToString("X"));
                    graphics.DrawRectangle(pen, x, y, 1, 1);*/
                }
            }

            double spent = (DateTime.Now - start).TotalSeconds;
            double pps = height * width / spent;
            double tpp = spent / (height * width) * 1000000;

            Console.WriteLine($"Max: {max}");
            Console.WriteLine($"Min: {min}");
            Console.WriteLine($"Seed: {seed}");
            Console.WriteLine($"Spent {spent}s");
            Console.WriteLine($"Pixels/Second: {pps}");
            Console.WriteLine($"Time per pixel: {tpp}us");

            bitmap.Save("generated.png", ImageFormat.Png);
            Console.Write("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
