using Helpers.Async;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class WavePlayerManager
{
    public static int BUFFER_SIZE = 2048;
    private const int BYTE_SIZE = 8;
    private static int TOTAL_BUFFER_SIZE { get { return BUFFER_SIZE * BYTE_SIZE; } }

    private static WaveLib.WaveOutPlayer m_Player;

    private static PlayerStream m_SelectedAudioStream => nameToFileStream.ContainsKey(selectedStream) ? nameToFileStream[selectedStream] : null;

    private static Dictionary<string, PlayerStream> nameToFileStream = new Dictionary<string, PlayerStream>();
    private static string selectedStream = "";

    public static bool loop;

    public static bool isPlaying { get; private set; }

    public static void Dispose()
    {
        StopWatch watch = new StopWatch("Disposing Files");

        Stop();

        foreach (PlayerStream stream in nameToFileStream.Values)
        {
            stream.Dispose();
        }

        watch.Stop();
    }

    public static void LoadFile(string key, string filePath)
    {
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError("File not found:" + filePath);
            return;
        }

        StopWatch watch = new StopWatch("Loading File:" + key);

        PlayerStream playerStream = new PlayerStream(filePath);
        nameToFileStream.Add(key, playerStream);
        selectedStream = key;

        watch.Stop();
    }

    public static void Stop()
    {
        StopWatch watch = new StopWatch("Disposing Player");
        if (m_Player != null)
        {
            try
            {
                m_Player.Dispose();
            }
            finally
            {
                m_Player = null;
            }
        }
        isPlaying = false;
        watch.Stop();
    }

    /// <summary>
    /// Creates a new thread and plays there
    /// </summary>
    public static void TryPlay_TS(string key, double durationMS, Action<bool> onDoneTS = null)
    {
        // Debug.Log("Requested :: " + key);
        if (!nameToFileStream.ContainsKey(key))
        {
            Debug.LogError("Key not found in cached AudioStreams:" + key);
            onDoneTS?.Invoke(false);
            return;
        }

        if (isPlaying)
        {
            Debug.LogError("Currently playing");
            onDoneTS?.Invoke(false);
            return;
        }

        AsyncThread.RequestRunOnNewThread(() =>
        {
            Play_Thread(key, durationMS, () => onDoneTS?.Invoke(true));
        }
        );
    }

    private static void Play_Thread(string key, double durationMS, Action onDone = null)
    {
        isPlaying = true;

        StopWatch watch = new StopWatch("Starting player");
        selectedStream = key;
        PlayerStream m_AudioStream = m_SelectedAudioStream;
        if (m_AudioStream != null)
        {
            m_AudioStream.audioStream.Position = 0;
            m_Player = new WaveLib.WaveOutPlayer(-1, m_AudioStream.waveFormat, TOTAL_BUFFER_SIZE, 3, new WaveLib.BufferFillEventHandler(Filler));
        }
        watch.Stop();

        watch = new StopWatch("Played audio for");
        DateTime timeStarted = DateTime.Now;
        while ((DateTime.Now - timeStarted).TotalMilliseconds < durationMS)
        {
            System.Threading.Thread.Sleep(1);
        }
        watch.Stop();

        Stop();

        onDone?.Invoke();
    }

    private static void Filler(IntPtr data, int size)
    {
        byte[] b = new byte[size];
        Stream m_AudioStream = m_SelectedAudioStream.audioStream;
        if (m_AudioStream != null)
        {
            int pos = 0;
            while (pos < size)
            {
                int toget = size - pos;
                int got = m_AudioStream.Read(b, pos, toget);
                if (got < toget)
                {
                    m_AudioStream.Position = 0; // loop if the file ends
                    //if (!loop)
                    //    Stop();
                }
                pos += got;
            }
        }
        else
        {
            for (int i = 0; i < b.Length; i++)
                b[i] = 0;
        }
        System.Runtime.InteropServices.Marshal.Copy(b, 0, data, size);
    }

}

public class PlayerStream
{
    public Stream audioStream;
    public WaveLib.WaveFormat waveFormat;

    public string filePath;

    public PlayerStream(string filePath)
    {
        this.filePath = filePath;
        WaveLib.WaveStream S = new WaveLib.WaveStream(filePath);
        if (S.Length <= 0)
            throw new Exception("Invalid WAV file");
        waveFormat = S.Format;
        if (waveFormat.wFormatTag != (short)WaveLib.WaveFormats.Pcm && waveFormat.wFormatTag != (short)WaveLib.WaveFormats.Float)
            throw new Exception("Olny PCM files are supported");

        if (S == null)
            throw new Exception("What?");

        audioStream = S;
    }

    public void Dispose()
    {
        StopWatch watch = new StopWatch("Closing File");
        if (audioStream != null)
        {
            try
            {
                audioStream.Close();
            }
            finally
            {
                audioStream = null;
            }
        }
        watch.Stop();
    }
}
public class StopWatch
{
    private static bool DEBUG = true;

    public DateTime timeStarted { get; private set; }
    private string actionName;

    public StopWatch(string actionName)
    {
        this.actionName = actionName;
        timeStarted = DateTime.Now;
    }

    public void Stop()
    {
        if (!DEBUG) return;

        string log = actionName + " | Done in:{0}";
        double doneIn = (DateTime.Now - timeStarted).TotalSeconds;
        // Debug.Log(string.Format(log, doneIn.ToString("0.000")));
    }

}