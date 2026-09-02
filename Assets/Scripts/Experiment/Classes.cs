namespace Experiment
{
    [System.Serializable]
    public enum ModuleType { None = -1,
        MEEG = 0, ECOG = 1, FMRI_Scanner = 2,
        MEEG_Preparation = 3, ECOG_Preparation = 4, FMRI_Preparation = 5,
        MEEG_Screening = 6, ECOG_Screening = 7, FMRI_Screening = 8
    }
    public enum PrepVsFull { Prep, Full, Screen }
    public enum WritingStrategy { AllInOne = 0, PerLevel = 1, PerTwoLevels = 2, PerWorld = 3 }
    public enum AudioSourceType { MUSIC, SFX, TONE, TRIGGER }
    public enum HeatmapType { Gaze = 0, Sacade = 1 }
    public enum HandType { Both = 0, Left = 1, Right = 2 }
    public enum EventType { None = 0, Background, Stimulus, Probe, Level }

    public static class Conversions
    {
        public static PrepVsFull ToPrepVsFull(this ModuleType module)
        {
            switch (module)
            {
                case ModuleType.MEEG:
                case ModuleType.ECOG:
                case ModuleType.FMRI_Scanner:
                    return PrepVsFull.Full;

                case ModuleType.MEEG_Preparation:
                case ModuleType.ECOG_Preparation:
                case ModuleType.FMRI_Preparation:
                    return PrepVsFull.Prep;

                case ModuleType.MEEG_Screening:
                case ModuleType.ECOG_Screening:
                case ModuleType.FMRI_Screening:
                    return PrepVsFull.Screen;
            }

            return default;
        }

        public static ModuleType ToModule(this PrepVsFull prepVsFull, ModuleType module)
        {
            switch (module)
            {
                case ModuleType.MEEG:
                case ModuleType.MEEG_Preparation:
                case ModuleType.MEEG_Screening:
                    return
                        prepVsFull == PrepVsFull.Full ? ModuleType.MEEG :
                        prepVsFull == PrepVsFull.Prep ? ModuleType.MEEG_Preparation :
                        prepVsFull == PrepVsFull.Screen ? ModuleType.MEEG_Screening : default;

                case ModuleType.ECOG:
                case ModuleType.ECOG_Preparation:
                case ModuleType.ECOG_Screening:
                    return
                        prepVsFull == PrepVsFull.Full ? ModuleType.ECOG :
                        prepVsFull == PrepVsFull.Prep ? ModuleType.ECOG_Preparation :
                        prepVsFull == PrepVsFull.Screen ? ModuleType.ECOG_Screening : default;

                case ModuleType.FMRI_Scanner:
                case ModuleType.FMRI_Preparation:
                case ModuleType.FMRI_Screening:
                    return
                        prepVsFull == PrepVsFull.Full ? ModuleType.FMRI_Scanner :
                        prepVsFull == PrepVsFull.Prep ? ModuleType.FMRI_Preparation :
                        prepVsFull == PrepVsFull.Screen ? ModuleType.FMRI_Screening : default;
            }

            return default;
        }
    }
}