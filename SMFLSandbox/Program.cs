using System;
using System.Linq;
using SFML.System;
using SFML.Audio;
using System.Collections.Generic;
using SFMLSandbox.Generation;
using SFMLSandbox.Music.Constants;
using SFMLSandbox.Music;

namespace SFMLSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var osc = new FunctionOscillator(Oscillator.SineWave, 22000 / 1000.0 * 2);
            SynthStream synth = new SynthStream(osc, 2, 22000);
            synth.Play();
            
            Console.ReadKey(false);
        }
    }

    public class SynthStream : SoundStream
    {
        private IEnumerable<double> _data;
        public SynthStream(IEnumerable<double> samples, uint channelCount, uint sampleRate)
        {
            _data = samples;
            Initialize(2, sampleRate);
        }

        //TODO: Need thread-safe way to update oscillators outside of sound stream
        int counter = 0;
        protected override bool OnGetData(out short[] samples)
        {
            Scale s = new Scale(440);
            Degree[] degrees = new[]
            {
                Degree.I,
                Degree.III,
                Degree.V,
                Degree.VII,
                Degree.I + 12,
                Degree.III + 12,
                Degree.VII + 12,
                Degree.V + 12,
                Degree.I + 24,
                Degree.III + 24,
                Degree.V + 24,
                Degree.VII + 24,
                Degree.I + 36,
                Degree.VII + 24,
                Degree.V + 24,
                Degree.III + 24,
                Degree.I + 24,
                Degree.VII + 12,
                Degree.V + 12,
                Degree.III + 12,
                Degree.I + 12,
                Degree.VII,
                Degree.V,
                Degree.III,
            };

            counter++;
            counter %= degrees.Length;

            double note = s.Degree(degrees[counter]);
            ((Oscillator)_data).SetFrequency(note);
            samples = _data.Take((int)SampleRate / 1000 * 100).Select(x => (short)(x * short.MaxValue)).ToArray();
            return true;
        }

        protected override void OnSeek(Time timeOffset)
        {
        }
    }
}
