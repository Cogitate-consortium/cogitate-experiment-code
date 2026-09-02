/*
 * https://www.codeproject.com/Articles/3352/A-low-level-audio-player-in-C
 */


using System.Collections.Generic;
using UnityEngine;
using TGP.Helpers;
using UnityEngine.UI;
using Experiment;
using Experiment.Subject;
using Peripherals.UserInput.HighAccu;
using ExperimentLibrary;
using Experiment.Managers;
using Helpers.Async;
using Helpers.Engine;

public class TestWavePlayer : MonoBehaviour
{
    public Image debugImage;

    public List<string> fileNames = new List<string>()
    {
        "Note.wav",
        "Powerup.wav"
    };

    public double durationMS = 70f;

    // Start is called before the first frame update
    private void Start()
    {
        debugImage.enabled = false;
        TimeWrapper.Initialize();
        EngineWrapper.Initialize();
        ExperimentManagerApplication.SOS_PREVENT_AUTO_LOAD_OF_MAIN_MENU = true;
        gameObject.AddComponentIfNotExists<ExperimentManagerApplication>();
        ExperimentManagerApplication.instance.InitializeSubjectSession(new SubjectInfo("_T", UnityEngine.Random.Range(100, 1000), "A", HandType.Both, null), a =>
        {
            HighAccuracyInput_USB highAccuracyInput_USB = new HighAccuracyInput_USB();
            highAccuracyInput_USB.onKeyDown_TS += HighAccuracyInput_USB_onKeyDown_TS;
            highAccuracyInput_USB?.TryInitialize(ExperimentLibraryManager.Config.Input.highAccuUSBConfig);
            highAccuracyInput_USB?.ToggleSleep(false);

            WavePlayerManager.BUFFER_SIZE = 2048;
            for (int i = 0; i < fileNames.Count; i++)
            {
                WavePlayerManager.LoadFile(fileNames[i], string.Format("{0}/TestAudio/{1}", Application.streamingAssetsPath, fileNames[i]));
            }

            // New Frame
            ExperimentManagerSession.LogSessionInfo("START");

            // Log that frame
            AsyncThread.RequestRunOnNewThread(() =>
            {
                while (true)
                {
                    int lastFrameCount = TimeWrapper.numRenderedFrames;
                    // Debug.Log(lastFrameCount);

                    while (lastFrameCount == TimeWrapper.numRenderedFrames)
                        System.Threading.Thread.Sleep(1);

                    // BEGINNING OF NEW FRAME (just rendered)
                    // SubjectPerformanceReport.LogSessionInfo("FRAME {0}"._Format(SubjectPerformanceReport.numRenderedFrames));
                    if (TimeWrapper.currentFrameCycleID == REQUESTED_PLAY_DURING_CYCLE + 1)
                    {
                        ExperimentManagerSession.LogSessionInfo("PHOTODIODE_ON_SCREEN {0}"._Format(TimeWrapper.currentFrameCycleID));
                        REQUESTED_PLAY_DURING_CYCLE = -1;
                    }
                }
            });
        }, "", true);

    }

    private void HighAccuracyInput_USB_onKeyDown_TS(object sender, HighAccuracyInput_Base.HighAccuracyEventArgs e)
    {
        if (e.key == PlayKeyCode)
        {
            Play();
        }
    }

    private KeyCode PlayKeyCode = KeyCode.P;

    private void LateUpdate()
    {
        if (REQUESTED_PLAY_DURING_CYCLE == -1)
        {
            debugImage.enabled = false;
        }

        // On the same frame that P was pressed
        if (Input.GetKeyDown(PlayKeyCode))
        {
            // Flip a Frame
            debugImage.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Stop();
        }
    }

    private void OnDestroy()
    {
        WavePlayerManager.Dispose();
    }

    [ContextMenu("Stop")]
    private void Stop()
    {
        WavePlayerManager.Stop();
    }

    int REQUESTED_PLAY_DURING_CYCLE = -1;

    private void Play()
    {
        REQUESTED_PLAY_DURING_CYCLE = TimeWrapper.currentFrameCycleID;
        int request_FrameCycleID = TimeWrapper.currentFrameCycleID;
        double request_TimestampMS = TimeWrapper.currentTimestampMS;

        WavePlayerManager.TryPlay_TS(fileNames[TimeWrapper.numRenderedFrames % 2], durationMS);

        // New Frame
        ExperimentManagerSession.LogSessionInfo("AUDIO_REQUESTED {0} {1} {2}".
            _Format(request_FrameCycleID, request_TimestampMS.ToString("#"),
            (TimeWrapper.currentTimestampMS - request_TimestampMS).ToString("#")));
    }
}
