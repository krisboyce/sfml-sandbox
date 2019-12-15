using System;
using System.Collections.Generic;
using System.Text;

namespace SFMLSandbox.Generation
{
    public class SineOscillator : Oscillator
    {
        public SineOscillator(double step) : base(step) { }
        protected override double GenerateWave(double time, double frequency, double amplitude, double phase)
        {
            return SineWave(time, frequency, amplitude, phase);
        }
    }
}
