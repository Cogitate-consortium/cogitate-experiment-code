using System;
using Peripherals.Audio;

namespace Game.Systems.Misc
{
    [Serializable]
    public class GameAudioConfig
    {
        public bool doReplaySounds = true;  //  ApplicationLibrary.Config.Audio.localizerSounds_Gameplay
        public bool playSFX = true;  //  ApplicationLibrary.Config.Audio.doSFX

        // --- GAME
        public SoundSystem.AudioClipConfig GameplayReward;
        public SoundSystem.AudioClipConfig GameplayPenalty;
        public SoundSystem.AudioClipConfig GameStart;
        public SoundSystem.AudioClipConfig GameOver;
        public SoundSystem.AudioClipConfig Progress;
    }
}