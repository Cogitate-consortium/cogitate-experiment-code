
%SETOUTPUTTABLE gets an event, with its address in the data structure and
% time and builds the output table in memory.
% input:
% ------
% eventType - the event type, as string: "Stimuli", "Response", "Jitter",
% "Fixation", "Save".
% miniBlocks - the complete data structure of the program.
% blockNum - the mini-block number.
% tr - the trial number.
% time - the time in which the event happened.

function [ ] = setOutputTable (eventType, miniBlocks, miniBlockNum, tr, time, PauseTime)

global EVENT_TYPE_COL CHAR_FALSE_MINIBLOCK FACE_OBJECT_MINIBLOCK WRONG_KEY  BLOCK_NUM_COL output_table_cntr OUTPUT_TABLE TIME_COL EVENT_COL DSRD_RESP_COL PLN_JIT_DUR_COL PLN_STIM_DUR_COL TARG2_COL TARG1_COL MINIBLK_TYP_COL SUB_HAND_COL SUB_AGE_COL  TRAIL_COL MINIBLK_COL BLK_COL EXP_COL OUTPUT_COLS EXP_DATE EXP_D
global TRIAL1_TIME_COL  ORIENTATION_COL  TYPE_COL CATEGORY_COL
global stimPtr TRIAL1_JITTER_TIME_COL  TRIAL1_BUTTON_PRESS_COL 
global ACCURATE_COL FAS_COL CRS_COL MS_COL BLANK HT_COL TARGET_KEY RT_COL  EXPERIMENT_NAME  TRIAL1_NAME_COL TARGET1_COL TARGET2_COL MINIBLOCK_TYPE_COL

% builds the output table with all its cols
OUTPUT_TABLE{output_table_cntr, EXP_COL} = EXPERIMENT_NAME;
OUTPUT_TABLE{output_table_cntr, EXP_D} = EXP_DATE;
OUTPUT_TABLE{output_table_cntr, BLK_COL} = miniBlocks{miniBlockNum,BLOCK_NUM_COL};
OUTPUT_TABLE{output_table_cntr, MINIBLK_COL} = miniBlockNum;
OUTPUT_TABLE{output_table_cntr, TRAIL_COL} = tr+1;
OUTPUT_TABLE{output_table_cntr, MINIBLK_TYP_COL} = miniBlocks{miniBlockNum,MINIBLOCK_TYPE_COL};
OUTPUT_TABLE{output_table_cntr, TARG1_COL} = miniBlocks{miniBlockNum,TARGET1_COL};
OUTPUT_TABLE{output_table_cntr, TARG2_COL} = miniBlocks{miniBlockNum,TARGET2_COL};
OUTPUT_TABLE{output_table_cntr, PLN_STIM_DUR_COL} = miniBlocks{miniBlockNum,TRIAL1_TIME_COL + tr};
OUTPUT_TABLE{output_table_cntr, PLN_JIT_DUR_COL} = miniBlocks{miniBlockNum,TRIAL1_JITTER_TIME_COL + tr};
OUTPUT_TABLE{output_table_cntr, DSRD_RESP_COL} = isTarget(miniBlocks, miniBlockNum, tr); % 1 button press 0 no button press
OUTPUT_TABLE{output_table_cntr, TIME_COL} = time;

if OUTPUT_TABLE{output_table_cntr, MINIBLK_TYP_COL} == FACE_OBJECT_MINIBLOCK
    OUTPUT_TABLE{output_table_cntr, MINIBLK_TYP_COL} = 'face & object';
elseif OUTPUT_TABLE{output_table_cntr, MINIBLK_TYP_COL} == CHAR_FALSE_MINIBLOCK
    OUTPUT_TABLE{output_table_cntr, MINIBLK_TYP_COL} = 'letter & false';
end

switch (eventType)
    case 'Stimulus'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = miniBlocks{miniBlockNum,TRIAL1_NAME_COL + tr};
        stimPtr = output_table_cntr;
        OUTPUT_TABLE{stimPtr, HT_COL} = 0;
        OUTPUT_TABLE{stimPtr, MS_COL} = 0;
        OUTPUT_TABLE{stimPtr, CRS_COL} = 0;
        OUTPUT_TABLE{stimPtr, FAS_COL} = 0;
        if OUTPUT_TABLE{output_table_cntr, DSRD_RESP_COL}
            OUTPUT_TABLE{stimPtr, MS_COL} = 1;
            OUTPUT_TABLE{stimPtr, ACCURATE_COL} = 0;
        else
            OUTPUT_TABLE{stimPtr, CRS_COL} = 1;
            OUTPUT_TABLE{stimPtr, ACCURATE_COL} = 1;
        end
        OUTPUT_TABLE{stimPtr, ORIENTATION_COL} = floor(mod(OUTPUT_TABLE{output_table_cntr, EVENT_COL},1000)/100);
        OUTPUT_TABLE{stimPtr, TYPE_COL} = OUTPUT_TABLE{output_table_cntr, EVENT_COL} - OUTPUT_TABLE{stimPtr, ORIENTATION_COL}*100;
        OUTPUT_TABLE{stimPtr, CATEGORY_COL} = floor(OUTPUT_TABLE{output_table_cntr, EVENT_COL}/1000);
    case 'Fixation'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;
    case 'Jitter'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;
    case 'Baseline'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;  
    case 'RunOnset'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;  
    case 'TargetScreenOnset'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;
    case 'Response'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr};
        OUTPUT_TABLE{stimPtr, RT_COL} = OUTPUT_TABLE{output_table_cntr, TIME_COL} - OUTPUT_TABLE{stimPtr, TIME_COL};
        if OUTPUT_TABLE{output_table_cntr, DSRD_RESP_COL}
            if miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr} == TARGET_KEY
                OUTPUT_TABLE{stimPtr, HT_COL} = 1;
                OUTPUT_TABLE{stimPtr, ACCURATE_COL} = 1;
                OUTPUT_TABLE{stimPtr, MS_COL} = 0;
            elseif miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr} == WRONG_KEY
                %note that wrong key is considered as target key - change if you wish
                OUTPUT_TABLE{stimPtr, HT_COL} = 1;
                OUTPUT_TABLE{stimPtr, ACCURATE_COL} = 1;
                OUTPUT_TABLE{stimPtr, MS_COL} = 0;
            end
        else
            if miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr} == TARGET_KEY
                OUTPUT_TABLE{stimPtr, FAS_COL} = 1;
            elseif miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr} == WRONG_KEY
                %note that wrong key is considered as target key - change if you wish
                OUTPUT_TABLE{stimPtr, FAS_COL} = 1;
            end
            OUTPUT_TABLE{stimPtr, CRS_COL} = 0;
            OUTPUT_TABLE{stimPtr, ACCURATE_COL} = 0;
        end
    case 'Save'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = GetSecs - time; % for save, the event is save duration time
    case 'Pause'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = PauseTime;
    case 'Interruption'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;
    case 'Abortion'
        OUTPUT_TABLE{output_table_cntr, EVENT_COL} = BLANK;
    otherwise
end
OUTPUT_TABLE{output_table_cntr, EVENT_TYPE_COL} = eventType;

output_table_cntr = output_table_cntr + 1;
end