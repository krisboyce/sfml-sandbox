using System;
using SFMLSandbox.Music.Constants;

namespace SFMLSandbox.Music
{
    public class Scale
    {
        private double root;
        public Scale(double freq)
        {
            root = freq;
        }

        public double Degree(Degree degree)
        {
            return Step((int)degree);
        }

        public static double Interval(double freq, int steps)
        {
            return freq * Math.Pow(2, steps / 12.0);
        }

        public double Step(int steps)
        {
            return Interval(root, steps);
        }
    }
}
