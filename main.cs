using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;

class Program
{
    // algorithm settings, feel free to mess with it
    const bool AVERAGE = false;
    static int NUMCOLORS = 32;
    const int WIDTH = 512;
    const int HEIGHT = 512;
    static int STARTX = 128;
    static int STARTY = 64;

    // represent a coordinate
    struct XY
    {
        public int x, y;
        public XY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override int GetHashCode()
        {
            return x ^ y;
        }
        public override bool Equals(object obj)
        {
            var that = (XY)obj;
            return this.x == that.x && this.y == that.y;
        }
    }

    // gets the difference between two colors
    static int coldiff(Color c1, Color c2)
    {
        var r = c1.R - c2.R;
        var g = c1.G - c2.G;
        var b = c1.B - c2.B;
        return r * r + g * g + b * b;
    }

    // gets the neighbors (3..8) of the given coordinate
    static List<XY> getneighbors(XY xy)
    {
        var ret = new List<XY>(8);
        for (var dy = -1; dy <= 1; dy++)
        {
            if (xy.y + dy == -1 || xy.y + dy == HEIGHT)
                continue;
            for (var dx = -1; dx <= 1; dx++)
            {
                if (xy.x + dx == -1 || xy.x + dx == WIDTH)
                    continue;
                ret.Add(new XY(xy.x + dx, xy.y + dy));
            }
        }
        return ret;
    }

    // calculates how well a color fits at the given coordinates
    // by similarity
    static int calcdiff1(Color[,] pixels, XY xy, Color c)
    {
        // get the diffs for each neighbor separately
        var diffs = new List<int>(8);
        foreach (var nxy in getneighbors(xy))
        {
            var nc = pixels[nxy.y, nxy.x];
            if (!nc.IsEmpty)
                diffs.Add(coldiff(nc, c));
        }

        // average or minimum selection
        if (AVERAGE)
            return (int)diffs.Average();
        else
            return diffs.Min();
    }

    static int calcdiff2(Bitmap imgref, XY xy, Color c)
    {
        // geometric bias
        // double phi = Math.Atan2(xy.y, xy.x);
        double phi = Math.Atan2(xy.y - STARTY, xy.x -STARTY);
        double v = phi * 1000;

        // similarity to reference image
        // int x = coldiff(imgref.GetPixel(xy.x, xy.y), c);
        // double v = x * 0.1;

        return Convert.ToInt32(v);
    }
    
    static int calcdiff(Color[,] pixels, Bitmap imgref, XY xy, Color c)
    {
        return calcdiff1(pixels, xy, c) + calcdiff2(imgref, xy, c);
    }

    static void Main(string[] args)
    {
        // set params
        int NUMPIXELS = WIDTH * HEIGHT;
        NUMCOLORS = 0;
        for (var guess = 1; guess < 10000; guess++) {
            int g2 = guess * guess * guess;
            if (g2 == NUMPIXELS) {
                NUMCOLORS = guess;
            }
        }
        Trace.Assert(NUMCOLORS > 0);
        STARTX = WIDTH / 2;
        STARTY = HEIGHT / 2;

        // print settings
        Console.WriteLine("NUMCOLORS {0}", NUMCOLORS);
        Console.WriteLine("WIDTH {0}", WIDTH);
        Console.WriteLine("HEIGHT {0}", HEIGHT);
        Console.WriteLine("STARTX {0}", STARTX);
        Console.WriteLine("STARTY {0}", STARTY);


        var imgref = new Bitmap("./refimg/lena-512x512.png");

        // create every color once and randomize the order
        var colors = new List<Color>();
        for (var r = 0; r < NUMCOLORS; r++)
            for (var g = 0; g < NUMCOLORS; g++)
                for (var b = 0; b < NUMCOLORS; b++)
                    colors.Add(Color.FromArgb(r * 255 / (NUMCOLORS - 1), g * 255 / (NUMCOLORS - 1), b * 255 / (NUMCOLORS - 1)));
        var rnd = new Random();
        colors.Sort(new Comparison<Color>((c1, c2) => rnd.Next(3) - 1));
        // var red = Color.FromArgb(255, 0, 0);
        // colors = colors.OrderBy((c) => -coldiff(c, red)).ToList();

        // temporary place where we work (faster than all that many GetPixel calls)
        var pixels = new Color[HEIGHT, WIDTH];
        Trace.Assert(pixels.Length == colors.Count);

        // constantly changing list of available coordinates (empty pixels which have non-empty neighbors)
        var available = new HashSet<XY>();

        // calculate the checkpoints in advance
        var checkpoints = Enumerable.Range(1, 10).ToDictionary(i => i * colors.Count / 10 - 1, i => i - 1);

        // loop through all colors that we want to place
        for (var i = 0; i < colors.Count; i++)
        {
            if (i % 256 == 0)
                Console.WriteLine("{0:P}, queue size {1}", (double)i / WIDTH / HEIGHT, available.Count);

            XY bestxy;
            if (available.Count == 0)
            {
                // use the starting point
                bestxy = new XY(STARTX, STARTY);
            }
            else
            {
                bestxy = available.First();
                int bestdiff = calcdiff(pixels, imgref, bestxy, colors[i]);

                // find the best place from the list of available coordinates
                foreach (var xy in available) {
                    int diff = calcdiff(pixels, imgref, xy, colors[i]);
                    if (diff < bestdiff) {
                            bestdiff = diff;
                            bestxy = xy;
                    }
                }
            }

            // put the pixel where it belongs
            Trace.Assert(pixels[bestxy.y, bestxy.x].IsEmpty);
            pixels[bestxy.y, bestxy.x] = colors[i];

            // adjust the available list
            available.Remove(bestxy);
            foreach (var nxy in getneighbors(bestxy))
                if (pixels[nxy.y, nxy.x].IsEmpty)
                    available.Add(nxy);

            // save a checkpoint
            int chkidx;
            if (checkpoints.TryGetValue(i, out chkidx))
            {
                Console.WriteLine("checkpoint");
                var img = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb);
                for (var y = 0; y < HEIGHT; y++)
                {
                    for (var x = 0; x < WIDTH; x++)
                    {
                        img.SetPixel(x, y, pixels[y, x]);
                    }
                }
                img.Save("result" + chkidx + ".png");
            }
        }

        Trace.Assert(available.Count == 0);
    }
}
