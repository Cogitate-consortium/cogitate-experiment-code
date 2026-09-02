using System.Collections.Generic;
using TGP.Helpers;
using UnityEngine;

namespace Experiment.Stimulus
{
    public class StimulusManager_QueueGenerator_Helper_FMRI : MonoBehaviour
    {

        #region World 1
        /*
         * Replay level 1 (Run 1)										
                        1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLbO, uRtF	pLtO, uRbF	pLbF, uRbO	pLtF, uRtO	uLbO, uRtF	uLtO, uRbF	uLbF, pRbO	uLbF, pRtO	uLbO, pRbF	uLtF, uRtO	pLt, uLb, uRt, pRb, uLb
         */
        public static readonly List<Option> optionsLocalizerW1L1 = new List<Option>()
                {
                    new Option(1,   "pLbO"), new Option(1,  "uRtF"),
                    new Option(2,   "pLtO"), new Option(2,  "uRbF"),
                    new Option(3,   "pLbF"), new Option(3,  "uRbO"),
                    new Option(4,   "pLtF"), new Option(4,  "uRtO"),
                    new Option(5,   "uLbO"), new Option(5,  "uRtF"),
                    new Option(6,   "uLtO"), new Option(6,  "pRtF"),
                    new Option(7,   "uLbF"), new Option(7,  "pRbO"),
                    new Option(8,   "uLbF"), new Option(8,  "pRtO"),
                    new Option(9,   "uLtO"), new Option(9,  "pRbF"),
                    new Option(10,  "uLtF"), new Option(10, "uRbO"),
                    new Option("pLt"), new Option("uLb"), new Option("uRt"), new Option("pRb"), new Option("uLt")
                };

        /*
         * Replay level 2											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLbF, uRtO	pLtF, uRbO	pLbO, uRbF	pLtO, uRtF	uLbF, uRtO	uLtF, uRbO	uLbO, pRbF	uLbO, pRtF	uLbF, pRbO	uLtO, uRtF	uLt, pLb, pRt, uRb, uRt
         */
        public static readonly List<Option> optionsLocalizerW1L2 = new List<Option>()
                {
                    new Option(1,   "uLtF"), new Option(1,  "uRtO"),
                    new Option(2,   "pLtF"), new Option(2,  "uRbO"),
                    new Option(3,   "pLbO"), new Option(3,  "uRbF"),
                    new Option(4,   "pLtO"), new Option(4,  "uRtF"),
                    new Option(5,   "pLbF"), new Option(5,  "uRtO"),
                    new Option(6,   "uLtF"), new Option(6,  "pRtO"),
                    new Option(7,   "uLbO"), new Option(7,  "pRbF"),
                    new Option(8,   "uLbO"), new Option(8,  "pRtF"),
                    new Option(9,   "uLbF"), new Option(9,  "pRbO"),
                    new Option(10,  "uLtO"), new Option(10, "uRbF"),
                    new Option("uLt"), new Option("pLb"), new Option("pRt"), new Option("uRb"), new Option("uRt")
                };

        /*
         * 	World 1	(probed)	 	REPLAY RUN
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 1 (Faces)	3,5,10	            9,7	                4,2	                8,1,6
        Version 1 (Objects) 3,1	                7,9,6	            2,4,10	            8,5
        Version 1 (Blanks)	1	                1	                2	                1

         */
        public static readonly List<Option> optionsGameplayW1_ReplayRun_Probed = new List<Option>()
                {
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	World 1	(Unprobed)		REPLAY RUN
                                 Left bottom	            Right bottom	        Left top	            Right top
        Version 1 (Faces)	        7,8,1,5,9	            2,6,3,8,9	            10,6,2,3,4	            1,5,4,10,7
        Version 1 (Objects)         1,5,9,7,8	            3,2,6,8,9	            6,10,2,3,4	            4,10,1,5,7
        Version 1 (Blank)           3	                    2	                    2	                    3


         */
        public static readonly List<Option> optionsGameplayW1_ReplayRun_Unprobed = new List<Option>()
                {
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	World 1	(probed)		NON REPLAY RUN
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 1 (Faces)	1,2	                3,4,5	            6,7,8	            9,10
        Version 1 (Objects) 9,8,6	            7,10	            5,4	                3,2,1

        Version 1 (Blanks)	1	                1	                1	                2

         */
        public static readonly List<Option> optionsGameplayW1_NonReplayRun_Probed = new List<Option>()
                {
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	World 1	(unprobed)		NON REPLAY RUN
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 1 (Faces)	1,2,3,4,5	        2,4,6,8,10	        6,7,8,9,10	        1,3,5,7,9
        Version 1 (Objects) 10,9,8,7,6	        2,5,8,1,4	        5,4,3,2,1	        3,6,7,9,10
        Version 1 (Blanks)	2	                3	                3	                2

         */
        public static readonly List<Option> optionsGameplayW1_NonReplayRun_Unprobed = new List<Option>()
                {
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        #endregion


        #region World 2
        /*
         * Replay level 1											
                         1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLtF, pRbO	uLbF, uRtO	pLtO, uRtF	uLbO, uRbF	uLtF, uRbO	pLbF, uRtO	uLtO, pRtF	pLtO, uRbF	uLtF, uRtO	uLbO, pRbF	uRb, pLb, uRt, pRb, uLb
         */
        public static readonly List<Option> optionsLocalizerW2L1 = new List<Option>()
                {
                    new Option(1,   "uLtF"), new Option(1,  "pRbO"),
                    new Option(2,   "uLbO"), new Option(2,  "pRtF"),
                    new Option(3,   "pLtO"), new Option(3,  "uRtF"),
                    new Option(4,   "uLtF"), new Option(4,  "pRtO"),
                    new Option(5,   "uRtO"), new Option(5,  "uLbF"),
                    new Option(6,   "pLbO"), new Option(6,  "uRbF"),
                    new Option(7,   "uLtO"), new Option(7,  "uRtF"),
                    new Option(8,   "pLtF"), new Option(8,  "uRbO"),
                    new Option(9,   "pLbF"), new Option(9,  "uRbO"),
                    new Option(10,  "uLbO"), new Option(10, "pRbF"),
                    new Option("uRb"), new Option("pLb"), new Option("uRt"), new Option("pRb"), new Option("uLt")
                };

        /*
         * Replay level 2											
                         1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 1	uLtO, pRbF	uLbO, uRtF	pLtF, uRtO	uLbF, uRbO	uLtO, uRbF	pLbO, uRtF	uLtF, pRtO	pLtF, uRbO	uLtO, uRtF	uLbF, pRbO	pLt, uLb, pRt, uRb, uLt
         */
        public static readonly List<Option> optionsLocalizerW2L2 = new List<Option>()
                {
                    new Option(1,   "uLtO"), new Option(1,  "pRbF"),
                    new Option(2,   "uLbF"), new Option(2,  "uRbO"),
                    new Option(3,   "pLtF"), new Option(3,  "uRtO"),
                    new Option(4,   "uRtF"), new Option(4,  "uLtO"),
                    new Option(5,   "uLbO"), new Option(5,  "pRtF"),
                    new Option(6,   "pLbF"), new Option(6,  "uRtO"),
                    new Option(7,   "uLtF"), new Option(7,  "pRtO"),
                    new Option(8,   "pLtO"), new Option(8,  "uRbF"),
                    new Option(9,   "pLbO"), new Option(9,  "uRbF"),
                    new Option(10,  "uLbF"), new Option(10, "pRbO"),
                    new Option("pLt"), new Option("uLb"), new Option("pRt"), new Option("uRb"), new Option("uRt")
                };

        /*
         * 	world 2	(probed)		
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 1 (faces)	6,5,9	            10,1	            3,8	                7,2,4
        Version 1 (Objects) 6,9	                1,10,2	            3,8,4	            7,5
        Version 1 (blanks)  1	                2	                1	                1

         */
        public static readonly List<Option> optionsGameplayW2_ReplayRun_Probed = new List<Option>()
                {
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	world 2	(unprobed)	
                            Left bottom	            Right bottom	        Left top	            Right top
        Version 1(faces)	2,4,10,3,6	            4,8,5,1,7	            1,5,9,7,8	            3,2,6,9,10
        Version 1(objects)  4,10,2,3,6	            5,4,8,1,7	            7,1,5,9,8	            2,6,9,3,10
        Version 1(Blanks)   2	                    3	                    3	                       2


         */
        public static readonly List<Option> optionsGameplayW2_ReplayRun_Unprobed = new List<Option>()
                {
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        /*
         * 	world 2	(probed)	NON REPLAY	
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 1 (faces)	10,9	            8,7,6	            5,4,3	            2,1
        Version 1 (Objects) 1,2,3	            4,5	                6,7	                8,9,10
        Version 1 (blanks)  2	                1	                1	                1


         */
        public static readonly List<Option> optionsGameplayW2_NonReplayRun_Probed = new List<Option>()
                {
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),

                };

        /*
         * 	world 2	(unprobed)	NON REPLAY
                            Left bottom	            Right bottom	        Left top	            Right top
        Version 1(faces)	10,9,8,7,6	            1,4,7,10,3	            5,4,3,2,1	            2,5,6,8,9
        Version 1(objects)  1,2,3,4,5	            1,3,5,7,9	            6,7,8,9,10	            2,4,6,8,10
        Version 1(Blanks)   3	                    2	                    2	                    3

         */
        public static readonly List<Option> optionsGameplayW2_NonReplayRun_Unprobed = new List<Option>()
                {
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),

                };
        #endregion


        #region World 3
        /*
         * Replay level 1											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLbO, uRtF	pLtF, uRbO	uLtO, pRtF	uLbF, uRbO	uLbO, uRbF	uLbF, uRtO	pLtO, uRbF	uLtF, pRbO	uLbO, pRtF	uLtF, uRtO	pLt, uLb, uLt, pRb, uRb
         */
        public static readonly List<Option> optionsLocalizerW3L1 = new List<Option>()
                {
                    new Option(1,   "pLtO"), new Option(1,  "uRtF"),
                    new Option(2,   "uLbF"), new Option(2,  "pRtO"),
                    new Option(3,   "uLtF"), new Option(3,  "pRbO"),
                    new Option(4,   "pLbF"), new Option(4,  "uRbO"),
                    new Option(5,   "pLbO"), new Option(5,  "uRtF"),
                    new Option(6,   "uLtF"), new Option(6,  "uRbO"),
                    new Option(7,   "uLtO"), new Option(7,  "uRbF"),
                    new Option(8,   "pRbF"), new Option(8,  "uLbO"),
                    new Option(9,   "uRtO"), new Option(9,  "pLtF"),
                    new Option(10,  "uLbO"), new Option(10, "pRtF"),
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
                    new Option(2,   "uLtO"), new Option(2,  "uRbF"),
                    new Option(3,   "uLtO"), new Option(3,  "pRtF"),
                    new Option(4,   "pRbO"), new Option(4,  "uLbF"),
                    new Option(5,   "pRbF"), new Option(5,  "uLbO"),
                    new Option(6,   "pLtO"), new Option(6,  "uRtF"),
                    new Option(7,   "pLtF"), new Option(7,  "uRbO"),
                    new Option(8,   "pLbO"), new Option(8,  "uRbF"),
                    new Option(9,   "uLtF"), new Option(9,  "pRtO"),
                    new Option(10,  "uLbF"), new Option(10, "uRtO"),
                    new Option("uLt"), new Option("pLb"), new Option("pRt"), new Option("uRb"), new Option("uLb")
                };

        /*
         * 	world 3	(Probed)  REPLAY	
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 2(faces)	1,4	                8,5,10	            2,7,6	            3,9
        Version 2(objects)  1,5,6	            8,4	                7,2	                3,9,10
        Version 2(blanks)   1	                1	                1	                2  

         */
        public static readonly List<Option> optionsGameplayW3_ReplayRun_Probed = new List<Option>()
                {
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	world 3	(Unprobed) REPLAY
                              Left bottom	            Right bottom	        Left top	            Right top
        Version 2(faces)	    4,6,5,9,7	            7,2,4,3,8	            8,10,3,1,2	            1,6,10,5,9
        Version 2(objects)      5,9,4,6,1	            2,4,5,7,8	            3,8,10,2,7	            6,10,1,3,9
        Version 2(blanks)       2	                    3	                    3	                    2


         */
        public static readonly List<Option> optionsGameplayW3_ReplayRun_Unprobed = new List<Option>()
                {
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*
         * 	world 3	(Probed)  NON REPLAY	
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 2(faces)	1,3,5	            7,9	                2,4	                6,8,10
        Version 2(objects)  9,7	                5,3,1	            10,8,6	            4,2
        Version 2(blanks)   1	                1	                1	                1  

         */
        public static readonly List<Option> optionsGameplayW3_NonReplayRun_Probed = new List<Option>()
                {
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),

                };
        /*
         * 	world 3	(UNProbed)  NON REPLAY	
                            Left bottom	        Right bottom	    Left top	        Right top
        Version 2(faces)	2,6,8,7,10	        5,9,4,2,3	        3,9,4,5,1	        8,7,10,6,1
        Version 2(objects)  1,9,6,8,5	        9,4,5,10,2	        4,10,7,2,3	        1,3,8,6,7
        Version 2(blanks)   2	                3	                3	                2


         */

        public static readonly List<Option> optionsGameplayW3_NonReplayRun_Unprobed = new List<Option>()
                {
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),



                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };
        #endregion


        #region World 4
        /*
         * Replay Run2
         * Replay level 3											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLtF, uRbO	pLbO, uRtF	uLbF, pRbO	uLtO, uRtF	pLtF, uRtO	uLtO, uRbF	uLbF, uRtO	uLbO, pRtF	uLtF, pRbO	uLbO, uRbF	uRt, pLb, uRt, pRb, uLb
         */
        public static readonly List<Option> optionsLocalizerW4L1 = new List<Option>()
                {
                    new Option(1,   "pRtO"), new Option(1,  "uLbF"),
                    new Option(2,   "pLbF"), new Option(2,  "uRtO"),
                    new Option(3,   "uLtF"), new Option(3,  "uRbO"),
                    new Option(4,   "pRbF"), new Option(4,  "uLbO"),
                    new Option(5,   "pLtF"), new Option(5,  "uRtO"),
                    new Option(6,   "uLtO"), new Option(6,  "uRtF"),
                    new Option(7,   "pLtO"), new Option(7,  "uRbF"),
                    new Option(8,   "uLtF"), new Option(8,  "pRbO"),
                    new Option(9,   "pRtF"), new Option(9,  "uLbO"),
                    new Option(10,  "pLbO"), new Option(10, "uRbF"),
                    new Option("uLt"), new Option("pLb"), new Option("uRt"), new Option("pRb"), new Option("uLb")
                };

        /*
         * Replay Run2
         * Replay level 4											
                    1	        2	        3	        4 	        5	        6	        7	        8	        9	        10	        Blanks
         Version 2	pLtO, uRbF	pLbF, uRtO	uLbO, pRbF	uLtF, uRtO	pLtO, uRtF	uLtF, uRbO	uLbO, uRtF	uLbF, pRtO	uLtO, pRbF	uLbF, uRbO	pLt, uLb, pRt, uRb, uLt
         */
        public static readonly List<Option> optionsLocalizerW4L2 = new List<Option>()
                {
                    new Option(1,   "uRtF"), new Option(1,  "uLtO"),
                    new Option(2,   "pLbO"), new Option(2,  "uRbF"),
                    new Option(3,   "uLtO"), new Option(3,  "pRbF"),
                    new Option(4,   "uRbO"), new Option(4,  "uLtF"),
                    new Option(5,   "pRbO"), new Option(5,  "uLbF"),
                    new Option(6,   "uLbF"), new Option(6,  "pRtO"),
                    new Option(7,   "pRtF"), new Option(7,  "uLbO"),
                    new Option(8,   "pLbF"), new Option(8,  "uRtO"),
                    new Option(9,   "pLtO"), new Option(9,  "uRtF"),
                    new Option(10,  "pLtF"), new Option(10, "uRbO"),
                    new Option("pLt"), new Option("uLb"), new Option("pRt"), new Option("uRb"), new Option("uLt")
                };

        /*  PROBED
         *  run 4 (Replay)
         * 	world 2			
         * 	Version 2
                    Left bottom	        Right bottom	    Left top	        Right top
        Faces   	2,4	                3,9,7	            1,5,10	            8,6
        Objects   	2,6,10	            3,9	                1,5	                8,4,7
        Blanks	    2	                1	                1	                1
         */
        public static readonly List<Option> optionsGameplayW4_ReplayRun_Probed = new List<Option>()
                {
                    // ==== BOTTOM LEFT
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),


                    
                    // ==== BOTTOM RIGHT
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),


                     
                    // ==== TOP LEFT
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),


                    
                    // ==== TOP RIGHT
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),

                };

        /*
         *  Unprobed
         *  Run 4 (Replay)
         * 	world 2		
         * 	Version 2
                    Left bottom	            Right bottom	        Left top	            Right top
        Faces	    3,7,8,10,5	            6,10,1,8,9	            9,4,6,1,2	            2,4,5,7,3
        Objects     8,10,3,7,2	            1,6,10,3,9	            4,6,9,1,5	            5,7,2,4,8
        Blanks	    3	                    2	                    2	                    3
         */
        public static readonly List<Option> optionsGameplayW4_ReplayRun_Unprobed = new List<Option>()
                {
                    // ==== BOTTOM LEFT
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    // ==== BOTTOM RIGHT
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),


                    // ==== TOP LEFT
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    // ==== TOP RIGHT
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
                };

        /*  PROBED
         * 	Run 3 (Non Replay)
         * 	world 2		
         * 	Version 2
                    Left bottom             Right bottom            Left top                Right top
        Faces	    1,4,7	                10,3	                6,9	                    5,8,2
        Objects     10,7	                4,1,8	                5,2,9	                6,3
        Blanks	    1	                    2	                    1	                    1
        */
        public static readonly List<Option> optionsGameplayW4_NonReplayRun_Probed = new List<Option>()
        {
                    // ==== BOTTOM LEFT
                    new Option(true, 3, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 6, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(true, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),


                    
                    // ==== BOTTOM RIGHT
                    new Option(true, 2, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 5, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(true, 10, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 9, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(true, 4, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),


                     
                    // ==== TOP LEFT
                    new Option(true, 4, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(true, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 8, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(true, 1, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),


                    
                    // ==== TOP RIGHT
                    new Option(true, 1, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 8, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(true, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(true, 2, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(true, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(true, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
        };

        /*  UNPROBED
         * 	Run 3 (Non Replay)
         * 	world 2		
         * 	Version 2
                    Left bottom             Right bottom            Left top                Right top
        Faces	    3,4,5,1,9	            9,5,3,1,6	            10,8,6,7,2	            7,4,2,8,10
        Objects     5,3,4,2,6	            10,4,1,3,7	            7,10,1,9,8	            9,8,2,6,5
        Blanks	    2	                    3	                    3	                    2
        */
        public static readonly List<Option> optionsGameplayW4_NonReplayRun_Unprobed = new List<Option>()
        {
                    // ==== BOTTOM LEFT
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 8, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 3, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomLeft),



                    // ==== BOTTOM RIGHT
                    new Option(false, 3, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 9, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Face, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 6, StimulusType.Face, Direction_2D_Diagonal.BottomRight),

                    new Option(false, 6, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 1, StimulusType.Object, Direction_2D_Diagonal.BottomRight),
                    new Option(false, 8, StimulusType.Object, Direction_2D_Diagonal.BottomRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.BottomRight),


                    // ==== TOP LEFT
                    new Option(false, 5, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 2, StimulusType.Face, Direction_2D_Diagonal.TopLeft),

                    new Option(false, 2, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 5, StimulusType.Object, Direction_2D_Diagonal.TopLeft),
                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopLeft),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),
                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopLeft),



                    // ==== TOP RIGHT
                    new Option(false, 4, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Face, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Face, Direction_2D_Diagonal.TopRight),

                    new Option(false, 4, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 7, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 10, StimulusType.Object, Direction_2D_Diagonal.TopRight),
                    new Option(false, 9, StimulusType.Object, Direction_2D_Diagonal.TopRight),

                    new Option(false, -1, StimulusType.None, Direction_2D_Diagonal.TopRight),
        };
        #endregion

    }
}