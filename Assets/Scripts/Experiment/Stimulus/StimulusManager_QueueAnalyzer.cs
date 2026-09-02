// NS_REMOVE | Logs
using Experiment.Managers;
using Helpers.Engine;
using System.Collections.Generic;
using System.Linq;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Stimulus
{
    public class StimulusManager_QueueAnalyzer : MonoBehaviour
    {
        public static void AnalyzeQueue(List<SpriteLocation> queue, string fileName)
        {
            return;
            string log = "Analyzing Queue\r\n";
            int logCount = 0;

            List<SpriteLocation> queue_Probed = queue.FindAll(a => a.DEBUG_IS_PROBED);
            List<SpriteLocation> queue_Unprobed = queue.FindAll(a => !a.DEBUG_IS_PROBED);
            List<SpriteLocation> queue_W0 = queue.FindAll(a => a.DEBUG_WORLD_ID == 0);
            List<SpriteLocation> queue_W1 = queue.FindAll(a => a.DEBUG_WORLD_ID == 1);
            List<SpriteLocation> queue_Probed_W0 = queue_W0.FindAll(a => a.DEBUG_IS_PROBED);
            List<SpriteLocation> queue_Probed_W1 = queue_W1.FindAll(a => a.DEBUG_IS_PROBED);
            List<SpriteLocation> queue_Unprobed_W0 = queue_W0.FindAll(a => !a.DEBUG_IS_PROBED);
            List<SpriteLocation> queue_Unprobed_W1 = queue_W1.FindAll(a => !a.DEBUG_IS_PROBED);

            // ==== 1
            log += "\r\n1w1_p   1: Probed, world: 1 : 50 stim in total. 20 faces, 20 objects, 10 blanks";

            int probedW0 = queue_Probed_W0.Count;
            int probedW0_F = queue_Probed_W0.FindAll(a => a.type == StimulusType.Face).Count;
            int probedW0_O = queue_Probed_W0.FindAll(a => a.type == StimulusType.Object).Count;
            int probedW0_B = queue_Probed_W0.FindAll(a => a.type == StimulusType.None).Count;

            logCount = log.Length;
            if (probedW0 != 50) log += "\r\nprobedW0 != 50 ({0})"._Format(probedW0);
            if (probedW0_F != 20) log += "\r\nprobedW0_F != 20 ({0})"._Format(probedW0_F);
            if (probedW0_O != 20) log += "\r\nprobedW0_O != 20 ({0})"._Format(probedW0_O);
            if (probedW0_B != 10) log += "\r\nprobedW0_B != 10 ({0})"._Format(probedW0_B);
            if (logCount == log.Length) log += "\r\nAll Good!";

            // 2w1_p   2: Probed, world: 1 : Each individual face and object must appear exactly twice, once in a left location, once in a right location.

            // 3w1_p   3: Probed, world: 1 : Face stimuli and object stimuli must appear in each of the 4 locations exactly 5 times.This means, for each of the 4 locations, there should be 5 face stimuli and 5 object stimuli in total, regardless of the stim ID.

            // 4w1_p   4.Probed, world: 1 : Blanks must appear in each of the 4 locations either 2 or 3 times.

            // 5w1_u   5.Unprobed, world: 1 : 100 stimuli in total. 40 faces, 40 objects, 20 blanks.

            // 6w1_u   6.Unprobed, world: 1 : Each individual face and object must appear exactly 4 times, once in each of the 4 locations.

            // 7w1_u   7.Unprobed, world: 1 : Blanks must appear in each of the 4 locations exactly 5 times.

            // 8w1_all 8.All stimuli, world: 1 : 60 faces, 60 objects, 30 blanks

            // 9w1_all 9.All stimuli, world: 1 : Each individual face and object must appear exactly 6 times, 3 in a left location, 3 in a right location.

            // 10w1_all    10.All stimuli, world: 1 : Faces and objects must appear in each of the 4 locations exactly 15 times.

            // 11w1_all    11.All stimuli, world: 1 : Blanks must appear in each of the 4 locations 7 or 8 times.

            // 1w2_p   1: Probed, world: 2 : 50 stim in total. 20 faces, 20 objects, 10 blanks

            // 2w2_p   2: Probed, world: 2 : Each individual face and object must appear exactly twice, once in a left location, once in a right location.

            // 3w2_p   3: Probed, world: 1 : Face stimuli and object stimuli must appear in each of the 4 locations exactly 5 times.This means, for each of the 4 locations, there should be 5 face stimuli and 5 object stimuli in total, regardless of the stim ID.

            // 4w2_p   4.Probed, world: 1 : Blanks must appear in each of the 4 locations either 2 or 3 times.

            // 5w2_u   5.Unprobed, world: 1 : 100 stimuli in total. 40 faces, 40 objects, 20 blanks.

            // 6w2_u   6.Unprobed, world: 1 : Each individual face and object must appear exactly 4 times, once in each of the 4 locations.

            // 7w2_u   7.Unprobed, world: 1 : Blanks must appear in each of the 4 locations exactly 5 times.

            // 8w2_all 8.All stimuli, world: 1 : 60 faces, 60 objects, 30 blanks

            // 9w2_all 9.All stimuli, world: 1 : Each individual face and object must appear exactly 6 times, 3 in a left location, 3 in a right location.

            // 10w2_all    10.All stimuli, world: 1 : Faces and objects must appear in each of the 4 locations exactly 15 times.

            // 11w2_all    11.All stimuli, world: 1 : Blanks must appear in each of the 4 locations 7 or 8 times.

            // 12tot_p 12.Probed stimuli, across 2 worlds. 100 stim in total. 40 faves, 40 objects, 20 blanks.

            // 13tot_p 13.Probed stimuli, across 2 worlds.Each individual face and object must appear exactly 4 times, once in each of the 4 locations(so if an individual face appeared on the top right and bottom left in world 1, it must appear in the bottom right and top left locations in world 2).

            // 14tot_p 14.Probed stimuli, across 2 worlds: Blanks must appear in each of the 4 locations exactly 5 times.

            // 15tot_u 15.Unprobed stimuli across 2 worlds: 200 stim in total. 80 faces, 80 objects, 40 blanks.

            // 16tot_u 16.Unprobed stimuli, across 2 worlds: Each individual face and object must appear exactly 8 times, twice in each of the 4 locations. (follows from 6).

            // 17tot_all   17.All stimuli, across 2 worlds: 300 stimuli total: 120 faces, 120 objects, 60 blanks

            // 18tot_all   18.All stimuli, across 2 worlds: Each individual face and object must appear exactly 12 times, three times in each of the 4 locations.

            // 19tot_all   19.All stimuli, across 2 worlds: Blanks must appear in each of the 4 locations exactly 15 times..

            // 20r_I   20.Replay constraint I. For each replay level, there should be 25 stimuli, 10 objects, 10 faces and 5 blanks. 8(whereof 2 blanks) should be probed and 17(whereof 3 blanks) unprobed

            // 21r_II  21.Replay constraint II. Location & category.For each replay level, half of the stimuli should appear to the left, half to the right. More specifically, 5 faces(objects) to the left, 5 to the right, 2 - 3 blanks to the left/ right

            // 22r_III 22.Replay constraint III(bonus). Relevance & Category.For each replay level, 3(7) of the(un)probed should be faces, 3(7) should be objects and 2(3) blanks.

            // 23r_IV  23.Replay constraint IV(bonus). Location & Relevance.For each replay level, half the probed(unprobed) should be in a left vs right location.Or more specifically, 3(7) of the probed faces/ objects should be to the right and 3(7) to the left, and 1(1 / 2) of the blanks should be to the right and 1(2 / 1) to the left.

            // 24r_all 24.Replay additional constraint I.Across all replay levels: the blanks appear in each locations equally many times(5).This also ensures that replay constraint II is perfectly fulfilled across layers.

            // 25r_all 25.Replay additional constraint II.Across all replay levels: the(un)probed blanks appear in each locations equally many times, 1(4) 2(3)

            // 26r_lev 26.Replay additional constraint III.Within each level, for the blanks, the location that is repeated is always one probe, one unprobe

            // 27r_lev 27.Replay additional constraint IV.Within each level: At each specific location(both up / down & right / left specified), 2 / 3 faces, 3 / 2 objects and 1 - 2 blanks will be shown.

            // 28r_lev 28.Replay additional constraint V.Within each level: At each specific location(both up / down & right / left specified), a probe will be shown 1 - 2 times.

        }

        public static void AnalyzeQueue_OLD(List<SpriteLocation> queue, string fileName)
        {
            return;

            float timeStarted = TimeWrapper.realtimeSinceStartup_NotTS;

            // Distribution of Locations (C1)
            Dictionary<Direction_2D_Diagonal, int> num_L = new Dictionary<Direction_2D_Diagonal, int>();

            // Distribution of Stimuli (C2)
            Dictionary<Sprite, int> num_S = new Dictionary<Sprite, int>();

            // Distribution of Location-Stimuli Pairs (C3)
            Dictionary<SpriteLocation, int> num_SL = new Dictionary<SpriteLocation, int>();

            // Distribution of Types (C4)
            Dictionary<StimulusType, int> num_T = new Dictionary<StimulusType, int>();

            // Distribution of Type-Location Pairs (C5)
            Dictionary<TypeLocation, int> num_TL = new Dictionary<TypeLocation, int>();

            // Loop the queue, count instances
            foreach (SpriteLocation sL in queue)
            {
                // Count Types (C1)
                StimulusType t = sL.type;
                num_T.AddOrUpdate(t, num_T.TryGet(t, 0) + 1);

                // Count Locations (C2)
                Direction_2D_Diagonal l = sL.direction;
                num_L.AddOrUpdate(l, num_L.TryGet(l, 0) + 1);

                // Count Stimuli (C3)
                Sprite s = sL.sprite;
                if (s != null) // Don't Log Nulls (this can be drawn from Type
                    num_S.AddOrUpdate(s, num_S.TryGet(s, 0) + 1);

                // Count Type-Location Pairs (C4)
                TypeLocation tL = sL.typeLocation;
                num_TL.AddOrUpdate(tL, num_TL.TryGet(tL, 0) + 1);

                // Count Location-Stimuli Pairs (C5)
                if (s != null)
                    num_SL.AddOrUpdate(sL, num_SL.TryGet(sL, 0) + 1);
            }

            string analysis_T = num_T.ToReadableString();
            if ((fileName.Contains("game") && (
                num_T.TryGet(StimulusType.Face) != 80 ||
                num_T.TryGet(StimulusType.Object) != 80 ||
                num_T.TryGet(StimulusType.None) != 40)) ||
                (!fileName.Contains("game") && (
                num_T.TryGet(StimulusType.Face) != 20 ||
                num_T.TryGet(StimulusType.Object) != 20 ||
                num_T.TryGet(StimulusType.None) != 10)))
                Debug_Helper.LogWarning(typeof(StimulusManager), "Types unevenly distributed in {0}"._Format(fileName));

            string analysis_L = num_L.ToReadableString();
            if (fileName.Contains("game"))
                if (num_L.Aggregate((l, r) => l.Value > r.Value ? l : r).Value !=  // max
                    num_L.Aggregate((l, r) => l.Value < r.Value ? l : r).Value)    // min
                    Debug_Helper.LogWarning(typeof(StimulusManager), "Locations unevenly distributed in {0}"._Format(fileName));

            string analysis_S = num_S.ToReadableString();
            if (num_S.Aggregate((l, r) => l.Value > r.Value ? l : r).Value !=  // max
                num_S.Aggregate((l, r) => l.Value < r.Value ? l : r).Value)    // min
                Debug_Helper.LogWarning(typeof(StimulusManager), "Sprites unevenly distributed in {0}"._Format(fileName));

            string analysis_TL = num_TL.ToReadableString();
            if ((fileName.Contains("game") && (
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.BottomLeft)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.BottomRight)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.TopLeft)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.TopRight)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.BottomLeft)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.BottomRight)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.TopLeft)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.TopRight)) != 20 ||
                num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.BottomLeft)) != 10 ||
                num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.BottomRight)) != 10 ||
                num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.TopLeft)) != 10 ||
                num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.TopRight)) != 10) ||
                (!fileName.Contains("game") && (
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.BottomLeft)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.BottomRight)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.TopLeft)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Face, Direction_2D_Diagonal.TopRight)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.BottomLeft)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.BottomRight)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.TopLeft)) != 5 ||
                num_TL.TryGet(new TypeLocation(StimulusType.Object, Direction_2D_Diagonal.TopRight)) != 5 ||
                !num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.BottomLeft)).IsBetween(2, 3) ||
                !num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.BottomRight)).IsBetween(2, 3) ||
                !num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.TopLeft)).IsBetween(2, 3) ||
                !num_TL.TryGet(new TypeLocation(StimulusType.None, Direction_2D_Diagonal.TopRight)).IsBetween(2, 3)))))
                Debug_Helper.LogWarning(typeof(StimulusManager), "Type-Location unevenly distributed in {0}"._Format(fileName));

            string analysis_SL = num_SL.ToReadableString();
            if (num_SL.Count < 80 ||
                (num_SL.Aggregate((l, r) => l.Value > r.Value ? l : r).Value !=  // max
                num_SL.Aggregate((l, r) => l.Value < r.Value ? l : r).Value))    // min
                Debug_Helper.LogWarning(typeof(StimulusManager), "Sprite-Location unevenly distributed in {0}"._Format(fileName));

            string analysis = "Queue Analysis Complete" +
                "\r\n\r\nType Distribution\n{0}"._Format(analysis_T) +
                "\r\n\r\nLocation Distribution\n{0}"._Format(analysis_L) +
                "\r\n\r\nSprite Distribution\n{0}"._Format(analysis_S) +
                "\r\n\r\nType-Location Distribution\n{0}"._Format(analysis_TL) +
                "\r\n\r\nSprite-Location Distribution\n{0}"._Format(analysis_SL);

            ExperimentManagerSession.LogStimulusQueueAnalysis(fileName, analysis);
            Debug_Helper.Log(typeof(StimulusManager), "Analysis complete in {0}s - check {1}"._Format((TimeWrapper.realtimeSinceStartup_NotTS - timeStarted), fileName));
        }
    }
}