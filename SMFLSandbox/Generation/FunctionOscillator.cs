using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SFMLSandbox.Generation
{
    public class FunctionOscillator : Oscillator
    {
        private Func<double, double, double, double, double> _func;
        public FunctionOscillator(Func<double, double, double, double, double> waveFunction, double step) : base(step) {
            _func = waveFunction;
        }

        protected override double GenerateWave(double time, double frequency, double amplitude, double phase)
        {
            return _func(time, frequency, amplitude, phase);
        }
    }
}
