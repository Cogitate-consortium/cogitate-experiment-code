
% INITCONSTANTSPARAMETERS this function defines all the constants and parameters of
% the program. Please note was can be changed, and what cannot, without
% breaking the program.
% The codes used to designate stimuli:
% stimuli are coded as a 4 digit number.
% 1st digit = stimulus type (1 = face; 2 = object; 3 = letter; 4 = false font)
% 2nd digit = stimulus orientation (1 = center; 2 = left; 3 = right)
% 3rd & 4th digits = stimulus id (1...20; for faces 1...10 is male, 11...20 is female)
% e.g., "1219" = 1 is face, 2 is left orientation and 19 is a female stimulus #19
% Duration is encoded by the first decimal so that 1219.1 has duration 0.5,
% 1219.2 has duration 1 s and 1219.3 has duration 1.5 s
function initConstantsParameters(subNum)
    
    disp('')
    disp('WELCOME TO initConstantsParameters')
    disp('')   
    %% Header
    % GOBAL CONSTANTS
    % -----------------------------------------------------
    % Text and messages
    global LAB_ID INSTRUCTIONS1 INSTRUCTIONS2 INSTRUCTIONS3 INSTRUCTIONS4 INSTRUCTIONS5 INSTRUCTIONS6 TRUE FALSE SAVING_MESSAGE TRIGGER_DURATION_EXCEEDED_WARNING
    global LOADING_MESSAGE  CLEAN_EXIT_MESSAGE  END_OF_EXPERIMENT_MESSAGE MINIBLOCK_TEXT END_OF_BLOCK_MESSAGE MEG_BREAK_MESSAGE
    global END_OF_BLOCK_MESSAGE_fMRI_MEG PRACTICE_START_MESSAGE PRACTICE_START_MESSAGE_fMRI EXPERIMET_START_MESSAGE EXPERIMET_START_MESSAGE_fMRI 
    global END_OF_BLOCK_MESSAGE_ECOG EXPERIMENT_START_MESSAGE_ECOG EYETRACKER_CALIBRATION_MESSAGE_ECOG EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS_ECOG PRESS_ANY_BUTTON PRESS_SPACE PROGRESS_MESSAGE_ECOG PRACTICE_START_MESSAGE_ECOG GENERAL_BREAK_MESSAGE_ECOG MINIBLOCK_TEXT_ECOG
    global BLOCK_START_MESSAGE_fMRI BREAK_MESSAGE_fMRI FEEDBACK_MESSAGES FEEDBACK_MESSAGES_PRACTICE EYETRACKER_CALIBRATION_MESSAGE EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS EYETRACKER_CALIBRATION_MESSAGE_fMRI EYETRACKER_CALIBRATION_MESSAGE_fMRI_BETWEENBLOCKS PRESS_SPACE fontType fontSize fontColor GENERAL_BREAK_MESSAGE RESTART_MESSAGE RESTART_MESSAGE_fMRI RESTARTBLOCK_OR_MINIBLOCK_MESSAGE RESTART_PRACTICE_MESSAGE RESTART_PRACTICE_MESSAGE_ECoG PROGRESS_MESSAGE PROGRESS_MESSAGE_MEG
    % -----------------------------------------------------
    % Matrices info
    global BLOCK_NUM_COL TRIAL1_BUTTON_PRESS_COL SUBJECT_START_TIME SUBJECT_NUMBER_COL TARGET1_COL TARGET2_COL EXPERIMENT_NAME TRIAL1_RESPONSE_TIME_COL
    global CR_COL FA_COL HITS_COL MISSES_COL TRIAL1_TIME_COL TRIAL1_BLANK_DUR_COL DATA_TABLE_SIZE TRIAL1_JITTER_TIME_COL TRIAL1_STIM_END_TIME_COL
    global TRIAL1_DURATION_COL TRIAL1_START_TIME_COL TRIAL1_NAME_COL MINIBLOCK_TYPE_COL TRIAL1_ANSWER_COL MINI_BLOCK_SIZE_COL MINI_BLK_NUM_COL
    global TYPE_COL ORIENTATION_COL CATEGORY_COL ACCURATE_COL HT_COL MS_COL CRS_COL FAS_COL RT_COL OUTPUT_TABLE_HEADER TRIGGER_TABLE_HEADER
    global EVENT_TYPE_COL output_table_cntr OUTPUT_TABLE TIME_COL EVENT_COL DSRD_RESP_COL PLN_JIT_DUR_COL PLN_STIM_DUR_COL TARG2_COL
    global TARG1_COL MINIBLK_TYP_COL TRAIL_COL MINIBLK_COL BLK_COL EXP_COL OUTPUT_COLS BEHAV_FILE_NAMING BEHAV_FILE_NAMING_WHOLE BEHAV_FILE_SUMMARY_NAMING EYETRACKER_FILE_NAMING TRIG_LOG_FILE_NAMING LPTTRIG_LOG_FILE_NAMING
    % -----------------------------------------------------
    % Timing parameters
    global JITTER_RANGE_MEAN JITTER_RANGE_MIN JITTER_RANGE_MAX END_WAIT STIM_DURATION TRIAL_DURATION MRI_BASELINE_PERIOD
    % -----------------------------------------------------
    % Keys parameters
    global RESPONSE_KEY ValidationKey TARGET_KEY WRONG_KEY NO_KEY RESTART_KEY ABORT_KEY abortKey upKey RightKey LeftKey MEGbreakKey PauseKey RestartKey YesKey spaceBar MINIBLOCK_RESTART_KEY BLOCK_RESTART_KEY
    % -----------------------------------------------------
    % Trials parameters
    global DEBUG MIN_NUM_OF_TRIALS_PER_MINI_BLOCK FIXATION TRIAL1_STIM_DUR_COL NUM_OF_STIM_TYPE_PER_MINIBLOCK NUM_OF_TARGET_TYPES_PER_MINIBLOCK
    global NUM_OF_POSSIBLE_EVENTS_IN_TRIAL NUM_OF_TARGETS_PER_MINIBLOCK LEFT CENTER RIGHT NUM_OF_BLOCKS MAX_NUM_OF_TRIALS_PER_MINI_BLOCK
    global CHAR_FALSE_MINIBLOCK FACE_OBJECT_MINIBLOCK NUM_OF_MINIBLOCK_TYPE NUM_OF_MINI_BLOCKS_PER_BLOCK BLANK FALSE_FONT LETTER OBJECT
    global FACE NUM_OF_STIMULI_EACH NUM_OF_ORIENTATIONS NUM_OF_CATEGORIES
    % -----------------------------------------------------
    % Screen parameters 
    global FRAME_WIDTH MAX_VISUAL_ANGEL VIEWING_DISTANCE FRAME_COLOR  viewDistance FIXATION_COLOR FIXATION_FONT_SIZE  DIAMOUT_FIXATION DIAMIN_FIXATION
    % -----------------------------------------------------
    % Annex folders and files
    global CODE_FOLDER FUNCTIONS_FOLDER TEMPORARY_FOLDER SECRET_FOLDER STIM_FOLDER DATA_FOLDER OBJECTS_R_FOL FALSES_R_FOL CHARS_R_FOL FACES_R_FOL PRACTICE_MINIBLOCK_MAT
    global OBJECTS_FOLDER FALSES_FOLDER CHARS_FOLDER FACES_FOLDER OBJECTS_C_FOL OBJECTS_L_FOL FALSES_C_FOL FALSES_L_FOL CHARS_C_FOL CHARS_L_FOL ANIMAL_REWARD_FOL
    global FACES_C_FOL FACES_L_FOL FEMALE_FOLDER MALE_FOLDER FILE_POSTFIX PRACTICE_L_FOL PRACTICE_R_FOL PRACTICE_C_FOL FIXATION_FILE INSTRUCTIONS_FOLDER  EXP_D EXP_DATE
    % -----------------------------------------------------
    % Dummy variables
    global ECoG fMRI MEEG Behavior VERBOSE debugFactor
    % -----------------------------------------------------
    % Triggers
    global TRG_TASK_IRRELEVANT TRG_RIGHT TRG_STIM_END TRG_EXP_END_MSG TRG_EXP_START_MSG TRG_MINIBLOCK_STARTED TRG_RESPONSE 
    global TRG_TASK_RELEVANT_NON_TARGET TRG_TASK_RELEVANT TRG_DUR_1500 TRG_DUR_1000 TRG_DUR_500 TRG_LEFT TRG_CENTER TRG_STIM_ADD TRG_TIME_BETWEEN TRG_JITTER_START TRG_MB_ADD TRG_TRIAL_ADD LPT_CODE_START LPT_CODE_END
    global TRG_MBONSET_AUD TRG_EXP_END_MSG_AUD TRIGAUD_CODE_START TRG_RESPONSE_AUD
    % -----------------------------------------------------
    % Saving parameters
    global excelFormat excelFormatSummary
    % -----------------------------------------------------
    % Audio
    global NR_CHANNELS SAMPLE_RATE SND_FREQ VOLUME  matrix_LUT MB_ID_START_END_BUF 
    % global                         SUB_HAND_COL SUB_AGE_COL      
    % -----------------------------------------------------
    % Pseudorandomization parameters    
    global NUMBER_OF_NON_TARGET_SETS_PER_CAT NUMBER_OF_TOTAL_TRIALS NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB 
    global NUM_TARGETS_RIGHT_PER_CAT NUM_TARGETS_LEFT_PER_CAT NUM_TARGETS_CENTER_PER_CAT NUM_TARGETS_PER_CAT  
    global NUM_TARGETS_CENTER_DUR_05_PER_CAT NUM_TARGETS_CENTER_DUR_1_PER_CAT NUM_TARGETS_CENTER_DUR_15_PER_CAT
    global NUM_TARGETS_RIGHT_DUR_05_PER_CAT NUM_TARGETS_RIGHT_DUR_1_PER_CAT NUM_TARGETS_RIGHT_DUR_15_PER_CAT
    global NUM_TARGETS_LEFT_DUR_05_PER_CAT NUM_TARGETS_LEFT_DUR_1_PER_CAT NUM_TARGETS_LEFT_DUR_15_PER_CAT
    global NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE
    global NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE
    global NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NON_TARGETS_FACES_PER_MB NON_TARGETS_OBJECTS_PER_MB NON_TARGETS_CHARS_PER_MB NON_TARGETS_FALSES_PER_MB
    global FACES_TARGET_ECoG OBJECTS_TARGET_ECoG CHARS_TARGET_ECoG FALSES_TARGET_ECoG
    global FACES_TARGET_fMRI OBJECTS_TARGET_fMRI CHARS_TARGET_fMRI FALSES_TARGET_fMRI
    global version_duration
    % fMRI
    global bitsi_buttonbox bitsi_scanner
     % -----------------------------------------------------
     %Practice
    global MinPracticeHits MaxPracticeHits MaxPracticeFalseAlarms MaxPracticeFalseAlarms_Irrelevant MinPracticeHits_fMRI MaxPracticeHits_fMRI MaxPracticeFalseAlarms_fMRI MaxPracticeFalseAlarms_Irrelevant_fMRI
    % Diverse
    global NO_TRIAL PRACTICE   
    
    
    %%  PARAMETERS THAT MAY BE ALTERED
    EXPERIMENT_NAME = 'Exp1';
    BEHAV_FILE_NAMING = '_Beh_V1_RawDurR';
    BEHAV_FILE_NAMING_WHOLE = '_Beh_V1_RawDurWHOLE';
    BEHAV_FILE_SUMMARY_NAMING = '_Beh_V1_SumDur';
    LPTTRIG_LOG_FILE_NAMING = '_LPTTrigLog_V1_DurR';
    TRIG_LOG_FILE_NAMING = '_Beh_V1_TrigDurR';
    EYETRACKER_FILE_NAMING = '_ET_V1_DurR';
    FILE_POSTFIX = '*.png';
    %add date as a separate column 5 years rewind
    t=datenum(date);
    if ECoG
        t = addtodate(t,-5,'year');
    end
    EXP_DATE=datestr(t);

    
    trial_mod = 1;
    if fMRI || ECoG % if MRI or ECoG, reduce the number of trials by 50%
        trial_mod = 0.5;
    end
      

    NUMBER_OF_TOTAL_TRIALS = 1440 * trial_mod; % 1280 non-targets + 160 targets
    % This is the number of each stimuli (e.g. a specific face) that will
    % be shown during the experiment (all miniblocks) as non-target or
    % irrelevant.
    % For example, since there are 20 unique stimuli from each category,
    % and in total we will show 320 (160 as non-target, 160 as irrelevant) of each
    % category. Then we need to repeat each stimulus 320/20 = 16 times (half for ECoG)
    NUMBER_OF_NON_TARGET_SETS_PER_CAT = 16 * trial_mod; % 320 non-targets per catergory
    % For example Number of non-target faces (objects) for a face-object mb
    % 160 (total number of non-target faces in face-object mb)/20 (nr of face object mbs) 
    NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB = 8;
  if (fMRI)
     NUMBER_OF_TOTAL_TRIALS = 576; %512 non-targets + 96 targets
%      NUMBER_OF_NON_TARGET_SETS_PER_CAT=12;
     NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB=4;
  end
    %% TARGETS %%
    % Total nr of stimuli shown during the experiment that are targets
    % per category (faces, objects etc)
    
    % The nr of trials for each duration comes in three versions:
    % 1 1 2 2 3 3 1 1 2 2 3 3 etc
    u = mod(int64(subNum),6);
    g = round(u/2);
    j = mod(g,3);
    y = j -3;
    v = floor(abs(double(y)/double(3)))*3;
    version_duration = j + int64(v);
    
    
    
    NUM_TARGETS_PER_CAT = 40 * trial_mod; 
    
    % Center:
    NUM_TARGETS_CENTER_PER_CAT = 20 * trial_mod;
    if (fMRI)
    NUM_TARGETS_PER_CAT = 16; 
    NUM_TARGETS_CENTER_PER_CAT = 8;    
    end
    v = [7, 6, 7];
    NUM_TARGETS_CENTER_DUR_05_PER_CAT = v(version_duration);
    
    v = [7, 7, 6];
    NUM_TARGETS_CENTER_DUR_1_PER_CAT = v(version_duration);
    
    v = [6, 7, 7];
    NUM_TARGETS_CENTER_DUR_15_PER_CAT = v(version_duration);
    
    if ECoG
        
        v =  [3, 3, 4];
        NUM_TARGETS_CENTER_DUR_05_PER_CAT = v(version_duration);
        
        v = [3, 4, 3];
        NUM_TARGETS_CENTER_DUR_1_PER_CAT = v(version_duration);
        
        v = [4, 3, 3];
        NUM_TARGETS_CENTER_DUR_15_PER_CAT = v(version_duration);
    end % ECoG
    
    if fMRI
        
        v =  [3, 2, 3];
        NUM_TARGETS_CENTER_DUR_05_PER_CAT = v(version_duration);
        
        v = [3, 3, 2];
        NUM_TARGETS_CENTER_DUR_1_PER_CAT = v(version_duration);
        
        v = [2, 3, 3];
        NUM_TARGETS_CENTER_DUR_15_PER_CAT = v(version_duration);
    end % fMRI
    
    % Left
    NUM_TARGETS_LEFT_PER_CAT = 10 * trial_mod;
    if (fMRI)
        NUM_TARGETS_LEFT_PER_CAT = 4;
    end
    v =  [3, 4, 3];
    NUM_TARGETS_LEFT_DUR_05_PER_CAT = v(version_duration);
    
    v = [3, 3, 4];
    NUM_TARGETS_LEFT_DUR_1_PER_CAT = v(version_duration);
    
    v = [4, 3, 3];
    NUM_TARGETS_LEFT_DUR_15_PER_CAT = v(version_duration);
    
    if ECoG
        v =  [1, 2, 2];
        NUM_TARGETS_LEFT_DUR_05_PER_CAT = v(version_duration);
        
        v =  [2, 2, 1];
        NUM_TARGETS_LEFT_DUR_1_PER_CAT = v(version_duration);
        
        v = [2, 1, 2];
        NUM_TARGETS_LEFT_DUR_15_PER_CAT = v(version_duration);
    end % ECoG
    
    if fMRI
        v =  [1, 2, 1];
        NUM_TARGETS_LEFT_DUR_05_PER_CAT = v(version_duration);
        
        v =  [1, 1, 2];
        NUM_TARGETS_LEFT_DUR_1_PER_CAT = v(version_duration);
        
        v = [2, 1, 1];
        NUM_TARGETS_LEFT_DUR_15_PER_CAT = v(version_duration);
    end % fMRI
    
    % Right:
    NUM_TARGETS_RIGHT_PER_CAT = 10 * trial_mod;
    if(fMRI)
        NUM_TARGETS_RIGHT_PER_CAT = 4;
    end
    v = [4, 3, 3];
    NUM_TARGETS_RIGHT_DUR_05_PER_CAT = v(version_duration);
    
    v = [3, 4, 3];
    NUM_TARGETS_RIGHT_DUR_1_PER_CAT = v(version_duration);
    
    v = [3, 3, 4];
    NUM_TARGETS_RIGHT_DUR_15_PER_CAT = v(version_duration);
    
    if ECoG
        
        v = [2, 2, 1];
        NUM_TARGETS_RIGHT_DUR_05_PER_CAT = v(version_duration);
        
        v = [2, 1, 2];
        NUM_TARGETS_RIGHT_DUR_1_PER_CAT = v(version_duration);
        
        v = [1, 2, 2];
        NUM_TARGETS_RIGHT_DUR_15_PER_CAT = v(version_duration);
    end % ECoG
    
    if fMRI
        
        v = [2, 1, 1];
        NUM_TARGETS_RIGHT_DUR_05_PER_CAT = v(version_duration);
        
        v = [1, 2, 1];
        NUM_TARGETS_RIGHT_DUR_1_PER_CAT = v(version_duration);
        
        v = [1, 1, 2];
        NUM_TARGETS_RIGHT_DUR_15_PER_CAT = v(version_duration);
    end % fMRI
    
    %% NON-TARGETS and IRRELEVANTS
    
    % These are total number of stimuli presented for all the face-object miniblocks
    % or for all the letter-false-font miniblocks, respectively.
    % That is what "PER_MB_TYPE" means. The category that is going
    % to be non-target for the face-object miniblock are going to be
    % irrelevant for the letter-false_font and the reverse, but the
    % number of stimuli that are irrelevant is equal to the nr that 
    % are shown as non-target, hence these numbers are the same
    % across the two miniblock types.
    % To get the numbers for the whole experiment, multiply by 2 mb types 
    % and 4 (categories) (8).
    
    n_mb_types = 2; % 1) Face-object 2) Letter-False-font
    
    NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE = (160/n_mb_types) * trial_mod; % 80 as non-targets, 80 as irrelevants
    if(fMRI)
     NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE=32;   
    end
    % Durations for the 3 different duration verions for each orientation
    v = [27, 27, 26];
    NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); % 80/3
    
    v = [26, 27, 27];
    NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); % Vector for different versions. 
    
    v = [27, 26, 27];
    NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    if ECoG % 40/3
        v = [13, 14, 13];
        NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [14, 13, 13]; 
        NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [13, 13, 14]; 
        NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % ECoG
    
    if fMRI % 40/3
        v = [11, 11, 10];
        NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [10, 11, 11]; 
        NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [11, 10, 11]; 
        NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % fMRI

    NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE = (80/n_mb_types) * trial_mod;
    if(fMRI)
    NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE = 16;   
    end
    
    v = [13, 13, 14];
    NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration);  % 40/3
    
    v = [14, 13, 13]; 
    NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    v = [13, 14, 13]; 
    NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    if ECoG % 20/3
        
        v = [7, 6, 7]; 
        NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [6, 7, 7]; 
        NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [7, 7, 6]; 
        NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % ECoG
    
    if fMRI % 20/3
        
        v = [5, 5, 6]; 
        NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [6, 5, 5]; 
        NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [5, 6, 5]; 
        NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % fMRI
    
    NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE = (80/n_mb_types)  * trial_mod;
    if (fMRI)
        NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE = 16;
    end
    v = [13, 14, 13]; % 20/3
    NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    v =  [13, 13, 14]; 
    NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    v = [14, 13, 13]; 
    NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    if ECoG
        v = [7, 7, 6]; % 20/3
        NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [7, 6, 7]; 
        NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [6, 7, 7];
        NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % ECoG
    
    if fMRI
        v = [5, 6, 5]; % 20/3
        NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [5, 5, 6]; 
        NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [6, 5, 5];
        NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % fMRI
    
    %% IRRELEVANTS
    
    NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE = (160/n_mb_types) * trial_mod; % 80 as non-targets, 80 as irrelevants
    if(fMRI)
     NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE = 32;   
    end
    % Durations for the 3 different duration verions for each orientation
    v = [26, 27, 27];
    NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); % 80/3
    
    v = [27, 26, 27];
    NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); % Vector for different versions. 
    
    v = [27, 27, 26];
    NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    if ECoG % 40/3
        v = [14, 13, 13];
        NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [13, 13, 14]; 
        NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [13, 14, 13]; 
        NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % ECoG
    
    if fMRI % 40/3
        v = [10, 11, 11];
        NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [11, 10, 11]; 
        NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [11, 11, 10]; 
        NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % fMRI

    NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE = (80/n_mb_types) * trial_mod;
    if(fMRI)
    NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE =16;   
    end
    
    v = [14, 13, 13];
    NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration);  % 40/3
    
    v = [13, 14, 13]; 
    NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    v = [13, 13, 14]; 
    NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    if ECoG % 20/3
        
        v = [6, 7, 7]; 
        NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [7, 7, 6]; 
        NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [7, 6, 7]; 
        NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % ECoG
    
    if fMRI % 20/3
        
        v = [6, 5, 5];
        NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [5, 6, 5]; 
        NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v =  [5, 5, 6]; 
        NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % fMRI
    
    
    NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE = (80/n_mb_types)  * trial_mod;
    if(fMRI)
    NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE = 16;    
    end
    
    v = [13, 13, 14]; % 20/3
    NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    v =  [14, 13, 13]; 
    NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    v = [13, 14, 13]; 
    NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
    
    if ECoG
        v = [7, 6, 7]; % 20/3
        NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [6, 7, 7]; 
        NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [7, 7, 6];
        NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % ECoG
    
    if fMRI
        v = [5, 5, 6]; % 20/3
        NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [6, 5, 5]; 
        NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
        v = [5, 6, 5];
        NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE = v(version_duration); 
        
    end % fMRI
    

    NUM_OF_ORIENTATIONS = 3;
    NUM_OF_CATEGORIES = 4;
    NUM_OF_STIMULI_EACH = 20; % nr of unique stimuli (e.g. different human faces)
    
    NUM_OF_MINI_BLOCKS_PER_BLOCK = 4 ; % number of mini blocks in each block
    NUM_OF_BLOCKS = 10* trial_mod; %number of blocks
    if fMRI
       NUM_OF_BLOCKS = 8; 
    end
    NUM_OF_TARGETS_PER_MINIBLOCK = [2 3 4 5 6]; % possible number of targets to use in miniblock
    % Non-targets/irrelevants (holds for both)
    % For example for face-object miniblocks:
    % (160 (e.g. irrelevant false-fonts)/10 blocks)/ 2 miniblocks ( 2 out
    % of 4 mb per block
    % are face-object mb.) This is for the letter-false-font mb. The same numbers
    % apply for the lette-false font but then the false-fonts are non-targets.
    NUM_OF_STIM_TYPE_PER_MINIBLOCK = 8; % for each type of stimuli (letter, etc)
    if fMRI %|| ECoG % if MRI or ECoG, reduce the number of trials by 50%
        NUM_OF_STIM_TYPE_PER_MINIBLOCK = NUM_OF_STIM_TYPE_PER_MINIBLOCK/2; % for each type of stimuli
        NUM_OF_TARGETS_PER_MINIBLOCK = [1 2 3]; 
    end
    MIN_NUM_OF_TRIALS_PER_MINI_BLOCK = NUM_OF_STIM_TYPE_PER_MINIBLOCK*NUM_OF_CATEGORIES + min(NUM_OF_TARGETS_PER_MINIBLOCK); % minimum number of stimuli in each miniblock
    MAX_NUM_OF_TRIALS_PER_MINI_BLOCK = NUM_OF_STIM_TYPE_PER_MINIBLOCK*NUM_OF_CATEGORIES + max(NUM_OF_TARGETS_PER_MINIBLOCK); % maximum number of stimuli in each miniblock
    
    
    %%%%% SPECIFIC TARGETS FOR ECOG %%%%%%
    FACES_TARGET_ECoG = [1, 2, 4, 5, 9, 11, 12, 14, 15, 18]; % 1
    OBJECTS_TARGET_ECoG = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]; % 2
    CHARS_TARGET_ECoG = [1, 3, 5, 7, 8, 10, 11, 13, 15, 19]; % 3
    FALSES_TARGET_ECoG = [1, 3, 5, 7, 8, 10, 11, 13, 15, 19]; % 4
    
    %%%%% SPECIFIC TARGETS FOR fMRI %%%%%%
    
    FACES_TARGET_fMRI = [1, 2, 3, 4, 5, 6, 8, 9, 11, 12, 13, 14, 15, 17, 18, 19]; % 1
    OBJECTS_TARGET_fMRI = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]; % 2
    CHARS_TARGET_fMRI = [1, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20]; % 3
    FALSES_TARGET_fMRI = [1, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20]; % 4

                  
    FRAME_WIDTH = 0; % 0 for delete
    if DEBUG FRAME_WIDTH = 1; end
    FRAME_COLOR = [39,241,44];

    % TIMING
%    JITTER_RANGE_MEAN = 0.400;
    JITTER_RANGE_MEAN =0.2002;
    JITTER_RANGE_MIN = 0.200;
    JITTER_RANGE_MAX = 2.000;  
    if fMRI % only MRI
        JITTER_RANGE_MEAN =0.5;
        JITTER_RANGE_MIN = 2.500;
        JITTER_RANGE_MAX = 10.000;
        MRI_BASELINE_PERIOD = 12; % seconds
    end
    STIM_DURATION = [0.500 1.000 1.500]; % Planned duration in seconds. However, since every lab amy have a different refresh rate, we need to go as close as possible to it. So it will be actualized once we get the refresh rate:
    TRIAL_DURATION = 2.000; % Total trial duration in seconds, without jitter
    END_WAIT = 2.000; % time of "end of experiment" message (in s)

    if DEBUG == 2 %fast debug
        STIM_DURATION = [6 12 18] * (1/60); % 1/60 to allow at least one frame to appear on screen
        TRIAL_DURATION = 24 * (1/60); % leaves 3 frames for fixation
        JITTER_RANGE_MIN = 8 * (1/60);
        JITTER_RANGE_MAX = 24 * (1/60);
        JITTER_RANGE_MEAN = ((JITTER_RANGE_MIN+JITTER_RANGE_MAX)/2) * (1/60);
        debugFactor = 20; % by how much to quicken the run
        MRI_BASELINE_PERIOD = MRI_BASELINE_PERIOD / debugFactor;
    end

    % TEXT
    fontType = 'David';
    fontSize = 50; % general text size, not target
    FIXATION_COLOR = [205 33 42];
    FIXATION_FONT_SIZE = 20;
    fontColor = 0; % black;

    % messages
    END_OF_EXPERIMENT_MESSAGE = 'The End. Thank You!';
    FIXATION = 'o';
    LOADING_MESSAGE = 'Loading...';
    SAVING_MESSAGE = 'Saving...';
    CLEAN_EXIT_MESSAGE = 'Program aborted by user!';
    MINIBLOCK_TEXT = 'Press When These Appear:';
    MINIBLOCK_TEXT_ECOG = 'Press any button when any of these appear:';
    END_OF_BLOCK_MESSAGE = 'End of block.\n\n Press SPACE to continue...';
    END_OF_BLOCK_MESSAGE_ECOG = {'End of block.', 'Press any button to continue...'};
    MEG_BREAK_MESSAGE = 'We are saving the data, the experiment will proceed \n\n as soon as we are ready. \n\n Please wait';
    EXPERIMET_START_MESSAGE = 'The experiment starts now.\n\n Press SPACE to continue...';
    EXPERIMENT_START_MESSAGE_ECOG = {'The experiment starts now.', 'Press any button to continue...'};
    EYETRACKER_CALIBRATION_MESSAGE = 'Before we proceed, we need to calibrate the eyetracker.\n\n\n\n You will see a dot that will move to different locations on screen.\n\n Your task is to keep looking at the dot at all times.\n\n Try to avoid blinking as much as possible.\n\n\n Press SPACE to proceed to calibration...';
    EYETRACKER_CALIBRATION_MESSAGE_ECOG = {'Before we proceed, we need to calibrate the eyetracker.\n\n\n You will see a dot that will move to different locations on screen.\n\n Your task is to keep looking at the dot at all times.\n\n Try to avoid blinking as much as possible.', 'Press any button to proceed to calibration...'};
    EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS = 'Before we proceed, we need to calibrate the eyetracker.\n\n\n Press SPACE to proceed to calibration...';
    EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS_ECOG = {'Before we proceed, we need to calibrate the eyetracker.', 'Press any button to proceed to calibration...'};
    GENERAL_BREAK_MESSAGE = 'Feel free to take a break now.\n\n Press SPACE to continue...';
    GENERAL_BREAK_MESSAGE_ECOG = {'Feel free to take a break now.', 'Press any button to continue...'};
    FEEDBACK_MESSAGES={'Nice job! Keep it up!','Warning: You are missing targets.','Warning: You are selecting incorrect targets.'};
    FEEDBACK_MESSAGES_PRACTICE={'Nice job! Keep it up!','Warning: You missed x targets.','Warning: You selected x incorrect targets.'};

    EXPERIMET_START_MESSAGE_fMRI = 'The experiment starts now.';
    EYETRACKER_CALIBRATION_MESSAGE_fMRI='Before we proceed, we need to calibrate the eyetracker.\n\n\nYou will see a dot that will move \n to different locations on screen.\n\n Your task is to keep looking at the dot at all times.\n\n Try to avoid blinking as much as possible. \n\n\n Press the index finger button to proceed';
    EYETRACKER_CALIBRATION_MESSAGE_fMRI_BETWEENBLOCKS='Before we proceed, we need to calibrate the eyetracker. \n\n\n Press the index finger button to proceed';

    BLOCK_START_MESSAGE_fMRI = 'Please stay still and keep your eyes fixed\n\n on the center of the screen.\n\n\n\n Waiting for scanner.\n\n\n Next block:';
    BREAK_MESSAGE_fMRI = 'Break.';
    END_OF_BLOCK_MESSAGE_fMRI_MEG ='End of block.';
    
    PRESS_SPACE ='\nPress SPACE to continue...\n';
    PRESS_ANY_BUTTON ='\nPress any button to continue...\n';

    TRIGGER_DURATION_EXCEEDED_WARNING = 'The duration of the processes before flipping the photodiode back to black was too long. \n The photodiode square will be on for longer!!!';
    RESTART_MESSAGE='Are you sure you want to restart?';
    RESTARTBLOCK_OR_MINIBLOCK_MESSAGE = 'Do you want to restart the block or the miniblock?\n Press M to restart the miniBlock \Press B to restart the block';
    RESTART_MESSAGE_fMRI='Are you sure you want to restart the block?';
    RESTART_PRACTICE_MESSAGE='\n\n It is recommended to repeat the practice.\n\n\n Experimenter: Press R to repeat or Y to proceed.' ;
    RESTART_PRACTICE_MESSAGE_ECoG ='Great job! That was the first practise, let us proceed to the next.\n\n\n Experimenter: Press R to do the practice or Y to proceed to the experiment.' ;
    PROGRESS_MESSAGE = 'Congratulations! You completed a miniblock. Well Done! \n\n Press SPACE to continue...'; 
    PROGRESS_MESSAGE_ECOG = {'Congratulations! You completed a miniblock. Well Done!'; 'Press any button to continue...'}; 

    PROGRESS_MESSAGE_MEG = 'You completed a miniblock.';
    VIEWING_DISTANCE = viewDistance; % in centimeters
    MAX_VISUAL_ANGEL = [6,6]; % in degrees | "on a rectangular aperture at an average visual angle of 6? by 4?"

    % Size of the fixation in DVA:
    DIAMOUT_FIXATION = 0.6; % diameter of outer circle (degrees)
    DIAMIN_FIXATION = 0.1; % diameter of inner circle (degrees)
    
    % Format of saved data:
    excelFormat = '.csv';
    excelFormatSummary = '.xls';
    
    % Response params
    KbName('UnifyKeyNames');
   if (strcmp(LAB_ID,'SD'))
    RightKey      =  KbName('1!');
    LeftKey       =  KbName('2@');
    upKey         =  RightKey;
   elseif (strcmp(LAB_ID,'SA'))
    RightKey      =  KbName('7&');
    LeftKey       =  KbName('8*');
    upKey         =  RightKey;
   else
    upKey         =  KbName('UpArrow');
    RightKey      =  KbName('RightArrow');
    LeftKey       =  KbName('LeftArrow');
   end
    PauseKey      =  KbName('Q');   
    RestartKey    =  KbName('R'); 
    abortKey      =  KbName('ESCAPE'); % ESC aborts experiment
    MEGbreakKey   =  KbName('F5');
    YesKey        =  KbName('Y');
    spaceBar      =  KbName('SPACE');
    MINIBLOCK_RESTART_KEY = KbName('M');
    BLOCK_RESTART_KEY = KbName('B');
    RESPONSE_KEY = upKey;
    ValidationKey = KbName('V');
    
    if strcmp(LAB_ID,'SC')
            delete(instrfind)
            bitsi_buttonbox = Bitsi_Scanner('com2');    %init button boxes
            bitsi_buttonbox.clearResponses();
           
            bitsi_scanner = Bitsi_Scanner('com3');  %init bitsis receiving scanTrigger
    end
    % the trigger codes for MEEG and EYE_TRACKER

    TRG_TIME_BETWEEN = 0.050; % seconds
    TRG_STIM_ADD = 20;
    TRG_CENTER = 101;
    TRG_LEFT = 102;
    TRG_RIGHT = 103;
    TRG_DUR_500 = 151;
    TRG_DUR_1000 = 152;
    TRG_DUR_1500 = 153;
    TRG_TASK_RELEVANT = 201;
    TRG_TASK_RELEVANT_NON_TARGET = 202;
    TRG_TASK_IRRELEVANT = 203;
    TRG_RESPONSE = 255;
    TRG_MINIBLOCK_STARTED = 84;
    TRG_EXP_START_MSG = 86;
    TRG_EXP_END_MSG = 90;
    TRG_STIM_END = 96;
    TRG_JITTER_START = 97;
    MB_ID_START_END_BUF = 222;
    TRG_MB_ADD = 160;
    TRG_TRIAL_ADD = 110;
    LPT_CODE_START = 81; 
    LPT_CODE_END = 83;
    
    % Trigger codes for audio trigger
    TRIGAUD_CODE_START = 109;
    TRG_RESPONSE_AUD = 110;
    TRG_EXP_END_MSG_AUD = 116;
    TRG_MBONSET_AUD = 110; % It is not the same as the TRG_RESPONSE_AUD: we add 1 so the lowest code will be 111
    
    % Stim id:
    
    matrix_LUT = zeros(4,3,3,3);

    % Fill with values from 1 to 108:
    ctr = 1;

    for cat=1:4
        for rel=1:3
            for ori=1:3
                for dur=1:3

                    matrix_LUT(cat,rel,ori,dur) = ctr;
                    ctr = ctr + 1;

                end
            end
        end
    end

    %% Sound parameters
    % Number of channels and Frequency of the sound
    NR_CHANNELS = 2;
    SAMPLE_RATE = 48000;
    SND_FREQ = 500;
    % How loud should the sound be
    VOLUME = 0.5;
    
    %%  PARAMETERS THAT SHOULD NOT BE ALTERED, BUT SHOULD BE USED AS REFERENCE

    % STIMULI CODING
    % stimuli are coded as a 4 digit number.
    % 1st digit = stimulus type
    % 2nd digit = stimulus orientation
    % 3rd & 4th digits = stimulus id
    % e.g. "1219" = 1 is face, 2 is left orientation and 19 is stimulus #
    FACE = 1000;  OBJECT = 2000; LETTER = 3000; FALSE_FONT = 4000; BLANK = 0000;
    CENTER = 100; LEFT = 200; RIGHT = 300;

    NUM_OF_TARGET_TYPES_PER_MINIBLOCK = 2;
    
    NUM_OF_MINIBLOCK_TYPE = 2; % there are 2 types of pairings between stimuli types

    % possible mini-block types
    FACE_OBJECT_MINIBLOCK = 1;
    CHAR_FALSE_MINIBLOCK = 2;
    
    % Folders
    DATA_FOLDER = 'data';
    CODE_FOLDER = 'code';
    FUNCTIONS_FOLDER = 'functions';
    TEMPORARY_FOLDER = 'temporary';
    SECRET_FOLDER = 'DONT OPEN! DEAD INSIDE!';
    
    % program codes
    ABORT_KEY = 4;
    RESTART_KEY=3;
    WRONG_KEY = 2;
    TARGET_KEY = 1; % to mark if up was pressed
    NO_KEY = 0;
   
    
    TRUE = 1;
    FALSE = 0;

    NO_TRIAL = nan;

    % the structure of the output table. Each line is an event, and for each
    % event there are columns of data as follows:
    EXP_COL = 1;
    EXP_D = 2;
    BLK_COL = 3;
    MINIBLK_COL = 4;
    TRAIL_COL = 5;
    MINIBLK_TYP_COL = 6;
    TARG1_COL = 7;
    TARG2_COL = 8;
    PLN_STIM_DUR_COL = 9;
    PLN_JIT_DUR_COL = 10;
    DSRD_RESP_COL = 11;
    EVENT_COL = 12;
    TIME_COL = 13;
    EVENT_TYPE_COL = 14;
    RT_COL = 15;
    HT_COL = 16;
    MS_COL = 17;
    CRS_COL = 18;
    FAS_COL = 19;
    ACCURATE_COL = 20;
    TYPE_COL = 21;
    ORIENTATION_COL = 22;
    CATEGORY_COL = 23;

    OUTPUT_COLS = 23;
    NUM_OF_POSSIBLE_EVENTS_IN_TRIAL = 5; % the number of possible events in each trial: stimuli, fixation, jitter, response, and save.
    OUTPUT_TABLE = cell(1 + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK*NUM_OF_MINI_BLOCKS_PER_BLOCK*NUM_OF_BLOCKS*NUM_OF_POSSIBLE_EVENTS_IN_TRIAL,OUTPUT_COLS);
    OUTPUT_TABLE_HEADER = {'expName','date','block','miniBlock','trial','miniBlockType','targ1','targ2','plndStimulusDur','plndJitterDur','dsrdResponse','event','time','eventType', 'rt','hit','miss','cr','fa','accurate','type','orientation','category'};
    TRIGGER_TABLE_HEADER = {'Trigger','TimeStamp','TriggerStatus'};
    OUTPUT_TABLE(1,1:OUTPUT_COLS) = OUTPUT_TABLE_HEADER;
    output_table_cntr = 2;

    % instruction slides addresses
    if(fMRI)
    INSTRUCTIONS1 = 'instructions1_fMRI.png';
    INSTRUCTIONS2 = 'instructions2_fMRI.png';
    INSTRUCTIONS3 = 'instructions3_fMRI.png';   
    elseif ECoG
    INSTRUCTIONS1 = 'instructions1_ECoG.png';
    INSTRUCTIONS2 = 'instructions2_ECoG.png';
    INSTRUCTIONS3 = 'instructions3_ECoG.png';   
    INSTRUCTIONS4 = 'instructions4_ECoG.png';   
    INSTRUCTIONS5 = 'instructions5_ECoG.png';   
    INSTRUCTIONS6 = 'instructions6_ECoG.png';
    elseif MEEG
        switch LAB_ID
            case 'SA'
                INSTRUCTIONS1 = 'instructions1_MEEG_SA.png';
                INSTRUCTIONS2 = 'instructions2_MEEG_SA.png';
                INSTRUCTIONS3 = 'instructions3_MEEG_SA.png';
            case 'SB'
                INSTRUCTIONS1 = 'instructions1_MEEG_SB.png';
                INSTRUCTIONS2 = 'instructions2_MEEG_SB.png';
                INSTRUCTIONS3 = 'instructions3_MEEG_SB.png';
        end
    else
    INSTRUCTIONS1 = 'instructions1.png';
    INSTRUCTIONS2 = 'instructions2.png';
    INSTRUCTIONS3 = 'instructions3.png';
    end
%     INSTRUCTIONS4 = 'Instructions4.png';
    % stimuli folders addresses
    STIM_FOLDER = 'stimuli';
    FACES_FOLDER = 'faces';
    CHARS_FOLDER = 'chars';
    FALSES_FOLDER = 'falses';
    OBJECTS_FOLDER = 'objects';
    MALE_FOLDER = 'male';
    FEMALE_FOLDER = 'female';
    FIXATION_FILE = 'fixation.png';
    INSTRUCTIONS_FOLDER = 'instructions';

    % stimuli folders for each type of stimuli by left, right and center
    FACES_L_FOL = fullfile(pwd,STIM_FOLDER,FACES_FOLDER,'left');
    FACES_C_FOL = fullfile(pwd,STIM_FOLDER,FACES_FOLDER,'center');
    FACES_R_FOL = fullfile(pwd,STIM_FOLDER,FACES_FOLDER,'right');
    CHARS_L_FOL = fullfile(pwd,STIM_FOLDER,CHARS_FOLDER,'left');
    CHARS_C_FOL = fullfile(pwd,STIM_FOLDER,CHARS_FOLDER,'center');
    CHARS_R_FOL = fullfile(pwd,STIM_FOLDER,CHARS_FOLDER,'right');
    FALSES_L_FOL = fullfile(pwd,STIM_FOLDER,FALSES_FOLDER,'left');
    FALSES_C_FOL = fullfile(pwd,STIM_FOLDER,FALSES_FOLDER,'center');
    FALSES_R_FOL = fullfile(pwd,STIM_FOLDER,FALSES_FOLDER,'right');
    OBJECTS_L_FOL = fullfile(pwd,STIM_FOLDER,OBJECTS_FOLDER,'left');
    OBJECTS_C_FOL = fullfile(pwd,STIM_FOLDER,OBJECTS_FOLDER,'center');
    OBJECTS_R_FOL = fullfile(pwd,STIM_FOLDER,OBJECTS_FOLDER,'right');
    
    ANIMAL_REWARD_FOL = fullfile(pwd,'Pictures_between_miniblocks_exp1');

    % the main data structure "miniBlocks": a cell matrix, with a row for each mini-block
    % and with its columns as follows:
    DATA_TABLE_SIZE = 430;
    BLOCK_NUM_COL = 1;
    SUBJECT_NUMBER_COL = 2;
    SUBJECT_START_TIME = 3;
    MINIBLOCK_TYPE_COL = 4;
    TARGET1_COL = 5;
    TARGET2_COL = 6;
    MINI_BLOCK_SIZE_COL = 7;
    MISSES_COL = 8;
    HITS_COL = 9;
    FA_COL = 10;
    CR_COL = 11;
    MINI_BLK_NUM_COL = 12;
    TRIAL1_NAME_COL = 13;
    TRIAL1_TIME_COL = TRIAL1_NAME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_START_TIME_COL = TRIAL1_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_STIM_END_TIME_COL = TRIAL1_START_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_DURATION_COL = TRIAL1_STIM_END_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_JITTER_TIME_COL = TRIAL1_DURATION_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_BUTTON_PRESS_COL = TRIAL1_JITTER_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_ANSWER_COL = TRIAL1_BUTTON_PRESS_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_RESPONSE_TIME_COL = TRIAL1_ANSWER_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_STIM_DUR_COL = TRIAL1_RESPONSE_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;
    TRIAL1_BLANK_DUR_COL = TRIAL1_STIM_DUR_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK;

    %% Practice parameters :
    % the practice is hard coded in practice.mat, which will take the 2
    % first stimuli in stimuli/practice/left|right|center, as targets for
    % the practice run. Practice pictures are coded as 8000 in the program's inner data
    % structure.
    PRACTICE_MINIBLOCK_MAT = 'practice.mat';
    PRACTICE_START_MESSAGE = 'The practice starts now.\n\n Press SPACE to continue...';
    PRACTICE_START_MESSAGE_ECOG = {'The practice starts now.', 'Press any button to continue...'};

    PRACTICE_START_MESSAGE_fMRI = 'The practice starts now.';
    PRACTICE = 8000;
    PRACTICE_L_FOL = fullfile(pwd,STIM_FOLDER,'practice','left');
    PRACTICE_R_FOL = fullfile(pwd,STIM_FOLDER,'practice','right');
    PRACTICE_C_FOL = fullfile(pwd,STIM_FOLDER,'practice','center');
    
    MinPracticeHits=4;
    MaxPracticeHits=6;
    MaxPracticeFalseAlarms=2;
    MaxPracticeFalseAlarms_Irrelevant=0;
    MinPracticeHits_fMRI=2;
    MaxPracticeHits_fMRI=3;
    MaxPracticeFalseAlarms_fMRI=1;
    MaxPracticeFalseAlarms_Irrelevant_fMRI=0;

    %% PSEUDORANDOMIZATION OF NON-TARGETS FOR EACH MINIBLOCK %%%%%
    
    % Even gets version_non_targets 0, odd gets version_non_targets 1
    version_non_targets = mod(int64(subNum),2);
    display(version_non_targets, 'version_non_targets');

    % The keys are the target IDs and the values are the non-targets
    % For every miniblock, 
    % These have been generated with generatePseudoversions.
    
    NON_TARGETS_FACES_PER_MB = containers.Map;
    NON_TARGETS_OBJECTS_PER_MB = containers.Map;
    NON_TARGETS_CHARS_PER_MB = containers.Map;
    NON_TARGETS_FALSES_PER_MB = containers.Map;
    
    
    if version_non_targets == 0
        
        if VERBOSE disp('Inside version_non_targets 0'); end
        if(MEEG||Behavior)
            % FACE / OBJECT miniblocks
            NON_TARGETS_FACES_PER_MB('1001') = [1020,1009,1010,1013,1017,1004,1009,1003];
            NON_TARGETS_FACES_PER_MB('1002') = [1016,1007,1015,1001,1015,1009,1010, 1007];
            NON_TARGETS_FACES_PER_MB('1003') = [1006,1001,1001,1020,1013,1001,1019,1015];
            NON_TARGETS_FACES_PER_MB('1004') = [1010,1012,1006,1002,1008,1001,1014,1016];
            NON_TARGETS_FACES_PER_MB('1005') = [1018,1007,1011,1016,1007,1011,1016,1008];
            NON_TARGETS_FACES_PER_MB('1006') = [1012,1010,1002,1005,1008,1017,1019,1007];
            NON_TARGETS_FACES_PER_MB('1007') = [1017,1011,1010,1012,1013,1009,1019,1010];
            NON_TARGETS_FACES_PER_MB('1008') = [1003,1009,1019,1014,1012,1016,1013,1012];
            NON_TARGETS_FACES_PER_MB('1009') = [1017,1015,1018,1017,1011,1011,1018,1006];
            NON_TARGETS_FACES_PER_MB('1010') = [1002,1020,1019,1005,1004,1011,1018,1012];
            NON_TARGETS_FACES_PER_MB('1011') = [1016,1013,1001,1013,1020,1014,1005,1014];
            NON_TARGETS_FACES_PER_MB('1012') = [1010,1005,1020,1005,1014,1017,1003,1004];
            NON_TARGETS_FACES_PER_MB('1013') = [1002,1002,1007,1015,1018,1003,1018,1006];
            NON_TARGETS_FACES_PER_MB('1014') = [1005,1018,1004,1009,1006,1019,1003,1008];
            NON_TARGETS_FACES_PER_MB('1015') = [1020,1007,1019,1005,1013,1008,1006,1004];
            NON_TARGETS_FACES_PER_MB('1016') = [1007,1013,1003,1004,1008,1002,1002,1014];
            NON_TARGETS_FACES_PER_MB('1017') = [1015,1015,1002,1003,1009,1006,1001,1019];
            NON_TARGETS_FACES_PER_MB('1018') = [1005,1016,1012,1010,1020,1020,1006,1012];
            NON_TARGETS_FACES_PER_MB('1019') = [1011,1009,1011,1008,1001,1014,1018,1004];
            NON_TARGETS_FACES_PER_MB('1020') = [1008,1017,1014,1003,1015,1016,1004,1017];
            
            
            NON_TARGETS_OBJECTS_PER_MB('2001') = [2012,2007,2016,2007,2013,2009,2009,2017];
            NON_TARGETS_OBJECTS_PER_MB('2002') = [2006,2017,2003,2019,2009,2020,2005,2005];
            NON_TARGETS_OBJECTS_PER_MB('2003') = [2009,2014,2001,2018,2020,2020,2007,2002];
            NON_TARGETS_OBJECTS_PER_MB('2004') = [2018,2003,2011,2013,2016,2007,2007,2012];
            NON_TARGETS_OBJECTS_PER_MB('2005') = [2014,2003,2019,2016,2004,2002,2019,2015];
            NON_TARGETS_OBJECTS_PER_MB('2006') = [2010,2011,2015,2013,2014,2008,2009,2017];
            NON_TARGETS_OBJECTS_PER_MB('2007') = [2004,2019,2008,2001,2003,2020,2017,2013];
            NON_TARGETS_OBJECTS_PER_MB('2008') = [2005,2003,2001,2014,2015,2015,2009,2002];
            NON_TARGETS_OBJECTS_PER_MB('2009') = [2002,2012,2005,2019,2007,2004,2012,2012];
            NON_TARGETS_OBJECTS_PER_MB('2010') = [2016,2006,2018,2020,2014,2008,2003,2001];
            NON_TARGETS_OBJECTS_PER_MB('2011') = [2015,2019,2019,2015,2008,2018,2016,2001];
            NON_TARGETS_OBJECTS_PER_MB('2012') = [2018,2015,2010,2017,2013,2002,2002,2009];
            NON_TARGETS_OBJECTS_PER_MB('2013') = [2007,2018,2020,2005,2011,2012,2019,2015];
            NON_TARGETS_OBJECTS_PER_MB('2014') = [2010,2020,2017,2008,2010,2004,2005,2006];
            NON_TARGETS_OBJECTS_PER_MB('2015') = [2014,2014,2008,2011,2005,2003,2003,2018];
            NON_TARGETS_OBJECTS_PER_MB('2016') = [2010,2011,2012,2001,2017,2005,2006,2006];
            NON_TARGETS_OBJECTS_PER_MB('2017') = [2004,2008,2006,2013,2011,2002,2008,2014];
            NON_TARGETS_OBJECTS_PER_MB('2018') = [2010,2010,2001,2006,2001,2004,2011,2016];
            NON_TARGETS_OBJECTS_PER_MB('2019') = [2004,2009,2007,2013,2006,2020,2010,2018];
            NON_TARGETS_OBJECTS_PER_MB('2020') = [2002,2012,2004,2017,2016,2011,2013,2016];
            
            % LETTER / FALSE-FONT miniblocks
            
            
            NON_TARGETS_FALSES_PER_MB('4001') = [4017,4015,4016,4013,4018,4008,4012,4005];
            NON_TARGETS_FALSES_PER_MB('4002') = [4014,4009,4010,4007,4017,4018,4004,4019];
            NON_TARGETS_FALSES_PER_MB('4003') = [4019,4004,4008,4007,4002,4006,4016,4020];
            NON_TARGETS_FALSES_PER_MB('4004') = [4012,4003,4013,4001,4010,4018,4020,4005];
            NON_TARGETS_FALSES_PER_MB('4005') = [4012,4012,4016,4010,4019,4020,4009,4003];
            NON_TARGETS_FALSES_PER_MB('4006') = [4004,4017,4009,4020,4019,4016,4012,4007];
            NON_TARGETS_FALSES_PER_MB('4007') = [4006,4003,4016,4001,4017,4001,4001,4015];
            NON_TARGETS_FALSES_PER_MB('4008') = [4011,4015,4015,4003,4016,4005,4004,4014];
            NON_TARGETS_FALSES_PER_MB('4009') = [4006,4003,4010,4014,4008,4010,4011,4007];
            NON_TARGETS_FALSES_PER_MB('4010') = [4002,4012,4005,4015,4011,4003,4011,4007];
            NON_TARGETS_FALSES_PER_MB('4011') = [4002,4002,4013,4013,4005,4010,4006,4015];
            NON_TARGETS_FALSES_PER_MB('4012') = [4017,4001,4014,4008,4019,4007,4019,4005];
            NON_TARGETS_FALSES_PER_MB('4013') = [4014,4002,4012,4001,4006,4009,4010,4020];
            NON_TARGETS_FALSES_PER_MB('4014') = [4002,4018,4013,4013,4009,4006,4008,4010];
            NON_TARGETS_FALSES_PER_MB('4015') = [4003,4017,4004,4014,4002,4011,4020,4013];
            NON_TARGETS_FALSES_PER_MB('4016') = [4011,4002,4015,4007,4018,4011,4003,4006];
            NON_TARGETS_FALSES_PER_MB('4017') = [4008,4014,4008,4019,4004,4018,4016,4015];
            NON_TARGETS_FALSES_PER_MB('4018') = [4017,4001,4014,4005,4004,4020,4001,4013];
            NON_TARGETS_FALSES_PER_MB('4019') = [4009,4016,4006,4012,4017,4007,4020,4009];
            NON_TARGETS_FALSES_PER_MB('4020') = [4011,4018,4004,4008,4019,4009,4018,4005];
            
            
            NON_TARGETS_CHARS_PER_MB('3001') = [3019,3008,3004,3014,3012,3004,3004,3009];
            NON_TARGETS_CHARS_PER_MB('3002') = [3015,3010,3013,3014,3018,3020,3006,3016];
            NON_TARGETS_CHARS_PER_MB('3003') = [3012,3011,3009,3001,3011,3007,3002,3017];
            NON_TARGETS_CHARS_PER_MB('3004') = [3005,3006,3001,3009,3009,3010,3010,3012];
            NON_TARGETS_CHARS_PER_MB('3005') = [3018,3001,3006,3016,3004,3014,3011,3018];
            NON_TARGETS_CHARS_PER_MB('3006') = [3020,3014,3018,3011,3018,3019,3010,3015];
            NON_TARGETS_CHARS_PER_MB('3007') = [3003,3016,3001,3017,3013,3004,3008,3014];
            NON_TARGETS_CHARS_PER_MB('3008') = [3011,3003,3003,3017,3001,3019,3017,3005];
            NON_TARGETS_CHARS_PER_MB('3009') = [3020,3017,3019,3018,3008,3015,3011,3014];
            NON_TARGETS_CHARS_PER_MB('3010') = [3007,3020,3013,3008,3019,3011,3012,3005];
            NON_TARGETS_CHARS_PER_MB('3011') = [3007,3019,3007,3002,3007,3013,3015,3004];
            NON_TARGETS_CHARS_PER_MB('3012') = [3006,3013,3016,3006,3016,3005,3002,3019];
            NON_TARGETS_CHARS_PER_MB('3013') = [3009,3006,3015,3002,3017,3014,3015,3012];
            NON_TARGETS_CHARS_PER_MB('3014') = [3011,3016,3018,3009,3007,3010,3015,3013];
            NON_TARGETS_CHARS_PER_MB('3015') = [3003,3008,3010,3019,3001,3002,3016,3003];
            NON_TARGETS_CHARS_PER_MB('3016') = [3003,3012,3017,3009,3005,3002,3005,3006];
            NON_TARGETS_CHARS_PER_MB('3017') = [3015,3018,3008,3020,3009,3004,3020,3002];
            NON_TARGETS_CHARS_PER_MB('3018') = [3016,3002,3014,3003,3003,3001,3007,3020];
            NON_TARGETS_CHARS_PER_MB('3019') = [3020,3010,3006,3005,3013,3008,3005,3013];
            NON_TARGETS_CHARS_PER_MB('3020') = [3001,3008,3004,3012,3012,3007,3010,3017];
            
            
        elseif fMRI
            % FACE / OBJECT miniblocks
            
            NON_TARGETS_FACES_PER_MB('1011') = [1019,1014,1015,1013];
            NON_TARGETS_FACES_PER_MB('1012') = [1008,1003,1013,1018];
            NON_TARGETS_FACES_PER_MB('1003') = [1001,1005,1008,1002];
            NON_TARGETS_FACES_PER_MB('1009') = [1006,1018,1012,1019];
            NON_TARGETS_FACES_PER_MB('1004') = [1017,1013,1017,1009];
            NON_TARGETS_FACES_PER_MB('1002') = [1015,1014,1017,1014];
            NON_TARGETS_FACES_PER_MB('1014') = [1019,1019,1003,1005];
            NON_TARGETS_FACES_PER_MB('1017') = [1006,1011,1001,1011];
            NON_TARGETS_FACES_PER_MB('1006') = [1002,1012,1009,1002];
            NON_TARGETS_FACES_PER_MB('1013') = [1005,1018,1004,1001];
            NON_TARGETS_FACES_PER_MB('1018') = [1012,1001,1002,1013];
            NON_TARGETS_FACES_PER_MB('1001') = [1015,1009,1006,1015];
            NON_TARGETS_FACES_PER_MB('1019') = [1011,1008,1003,1005];
            NON_TARGETS_FACES_PER_MB('1008') = [1011,1014,1017,1004];
            NON_TARGETS_FACES_PER_MB('1015') = [1008,1018,1009,1012];
            NON_TARGETS_FACES_PER_MB('1005') = [1003,1004,1006,1004];
            
            NON_TARGETS_OBJECTS_PER_MB('2006') = [2008,2004,2014,2014];
            NON_TARGETS_OBJECTS_PER_MB('2014') = [2012,2010,2012,2002];
            NON_TARGETS_OBJECTS_PER_MB('2001') = [2016,2015,2004,2009];
            NON_TARGETS_OBJECTS_PER_MB('2007') = [2006,2014,2010,2011];
            NON_TARGETS_OBJECTS_PER_MB('2002') = [2015,2013,2003,2013];
            NON_TARGETS_OBJECTS_PER_MB('2004') = [2001,2003,2001,2012];
            NON_TARGETS_OBJECTS_PER_MB('2009') = [2007,2013,2014,2010];
            NON_TARGETS_OBJECTS_PER_MB('2010') = [2008,2006,2007,2011];
            NON_TARGETS_OBJECTS_PER_MB('2012') = [2015,2011,2009,2006];
            NON_TARGETS_OBJECTS_PER_MB('2003') = [2005,2013,2007,2004];
            NON_TARGETS_OBJECTS_PER_MB('2005') = [2002,2016,2007,2015];
            NON_TARGETS_OBJECTS_PER_MB('2008') = [2016,2005,2001,2010];
            NON_TARGETS_OBJECTS_PER_MB('2015') = [2016,2008,2003,2009];
            NON_TARGETS_OBJECTS_PER_MB('2013') = [2005,2012,2001,2009];
            NON_TARGETS_OBJECTS_PER_MB('2011') = [2008,2002,2004,2003];
            NON_TARGETS_OBJECTS_PER_MB('2016') = [2011,2002,2006,2005];
            
            % LETTER / FALSE-FONT miniblocks
            
            NON_TARGETS_FALSES_PER_MB('4006') = [4008,4010,4018,4009];
            NON_TARGETS_FALSES_PER_MB('4020') = [4009,4003,4018,4007];
            NON_TARGETS_FALSES_PER_MB('4015') = [4010,4010,4014,4011];
            NON_TARGETS_FALSES_PER_MB('4013') = [4007,4020,4009,4006];
            NON_TARGETS_FALSES_PER_MB('4016') = [4006,4020,4005,4013];
            NON_TARGETS_FALSES_PER_MB('4012') = [4016,4005,4013,4001];
            NON_TARGETS_FALSES_PER_MB('4014') = [4011,4015,4006,4012];
            NON_TARGETS_FALSES_PER_MB('4007') = [4014,4012,4015,4008];
            NON_TARGETS_FALSES_PER_MB('4011') = [4006,4020,4007,4016];
            NON_TARGETS_FALSES_PER_MB('4010') = [4016,4018,4016,4012];
            NON_TARGETS_FALSES_PER_MB('4008') = [4018,4007,4005,4012];
            NON_TARGETS_FALSES_PER_MB('4003') = [4020,4008,4001,4014];
            NON_TARGETS_FALSES_PER_MB('4001') = [4013,4003,4003,4013];
            NON_TARGETS_FALSES_PER_MB('4018') = [4001,4001,4011,4010];
            NON_TARGETS_FALSES_PER_MB('4009') = [4005,4003,4015,4015];
            NON_TARGETS_FALSES_PER_MB('4005') = [4008,4014,4011,4009];
            
            NON_TARGETS_CHARS_PER_MB('3010') = [3012,3012,3014,3008];
            NON_TARGETS_CHARS_PER_MB('3011') = [3020,3015,3013,3018];
            NON_TARGETS_CHARS_PER_MB('3009') = [3005,3015,3020,3010];
            NON_TARGETS_CHARS_PER_MB('3016') = [3012,3005,3001,3018];
            NON_TARGETS_CHARS_PER_MB('3005') = [3011,3009,3011,3011];
            NON_TARGETS_CHARS_PER_MB('3003') = [3010,3010,3013,3011];
            NON_TARGETS_CHARS_PER_MB('3013') = [3009,3005,3016,3012];
            NON_TARGETS_CHARS_PER_MB('3018') = [3014,3001,3006,3009];
            NON_TARGETS_CHARS_PER_MB('3015') = [3013,3013,3010,3008];
            NON_TARGETS_CHARS_PER_MB('3007') = [3016,3014,3020,3008];
            NON_TARGETS_CHARS_PER_MB('3006') = [3015,3008,3015,3003];
            NON_TARGETS_CHARS_PER_MB('3014') = [3016,3006,3006,3003];
            NON_TARGETS_CHARS_PER_MB('3020') = [3006,3003,3016,3007];
            NON_TARGETS_CHARS_PER_MB('3001') = [3018,3005,3007,3009];
            NON_TARGETS_CHARS_PER_MB('3012') = [3014,3007,3003,3007];
            NON_TARGETS_CHARS_PER_MB('3008') = [3001,3001,3020,3018];
            
        elseif ECoG
            
            % FACE / OBJECT miniblocks
            NON_TARGETS_FACES_PER_MB('1002') = [1011,1020,1009,1010,1016,1017,1018,1001];
            NON_TARGETS_FACES_PER_MB('1011') = [1019,1005,1006,1013,1016,1002,1019,1008];
            NON_TARGETS_FACES_PER_MB('1014') = [1017,1004,1020,1007,1015,1013,1011,1016];
            NON_TARGETS_FACES_PER_MB('1018') = [1007,1011,1004,1017,1008,1008,1015,1012];
            NON_TARGETS_FACES_PER_MB('1009') = [1018,1003,1003,1014,1014,1001,1010,1018];
            NON_TARGETS_FACES_PER_MB('1004') = [1008,1017,1001,1005,1003,1020,1006,1013];
            NON_TARGETS_FACES_PER_MB('1012') = [1002,1007,1001,1005,1006,1005,1015,1014];
            NON_TARGETS_FACES_PER_MB('1015') = [1010,1009,1003,1009,1014,1011,1016,1019];
            NON_TARGETS_FACES_PER_MB('1001') = [1002,1012,1002,1010,1020,1004,1006,1012];
            NON_TARGETS_FACES_PER_MB('1005') = [1019,1018,1015,1009,1012,1004,1013,1007];
            
            NON_TARGETS_OBJECTS_PER_MB('2008') = [2012,2011,2004,2013,2010,2014,2020,2005];
            NON_TARGETS_OBJECTS_PER_MB('2003') = [2013,2017,2016,2018,2017,2006,2012,2014];
            NON_TARGETS_OBJECTS_PER_MB('2005') = [2001,2004,2006,2020,2013,2020,2016,2014];
            NON_TARGETS_OBJECTS_PER_MB('2004') = [2015,2003,2011,2016,2009,2010,2010,2008];
            NON_TARGETS_OBJECTS_PER_MB('2007') = [2016,2014,2019,2001,2002,2012,2018,2008];
            NON_TARGETS_OBJECTS_PER_MB('2001') = [2004,2009,2013,2009,2005,2008,2010,2005];
            NON_TARGETS_OBJECTS_PER_MB('2006') = [2015,2019,2012,2008,2003,2002,2003,2018];
            NON_TARGETS_OBJECTS_PER_MB('2009') = [2007,2019,2015,2001,2006,2001,2011,2004];
            NON_TARGETS_OBJECTS_PER_MB('2002') = [2005,2018,2009,2017,2019,2006,2007,2007];
            NON_TARGETS_OBJECTS_PER_MB('2010') = [2017,2011,2003,2002,2015,2020,2002,2007];
            
            % CHAR / FALSE miniblocks
            NON_TARGETS_CHARS_PER_MB('3001') = [3020,3011,3002,3016,3002,3015,3014,3006];
            NON_TARGETS_CHARS_PER_MB('3015') = [3014,3006,3020,3004,3010,3013,3007,3010];
            NON_TARGETS_CHARS_PER_MB('3007') = [3008,3012,3016,3003,3002,3019,3020,3014];
            NON_TARGETS_CHARS_PER_MB('3011') = [3015,3001,3018,3004,3001,3013,3012,3006];
            NON_TARGETS_CHARS_PER_MB('3013') = [3018,3017,3004,3009,3018,3005,3012,3005];
            NON_TARGETS_CHARS_PER_MB('3010') = [3009,3004,3007,3011,3014,3008,3015,3006];
            NON_TARGETS_CHARS_PER_MB('3019') = [3003,3007,3007,3017,3001,3003,3012,3010];
            NON_TARGETS_CHARS_PER_MB('3005') = [3008,3010,3002,3013,3008,3011,3009,3018];
            NON_TARGETS_CHARS_PER_MB('3008') = [3016,3016,3005,3001,3009,3019,3003,3013];
            NON_TARGETS_CHARS_PER_MB('3003') = [3019,3011,3015,3017,3019,3005,3017,3020];
            
            NON_TARGETS_FALSES_PER_MB('4019') = [4018,4012,4008,4013,4003,4004,4010,4007];
            NON_TARGETS_FALSES_PER_MB('4003') = [4018,4004,4009,4019,4014,4017,4004,4005];
            NON_TARGETS_FALSES_PER_MB('4005') = [4003,4006,4017,4009,4010,4008,4016,4015];
            NON_TARGETS_FALSES_PER_MB('4007') = [4019,4016,4006,4012,4004,4011,4002,4018];
            NON_TARGETS_FALSES_PER_MB('4015') = [4007,4017,4010,4014,4007,4005,4006,4002];
            NON_TARGETS_FALSES_PER_MB('4011') = [4019,4017,4001,4003,4020,4009,4016,4014];
            NON_TARGETS_FALSES_PER_MB('4013') = [4006,4015,4015,4020,4020,4008,4003,4018];
            NON_TARGETS_FALSES_PER_MB('4010') = [4013,4013,4009,4005,4001,4007,4011,4012];
            NON_TARGETS_FALSES_PER_MB('4001') = [4008,4002,4016,4011,4012,4011,4005,4020];
            NON_TARGETS_FALSES_PER_MB('4008') = [4019,4015,4001,4002,4014,4001,4013,4010];
        end
    else % version_non_targets 1
        
        disp('Inside version_non_targets 1');
        if(MEEG||Behavior)
            % FACE / OBJECT miniblocks
            NON_TARGETS_FACES_PER_MB('1002') = [1013,1011,1020,1018,1001,1018,1009,1013];
            NON_TARGETS_FACES_PER_MB('1011') = [1008,1009,1008,1006,1016,1004,1005,1012];
            NON_TARGETS_FACES_PER_MB('1006') = [1014,1016,1012,1008,1010,1020,1009,1012];
            NON_TARGETS_FACES_PER_MB('1020') = [1014,1016,1004,1007,1018,1014,1019,1018];
            NON_TARGETS_FACES_PER_MB('1008') = [1020,1009,1001,1019,1006,1015,1020,1013];
            NON_TARGETS_FACES_PER_MB('1001') = [1010,1009,1013,1013,1017,1013,1018,1007];
            NON_TARGETS_FACES_PER_MB('1004') = [1006,1010,1020,1019,1015,1001,1003,1006];
            NON_TARGETS_FACES_PER_MB('1015') = [1016,1010,1009,1020,1004,1009,1018,1005];
            NON_TARGETS_FACES_PER_MB('1012') = [1014,1019,1016,1003,1006,1007,1011,1015];
            NON_TARGETS_FACES_PER_MB('1014') = [1013,1010,1008,1001,1010,1006,1017,1002];
            NON_TARGETS_FACES_PER_MB('1013') = [1014,1016,1011,1005,1009,1002,1012,1010];
            NON_TARGETS_FACES_PER_MB('1005') = [1007,1011,1012,1001,1014,1017,1004,1003];
            NON_TARGETS_FACES_PER_MB('1017') = [1001,1007,1008,1019,1016,1004,1006,1002];
            NON_TARGETS_FACES_PER_MB('1003') = [1004,1014,1012,1005,1017,1002,1012,1015];
            NON_TARGETS_FACES_PER_MB('1018') = [1013,1017,1017,1005,1017,1005,1003,1014];
            NON_TARGETS_FACES_PER_MB('1009') = [1015,1003,1006,1011,1001,1011,1015,1007];
            NON_TARGETS_FACES_PER_MB('1016') = [1004,1019,1019,1011,1003,1015,1002,1002];
            NON_TARGETS_FACES_PER_MB('1019') = [1002,1004,1003,1002,1003,1020,1015,1005];
            NON_TARGETS_FACES_PER_MB('1007') = [1001,1018,1011,1008,1008,1008,1010,1012];
            NON_TARGETS_FACES_PER_MB('1010') = [1007,1018,1005,1020,1017,1019,1007,1016];
            
            NON_TARGETS_OBJECTS_PER_MB('2011') = [2018,2012,2017,2015,2009,2010,2007,2005];
            NON_TARGETS_OBJECTS_PER_MB('2002') = [2004,2008,2003,2013,2001,2008,2020,2003];
            NON_TARGETS_OBJECTS_PER_MB('2004') = [2018,2016,2019,2016,2020,2012,2016,2006];
            NON_TARGETS_OBJECTS_PER_MB('2019') = [2012,2003,2008,2006,2002,2006,2005,2007];
            NON_TARGETS_OBJECTS_PER_MB('2016') = [2013,2002,2006,2019,2017,2004,2020,2014];
            NON_TARGETS_OBJECTS_PER_MB('2006') = [2014,2018,2004,2016,2017,2020,2009,2015];
            NON_TARGETS_OBJECTS_PER_MB('2020') = [2001,2007,2011,2007,2019,2018,2006,2019];
            NON_TARGETS_OBJECTS_PER_MB('2003') = [2014,2012,2009,2001,2005,2013,2010,2005];
            NON_TARGETS_OBJECTS_PER_MB('2001') = [2004,2017,2006,2010,2002,2010,2018,2009];
            NON_TARGETS_OBJECTS_PER_MB('2015') = [2014,2020,2010,2002,2007,2013,2012,2008];
            NON_TARGETS_OBJECTS_PER_MB('2013') = [2004,2004,2008,2006,2008,2016,2007,2019];
            NON_TARGETS_OBJECTS_PER_MB('2010') = [2014,2006,2005,2020,2014,2001,2011,2019];
            NON_TARGETS_OBJECTS_PER_MB('2012') = [2013,2016,2010,2004,2016,2009,2002,2003];
            NON_TARGETS_OBJECTS_PER_MB('2017') = [2008,2013,2012,2018,2008,2009,2010,2011];
            NON_TARGETS_OBJECTS_PER_MB('2009') = [2020,2012,2018,2015,2015,2001,2011,2017];
            NON_TARGETS_OBJECTS_PER_MB('2007') = [2001,2003,2003,2002,2003,2012,2019,2014];
            NON_TARGETS_OBJECTS_PER_MB('2008') = [2011,2009,2013,2015,2015,2001,2009,2015];
            NON_TARGETS_OBJECTS_PER_MB('2018') = [2007,2011,2004,2002,2005,2007,2005,2001];
            NON_TARGETS_OBJECTS_PER_MB('2014') = [2017,2005,2011,2017,2019,2011,2013,2017];
            NON_TARGETS_OBJECTS_PER_MB('2005') = [2002,2014,2018,2020,2015,2003,2010,2016];
            
            NON_TARGETS_CHARS_PER_MB('3006') = [3001,3002,3010,3018,3014,3009,3010,3018];
            NON_TARGETS_CHARS_PER_MB('3009') = [3006,3014,3005,3010,3010,3012,3012,3006];
            NON_TARGETS_CHARS_PER_MB('3002') = [3008,3017,3020,3001,3020,3009,3018,3003];
            NON_TARGETS_CHARS_PER_MB('3007') = [3013,3005,3002,3020,3008,3015,3005,3002];
            NON_TARGETS_CHARS_PER_MB('3020') = [3011,3005,3002,3005,3002,3007,3008,3001];
            NON_TARGETS_CHARS_PER_MB('3001') = [3002,3017,3008,3020,3008,3004,3012,3014];
            NON_TARGETS_CHARS_PER_MB('3016') = [3017,3017,3015,3009,3019,3012,3003,3003];
            NON_TARGETS_CHARS_PER_MB('3014') = [3015,3009,3004,3012,3011,3016,3004,3004];
            NON_TARGETS_CHARS_PER_MB('3010') = [3017,3019,3016,3006,3001,3011,3019,3013];
            NON_TARGETS_CHARS_PER_MB('3015') = [3006,3012,3002,3018,3013,3018,3010,3007];
            NON_TARGETS_CHARS_PER_MB('3018') = [3016,3001,3011,3020,3012,3009,3013,3002];
            NON_TARGETS_CHARS_PER_MB('3013') = [3007,3015,3009,3016,3018,3019,3004,3001];
            NON_TARGETS_CHARS_PER_MB('3017') = [3015,3019,3004,3014,3003,3003,3016,3001];
            NON_TARGETS_CHARS_PER_MB('3011') = [3007,3014,3004,3007,3006,3007,3009,3005];
            NON_TARGETS_CHARS_PER_MB('3012') = [3010,3006,3016,3006,3016,3003,3020,3008];
            NON_TARGETS_CHARS_PER_MB('3005') = [3017,3014,3011,3001,3016,3013,3017,3014];
            NON_TARGETS_CHARS_PER_MB('3004') = [3007,3006,3005,3005,3015,3019,3018,3008];
            NON_TARGETS_CHARS_PER_MB('3003') = [3008,3011,3010,3004,3020,3015,3007,3020];
            NON_TARGETS_CHARS_PER_MB('3019') = [3003,3010,3009,3017,3013,3011,3013,3011];
            NON_TARGETS_CHARS_PER_MB('3008') = [3014,3003,3015,3013,3018,3019,3012,3019];
            
            NON_TARGETS_FALSES_PER_MB('4019') = [4016,4015,4006,4001,4006,4014,4018,4016];
            NON_TARGETS_FALSES_PER_MB('4005') = [4017,4008,4003,4008,4017,4008,4009,4008];
            NON_TARGETS_FALSES_PER_MB('4008') = [4019,4010,4014,4019,4019,4001,4013,4005];
            NON_TARGETS_FALSES_PER_MB('4006') = [4017,4014,4001,4007,4005,4020,4007,4002];
            NON_TARGETS_FALSES_PER_MB('4002') = [4012,4007,4018,4003,4005,4015,4004,4015];
            NON_TARGETS_FALSES_PER_MB('4004') = [4020,4009,4009,4019,4002,4018,4018,4008];
            NON_TARGETS_FALSES_PER_MB('4016') = [4010,4018,4009,4003,4013,4003,4012,4004];
            NON_TARGETS_FALSES_PER_MB('4009') = [4004,4017,4006,4011,4002,4003,4005,4016];
            NON_TARGETS_FALSES_PER_MB('4001') = [4013,4011,4018,4010,4006,4015,4005,4002];
            NON_TARGETS_FALSES_PER_MB('4007') = [4020,4010,4014,4011,4010,4020,4006,4017];
            NON_TARGETS_FALSES_PER_MB('4020') = [4001,4007,4012,4011,4009,4003,4014,4014];
            NON_TARGETS_FALSES_PER_MB('4013') = [4006,4018,4019,4018,4008,4015,4016,4012];
            NON_TARGETS_FALSES_PER_MB('4012') = [4014,4009,4001,4004,4019,4005,4014,4005];
            NON_TARGETS_FALSES_PER_MB('4014') = [4002,4015,4003,4019,4011,4017,4016,4001];
            NON_TARGETS_FALSES_PER_MB('4015') = [4020,4007,4008,4002,4013,4008,4012,4013];
            NON_TARGETS_FALSES_PER_MB('4017') = [4015,4004,4003,4007,4002,4016,4013,4012];
            NON_TARGETS_FALSES_PER_MB('4011') = [4020,4015,4012,4001,4007,4001,4006,4010];
            NON_TARGETS_FALSES_PER_MB('4018') = [4017,4020,4012,4011,4010,4004,4016,4004];
            NON_TARGETS_FALSES_PER_MB('4010') = [4011,4005,4017,4007,4011,4004,4009,4009];
            NON_TARGETS_FALSES_PER_MB('4003') = [4006,4002,4013,4010,4013,4019,4016,4020];
            
        elseif fMRI
            % FACE / OBJECT miniblocks
            NON_TARGETS_FACES_PER_MB('1008') = [1012,1003,1012,1004];
            NON_TARGETS_FACES_PER_MB('1006') = [1019,1009,1013,1015];
            NON_TARGETS_FACES_PER_MB('1019') = [1008,1015,1005,1005];
            NON_TARGETS_FACES_PER_MB('1004') = [1003,1012,1001,1015];
            NON_TARGETS_FACES_PER_MB('1005') = [1003,1013,1011,1006];
            NON_TARGETS_FACES_PER_MB('1014') = [1002,1004,1006,1013];
            NON_TARGETS_FACES_PER_MB('1013') = [1004,1009,1005,1014];
            NON_TARGETS_FACES_PER_MB('1018') = [1019,1009,1001,1014];
            NON_TARGETS_FACES_PER_MB('1003') = [1017,1017,1014,1008];
            NON_TARGETS_FACES_PER_MB('1009') = [1006,1003,1018,1008];
            NON_TARGETS_FACES_PER_MB('1017') = [1015,1001,1018,1018];
            NON_TARGETS_FACES_PER_MB('1011') = [1001,1013,1002,1005];
            NON_TARGETS_FACES_PER_MB('1001') = [1009,1011,1019,1006];
            NON_TARGETS_FACES_PER_MB('1015') = [1004,1012,1017,1011];
            NON_TARGETS_FACES_PER_MB('1002') = [1018,1011,1008,1014];
            NON_TARGETS_FACES_PER_MB('1012') = [1002,1019,1002,1017];
            
            NON_TARGETS_OBJECTS_PER_MB('2011') = [2003,2008,2001,2003];
            NON_TARGETS_OBJECTS_PER_MB('2012') = [2014,2007,2004,2014];
            NON_TARGETS_OBJECTS_PER_MB('2008') = [2009,2011,2003,2012];
            NON_TARGETS_OBJECTS_PER_MB('2015') = [2010,2004,2002,2003];
            NON_TARGETS_OBJECTS_PER_MB('2009') = [2002,2011,2006,2005];
            NON_TARGETS_OBJECTS_PER_MB('2004') = [2016,2006,2005,2014];
            NON_TARGETS_OBJECTS_PER_MB('2016') = [2001,2001,2015,2010];
            NON_TARGETS_OBJECTS_PER_MB('2001') = [2006,2016,2009,2012];
            NON_TARGETS_OBJECTS_PER_MB('2005') = [2016,2012,2014,2008];
            NON_TARGETS_OBJECTS_PER_MB('2003') = [2008,2016,2001,2004];
            NON_TARGETS_OBJECTS_PER_MB('2006') = [2008,2005,2013,2002];
            NON_TARGETS_OBJECTS_PER_MB('2010') = [2011,2007,2007,2004];
            NON_TARGETS_OBJECTS_PER_MB('2007') = [2013,2015,2009,2010];
            NON_TARGETS_OBJECTS_PER_MB('2002') = [2005,2009,2010,2013];
            NON_TARGETS_OBJECTS_PER_MB('2013') = [2002,2015,2007,2011];
            NON_TARGETS_OBJECTS_PER_MB('2014') = [2013,2015,2012,2006];
            
            
            NON_TARGETS_CHARS_PER_MB('3015') = [3009,3011,3018,3006];
            NON_TARGETS_CHARS_PER_MB('3010') = [3008,3006,3012,3006];
            NON_TARGETS_CHARS_PER_MB('3001') = [3015,3018,3007,3012];
            NON_TARGETS_CHARS_PER_MB('3011') = [3003,3013,3009,3005];
            NON_TARGETS_CHARS_PER_MB('3012') = [3003,3010,3011,3020];
            NON_TARGETS_CHARS_PER_MB('3016') = [3005,3010,3013,3008];
            NON_TARGETS_CHARS_PER_MB('3008') = [3009,3011,3015,3011];
            NON_TARGETS_CHARS_PER_MB('3003') = [3014,3015,3020,3010];
            NON_TARGETS_CHARS_PER_MB('3006') = [3008,3010,3001,3020];
            NON_TARGETS_CHARS_PER_MB('3020') = [3003,3006,3009,3001];
            NON_TARGETS_CHARS_PER_MB('3018') = [3007,3016,3007,3003];
            NON_TARGETS_CHARS_PER_MB('3013') = [3016,3014,3005,3012];
            NON_TARGETS_CHARS_PER_MB('3007') = [3016,3018,3016,3001];
            NON_TARGETS_CHARS_PER_MB('3014') = [3020,3005,3008,3013];
            NON_TARGETS_CHARS_PER_MB('3005') = [3013,3014,3001,3015];
            NON_TARGETS_CHARS_PER_MB('3009') = [3007,3018,3012,3014];
            
            NON_TARGETS_FALSES_PER_MB('4018') = [4010,4009,4008,4001];
            NON_TARGETS_FALSES_PER_MB('4005') = [4013,4001,4010,4015];
            NON_TARGETS_FALSES_PER_MB('4008') = [4020,4014,4018,4006];
            NON_TARGETS_FALSES_PER_MB('4006') = [4003,4009,4016,4020];
            NON_TARGETS_FALSES_PER_MB('4009') = [4001,4007,4012,4012];
            NON_TARGETS_FALSES_PER_MB('4007') = [4008,4011,4001,4008];
            NON_TARGETS_FALSES_PER_MB('4012') = [4003,4015,4011,4013];
            NON_TARGETS_FALSES_PER_MB('4013') = [4014,4005,4007,4007];
            NON_TARGETS_FALSES_PER_MB('4016') = [4010,4012,4012,4011];
            NON_TARGETS_FALSES_PER_MB('4020') = [4014,4015,4006,4013];
            NON_TARGETS_FALSES_PER_MB('4011') = [4010,4018,4020,4009];
            NON_TARGETS_FALSES_PER_MB('4001') = [4016,4015,4013,4014];
            NON_TARGETS_FALSES_PER_MB('4010') = [4006,4018,4005,4005];
            NON_TARGETS_FALSES_PER_MB('4003') = [4006,4007,4008,4009];
            NON_TARGETS_FALSES_PER_MB('4015') = [4011,4018,4020,4016];
            NON_TARGETS_FALSES_PER_MB('4014') = [4005,4003,4003,4016];
            
        elseif ECoG
            % FACE / OBJECT miniblocks
            NON_TARGETS_FACES_PER_MB('1018') = [1011,1015,1016,1002,1010,1010,1013,1010];
            NON_TARGETS_FACES_PER_MB('1015') = [1020,1009,1014,1008,1005,1018,1018,1001];
            NON_TARGETS_FACES_PER_MB('1005') = [1003,1011,1019,1017,1012,1015,1019,1014];
            NON_TARGETS_FACES_PER_MB('1011') = [1007,1008,1009,1009,1012,1005,1005,1008];
            NON_TARGETS_FACES_PER_MB('1002') = [1005,1015,1004,1003,1016,1020,1007,1016];
            NON_TARGETS_FACES_PER_MB('1014') = [1017,1008,1001,1019,1007,1003,1004,1013];
            NON_TARGETS_FACES_PER_MB('1009') = [1011,1014,1002,1016,1006,1011,1012,1001];
            NON_TARGETS_FACES_PER_MB('1012') = [1002,1003,1018,1010,1017,1019,1020,1006];
            NON_TARGETS_FACES_PER_MB('1001') = [1006,1013,1015,1004,1020,1018,1013,1004];
            NON_TARGETS_FACES_PER_MB('1004') = [1002,1001,1014,1017,1007,1012,1006,1009];
            
            NON_TARGETS_OBJECTS_PER_MB('2004') = [2016,2016,2017,2012,2018,2017,2011,2016];
            NON_TARGETS_OBJECTS_PER_MB('2007') = [2011,2013,2011,2001,2004,2004,2012,2014];
            NON_TARGETS_OBJECTS_PER_MB('2009') = [2001,2012,2001,2013,2010,2015,2018,2006];
            NON_TARGETS_OBJECTS_PER_MB('2005') = [2006,2019,2008,2003,2013,2014,2006,2007];
            NON_TARGETS_OBJECTS_PER_MB('2008') = [2020,2007,2003,2016,2020,2002,2005,2018];
            NON_TARGETS_OBJECTS_PER_MB('2003') = [2010,2008,2002,2013,2017,2009,2019,2002];
            NON_TARGETS_OBJECTS_PER_MB('2002') = [2004,2010,2007,2012,2020,2005,2011,2015];
            NON_TARGETS_OBJECTS_PER_MB('2010') = [2003,2002,2007,2019,2019,2009,2005,2008];
            NON_TARGETS_OBJECTS_PER_MB('2006') = [2003,2018,2015,2009,2005,2014,2001,2008];
            NON_TARGETS_OBJECTS_PER_MB('2001') = [2020,2006,2009,2010,2015,2014,2017,2004];
            
            % LETTER / FALSE-FONT miniblocks
            NON_TARGETS_CHARS_PER_MB('3010') = [3003,3011,3009,3012,3015,3016,3012,3008];
            NON_TARGETS_CHARS_PER_MB('3019') = [3005,3020,3018,3017,3009,3020,3001,3011];
            NON_TARGETS_CHARS_PER_MB('3011') = [3019,3014,3016,3019,3019,3009,3005,3008];
            NON_TARGETS_CHARS_PER_MB('3007') = [3006,3010,3006,3003,3012,3008,3005,3018];
            NON_TARGETS_CHARS_PER_MB('3003') = [3020,3008,3002,3006,3004,3017,3004,3005];
            NON_TARGETS_CHARS_PER_MB('3015') = [3013,3020,3014,3006,3011,3002,3013,3002];
            NON_TARGETS_CHARS_PER_MB('3001') = [3019,3014,3015,3010,3002,3004,3016,3015];
            NON_TARGETS_CHARS_PER_MB('3005') = [3015,3011,3013,3010,3012,3018,3013,3014];
            NON_TARGETS_CHARS_PER_MB('3013') = [3018,3003,3001,3004,3007,3007,3016,3007];
            NON_TARGETS_CHARS_PER_MB('3008') = [3017,3001,3017,3009,3003,3001,3010,3007];
            
            NON_TARGETS_FALSES_PER_MB('4011') = [4002,4003,4003,4015,4005,4017,4013,4001];
            NON_TARGETS_FALSES_PER_MB('4008') = [4002,4005,4011,4017,4011,4012,4012,4010];
            NON_TARGETS_FALSES_PER_MB('4013') = [4001,4002,4009,4007,4006,4008,4014,4010];
            NON_TARGETS_FALSES_PER_MB('4010') = [4020,4014,4020,4004,4006,4006,4007,4019];
            NON_TARGETS_FALSES_PER_MB('4007') = [4016,4006,4009,4013,4004,4001,4016,4018];
            NON_TARGETS_FALSES_PER_MB('4003') = [4017,4019,4010,4018,4020,4007,4018,4011];
            NON_TARGETS_FALSES_PER_MB('4005') = [4016,4015,4003,4014,4009,4002,4017,4015];
            NON_TARGETS_FALSES_PER_MB('4001') = [4008,4012,4019,4008,4015,4003,4013,4020];
            NON_TARGETS_FALSES_PER_MB('4019') = [4005,4013,4004,4014,4008,4016,4004,4012];
            NON_TARGETS_FALSES_PER_MB('4015') = [4009,4019,4010,4011,4018,4007,4001,4005];
        end
    end % if version_non_targetss
        
    
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
end % end of initConstantParameters function
