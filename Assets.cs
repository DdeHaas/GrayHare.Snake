using System.Text;

namespace GrayHare.Snake;

/// <summary>Ensures all required assets exist, generating procedural WAV files when missing.</summary>
internal static class Assets
{
    /// <summary>Creates missing WAV files and returns the asset manifest with relative paths.</summary>
    /// <param name="contentRoot">Absolute path to the Assets directory.</param>
    public static AssetsManifest EnsureCreated(string contentRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);

        string soundsDir = Path.Combine(contentRoot, "sounds");
        Directory.CreateDirectory(soundsDir);

        string eatPath = Path.Combine(soundsDir, "eat.wav");
        string bonusEatPath = Path.Combine(soundsDir, "bonus_eat.wav");
        string deathPath = Path.Combine(soundsDir, "death.wav");
        string musicPath = Path.Combine(soundsDir, "music.wav");

        File.WriteAllBytes(eatPath, CreateBeepWav(44100, 880f, 0.12f, 0.4f));

        if (!File.Exists(bonusEatPath))
        {
            File.WriteAllBytes(bonusEatPath, CreateArpeggioWav(44100, [523f, 659f, 784f], 0.08f));
        }

        if (!File.Exists(deathPath))
        {
            File.WriteAllBytes(deathPath, CreateDescendingSweepWav(44100, 440f, 110f, 0.5f));
        }

        if (!File.Exists(musicPath))
        {
            File.WriteAllBytes(musicPath, CreateDroneWav(44100, 130f, 195f, 4.0f));
        }

        return new AssetsManifest(
            eatSoundPath: "sounds/eat.wav",
            bonusEatSoundPath: "sounds/bonus_eat.wav",
            deathSoundPath: "sounds/death.wav",
            musicPath: "sounds/music.wav",
            fontPath: GameConstants.FontName);
    }

    // ── WAV helpers ────────────────────────────────────────────────────────────

    /// <summary>Creates a sine-wave beep with a linear decay envelope.</summary>
    private static byte[] CreateBeepWav(int sampleRate, float frequency, float durationSeconds, float amplitude = 0.35f)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        short[] samples = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            double sample = Math.Sin(2 * Math.PI * frequency * i / sampleRate);
            double envelope = 1.0 - ((double)i / sampleCount);
            samples[i] = (short)(sample * envelope * short.MaxValue * amplitude);
        }

        return BuildWav(sampleRate, samples);
    }

    /// <summary>Creates a WAV with a sine sweep descending from <paramref name="startFreq"/> to <paramref name="endFreq"/>.</summary>
    private static byte[] CreateDescendingSweepWav(int sampleRate, float startFreq, float endFreq, float durationSeconds)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        short[] samples = new short[sampleCount];
        double phase = 0.0;
        for (int i = 0; i < sampleCount; i++)
        {
            float progress = (float)i / sampleCount;
            float freq = startFreq + ((endFreq - startFreq) * progress);
            double sample = Math.Sin(phase);
            double envelope = 1.0 - progress;
            samples[i] = (short)(sample * envelope * short.MaxValue * 0.5f);
            phase += 2 * Math.PI * freq / sampleRate;
        }

        return BuildWav(sampleRate, samples);
    }

    /// <summary>Creates a WAV that concatenates simple sine beeps at each frequency.</summary>
    private static byte[] CreateArpeggioWav(int sampleRate, float[] frequencies, float noteDuration)
    {
        int noteSamples = (int)(sampleRate * noteDuration);
        int totalSamples = noteSamples * frequencies.Length;
        short[] samples = new short[totalSamples];
        for (int n = 0; n < frequencies.Length; n++)
        {
            float freq = frequencies[n];
            int offset = n * noteSamples;
            for (int i = 0; i < noteSamples; i++)
            {
                double sample = Math.Sin(2 * Math.PI * freq * i / sampleRate);
                double envelope = 1.0 - ((double)i / noteSamples);
                samples[offset + i] = (short)(sample * envelope * short.MaxValue * 0.5f);
            }
        }

        return BuildWav(sampleRate, samples);
    }

    /// <summary>Creates a WAV mixing two sine drones at <paramref name="freq1"/> and <paramref name="freq2"/>.</summary>
    private static byte[] CreateDroneWav(int sampleRate, float freq1, float freq2, float durationSeconds)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        short[] samples = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            double s1 = Math.Sin(2 * Math.PI * freq1 * i / sampleRate);
            double s2 = Math.Sin(2 * Math.PI * freq2 * i / sampleRate);
            double mixed = (s1 + s2) * 0.35;
            samples[i] = (short)(mixed * short.MaxValue);
        }

        return BuildWav(sampleRate, samples);
    }

    /// <summary>Builds a 16-bit mono PCM WAV file from the given sample array.</summary>
    private static byte[] BuildWav(int sampleRate, short[] samples)
    {
        const short channels = 1;
        const short bitsPerSample = 16;
        short blockAlign = channels * bitsPerSample / 8;
        int byteRate = sampleRate * blockAlign;
        int dataSize = samples.Length * blockAlign;

        using MemoryStream ms = new();
        using BinaryWriter w = new(ms, Encoding.ASCII, leaveOpen: true);

        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataSize);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);
        w.Write(channels);
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write(blockAlign);
        w.Write(bitsPerSample);
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataSize);
        foreach (short s in samples)
        {
            w.Write(s);
        }

        w.Flush();

        return ms.ToArray();
    }
}
