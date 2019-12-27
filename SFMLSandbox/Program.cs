using System;
using System.Linq;
using SFML.System;
using SFML.Audio;
using SFML.Window;
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
            SynthStream synth = new SynthStream(1, 44000);
            synth.Play();
            System.Threading.Thread.Sleep(1000);
            Scale s = new Scale(220);

            Dictionary<Keyboard.Key, double> notes = new Dictionary<Keyboard.Key, double>
            {
                [Keyboard.Key.Z] = s.Degree(Degree.I),
                [Keyboard.Key.S] = s.Degree(Degree.ii),
                [Keyboard.Key.X] = s.Degree(Degree.II),
                [Keyboard.Key.D] = s.Degree(Degree.iii),
                [Keyboard.Key.C] = s.Degree(Degree.III),
                [Keyboard.Key.V] = s.Degree(Degree.IV),
                [Keyboard.Key.G] = s.Degree(Degree.bV),
                [Keyboard.Key.B] = s.Degree(Degree.V),
                [Keyboard.Key.H] = s.Degree(Degree.vi),
                [Keyboard.Key.N] = s.Degree(Degree.VI),
                [Keyboard.Key.J] = s.Degree(Degree.vii),
                [Keyboard.Key.M] = s.Degree(Degree.VII),
                [Keyboard.Key.Comma] = s.Degree(Degree.I2),
            };

            Dictionary<Keyboard.Key, Note> held = new Dictionary<Keyboard.Key, Note>();

            Window window = new Window(new VideoMode(640, 480), "Synth");
            window.Closed += (o, e) => window.Close();
            window.KeyPressed += (o, e) =>
            {
                if (notes.ContainsKey(e.Code))
                {
                    held[e.Code] = new Note(0.5, notes[e.Code], new Envelope(0.2, 1, 0.1, 0.2));
                    held[e.Code].Start();
                    synth.AddNote(held[e.Code]);
                }
            };

            window.KeyReleased += (o, e) =>
            {
                if (notes.ContainsKey(e.Code))
                {
                    held[e.Code].Stop();
                }
            };
            window.SetKeyRepeatEnabled(false);

            while (window.IsOpen)
            {
                window.DispatchEvents();
            }
        }
    }

    public class Note
    {
        private long start = -1;
        private long stop = -1;
        private double amplitude = 1;
        private double max_amp;
        private double frequency;
        private double phase;
        private Envelope envelope;

        private Func<double, double, double, double, double> Wave;
        public Note(double a, double f, Envelope e)
        {
            max_amp = a;
            frequency = f;
            envelope = e;
            Wave = Oscillator.SineWave;
        }

        public void Start()
        {
            start = DateTime.Now.Ticks;
            stop = -1;
        }

        public void Stop()
        {
            stop = DateTime.Now.Ticks;
        }

        public bool playing()
        {
            return stop == -1 || TimeSpan.FromTicks(DateTime.Now.Ticks - stop).TotalSeconds < envelope.Release;
        } 

        public double GetSample(double time)
        {
            if (start != -1) {
                double elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks - start).TotalSeconds;
                if (elapsed <= envelope.Attack)
                    amplitude = (elapsed / envelope.Attack) * max_amp;
                else if (elapsed - envelope.Attack <= envelope.Decay)
                    amplitude = max_amp - (elapsed - envelope.Attack) / envelope.Decay * (max_amp - envelope.Sustain);
                else
                    amplitude = envelope.Sustain;

                if(stop != -1)
                {
                    double end_elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks - stop).TotalSeconds;
                    if (end_elapsed <= envelope.Release)
                        amplitude -= end_elapsed / envelope.Release * amplitude;
                }

                return Wave(time, frequency, amplitude, phase);
            };
            return 0;
        }

    }

    public struct Envelope
    {
        public double Attack;
        public double Decay;
        public double Sustain;
        public double Release;

        public Envelope(double a, double d, double s, double r)
        {
            Attack = a;
            Decay = d;
            Sustain = s;
            Release = r;
        }
    }

    public class SynthStream : SoundStream
    {
        long sampleCount = 0;
        private object _lock = new object();
        List<Note> currentNotes = new List<Note>();
        double samplesPerSecond;
        public SynthStream(uint channelCount, uint sampleRate)
        {
            Initialize(channelCount, sampleRate);
            samplesPerSecond = sampleRate * channelCount;
        }

        public void AddNote(Note n)
        {
            lock (_lock)
            {
                currentNotes.Add(n);
            }
        }

        protected override bool OnGetData(out short[] samples)
        {
            samples = new short[SampleRate / 20];
            Note[] notes;
            lock (_lock)
            {
                notes = new Note[currentNotes.Count];
                currentNotes.CopyTo(notes);
            }

            foreach (var n in notes)
            {
                if (!n.playing())
                {
                    lock (_lock)
                    {
                        currentNotes.Remove(n);
                    }
                    continue;
                }

                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] += (short)(n.GetSample((i + sampleCount) / samplesPerSecond) * short.MaxValue);
                    samples[i] = (short)Math.Clamp(samples[i] / 2, short.MinValue, short.MaxValue);
                }
            }
            
            sampleCount += samples.Length;
            return true;
        }

        protected override void OnSeek(Time timeOffset)
        {
        }
    }
}
