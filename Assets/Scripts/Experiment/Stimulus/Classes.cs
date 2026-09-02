using Experiment.Background;
using System;
using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Stimulus
{

    [Serializable]
    public class StimulusTimingsConfig
    {
        public enum Type { Uniform = 0, TruncExp = 1 }
        public string type_Comment = "Uniform = 0, TruncExp = 1";
        public Type type = Type.Uniform;

        public float uniform_backgroundToStimulusRatio = 3;
        public TruncatedExpConfig truncExp_Config = new TruncatedExpConfig(0.6f, 5.3333f, 8f, 100);
        private Dictionary<float, float> truncExp_Distribution = new Dictionary<float, float>();

        public StimulusTimingsConfig(float uniform_backgroundToStimulusRatio)
        {
            type = Type.Uniform;
            this.uniform_backgroundToStimulusRatio = uniform_backgroundToStimulusRatio;
        }

        public StimulusTimingsConfig(TruncatedExpConfig truncExp_Config)
        {
            type = Type.TruncExp;
            this.truncExp_Config = truncExp_Config;
        }

        public MinMax GetPeriodMinMax(MinMax minMax_Background)
        {
            if (type == Type.Uniform) return minMax_Background * uniform_backgroundToStimulusRatio;
            if (type == Type.TruncExp) return truncExp_Config.GetMinMax();

            return default(MinMax);
        }

        public float GetRandom(MinMax minMax_Background)
        {
            if (type == Type.Uniform) return Utility_Helper.RandomRange(GetPeriodMinMax(minMax_Background));
            if (type == Type.TruncExp)
            {
                // Cache the distribution
                if (truncExp_Distribution.Count == 0)
                    truncExp_Distribution = Math_Helper.TruncExp_GetCumProb(truncExp_Config);

                return Math_Helper.QueryCDF(truncExp_Distribution);
            }

            return -1;
        }
    }

    [System.Serializable]
    public enum StimulusType { None, Object, Face }

    [System.Serializable]
    public struct SpriteLocation
    {
        public StimulusType type { get { return typeLocation.type; } }
        public Direction_2D_Diagonal direction { get { return typeLocation.direction; } }

        public string spriteName;
        public Sprite sprite;
        public TypeLocation typeLocation;

        public int DEBUG_WORLD_ID;
        public int DEBUG_LEVEL_ID_WITHIN_WORLD;
        public bool DEBUG_IS_PROBED;
        public int DEBUG_REPLAY_ID_WITHIN_WORLD;

        public SpriteLocation(Sprite sprite, Direction_2D_Diagonal direction, string spriteName, StimulusType type)
        {
            this.sprite = sprite;
            this.spriteName = spriteName.IsNullOrEmpty() ? StimulusManager.EMPTY_STIM_NAME : spriteName;
            typeLocation = new TypeLocation(type, direction);
            DEBUG_WORLD_ID = -1;
            DEBUG_LEVEL_ID_WITHIN_WORLD = -1;
            DEBUG_IS_PROBED = false;
            DEBUG_REPLAY_ID_WITHIN_WORLD = -1;
        }
        
        public override bool Equals(object obj)
        {
            SpriteLocation other = (SpriteLocation)obj;
            return other.sprite == sprite && other.direction == direction;
        }

        public override string ToString()
        {
            return "({0}, {1}, {2}, {3}, {4}, {5})"._Format(spriteName, type, direction, DEBUG_IS_PROBED ? "[P]" : "[U]", "[R] " + DEBUG_REPLAY_ID_WITHIN_WORLD, "[L] " + DEBUG_LEVEL_ID_WITHIN_WORLD);
        }

        public override int GetHashCode()
        {
            var hashCode = 557527014;
            hashCode = hashCode * -1521134295 + EqualityComparer<Sprite>.Default.GetHashCode(sprite);
            hashCode = hashCode * -1521134295 + direction.GetHashCode();
            return hashCode;
        }

        internal int GetSimilarity(SpriteLocation sL)
        {
            int what = 1;
            int where = 1;

            // Same Sprite ?
            if (sL.spriteName == spriteName && sL.type != StimulusType.None)
                what = 60;
            // Same Type ?
            else if (sL.type == type)
                what = sL.type == StimulusType.None ? 15 : 6;

            // Same Direction ?
            if (sL.direction == direction)
                where = 10;
            else if (sL.direction.ToLeftRight() == direction.ToLeftRight())
                where = 5;

            return what * where;
        }

        internal int GetSimilarity(List<SpriteLocation> list)
        {
            /*
            SpriteLocation self = this;
            int numSameID = list.FindAll(sL => sL.Equals(self)).Count;
            int numSameSprite = list.FindAll(sL => sL.spriteName == self.spriteName).Count;
            int numSameType = list.FindAll(sL => sL.type == self.type).Count;
            int numSameDirection = list.FindAll(sL => sL.direction == sL.direction).Count;
            int numSameLR = list.FindAll(sL => sL.direction.ToLeftRight() == sL.direction.ToLeftRight()).Count;

            return 
                numSameID * 10 +
                (numSameSprite - )
            */

            int total = 0;
            foreach (SpriteLocation sL in list)
                total += GetSimilarity(sL);


            return total;
        }
    }

    public struct TypeLocation
    {
        public StimulusType type;
        public Direction_2D_Diagonal direction;

        public TypeLocation(StimulusType type, Direction_2D_Diagonal direction)
        {
            this.type = type;
            this.direction = direction;
        }

        public override bool Equals(object obj)
        {
            TypeLocation other = (TypeLocation)obj;
            return other.type.Equals(type) && other.direction.Equals(direction);
        }

        public override int GetHashCode()
        {
            var hashCode = -511509057;
            hashCode = hashCode * -1521134295 + type.GetHashCode();
            hashCode = hashCode * -1521134295 + direction.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return "({0}, {1})"._Format(type, direction);
        }
    }

    public class StimulusEventArgs : EventArgs
    {
        public int stimulusIdxCycle;
        public SpriteLocation stimulus;
        public BackgroundObject backgroundObject;
        public bool toBeProbed;
        public string worldLevelTrial;
        public bool shouldFireTriggers;
        internal string stimulusName { get { return stimulus.spriteName; } }
        internal StimulusType stimulusType { get { return stimulus.type; } }
        internal Direction_2D_Diagonal stimulusLocation { get { return stimulus.direction; } }

        public StimulusEventArgs(int stimulusIdxCycle, BackgroundObject backgroundObject, SpriteLocation stimulus, bool toBeProbed, string worldLevelTrial, bool shouldFireTriggers)
        {
            this.stimulusIdxCycle = stimulusIdxCycle;
            this.stimulus = stimulus;
            this.backgroundObject = backgroundObject;
            this.toBeProbed = toBeProbed;
            this.worldLevelTrial = worldLevelTrial;
            this.shouldFireTriggers = shouldFireTriggers;
        }
    }
}