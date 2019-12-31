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
            SynthStream synth = new SynthStream(1, 44100);
            Scale s = new Scale(220);

            // Key to note mapping
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

            Dictionary<Keyboard.Key, int> held = new Dictionary<Keyboard.Key, int>();
            Voice v = new Instrument();

            Window window = new Window(new VideoMode(640, 480), "Synth");
            window.Closed += (o, e) => window.Close();
            int octave = 0;
            window.KeyPressed += (o, e) =>
            {
                if (e.Code == Keyboard.Key.LShift)
                    octave++;
                else if (e.Code == Keyboard.Key.LControl)
                    octave--;

                if (notes.ContainsKey(e.Code) && !held.ContainsKey(e.Code))
                {
                    var freq = Scale.Interval(notes[e.Code], octave * 12);
                    held[e.Code] = synth.AddNote(v, freq);
                    synth.StartNote(held[e.Code]);
                }
            };

            window.KeyReleased += (o, e) =>
            {
                if (held.ContainsKey(e.Code))
                {
                    synth.StopNote(held[e.Code]);
                    held.Remove(e.Code);
                }
            };

            //Save overhead by not trying to play already held notes
            window.SetKeyRepeatEnabled(false);

            List<(int? id, double time, double freq, double length)> seq = new List<(int? id, double time, double freq, double length)>
            {
                (null, 0.0, 440.0, 4.0),
                (null, 1, Scale.Interval(440, 4), 3.0),
                (null, 2, Scale.Interval(440, 7), 2.0),
                (null, 3, Scale.Interval(440, 12), 1.0),
                (null, 4, Scale.Interval(440, 0), 1.0),
                (null, 4, Scale.Interval(440, 4), 1.0),
                (null, 4, Scale.Interval(440, 7), 1.0),
                (null, 4, Scale.Interval(440, 12), 1.0)
            };

            synth.Play();
            DateTime start = DateTime.Now;
            while (window.IsOpen)
            {
                double elapsed = (DateTime.Now - start).TotalSeconds;
                for(int i = 0; i<seq.Count; i++){
                    var note = seq[i];
                    if(elapsed > note.time && note.id == null)
                    {
                        note.id = synth.AddNote(v, note.freq);
                        synth.StartNote(note.id.Value);
                        seq[i] = note;
                    }

                    if(note.time + note.length <= elapsed && note.id != null)
                    {
                        synth.StopNote(note.id.Value);
                    }
                }

                window.DispatchEvents();
            }
        }
    }
    public class Instrument : Voice
    {
        public Instrument()
        {
            amplitude = 0.5;
            envelope = new ADSREnvelope { Attack = 0.001, Decay = 0.01, Release = 1, Sustain = 0.2 };
        }

        protected override double Wave(double time, double frequency, double amplitude, double phase)
        {
            return Oscillator.SineWave(time, frequency, amplitude / 2, Math.Sin(time * Math.PI * 2 * 3))
            + Oscillator.SineWave(time, frequency * 4, amplitude / 4, Math.Sin(time * Math.PI * 2 * 5))
            + Oscillator.SineWave(time, frequency * 8, amplitude / 8, Math.Sin(time * Math.PI * 7))
            + Oscillator.Noise(time, amplitude / 8);
        }
    }
    public abstract class Voice
    {
        protected double amplitude = 0.0;
        protected double phase = 0.0;
        protected Envelope envelope = new ADSREnvelope();
        protected abstract double Wave(double time, double frequency, double amplitude, double phase);

        public virtual Note GetNote(double frequency)
        {
            return new Note(Wave, envelope, amplitude, frequency, phase);
        }
    }

    public class Note
    {
        private double triggerOnTime;
        private double triggerOffTime;
        private bool noteOn;
        private bool started = false;
        private double amplitude;
        private double currentAmplitude;
        private double releaseAmplitude;
        private double frequency;
        private double phase;
        private Envelope envelope;

        private Func<double, double, double, double, double> Wave;
        public Note(Func<double, double, double, double, double> waveform, Envelope e, double a, double f, double p)
        {
            amplitude = a;
            frequency = f;
            phase = p;
            envelope = e;
            Wave = waveform;
        }

        public void Start(double time)
        {
            noteOn = true;
            started = true;
            triggerOnTime = time;
        }

        public void Stop(double time)
        {
            noteOn = false;
            triggerOffTime = time;
            releaseAmplitude = currentAmplitude;
        }

        public bool NoteActive()
        {
            return !started || noteOn || currentAmplitude > 0.0;
        }

        public double GetSample(double time)
        {
            currentAmplitude = envelope.GetAmplitude(
                noteOn,
                time,
                triggerOnTime,
                triggerOffTime,
                releaseAmplitude,
                amplitude);

            return Wave(time, frequency, currentAmplitude, phase);
        }
    }

    public abstract class Envelope {
        public double Attack;
        public double Decay;
        public double Sustain;
        public double Release;
        public abstract double GetAmplitude(bool held, double now, double startTime, double releaseTime, double releaseAmplitude, double maxAmplitude);
    }

    public class ADSREnvelope : Envelope
    {
        public override double GetAmplitude(bool held, double now, double startTime, double releaseTime, double releaseAmplitude, double maxAmplitude)
        {
            if (held)
            {
                double timeElapsed = now - startTime;

                if (timeElapsed <= Attack)
                {
                    double progress = timeElapsed / Attack;
                    return progress * maxAmplitude;
                }
                else if (timeElapsed - Attack <= Decay)
                {
                    double progress = (timeElapsed - Attack) / Decay;
                    return maxAmplitude + ((Sustain - maxAmplitude) * progress);
                }
                else
                {
                    return Sustain;
                }
            }
            else
            {
                double timeElapsed = now - releaseTime;
                double progress = timeElapsed / Release;
                return releaseAmplitude - progress * releaseAmplitude;
            }
        }
    }

    public class SynthStream : SoundStream
    {
        int nextId = 0;
        uint sampleCount = 0;
        private object _lock = new object();
        Dictionary<int, Note> currentNotes = new Dictionary<int, Note>();
        List<int> notesToStart = new List<int>();
        List<int> notesToStop = new List<int>();
        double samplesPerSecond;
        public SynthStream(uint channelCount, uint sampleRate)
        {
            Initialize(channelCount, sampleRate);
            samplesPerSecond = sampleRate * channelCount;
        }

        public int AddNote(Voice voice, double frequency)
        {
            lock (_lock)
            {
                int id = nextId;
                currentNotes[nextId] = voice.GetNote(frequency);
                nextId++;
                return id;
            }
        }

        public void StartNote(int id)
        {
            lock (_lock)
            {
                notesToStart.Add(id);
            }
        }

        public void StopNote(int id)
        {
            lock (_lock)
            {
                notesToStop.Add(id);
            }
        }

        protected override bool OnGetData(out short[] samples)
        {
            // Buffer size determines latency i.e. length = SampleRate = 1s
            // Low latency can cause buffer underrun which sounds like trash
            samples = new short[SampleRate / 100];
            
            // Sync for starting and stopping notes with the appropriate timestamp
            // Also cleans up dead notes
            Note[] notes;
            lock (_lock)
            {
                currentNotes = currentNotes.Where(x => x.Value.NoteActive())
                                 .ToDictionary(x => x.Key,
                                               x => x.Value);

                foreach (var id in notesToStart)
                {
                    if (currentNotes.TryGetValue(id, out var n))
                    {
                        n.Start(sampleCount / samplesPerSecond);
                    }
                }
                notesToStart.Clear();

                foreach (var id in notesToStop)
                {
                    if (currentNotes.TryGetValue(id, out var n))
                    {
                        n.Stop(sampleCount / samplesPerSecond);
                    }
                }
                notesToStop.Clear();

                notes = currentNotes.Values.ToArray();
            }

            for (int i = 0; i < samples.Length; i++)
            {
                lock (_lock)
                {
                    sampleCount++;
                }

                double sample = 0.0;
                double time = sampleCount / samplesPerSecond;
                foreach (var n in notes)
                {
                    var nSample = n.GetSample(time);

                    // Not an ideal mixing technique but still learning ;)
                    // Gonna need to learn about limiters
                    if (nSample < 0 && sample < 0)
                    {
                        sample = (sample + nSample) - (sample * nSample / -notes.Length);
                    }
                    else if (nSample > 0 && sample > 0)
                    {
                        sample = (sample + nSample) + (sample * nSample / notes.Length);
                    }
                    else
                    {
                        sample += nSample;
                    }
                }

                // Clip any runaway samples lol
                samples[i] = (short)(Math.Clamp(sample, -1, 1) * short.MaxValue);
            }

            // Avoid overflow 'edgecase' without interrupting any current notes.
            // Possible sample discontinuity (i.e. a pop) when playing notes continuously for 2^32 samples
            // for reference 2^32 samples at 44.1kHz is ~27hrs of continuous playback
            if (notes.Length == 0)
                sampleCount = 0;

            return true;
        }

        //Seeking isn't a feature
        protected override void OnSeek(Time timeOffset)
        {
        }
    }
}
