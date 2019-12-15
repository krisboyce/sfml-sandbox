using System;
using System.Collections;
using System.Collections.Generic;

namespace SFMLSandbox.Generation
{
    public abstract class Oscillator : IEnumerable<double>
    {
        //Samples per ms
        protected double step;

        //Frequency of waveform in Hz
        protected double frequency;

        //Based on half the sine functions default range -1, 1
        protected double amplitude = 1.0;

        //Waveform offset in radians
        protected double phase = 0.0;

        public Oscillator(double timeStep)
        {
            step = timeStep;
        }

        public void SetFrequency(double freq)
        {
            frequency = freq;
        }

        public static double SineWave(double time, double frequency, double amplitude, double phase)
        {
            return amplitude * Math.Sin(Math.PI * 2 * time * frequency + phase);
        }

        protected abstract double GenerateWave(double time, double frequency, double amplitude, double phase);

        public IEnumerator<double> GetEnumerator()
        {
            double i = 0;
            while (true)
            {
                i++;

                double x = i / (step * 1000);
                yield return GenerateWave(x, frequency, amplitude, phase);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
