using System;
using System.Collections.Generic;
using System.Text;

namespace SFMLSandbox.Generation
{
    public class SawtoothOscillator : Oscillator
    {
        private int resolution = 100;
        public SawtoothOscillator(double step) : base(step) { }
        protected override double GenerateWave(double time, double frequency, double amplitude, double phase)
        {
            double y = 0.0;
            for(double i = 1; i<resolution; i++)
            {
                y += SineWave(time, frequency * i, 1 / i * amplitude, phase);
            }

            return 2 * amplitude / Math.PI * y;
        }
    }
}
