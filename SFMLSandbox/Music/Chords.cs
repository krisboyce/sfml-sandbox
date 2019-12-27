using SFMLSandbox.Music.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFMLSandbox.Music
{
    public class Chords
    {
        private Scale _scale;
        private Dictionary<Chord, Degree[]> _chords;
        public Chords(double freq)
        {
            _scale = new Scale(freq);
            _chords = new Dictionary<Chord, Degree[]>()
            {
                [Chord.maj] = new[] { Degree.I, Degree.III, Degree.V },
                [Chord.min] = new[] { Degree.I, Degree.iii, Degree.V },
                [Chord.dim] = new[] { Degree.I, Degree.iii, Degree.bV },
                [Chord.maj7] = new[] { Degree.I, Degree.III, Degree.V, Degree.VII },
                [Chord.minMaj7] = new[] { Degree.I, Degree.iii, Degree.V, Degree.VII },
                [Chord.dom7] = new[] { Degree.I, Degree.III, Degree.V, Degree.vii },
                [Chord.min7] = new[] { Degree.I, Degree.iii, Degree.V, Degree.vii },
                [Chord.dim7] = new[] { Degree.I, Degree.III, Degree.V, Degree.vii },
            };
        }

        public double[] BuildChord(params Degree[] degrees)
        {
            List<double> notes = new List<double>();
            foreach (Degree degree in degrees)
            {
                notes.Add(_scale.Degree(degree));
            }

            return notes.ToArray();
        }

        public Degree[] GetChord(Chord chord) => _chords[chord];
    }
}
