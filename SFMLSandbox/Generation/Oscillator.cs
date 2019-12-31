using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SFMLSandbox.Generation
{
    public static class Oscillator
    {
        static double[] rngBuffer;
        static Oscillator()
        {
            rngBuffer = new double[44100 * 10];
            rngBuffer = rngBuffer.Select(x => 2 * new Random().NextDouble() - 1).ToArray();
        }
        public static double SineWave(double time, double frequency, double amplitude, double phase)
        {
            return amplitude * Math.Sin(Math.PI * 2 * time * frequency + phase);
        }

        public static double SawTooth(double time, double frequency, double amplitude, double phase)
        {
            double y = 0.0;
            for (double i = 1; i < 100; i++)
            {
                y += SineWave(time, frequency * i, 1 / i * amplitude, phase);
            }

            return 2 * amplitude / Math.PI * y;
        }

        public static double Square(double time, double frequency, double amplitude, double phase)
        {
            double y = 0;
            for (double i = 1; i < 100; i += 2)
            {
                y += SineWave(time, frequency * i, 1 / i * amplitude, phase);
            }

            return y;
        }

        public static double Triangle(double time, double frequency, double amplitude, double phase)
        {
            double y = 0.0;
            double invert = 1;
            for (double i = 1; i < 100; i += 2)
            {
                y += SineWave(time, invert * frequency * i, 1 / (i * i) / amplitude, phase);
                invert *= -1;
            }

            return 8 / (Math.PI * Math.PI) * y;
        }

        public static double Noise(double time, double amplitude)
        {
            return amplitude * rngBuffer[(int)(time * rngBuffer.Length) % rngBuffer.Length];
            //return amplitude * (new Random().NextDouble() * 2 - 1);
        }
    }
}
