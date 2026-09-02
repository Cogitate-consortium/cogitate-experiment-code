using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Stimulus
{
    public class StimulusManager_QueueGenerator_Helper_MEEG_ECOG : MonoBehaviour
    {

        #region World 1
        /*
         * Replay level 1											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLbO, uRtF	pLtO, uRbF	pLbF, uRbO	pLtF, uRtO	uLbO, uRtF	uLtO, uRbF	uLbF, pRbO	uLbF, pRtO	uLbO, pRbF	uLtF, uRtO	pLt, uLb, uRt, pRb, uRb
         */
        public static readonly List<Option> optionsLocalizerW1L1 = new List<Option>()
                {
                    new Option(1,   "uLbO"), new Option(1,  "uRtF"),
                    new Option(2,   "pLtO"), new Option(2,  "uRbF"),
                    new Option(3,   "pLbF"), new Option(3,  "uRbO"),
                    new Option(4,   "pLtF"), new Option(4,  "uRtO"),
                    new Option(5,   "uLbO"), new Option(5,  "uRtF"),
                    new Option(6,   "uLtO"), new Option(6,  "uRbF"),
                    new Option(7,   "uLbF"), new Option(7,  "pRbO"),
                    new Option(8,   "uLbF"), new Option(8,  "pRtO"),
                    new Option(9,   "uLbO"), new Option(9,  "pRbF"),
                    new Option(10,  "uLtF"), new Option(10, "uRtO"),
                    new Option("pLt"), new Option("uLb"), new Option("uRt"), new Option("pRb"), new Option("uRb")
                };

        /*
         * Replay level 2											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLbF, uRtO	pLtF, uRbO	pLbO, uRbF	pLtO, uRtF	uLbF, uRtO	uLtF, uRbO	uLbO, pRbF	uLbO, pRtF	uLbF, pRbO	uLtO, uRtF	uLt, pLb, pRt, uRb, uRt
         */
        public static readonly List<Option> optionsLocalizerW1L2 = new List<Option>()
                {
                    new Option(1,   "uLbF"), new Option(1,  "uRtO"),
                    new Option(2,   "pLtF"), new Option(2,  "uRbO"),
                    new Option(3,   "pLbO"), new Option(3,  "uRbF"),
                    new Option(4,   "pLtO"), new Option(4,  "uRtF"),
                    new Option(5,   "uLbF"), new Option(5,  "uRtO"),
                    new Option(6,   "uLtF"), new Option(6,  "uRbO"),
                    new Option(7,   "uLbO"), new Option(7,  "pRbF"),
                    new Option(8,   "uLbO"), new Option(8,  "pRtF"),
                    new Option(9,   "uLbF"), new Option(9,  "pRbO"),
                    new Option(10,  "uLtO"), new Option(10, "uRtF"),
                    new Option("uLt"), new Option("pLb"), new Option("pRt"), new Option("uRb"), new Option("uRt")
                };

        /*
         * 	world 1			
                    Left bottom	        Right bottom	    Left top	        Right top
        Version 1	1,3,5,7,9 (set 1)	7,9,2,4,6 (set 2)	2,4,6,8,10 (set 3)	1,3,5,8,10 (set 4)
        Blanks	    2	                2	                3	                3

         */
        public static readonly List<Option> optionsGameplayW1_Probed = new List<Option>()
                {
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	world 1			
                    Left bottom	            Right bottom	        Left top	            Right top
        Version 1	1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10
        Blanks	    5                       5                       5                       5

         */
        public static readonly List<Option> optionsGameplayW1_Unprobed = new List<Option>()
                {
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        #endregion


        #region World 2
        /*
         * Replay level 1											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLtF, pRbO	uLbF, uRtO	pLtO, uRtF	uLbO, uRbF	uLtF, uRbO	pLbF, uRtO	pLtO, pRtF	uLtO, uRbF	uLtF, uRtO	uLbO, pRbF	uLt, pLb, uRt, pRb, uLb
         */
        public static readonly List<Option> optionsLocalizerW2L1 = new List<Option>()
                {
                    new Option(1,   "uLtF"), new Option(1,  "pRbO"),
                    new Option(2,   "uLbF"), new Option(2,  "uRtO"),
                    new Option(3,   "pLtO"), new Option(3,  "uRtF"),
                    new Option(4,   "uLbO"), new Option(4,  "uRbF"),
                    new Option(5,   "uLtF"), new Option(5,  "uRbO"),
                    new Option(6,   "pLbF"), new Option(6,  "uRtO"),
                    new Option(7,   "pLtO"), new Option(7,  "pRtF"),
                    new Option(8,   "uLtO"), new Option(8,  "uRbF"),
                    new Option(9,   "uLtF"), new Option(9,  "uRtO"),
                    new Option(10,  "uLbO"), new Option(10, "pRbF"),
                    new Option("uLt"), new Option("pLb"), new Option("uRt"), new Option("pRb"), new Option("uLb")
                };

        /*
         * Replay level 2											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLtO, pRbF	uLbO, uRtF	pLtF, uRtO	uLbF, uRbO	uLtO, uRbF	pLbO, uRtF	pLtF, pRtO	uLtF, uRbO	uLtO, uRtF	uLbF, pRbO	pLt, uLb, pRt, uRb, uLt
         */
        public static readonly List<Option> optionsLocalizerW2L2 = new List<Option>()
                {
                    new Option(1,   "uLtO"), new Option(1,  "pRbF"),
                    new Option(2,   "uLbO"), new Option(2,  "uRtF"),
                    new Option(3,   "pLtF"), new Option(3,  "uRtO"),
                    new Option(4,   "uLbF"), new Option(4,  "uRbO"),
                    new Option(5,   "uLtO"), new Option(5,  "uRbF"),
                    new Option(6,   "pLbO"), new Option(6,  "uRtF"),
                    new Option(7,   "pLtF"), new Option(7,  "pRtO"),
                    new Option(8,   "uLtF"), new Option(8,  "uRbO"),
                    new Option(9,   "uLtO"), new Option(9,  "uRtF"),
                    new Option(10,  "uLbF"), new Option(10, "pRbO"),
                    new Option("pLt"), new Option("uLb"), new Option("pRt"), new Option("uRb"), new Option("uLt")
                };

        /*
         * 	world 2			
                    Left bottom	        Right bottom	    Left top	        Right top
        Version 1	2,4,6,8,10 (set 3)	1,3,5,8,10 (set 4)	1,3,5,7,9 (set 1)	7,9,2,4,6 (set 2)
        Blanks	    3	                3	                2	                2

         */
        public static readonly List<Option> optionsGameplayW2_Probed = new List<Option>()
                {
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	world 2		
                    Left bottom	            Right bottom	        Left top	            Right top
        Version 1	1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10
        Blanks	    5                       5                       5                       5

         */
        public static readonly List<Option> optionsGameplayW2_Unprobed = new List<Option>()
                {
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        #endregion


        #region World 3
        /*
         * Replay level 1											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLbO, uRtF	pLtF, uRbO	uLtO, pRtF	uLbF, uRbO	uLbO, uRbF	uLbF, uRtO	pLtO, uRbF	uLtF, pRbO	uLbO, pRtF	uLtF, uRtO	pLt, uLb, uRt, pRb, uRb
         */
        public static readonly List<Option> optionsLocalizerW3L1 = new List<Option>()
                {
                    new Option(1,   "pLbO"), new Option(1,  "uRtF"),
                    new Option(2,   "pLtF"), new Option(2,  "uRbO"),
                    new Option(3,   "uLtO"), new Option(3,  "pRtF"),
                    new Option(4,   "uLbF"), new Option(4,  "uRbO"),
                    new Option(5,   "uLbO"), new Option(5,  "uRbF"),
                    new Option(6,   "uLbF"), new Option(6,  "uRtO"),
                    new Option(7,   "pLtO"), new Option(7,  "uRbF"),
                    new Option(8,   "uLtF"), new Option(8,  "pRbO"),
                    new Option(9,   "uLbO"), new Option(9,  "pRtF"),
                    new Option(10,  "uLtF"), new Option(10, "uRtO"),
                    new Option("pLt"), new Option("uLb"), new Option("uRt"), new Option("pRb"), new Option("uRb")
                };

        /*
         * Replay level 2											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLbF, uRtO	pLtO, uRbF	uLtF, pRtO	uLbO, uRbF	uLbF, uRbO	uLbO, uRtF	pLtF, uRbO	uLtO, pRbF	uLbF, pRtO	uLtO, uRtF	uLt, pLb, pRt, uRb, uRt
         */
        public static readonly List<Option> optionsLocalizerW3L2 = new List<Option>()
                {
                    new Option(1,   "pLbF"), new Option(1,  "uRtO"),
                    new Option(2,   "pLtO"), new Option(2,  "uRbF"),
                    new Option(3,   "uLtF"), new Option(3,  "pRtO"),
                    new Option(4,   "uLbO"), new Option(4,  "uRbF"),
                    new Option(5,   "uLbF"), new Option(5,  "uRbO"),
                    new Option(6,   "uLbO"), new Option(6,  "uRtF"),
                    new Option(7,   "pLtF"), new Option(7,  "uRbO"),
                    new Option(8,   "uLtO"), new Option(8,  "pRbF"),
                    new Option(9,   "uLbF"), new Option(9,  "pRtO"),
                    new Option(10,  "uLtO"), new Option(10, "uRtF"),
                    new Option("uLt"), new Option("pLb"), new Option("pRt"), new Option("uRb"), new Option("uRt")
                };

        /*
         * 	world 3		
                    Left bottom	        Right bottom	    Left top	        Right top
        Version 2	1,3,5,8,10 (set 4)	2,4,6,8,10 (set 3)	7,9,2,4,6 (set 2)	1,3,5,7,9 (set 1)
        Blanks	    3	                3	                2	                2
         */
        public static readonly List<Option> optionsGameplayW3_Probed = new List<Option>()
                {
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	world 3	
                    Left bottom	            Right bottom	        Left top	            Right top
        Version 1	1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10
        Blanks	    5                       5                       5                       5

         */
        public static readonly List<Option> optionsGameplayW3_Unprobed = new List<Option>()
                {
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        #endregion


        #region World 4
        /*
         * Replay level 1											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLtF, uRbO	pLbO, uRtF	uLbF, pRbO	uLtO, uRtF	pLtF, uRtO	uLtO, uRbF	uLbF, uRtO	uLbO, pRtF	uLtF, pRbO	uLbO, uRbF	uLt, pLb, uRt, pRb, uLb
         */
        public static readonly List<Option> optionsLocalizerW4L1 = new List<Option>()
                {
                    new Option(1,   "pLtF"), new Option(1,  "uRbO"),
                    new Option(2,   "pLbO"), new Option(2,  "uRtF"),
                    new Option(3,   "uLbF"), new Option(3,  "pRbO"),
                    new Option(4,   "uLtO"), new Option(4,  "uRtF"),
                    new Option(5,   "pLtF"), new Option(5,  "uRtO"),
                    new Option(6,   "uLtO"), new Option(6,  "uRbF"),
                    new Option(7,   "uLbF"), new Option(7,  "uRtO"),
                    new Option(8,   "uLbO"), new Option(8,  "pRtF"),
                    new Option(9,   "uLtF"), new Option(9,  "pRbO"),
                    new Option(10,  "uLbO"), new Option(10, "uRbF"),
                    new Option("uLt"), new Option("pLb"), new Option("uRt"), new Option("pRb"), new Option("uLb")
                };

        /*
         * Replay level 2											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLtO, uRbF	pLbF, uRtO	uLbO, pRbF	uLtF, uRtO	pLtO, uRtF	uLtF, uRbO	uLbO, uRtF	uLbF, pRtO	uLtO, pRbF	uLbF, uRbO	pLt, uLb, pRt, uRb, uLt
         */
        public static readonly List<Option> optionsLocalizerW4L2 = new List<Option>()
                {
                    new Option(1,   "pLtO"), new Option(1,  "uRbF"),
                    new Option(2,   "pLbF"), new Option(2,  "uRtO"),
                    new Option(3,   "uLbO"), new Option(3,  "pRbF"),
                    new Option(4,   "uLtF"), new Option(4,  "uRtO"),
                    new Option(5,   "pLtO"), new Option(5,  "uRtF"),
                    new Option(6,   "uLtF"), new Option(6,  "uRbO"),
                    new Option(7,   "uLbO"), new Option(7,  "uRtF"),
                    new Option(8,   "uLbF"), new Option(8,  "pRtO"),
                    new Option(9,   "uLtO"), new Option(9,  "pRbF"),
                    new Option(10,  "uLbF"), new Option(10, "uRbO"),
                    new Option("pLt"), new Option("uLb"), new Option("pRt"), new Option("uRb"), new Option("uLt")
                };

        /*
         * 	world 2			
                    Left bottom	        Right bottom	    Left top	        Right top
        Version 2	7,9,2,4,6 (set 2)	1,3,5,7,9 (set 1)	1,3,5,8,10 (set 4)	2,4,6,8,10 (set 3)
        Blanks	    2	                2	                3	                3

         */
        public static readonly List<Option> optionsGameplayW4_Probed = new List<Option>()
                {
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),

                };

        /*
         * 	world 2		
                    Left bottom	            Right bottom	        Left top	            Right top
        Version 1	1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10    1,2,3,4,5,6,7,8,9,10
        Blanks	    5                       5                       5                       5

         */
        public static readonly List<Option> optionsGameplayW4_Unprobed = new List<Option>()
                {
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        #endregion

    }
}