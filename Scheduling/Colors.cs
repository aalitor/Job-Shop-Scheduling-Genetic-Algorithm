using System;
using System.Drawing;

namespace Scheduling
{
    public static class Colors
    {
        static Random rnd = new Random();
        public static Color[] JobColors;
        public static void GenerateRandomHSV(int jobs)
        {
            JobColors = new Color[jobs];
            double a = 0.8d / (jobs + 1);

            for (int i = 0; i < jobs; i++)
            {
                double h = a * (i + 1) + 0.2d;
                double s = rnd.Next(0, 5) * 0.05 + 0.4;
                double v = rnd.Next(0, 5) * 0.05 + 0.7;
                JobColors[i] = GenerateHSVColor(h, s, v);
            }
        }
        public static Color GenerateHSVColor(double h, double s, double v)
        {
            h += 0.61803398;
            h = h > 1 ? h - 1 : h;
            int hi = (int)(h * 6);
            double f = h * 6 - hi;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);
            double r = 0, g = 0, b = 0;
            if (hi == 0)
            {
                r = v;
                g = t;
                b = p;
            }
            if (hi == 1)
            {
                r = q;
                g = v;
                b = p;
            }
            else if (hi == 2)
            {
                r = p;
                g = v;
                b = t;
            }
            else if (hi == 3)
            {
                r = p;
                g = q;
                b = v;
            }
            else if (hi == 4)
            {
                r = t;
                g = p;
                b = v;
            }
            else if (hi == 5)
            {
                r = v;
                g = p;
                b = q;
            }
            int R = (int)(r * 256);
            int G = (int)(g * 256);
            int B = (int)(b * 256);

            return Color.FromArgb(R, G, B);
        }
    }
}
