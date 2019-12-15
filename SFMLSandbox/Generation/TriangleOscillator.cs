using System;
using System.Collections.Generic;
using System.Text;

namespace SFMLSandbox.Generation
{
    public class TriangleOscillator : Oscillator
    {
        private int resolution = 100;
        public TriangleOscillator(double step) : base(step) { }
        protected override double GenerateWave(double time, double frequency, double amplitude, double phase)
        {
            double y = 0.0;
            double invert = 1;
            for(double i = 1; i<resolution; i += 2)
            {
                y += SineWave(time, invert * frequency * i, 1 / (i*i) / amplitude, phase);
                invert *= -1;
            }

            return 8 / (Math.PI * Math.PI) * y;
        }
    }
}
