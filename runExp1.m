%% Coded by Yoav Roll yoav.roll@gmail.com

% Dr. Liad Mudrik's Lab, Tel-Aviv University
% Last changed: 12/03/20 by Yoav Roll
% Updated by Alex Lepauvre, Dr. Katarina Bendtz, Aya Khalaf 
% Eyetracking code added by Alex Lepauvre on the 26/09/2019 alex.lepauvre@ae.mpg.de
% 05/14/2020: fMRI modifications added by Aya Khalaf(email:aya.khalaf@yale.edu)
% WARNING: Do not use with MacOS! PTB is strictly *not* designed for MacOS experiments.
%
% The function begins with Instruments parameters containing options
% for different combinations of instruments:
% fMRI, ECoG, MEEG, EYE-TRACKER, the desired screen refresh rate and
% user viewing distance. If marked as 1 they are active and the program
% will send triggers or adapt accordingly
% Afterwards there are debugging parameters. To run the experiment,
% DEBUG has to be set to 0.
% IMPORTANT! The physical screen size in centimeter must be measured
% and put into SCREEN_SIZE_CM thusly: [width height]
% The function first calls the header function "initConstantsParameters"
% which contains all constants and parameters.
% To edit any of the constant and parameters, please look up
% the function below, and refer to its documentation.
%
% input:
% ------
% subNum - the number of the subject
% viewDist - subject's viewing distance in centimeters (default = 60)
%
% Stimuli are extracted from the "/stimuli/" folder. They are to be
% arragned by type
% (Faces - "/faces/", Objects - "/objects/", Letters - "/chars/", and
% False Fonts - "/false/") then by orientation (Center - "/center/",
% Left - "/left/", Right - "/right/"), than by sex (only for faces:
% Male - "/male/" and Female - "/female/")
%
% output:
% -------
% The experiment generates several csv files in the data folder ("/data/").

% *Results* - a dataset containing all events in experiment:
% stimuli, fixations, jitters, responses and "save" events (including
% their timings).

% *Summary* - summary tables of % accuracy, hits, misses, FA, CR and RT
% (in seconds) for every stimuli, orientation and type.

% *Code* - a copy of the current Matlab code, which is saved in ("/data/code/")

% *Backup* - a backup of the inner data structure of the experiment is
% saved as "Backup" which is more detailed and is used for debugging.
%---------------------------------------

% The results dataset contains the following columns (by this order):
% expName = The experiement's name (Exp1)
% block = The block number (1...10)
% miniBlock = The mini-block number (1...40)
% trial = The trial number (1...34/38)
% miniBlockType = The type of mini-block from the 2 options
%                 (face & object / letter & false (font))
% targ1 & targ2 = The targets in a given mini-block (see explanation
%                 for target name code below)
% plndStimulusDur = The planned time in which a stimuli should be shown
% plndJitterDur = The planned time in which a jitter should take place
% dsrdResponse = The desired response (0 - no response expected / 1 -
%                response expected)
% event = Information regarding the event in eventType
%       (if "stimuli" its the stimuli name (see explanation below) /
%       if "response": 0 - no key was pressed; 1 - pressed the target key
%       (up); 2 - wrong key was pressed)
% time - The time in which the event happned
% eventType = The type of event that happened (stimuli, fixations, jitters, responses, save)

% Explanation of stimuli (and target) names:
% These are coded as a 4 digit number.
% 1st digit = stimulus type (1 = face; 2 = object; 3 = letter; 4 = false font)
% 2nd digit = stimulus orientation (1 = center; 2 = left; 3 = right)
% 3rd & 4th digits = stimulus id (1...20; for faces 1...10 is male, 11...20 is female)
% e.g., "1219" = 1 is face, 2 is left orientation and 19 is a female stimulus #19
% The decimal is for duration

%% RunExp1
function runExp1( subNum, viewDist)
%% Housekeeping:
% Clearing the command window before we start saving it
clc

% Add functions folder to path (when we separate all functions)
function_folder = [pwd,filesep,'functions\'];
addpath(function_folder)

% Logging everything that is printed into the command window! If the
% log file already exist, delete it, otherwise the logs will be
% appended and it won't be specific to that participant. Moreover, the
% logs are always saved
dfile ='log_exp1.txt';
if exist(dfile, 'file') ; delete(dfile); end
Str = CmdWinTool('getText');
dlmwrite(dfile,Str,'delimiter','');
% To get different seeds for matlab randomization functions.
rng('shuffle');


%% Instruments and programming parameters - may be changed
global DIOD_DURATION ECoG MEEG fMRI EYE_TRACKER PHOTODIODE VIEW_DISTANCE
global LAB_ID refRate
global VERBOSE NO_PRACTICE DEBUG RESOLUTION_FORCE NO_AUDIO
global MATRIX_GENERATION PREEXISTING_MATRICES
% This function sets all the parameters for the recording modalities
% and code. Gives back the VIEW_DISTANCE, which is the default viewing
% distance.
initRuntimeParameters;

%% Setting parameters in case of missing inputs:
% If there are less than two inputs in the runExp1 function
if nargin < 2
    % If debug mode, replace the subNum by 999
%     if DEBUG
%         subNum = 999;
%     end
    % Since the experimenter didn't set the viewing distance, we take
    % the default
    viewDist = VIEW_DISTANCE; % default value in centimeters
    Screen('Preference', 'SkipSyncTests', 1);
end

if nargin < 1 && DEBUG == 0     % Check if all needed parameters given:
    error('Must provide required input parameters %d!',subNum);
end

%% Checking if participant already exists:
global viewDistance compKbDevice subjectNum END_WAIT END_OF_EXPERIMENT_MESSAGE LOADING_MESSAGE 
global RUN_PRACTICE PRACTICE_START_MESSAGE PRACTICE_START_MESSAGE_fMRI LPT_CODE_START LPT_OBJECT LPT_ADDRESS PRACTICE_START_MESSAGE_ECOG ScreenHeight
global TRIGAUD_CODE_START WhereToRestart TOBII_EYETRACKER
global ExistFlag DATA_FOLDER TEMPORARY_FOLDER EXPERIMENT_NAME BEHAV_FILE_NAMING_WHOLE miniBlocksInfo

subjectNum = subNum;
compKbDevice = -1;
viewDistance = viewDist;

% Before we go further, we check whether the participant number was
% already used:
ParticipantFolder = sprintf('%s%c%s%c%s',pwd,filesep,'data',filesep,[LAB_ID,num2str(subjectNum),filesep]);

if ~MATRIX_GENERATION
    ExistFlag=exist(ParticipantFolder,'dir');
    if ExistFlag
        warning ('This participant number was already attributed!')
        proceedInput = questdlg({'This participant number was already attributed!', 'Are you sure you want to proceed?'},'RestartPrompt','yes','no','no');
        if strcmp(proceedInput,'no')
            error('Program aborted by user')
        end
        % Resume or reset? If the experimenter chooses to, he/she can
        % completely start anew, renaming the existing file and
        % starting completely anew:
        ResumeOrReset = questdlg('Do you want to resume this participant session or start anew?','RestartPrompt','resume','start anew', 'start anew');
        if strcmp(ResumeOrReset,'start anew')
            newFile = [ParticipantFolder(1:end-1),'_ABORTED'];
            copyStatus = copyfile(ParticipantFolder,newFile);
            if ~copyStatus
                error('The existing participant file could not be copied. Make sure the files are closed in other apps!')
            end
            deleteStatus = rmdir(ParticipantFolder,'s');
            if ~deleteStatus
                error('The existing participant file could not be deleted. Make sure the files are closed in other apps!')
            end
            ExistFlag = 0;
        elseif strcmp(ResumeOrReset,'resume')
            % Resume from block or miniBlock?
            QuestionWhereToRestart = questdlg('Where do you want to resume?','RestartPrompt','Last block','Last miniBlock','Last miniBlock');
            if strcmp(QuestionWhereToRestart,'Last block')
                WhereToRestart = 'b';
            elseif strcmp(QuestionWhereToRestart,'Last miniBlock')
                WhereToRestart = 'm';
            end
            redoPractice = questdlg('Do you wish to redo the practice?','RestartPrompt','yes','no','no');
            if strcmp(redoPractice,'yes')
                REDO_PRACTICE = 1;
            else
                REDO_PRACTICE = 0;
            end
        else
            error('You pressed a key that was not recognized!')
        end  
    end
end

try
    %% Initializing experimental parameters and PTB:
    initConstantsParameters(subNum); % defines all constants and initilizes parameters of the program
    
    screenError = initPsychtooblox(); % initializes psychtoolbox window at correct resolution and refresh rate
    
    % if refresh rate is not as intended
    if screenError && RESOLUTION_FORCE
        showError('WARNING: screen refresh rate is not optimal !');
    end
    
    %% EXPERIMENT
    % ***************
    
    %% Initializing recording instruments
    
    % initilize MEEG IO:
    if MEEG
        initLPT_h();
        % For the very first trigger, the port is set to 0 first:
        sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
        % Waiting 10ms to turn it on:
        WaitSecs(0.01);
        sendTrig(LPT_CODE_START,LPT_OBJECT,LPT_ADDRESS);
        % Waiting 10ms to turn it off:
        WaitSecs(0.01);
        sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
    end
    
    % Initializing audio triggers for ECoG:
    if ECoG && ~NO_AUDIO
        % initialize clock of trigger, counting and saving variables
        initAudioTrigger();
        sendTrigAudio(dec2bin(TRIGAUD_CODE_START,7));
    end
    
    % Marking experiment onset with photodiode flashes for ECoG:
    if ECoG && PHOTODIODE
        multiplePhotodiodeFlashes(3)
    end
    
    
    % Initialize the eyetracker
    if EYE_TRACKER
        if ~TOBII_EYETRACKER
            initEyetracker(1); % ALEX: HERE THE 1 WILL NEED TO BE CHANGED IF WE RESTARTED THE EXPERIMENT!!!!
        else
            initTobiiEyetracker; % Initialization of the tobii eyetracker:
        end
    end
    
    %% Starting and setting up presentation:
    
    showMessage(LOADING_MESSAGE);
    
    % Saves a copy of code to disk
    saveCode();
    
    % Loads all textures from hard drive before the experiment runs
    loadTexturesFromHD();
    if ~MATRIX_GENERATION
        %% Restarting contingencies (None-restarted mode):
        % If the experiment wasn't restarted, then:
        if ~ExistFlag
            % Create all trials before expriement runs
            if ~PREEXISTING_MATRICES
                [miniBlocks, TriggerMatrix] = createMiniBlocks();
            else
                [miniBlocks, TriggerMatrix] = loadMiniBlocks();
            end
            
            %% Instructions and practice:
            % displays instructions
            Instructions();
            if ~NO_PRACTICE
                % Show the practice messages:
                if(~fMRI)
                    if ECoG
                        showMessages(PRACTICE_START_MESSAGE_ECOG, [0,2], [round(ScreenHeight*(1/2)), round((ScreenHeight*(5/6)))]);
                        KbWait(compKbDevice,3);
                    else
                        showMessage(PRACTICE_START_MESSAGE);
                        KbWait(compKbDevice,3);
                    end
                else
                    showMessage(PRACTICE_START_MESSAGE_fMRI);
                    WaitSecs(2);
                end
                RUN_PRACTICE = 1;
                % Launching the practice loop:
                getPracticeFeedback();
            end
            
        %% Restarting contingencies (Restarted mode):    
        else % If the experiment is being restarted, we need to log things back up:
            % Loading the back up file, a.k.a the trial matrix:
            BackupFile  = sprintf('%s%c%s%c%s%c%s%c%s_%s_ID%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,EXPERIMENT_NAME,'Backup',[LAB_ID,num2str(subjectNum)]);
            load(BackupFile)
            % Loading the trigger file:
            TriggerFile  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,'TriggerMatrix');
            load(TriggerFile)
            % Loading the whole log file:
            WholeFile = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING_WHOLE]);
            miniBlocksInfo=load(WholeFile);
            
            % displays instructions
            Instructions();
            if REDO_PRACTICE % If the experimenter wants to redo the practice, then it will happen. But otherwise,it won't
                if(~fMRI)
                    % Showing the practice instructions:
                    if ECoG
                        showMessages(PRACTICE_START_MESSAGE_ECOG, [0,2], [round(ScreenHeight*(1/2)), round((ScreenHeight*(5/6)))]);
                        KbWait(compKbDevice,3);
                    else
                        showMessage(PRACTICE_START_MESSAGE);
                        KbWait(compKbDevice,3);
                    end
                else
                    showMessage(PRACTICE_START_MESSAGE_fMRI);
                    WaitSecs(2);
                end
                RUN_PRACTICE = 1;
                % Launching the practice loop:
                getPracticeFeedback();
            end
        end
        
        %% runMiniBlocks
        % This is where the experiment actually gets run:
        miniBlocks = runMiniBlocks(miniBlocks,TriggerMatrix);
        
        %% Finishing the experiment:
        if VERBOSE disp('After runMiniBlocks'); end
        % Letting the participant that it is over:
        showMessage(END_OF_EXPERIMENT_MESSAGE);
        
        % Waiting a bit so that participant see the end message:
        WaitSecs(END_WAIT);
        %% save everything from command window
        Str = CmdWinTool('getText');
        dlmwrite(dfile,Str,'delimiter','');
    else % If we are in MATRIX generation mode, run the createMiniBlocks function
        [miniBlocks, TriggerMatrix] = createMiniBlocks();
    end
    % safe finish, closing Psychtoolbox, ends the recording methods:
    safeExit();
    
catch e % When the program has error, it safely ends, closing psychtoolbox
    Str = CmdWinTool('getText');
    dlmwrite(dfile,Str,'delimiter','');
    safeExit();
    rethrow(e);
    
end % try

end % function



%% runMiniBlocks
% this function runs the experiment given a block with all miniblocks
% input:
% ------
% miniBlocks - the mini-blocks to run

function [ miniBlocks ] = runMiniBlocks( miniBlocks,TriggerMatrix )

% Getting the relevant global variables:
global output_table_cntr miniBlocksInfo BLOCK_NUM_COL MINIBLK_COL TRIAL1_ANSWER_COL TRIAL1_TIME_COL MINI_BLOCK_SIZE_COL EVENT_TYPE_COL compKbDevice TRIAL1_RESPONSE_TIME_COL   FALSE TRUE    TRIAL_DURATION
global TRIAL1_BLANK_DUR_COL TRIAL1_STIM_DUR_COL refRate SAVING_MESSAGE NO_KEY TRIAL1_JITTER_TIME_COL TRIAL1_STIM_END_TIME_COL TRIAL1_BUTTON_PRESS_COL  TRIAL1_START_TIME_COL TRIAL1_DURATION_COL
global MISSES_COL HITS_COL FA_COL CR_COL RightKey WRONG_KEY RESTART_KEY TARGET_KEY Behavior ExistFlag YesKey MINIBLOCK_RESTART_KEY BLOCK_RESTART_KEY
global EXPERIMET_START_MESSAGE EXPERIMET_START_MESSAGE_fMRI END_OF_BLOCK_MESSAGE END_OF_BLOCK_MESSAGE_fMRI_MEG BLOCK_START_MESSAGE_fMRI BREAK_MESSAGE_fMRI MEG_BREAK_MESSAGE RESTART_MESSAGE RESTART_MESSAGE_fMRI RESTARTBLOCK_OR_MINIBLOCK_MESSAGE FEEDBACK_MESSAGES DEBUG OUTPUT_TABLE MRI_BASELINE_PERIOD MEGbreakKey EYETRACKER_CALIBRATION_MESSAGE EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS EYETRACKER_CALIBRATION_MESSAGE_fMRI EYETRACKER_CALIBRATION_MESSAGE_fMRI_BETWEENBLOCKS GENERAL_BREAK_MESSAGE
global EXPERIMENT_START_MESSAGE_ECOG EYETRACKER_CALIBRATION_MESSAGE_ECOG EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS_ECOG PROGRESS_MESSAGE_ECOG GENERAL_BREAK_MESSAGE_ECOG END_OF_BLOCK_MESSAGE_ECOG
global bitsi_buttonbox bitsi_scanner LAB_ID MEEG fMRI ECoG  LPT_OBJECT LPT_ADDRESS TRG_RESPONSE TRG_STIM_END TRG_JITTER_START TRG_MB_ADD
global el EYE_TRACKER DIOD_DURATION RESP_TRIG_ONSET% This is a parameter generated when initializing the eyetracker, which is required for the calibratio
global VERBOSE PROGRESS_MESSAGE PROGRESS_MESSAGE_MEG TEXTURES_ANIMAL_REWARD
global PHOTODIODE ABORT_KEY Block_ctr WhereToRestart RestartFlag InterruptFlag exp_Interrupt_counter TOBII_EYETRACKER tobii_eyetracker tobii tobii_TimeCell
global NO_AUDIO triggers TRIGGER_ARRAY_SIZE triggsCounter triggersAudio triggsAudioCounter TRG_MBONSET_AUD
global ABORTED % This is a flag that is turned to 1 if the experiment was aborted to store the data accordingly
global BLK_COL DSRD_RESP_COL EVENT_TYPE_COL HT_COL FAS_COL TYPE_COL TARG1_COL TARG2_COL TRG_EXP_START_MSG
global ScreenHeight w text

if VERBOSE
    disp('WEL COME TO runMiniBlocks')
end

try
    % First things first, setting the aborted flag to 0:
    ABORTED = 0;
    InterruptFlag=0; % This is a flag to know if we interrupted, 0 means no
    
    %% Restarting contingency to initiate the looping:
    % In the case where the experiment runs not in resuming mode:
    if ~ExistFlag
        % this counts the main blocks, there are 10 blocks in the experiemnt
        % with 4 miniblocks each
        Block_ctr = 0;
        miniBlockNum=1;
        RestartFlag = 0; % This is a flag to know if we restarted, 0 means no
        
    else % If the participant was already ran and you want to restart where you left off:
        RestartFlag = 1; % If we restarted, we need the trigger and eyetracker to know to give the right name to the file
        
        % The output table was loaded back up to continue filling it from where we left off
        OUTPUT_TABLE=miniBlocksInfo.miniBlocks; % The output table was loaded back up to continue filling it from where we left off

        % Get the indice of the miniBlock at which to restart, based
        % on the output table
        miniBlockIndices=find(cell2mat(OUTPUT_TABLE(2:end,MINIBLK_COL)))+1;   % The miniBlock index is fecthed back +1 to account for the table header
        
        % Here we fetch the miniBlock number
        miniBlockNum=cell2mat(OUTPUT_TABLE(miniBlockIndices(end),MINIBLK_COL)); % The Block number is also fetched back
        
        % If we are in fMRI. we want to restart from the beginning of the block
        if(fMRI)
            if(~strcmp(OUTPUT_TABLE{miniBlockIndices(end),EVENT_TYPE_COL},'Save'))
            % So the miniBlock number will be the first of the block we
            % are at
            miniBlockNum=1+4*floor((miniBlockNum-1)/4); 
            else
            miniBlockNum=miniBlockNum+1;      
            end
            % Computing the block number
            Block_ctr=floor(miniBlockNum/4);
            % Storing the block number at which the experiment was
            % restarted to know when to reset the RestartFlag
            RestartBlockNumber = Block_ctr;
            % If we are in another modality that fMRI, the experimenter
            % can choose where to restart from:
        elseif strcmp(WhereToRestart,'m')% If the Experimenter wants to, he/she can restart at the last miniBlock
            if mod(miniBlockNum,4) == 1
                Block_ctr=floor((miniBlockNum)/4); % If the experiment stopped in the first miniblock of a block, need to have block counter be -1 because it will go into +1 right after
                RestartBlockNumber = Block_ctr; % I store the number of the block at which we restarted to know when to reset the restart flag
            else
                Block_ctr=1+floor((miniBlockNum-1)/4);
                RestartBlockNumber = Block_ctr - 1; % I store the number of the block at which we restarted to know when to reset the restart flag
            end
        elseif strcmp(WhereToRestart,'b')% Or at the last Block
            % if(~strcmp(OUTPUT_TABLE{miniBlockIndices(end),EVENT_TYPE_COL},'Save'))
            miniBlockNum=1+4*floor((miniBlockNum-1)/4);
            % else
                % miniBlockNum=miniBlockNum+1;     
            % end
            Block_ctr=floor(miniBlockNum/4);
            RestartBlockNumber = Block_ctr;
        end
        
        % Fetch the counter to keep feeling the output table:
        if(fMRI && strcmp(OUTPUT_TABLE{miniBlockIndices(end),EVENT_TYPE_COL},'Save'))
            output_table_cntr=find(cell2mat(OUTPUT_TABLE(2:end,MINIBLK_COL))==miniBlockNum-1,1,'last')+2; % Fetching where to restart from when filling the output table. We append to the table rather than overwritting    
        else
            output_table_cntr=find(cell2mat(OUTPUT_TABLE(2:end,MINIBLK_COL))==miniBlockNum,1,'last')+2; % Fetching where to restart from when filling the output table. We append to the table rather than overwritting
        end
        
        tr = 0; % This is necessary for things not to crash: when restarting, the tr will be called before being defined otherwise
        clear miniBlocksInfo
        
        % Since we are restarting, we should calibrate the eyetracker.
        % Except if we are in the first miniBlock, because in that
        % case, it will be done anyways down the line:
        if EYE_TRACKER && miniBlockNum ~= 1
            % Showing the messages for the eyetracker calibration:
            if(~fMRI)
                if ECoG
                    showMessages(EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS_ECOG, [0,2], [round(ScreenHeight*(1/6)), round((ScreenHeight*(5/6)))]);
                    KbWait(compKbDevice,3);
                    % Here we send photodiode triggers for the ECoG to mark
                    % the onset of the new block:
                    if PHOTODIODE
                        multiplePhotodiodeFlashes(4)
                    end               
                else  
                    showMessage(EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS);
                    KbWait(compKbDevice,3);
                end
            else
                showMessage(EYETRACKER_CALIBRATION_MESSAGE_fMRI_BETWEENBLOCKS);
                switch LAB_ID
                    case 'SC'
                        response = 0;
                        bitsi_buttonbox.clearResponses()
                        while ~(response == 97)
                            [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
                        end
                    case 'SD'
                        [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
                        while (~InstructionsResp(RightKey))
                            [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
                        end
                        
                end
            end
            if ~TOBII_EYETRACKER
                % Starting the eyetracker recording for the first block
                % Calibrate or drift correction of the eye tracker
                EyelinkDoTrackerSetup(el);
                % Starting the recording
                Eyelink('StartRecording');
            else % For the tobii:
                tobiiCalibration; % Perform the calibration
                % And then, we start the recording anew:
                GazeData = tobii_eyetracker.get_gaze_data();
                % We then set the time table for triggers
                tobii_TimeCell = {'system_time_stamp', 'point_description'};
            end
        end % eye tracker
        
        % Showing the starting messages:
        if(~fMRI)
            if ECoG
                showMessages(EXPERIMENT_START_MESSAGE_ECOG, [0,2], [round(ScreenHeight*(1/2)), round((ScreenHeight*(5/6)))]);
                KbWait(compKbDevice,3);
            else
                showMessage(EXPERIMET_START_MESSAGE);
                KbWait(compKbDevice,3);
            end
            
        else
            showMessage(EXPERIMET_START_MESSAGE_fMRI);
            WaitSecs(2);
        end
        
        if MEEG
            sendTrig(TRG_EXP_START_MSG,LPT_OBJECT,LPT_ADDRESS);
            % Waiting 10ms to turn it off:
            WaitSecs(0.01);
            sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
        end

    end
    
    %% Miniblocks loop:
    
    % Running through miniBlocks. Use of a while so that we can go back
    % if the experimenter wants to (for loop don't go back)
    while miniBlockNum <= size(miniBlocks,1)
        if VERBOSE display(miniBlockNum, 'now we are just starting miniBlock nr: '); end
        
        % -----------------------------------------------------------------
        % Reward messages:
        % -----------------------------------------------------------------
        % Progress message with progress bar and animal pics for patients to
        % motivate them
        if (miniBlockNum > 1) && ECoG
            prog_message = PROGRESS_MESSAGE_ECOG;
            prog_message{1} = sprintf(strcat(prog_message{1}, '\n\n%d/20'), miniBlockNum-1);
            texturePtr_animal = TEXTURES_ANIMAL_REWARD(1,(miniBlockNum-1));
            displayProgressReward(prog_message, texturePtr_animal, miniBlockNum-1, size(miniBlocks,1));
            KbWait(compKbDevice,3)
        elseif (miniBlockNum > 1) && fMRI
            showMessage('');
            WaitSecs(1);
        % elseif (miniBlockNum > 1) && MEEG
            % showMessage('');
            % WaitSecs(1);
        end
        
        
        % -----------------------------------------------------------------
        % Eyetracker calibration:
        % -----------------------------------------------------------------
        if EYE_TRACKER && miniBlockNum == 1
            
            % Showing the messages for the calibration:
            if(~fMRI)
                if ECoG
                    showMessages(EYETRACKER_CALIBRATION_MESSAGE_ECOG, [0,2], [round(ScreenHeight*(1/6)), round((ScreenHeight*(5/6)))]);
                    KbWait(compKbDevice,3);
                else  
                    showMessage(EYETRACKER_CALIBRATION_MESSAGE);
                    KbWait(compKbDevice,3);
                end
            elseif fMRI && ~InterruptFlag % If we are back there because of an interruption, calibration shouldn't be performed again
                showMessage(EYETRACKER_CALIBRATION_MESSAGE_fMRI);
                
                switch LAB_ID
                    case 'SC'
                        response = 0;
                        bitsi_buttonbox.clearResponses()
                        while ~(response == 97)
                            [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
                        end
                    case 'SD'
                        [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
                        while (~InstructionsResp(RightKey))
                            [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
                        end
                        
                end
            end
            
            % Calibration, validation or drift correction: 
            if ~TOBII_EYETRACKER
                % For the fMRI modality, if the reason why we now are here
                % is because we interrupted, the data need to be saved
                % before interruption:
                if fMRI && InterruptFlag
                    Eyelink('StopRecording');
                    % Pulling the edf from the eyetracker computer:
                    % If we are not in MEEG, we do so every block
                    % At the end of the block we are in, importing
                    % the EDF files from the eyetracker.
                    importEyetrackerEDF(Block_ctr-1,RestartFlag,InterruptFlag)
                    InterruptFlag=0;
                    initEyetracker(Block_ctr) % Then, initialize the tracker again, with new file name:
                    
                    % Starting the eyetracker recording again
                    Eyelink('StartRecording');
                elseif ~fMRI && InterruptFlag
                    Eyelink('StopRecording');
                    % Starting the eyetracker recording for the first block
                    % Calibrate or drift correction of the eye tracker
                    EyelinkDoTrackerSetup(el);
                    % Starting the recording
                    Eyelink('StartRecording');
                else % If there was no interruption, then we should do the calibration normally
                    % Starting the eyetracker recording for the first block
                    % Calibrate or drift correction of the eye tracker
                    EyelinkDoTrackerSetup(el);
                    % Starting the recording
                    Eyelink('StartRecording');
                end
            else % If using tobii tracker: not saving if there was an interruption, only for abortion 
                if ~InterruptFlag % If there was no interruption
                    tobiiCalibration; % Perform the calibration
                    % And then, we start the recording anew:
                    GazeData = tobii_eyetracker.get_gaze_data();
                    tobii_TimeCell = {'system_time_stamp', 'point_description'};
                end % If there was an interruption, simply keep on recording
            end
        end % eye tracker
        
        
        % -----------------------------------------------------------------
        % First miniblock actions: showing the begining of experiment
        % messages and triggers
        % -----------------------------------------------------------------
        if  miniBlockNum == 1
            % runs the mini-blocks
            if(~fMRI)
                if ECoG
                    showMessages(EXPERIMENT_START_MESSAGE_ECOG, [0,2], [round(ScreenHeight*(1/2)), round((ScreenHeight*(5/6)))]);
                    KbWait(compKbDevice,3);
                    % Here we send photodiode triggers for the ECoG to mark
                    % the onset of the new block:
                    if PHOTODIODE
                        multiplePhotodiodeFlashes(4)
                    end                    
                else
                    showMessage(EXPERIMET_START_MESSAGE);
                    KbWait(compKbDevice,3);
                end
            
            else
                showMessage(EXPERIMET_START_MESSAGE_fMRI);
                WaitSecs(2);
            end
            
            if MEEG
                sendTrig(TRG_EXP_START_MSG,LPT_OBJECT,LPT_ADDRESS);
                % Waiting 10ms to turn it off:
                WaitSecs(0.01);
                sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
            end
        end
        
        % -----------------------------------------------------------------
        % New blocks:
        % -----------------------------------------------------------------
        % Several things need to happen in the begining of a new block:
        % eyetracker data saving, displaying messages...:
        if mod(miniBlockNum,4) == 1 % end of miniblock or rather the beginning of a new one
            
            Block_ctr = Block_ctr + 1; % Actualize the Block counter
            
            % If we are not in the last mini block nor in the first, we
            % display the different messages for end of a miniblock
            if miniBlockNum < size(miniBlocks,1) && miniBlockNum > 1
                
                % If we are at the end of a miniblock, saving the data
                % (only if we are in debug mode)
                if DEBUG && miniBlockNum > 1
                    saveBlockToHD(OUTPUT_TABLE,miniBlockNum,'Results',tr);
                    saveTrialBackupToHD(miniBlocks,1,'Backup');
                    saveBlockToExcel(OUTPUT_TABLE,miniBlockNum,tr);
                    saveSummaryToExcel(OUTPUT_TABLE, 1);
                end
                
                
                % ---------------------------------------------------------
                % End of block messages (only for MEEG and ECoG):
                % If we are in MEEG recording, the experimenter needs
                % to press the MEGbreakKey to leave time to save the
                % MEEG data, as these needs to be saved every couple
                % of 20 minutes:
                if MEEG
                    % Resetting the triggers log file, only every two blocks:
                    if miniBlockNum > 1 && floor((miniBlockNum-1)/8)+1 ~= floor((miniBlockNum-2)/8)+1
                        triggers = cell(TRIGGER_ARRAY_SIZE,3);
                        % Restting the trigger counter:
                        triggsCounter = 0;
                    end
                    
                    % Showing end of block messages:
                    if (length(feedback_messages)==1)
                        end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE_fMRI_MEG, '\n%d/10\n\n%s%s\n\n%s\n\n%s'), Block_ctr-1,'Your score is ',strcat(num2str(round(TotalScore)),'%'),feedback_messages{1},'Press SPACE to proceed');
                    else
                        end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE_fMRI_MEG, '\n%d/10\n\n%s%s\n\n%s\n%s\n\n%s\n\n%s'), Block_ctr-1,'Your score is ',strcat(num2str(round(TotalScore)),'%'),feedback_messages{1},feedback_messages{2},'Press SPACE to proceed');  
                    end
                    % Showing the animal messages if they performed well
                    % enough:
                    if (feedback_message_flag1==1)
                        texturePtr_animal = TEXTURES_ANIMAL_REWARD(1,(Block_ctr-1));
                        displayProgressReward(end_of_this_block_message, texturePtr_animal, Block_ctr-1, floor(size(miniBlocks,1)/4));
                    else
                        displayProgressReward(end_of_this_block_message,'', Block_ctr-1, floor(size(miniBlocks,1)/4));
                    end
                    KbWait(compKbDevice,3);
                    
                    % Generate the message, telling the participant to wait:
                    wait_for_MEG_message = sprintf(strcat(MEG_BREAK_MESSAGE, '\n\n%d/10'), Block_ctr);
                    % Show the message
                    showMessage(wait_for_MEG_message);
                    % Loop until the experimenter press the MEGbreakKey
                    while true
                        [~,keyCode,~] = KbWait(compKbDevice,3);
                        if keyCode(MEGbreakKey)
                            break
                        end
                    end
                    
                end
                
                % If in ECOG mode, show the participant the end of
                % block message, and wait for the participant to
                % press a key
                if ECoG
                    
                    % Resetting the audio trigger matrix every block:
                    if ~NO_AUDIO && miniBlockNum > 1 && floor((miniBlockNum-1)/4)+1 ~= floor((miniBlockNum-2)/4)+1
                        % Resetting the audio triggers for the saving
                        triggersAudio = [];
                        triggsAudioCounter = 1;
                    end
                    
                    % Creating the end of block messages:
                    eob_message = END_OF_BLOCK_MESSAGE_ECOG;
                    eob_message{1} = sprintf(strcat(eob_message{1}, '\n\n%d/5'), Block_ctr);
                    showMessages(eob_message, [0,2], [round(ScreenHeight*(1/2)), round((ScreenHeight*(5/6)))]);
                    KbWait(compKbDevice,3);
                    
                    if PHOTODIODE
                        % Then, we flash the photodiode to mark end of a block:
                        multiplePhotodiodeFlashes(2)
                    end
                end
                
                % ---------------------------------------------------------
                % Break messages:
                %%%You can take a break message
                if (MEEG||Behavior||ECoG)
                    if ECoG
                        end_of_this_block__take_a_break_message = GENERAL_BREAK_MESSAGE_ECOG;
                        end_of_this_block__take_a_break_message{1} = sprintf(strcat(end_of_this_block__take_a_break_message{1}, '\n\n%d/5'), Block_ctr);
                        showMessages(end_of_this_block__take_a_break_message, [0,2], [round(ScreenHeight*(1/2)), round((ScreenHeight*(5/6)))]);
                    else
                        end_of_this_block__take_a_break_message = sprintf(strcat(GENERAL_BREAK_MESSAGE, '\n\n%d/10'), Block_ctr);
                        showMessage(end_of_this_block__take_a_break_message);
                    end
                    KbWait(compKbDevice,3);%
                    
                    % Here we send photodiode triggers for the ECoG to mark
                    % the onset of the new block:
                    if ECoG && PHOTODIODE
                        % Then, we flash the photodiode to mark end of a block:
                        multiplePhotodiodeFlashes(4)
                    end                    
                end

                % ---------------------------------------------------------
                % End of block message for fMRI:
                if (fMRI && ~RestartFlag && ~InterruptFlag)% If in fMRI mode, the experiment will proceed on its own after a set amount of time
                    % Generate the end of block message
                    % end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE_fMRI, '\n\n%d/8\n\n\n\n%s%s'), Block_ctr-1,'Your score is ',strcat(num2str(round(TotalScore)),'%'));
                    if (length(feedback_messages)==1)
                        end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE_fMRI_MEG, '\n%d/8\n\n%s%s\n\n%s\n\n%s'), Block_ctr-1,'Your score is ',strcat(num2str(round(TotalScore)),'%'),feedback_messages{1},'Press the index finger button to proceed');
                    else
                        end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE_fMRI_MEG, '\n%d/8\n\n%s%s\n\n%s\n\n%s\n\n%s\n\%s'), Block_ctr-1,'Your score is ',strcat(num2str(round(TotalScore)),'%'),feedback_messages{1},feedback_messages{2},'Press the index finger button to proceed');
                    end
                    if (feedback_message_flag1==1)
                        texturePtr_animal = TEXTURES_ANIMAL_REWARD(1,(Block_ctr-1));
                        displayProgressReward(end_of_this_block_message, texturePtr_animal, Block_ctr-1, floor(size(miniBlocks,1)/4));
                    else
                        displayProgressReward(end_of_this_block_message,'', Block_ctr-1, floor(size(miniBlocks,1)/4));
                    end
                    % Show the message
                    % showMessage(end_of_this_block_message);
                    
                    switch LAB_ID
                        case 'SC'
                            response = 0;
                            bitsi_buttonbox.clearResponses()
                            while ~(response == 97)
                                [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
                            end
                        case 'SD'
                            
                        [~, Resp, ~] =KbWait(compKbDevice,3);
                        while (~Resp(RightKey))
                            [~, Resp, ~] =KbWait(compKbDevice,3);
                        end
                    end
                    % Create a server interface and open it to let the
                    % scanner listener know that the run is finished.
                    % TCPIP = tcpip('0.0.0.0', 20000, 'NetworkRole', 'server','Timeout',Inf);
                    % fopen(TCPIP);
                    % fread(TCPIP, 1, 'double');
                    % fclose(TCPIP);
                    
                    % Show the BREAK_MESSAGE_fMRI
                    showMessage(BREAK_MESSAGE_fMRI);
                    % Wait a few seconds
                    WaitSecs(5);
                end
                
                
                % ---------------------------------------------------------
                % Eyetracker saving and calibration:
                % ---------------------------------------------------------
                % If we are recoding with the eyetracker, now is time
                % to do the calibration or drift correction
                if EYE_TRACKER
                    if ~TOBII_EYETRACKER
                        % In case of restarting, the data should not be
                        % saved again, because they were not collected so
                        % far. The calibration was done already, so no need
                        % to repeat:
                        if ~RestartFlag || (RestartFlag && RestartBlockNumber+1 < Block_ctr)
                            % If the block was restarted during the
                            % experiment and we are back here, the
                            % eyetracker recordings should only be
                            % interrupted for fMRI
                            if ~InterruptFlag && ~fMRI
                                % Stopping the eyetracker recording at the end of each
                                % block
                                Eyelink('StopRecording');
                                % Pulling the edf from the eyetracker computer:
                                % If we are not in MEEG, we do so every block
                                if ~MEEG
                                    % At the end of the block we are in, importing
                                    % the EDF files from the eyetracker.
                                    importEyetrackerEDF(Block_ctr-1,RestartFlag,InterruptFlag)
                                    initEyetracker(Block_ctr) % Then, initialize the tracker again, with new file name:
                                else % Otherwise, every second block:
                                    if mod(Block_ctr,2) == 1
                                        DoubleBlockNumber = floor((Block_ctr-1)/2);
                                        importEyetrackerEDF(DoubleBlockNumber,RestartFlag,InterruptFlag)
                                        initEyetracker(Block_ctr) % Then, initialize the tracker again, with new file name
                                    end
                                end
                                
                                if ECoG
                                    showMessages(EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS_ECOG, [0,2], [round(ScreenHeight*(1/6)), round((ScreenHeight*(5/6)))]);
                                    KbWait(compKbDevice,3);
                                else
                                    showMessage(EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS);
                                    KbWait(compKbDevice,3);
                                end
                                
                                EyelinkDoTrackerSetup(el);
                                % Starting the eyetracker recording again
                                Eyelink('StartRecording');
                                
                            elseif InterruptFlag && ~fMRI
                                if ECoG
                                    showMessages(EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS_ECOG, [0,2], [round(ScreenHeight*(1/6)), round((ScreenHeight*(5/6)))]);
                                    KbWait(compKbDevice,3);
                                else
                                    showMessage(EYETRACKER_CALIBRATION_MESSAGE_BETWEENBLOCKS);
                                    KbWait(compKbDevice,3);
                                end
                                % Stopping the eyetracker recording at the end of each
                                % block
                                Eyelink('StopRecording');
                                
                                %  Calibrate:
                                EyelinkDoTrackerSetup(el);
                                % Starting the eyetracker recording again
                                Eyelink('StartRecording');
                            else % If we are in fMRI, then we need to save the data even if there was a restart of the block:
                                
                                Eyelink('StopRecording');
                                % Pulling the edf from the eyetracker computer:
                                % If we are not in MEEG, we do so every block
                                % At the end of the block we are in, importing
                                % the EDF files from the eyetracker.
                                importEyetrackerEDF(Block_ctr-1,RestartFlag,InterruptFlag)
                                InterruptFlag=0;
                                initEyetracker(Block_ctr) % Then, initialize the tracker again, with new file name:
                                
                                % Starting the eyetracker recording again
                                Eyelink('StartRecording');
                            end
                        end
                    else % If we are using the tobii eyetracker:
                        % In case of restarting, the data should not be
                        % saved again, because they were not collected so
                        % far. The calibration was done already, so no need
                        % to repeat:
                        if ~RestartFlag || (RestartFlag && RestartBlockNumber+1 < Block_ctr)
                            if ~InterruptFlag && ~fMRI % If there was no interruption and we are not in fMRI mode, saving the data and redo calibration
                                % At the end of each bl ock we get the
                                % eyetracker data back:
                                saveTobiiGazeData(RestartFlag);
                                % Before the begining of a new block, performing the
                                % eyetracker calibration
                                tobiiCalibration; % Perform the calibration
                                % And then, we start the recording anew:
                                GazeData = tobii_eyetracker.get_gaze_data();
                                tobii_TimeCell = {'system_time_stamp', 'point_description'};
                            elseif ~InterruptFlag && fMRI % But if we are in fMRI, simply save the data, no recalibration
                                saveTobiiGazeData(RestartFlag);
                                % And then, we start the recording anew:
                                GazeData = tobii_eyetracker.get_gaze_data();
                                tobii_TimeCell = {'system_time_stamp', 'point_description'};
                            end
                        end
                    end
                end
            end
        
            % For the fMRI crew, before starting a mini-block, they
            % need to wait for the TR trigger of the other matlab
            % session
            if(fMRI)
                start_of_this_block_message = sprintf(strcat(BLOCK_START_MESSAGE_fMRI, '\n\n%d/8'), Block_ctr);
                showMessage(start_of_this_block_message);
                scannerListener()
                %                 TCPIP = tcpip('0.0.0.0', 30000, 'NetworkRole', 'server','Timeout',Inf);
                %                 fopen(TCPIP);
                %                 fread(TCPIP, 1, 'double');
                %                 Receiving_Time=GetSecs;
                %                 fclose(TCPIP);
                %                 save('Receiving_Time','Receiving_Time');
                RunOnset=showFixation('PhotodiodeOn');
                WaitSecs(1);
                %             end
            end
        end
        
        
        % -----------------------------------------------------------------
        % Checking if restart flag needs to be reset to 0:
        % -----------------------------------------------------------------        
        % If the experiment was restarted, a flag is set to give
        % the RESTARTED suffix to the trigger logs and eyetracker
        % file. But only for the block during which the
        % interruption occured. If we change block, then we reset
        % the flag to 0. !! This has to be done after the statement
        % saving the EDF files, it:
        if RestartFlag && RestartBlockNumber+1 < Block_ctr
            if ~MEEG % If we are not in MEEG, then we reset the restart flag, because the new data files produced were not restarted anymore
                RestartFlag = 0;
            else % If we are in the MEEG modality
                DoubleBlockNumber = floor((Block_ctr-1)/2);
                doubleRestartBlockNumber = floor((RestartBlockNumber-1)/2);
                if DoubleBlockNumber > doubleRestartBlockNumber % The reset flag should only be reset if the doubleblock number differs from the one when the experiment was restarted
                    RestartFlag = 0;
                end
            end
        end
        
        
        % -----------------------------------------------------------------
        % Prepare start of miniblock:
        % -----------------------------------------------------------------
        miniBlocks{miniBlockNum,BLOCK_NUM_COL} = Block_ctr; % add block_nr info to miniBlocks
        % for each miniblock count the number of HITS, MISSES, FA and CR
        misses = 0;
        hits = 0;
        fa = 0;
        cr = 0; % correct rejection
        
        % Showing the miniblock begin screen. This is the target screen
        TargetScreenOnset=showMiniBlockBeginScreen(miniBlocks, miniBlockNum);
        if ECoG % For the ECOG and MEEG or Behavior, wait for the participant to press a key to proceed
            KbWait(compKbDevice,3);
        elseif MEEG %for MEEG, wait 5 seconds max
            KbWait(compKbDevice,3,WaitSecs(0)+5);
        else % for fMRI, wait 5 seconds
            WaitSecs(5);
        end
        
        % Just before we really get started, the MB number is sent to
        % via the LPT trigger:
        if MEEG
            sendTrig(TRG_MB_ADD+miniBlockNum, LPT_OBJECT,LPT_ADDRESS);
            WaitSecs(refRate);
            sendTrig(0, LPT_OBJECT,LPT_ADDRESS);
            WaitSecs(refRate);
        end
        
        if EYE_TRACKER
            if ~TOBII_EYETRACKER
                Eyelink('Message',num2str(TRG_MB_ADD+miniBlockNum));
            else
                tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(TRG_MB_ADD+miniBlockNum)};
            end
        end
        
        %% Looping through Trials:
        % This where the experiment really starts:
        % loop through all trials; trials start from 0, so that trial 0 is
        % actually trial 1, 1 is 2 etc.
        for tr = 0 : miniBlocks{miniBlockNum,MINI_BLOCK_SIZE_COL} - 1
            
            if VERBOSE display(miniBlockNum); end
        

            % First, a bunch of flags needs to be initialized. They are
            % here because when you loop through time, you want to flag
            % when something has already happened. Otherwise, you will
            % redo it everytime you loop through time.
            hasInput = FALSE; % input flag, marks if participant already replied
            fixShown = FALSE; % fixation flag, marks if the fixation was already displayed
            jitterLogged = FALSE; % logging jitter flag, marks if the jitter already started: ALEX: HERE CSABA. I THINK WE SHOULD CHANGE THE NAME TO jitterStarted OR SOMETHING, OTHERWISE IT IS A BIT WEIRD
            PauseTimeLogged = FALSE; % This one will only be usefull if a pause occured
            
            % For ECoG, to avoid audio triggers conflict with the
            % response, we only send it if the response is in a window
            % of 200ms to 2000ms after stimulus onset. 200ms to avoid
            % conflict with the stim trigger
            if ECoG && ~NO_AUDIO CanSendAudioTrigger = FALSE;end

            % clear bitsi
            if strcmp(LAB_ID,'SC')
               bitsi_buttonbox.clearResponses();
            end
            % Flag for the first jitter:
            firstJitShown = FALSE; % For the first jitter, we set it separately, because things happen in a separate while loop
            
            % Resetting the pause time and the restart time:
            PauseTime = 0; % If the experiment is paused, the duration of the pause is stored to account for it.
            RestartInterval=0; % The duration until the experimenter confirms restarting is stored to account for it.
            
            % Here we need to set a bunch of flags for the LPT and the
            % phototriggers. Whenever they are turned on, they need to
            % be turned off. So we need to flag when we turn them off,
            % otherwise, we will "return" them off through every
            % iteration:
            % =========================================================
            % Setting the triggers flags:
            % LPT TRIGGER FLAGS
            TriggerOFF.LPT.Fixation = FALSE;
            TriggerOFF.LPT.Jitter   = FALSE;
            TriggerOFF.LPT.Response = FALSE;
            
            % Photo TRIGGER FLAGS
            TriggerOFF.Photo.Fixation = FALSE;
            TriggerOFF.Photo.Jitter   = FALSE;
            TriggerOFF.Photo.Response = FALSE;
            
            % First trial jitter and fixation trigger flags:
            firstTrialTriggerOFF.LPT.Fixation = FALSE;
            firstTrialTriggerOFF.LPT.Jitter = FALSE;
            % =========================================================
            
            
            %% First trial fixation:
            % If we are at the first trial of a miniblock, a fixation followed by a jitter must be presented.
            if tr == 0
                % Sending the first fixation with jitter of each
                % mini-block. Here the participant cannot respond. The
                % photodiode is turned off, because we do not want to
                % log the first fixation with a photodiode
                
                fixOnset = showFixation('PhotodiodeOff'); % 1
                
                % Then, the different triggers are sent:
                if MEEG sendTrig(TRG_STIM_END,LPT_OBJECT,LPT_ADDRESS); end
                % This trigger can stay because only takes 75 ms and we
                % have a window of 200
                if EYE_TRACKER
                    if ~TOBII_EYETRACKER
                        Eyelink('Message',num2str(TRG_STIM_END));
                    else
                        tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(num2str(TRG_STIM_END))};
                    end
                end
                if ECoG && ~NO_AUDIO sendTrigAudio(dec2bin(TRG_MBONSET_AUD+Block_ctr,7)); end % Sending an audio trigger to mark onset of mb
                if (fMRI && mod(miniBlockNum,4) == 1)
                setOutputTable('RunOnset', miniBlocks, miniBlockNum,tr,RunOnset);
                end
                %Save Target Screen Onset
                setOutputTable('TargetScreenOnset', miniBlocks, miniBlockNum,tr,TargetScreenOnset);
                % log fixation in journal
                setOutputTable('Fixation', miniBlocks, miniBlockNum, tr, fixOnset); %4 %setting all the trial values in the output table
                % Starting the while loop for the jitter of the first
                % trial. Not relying on waitsec because it disables the
                % possibility to turn off the photodiode and LPT triggers:
                
                elapsedTime = 0;
                while elapsedTime<((TRIAL_DURATION - miniBlocks{miniBlockNum, TRIAL1_TIME_COL}) ...
                        + miniBlocks{miniBlockNum, TRIAL1_JITTER_TIME_COL + tr}) - refRate*(2/3)

                    % If the LPT trigger was sent, the port needs to be
                    % set back to 0 after a frame
                    if MEEG && floor(elapsedTime/refRate) >= 1 && firstTrialTriggerOFF.LPT.Fixation == FALSE
                        sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                        firstTrialTriggerOFF.LPT.Fixation = TRUE;
                    end
                    
                    
                    % Then if the time of the fixation is exceeded,
                    % show the jitter:
                    if elapsedTime >=  (TRIAL_DURATION - miniBlocks{miniBlockNum, TRIAL1_TIME_COL})...
                            && firstJitShown == FALSE
                        
                        JitOnset = showFixation('PhotodiodeOff'); % 6
                        
                        % log jitter started
                        setOutputTable('Jitter', miniBlocks, miniBlockNum, tr, JitOnset);
                        if MEEG sendTrig(TRG_JITTER_START,LPT_OBJECT,LPT_ADDRESS); end
                        %if ECoG sendTrigAudio(TRG_JITTER_START); end % 7
                        if EYE_TRACKER
                            if ~TOBII_EYETRACKER
                                Eyelink('Message',num2str(TRG_JITTER_START));
                            else
                                tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(TRG_JITTER_START)};
                            end
                        end
                        firstJitShown = TRUE;
                    end
                    
                    % Turn off the jitter LPT trigger:
                    if firstJitShown == TRUE && MEEG && floor((GetSecs-JitOnset)/refRate) >= 1  && ...
                            firstTrialTriggerOFF.LPT.Jitter == FALSE
                        sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                        firstTrialTriggerOFF.LPT.Jitter = TRUE;
                    end
                    
                    elapsedTime = GetSecs - fixOnset;
                end
                
            end
                

            %% Showing the stimulus and everything that comes after:
            % present first stimuli and start the clock on the experiment
            
            miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr} = showStimuli(miniBlocks, miniBlockNum, tr); % 2
            
            % I then set a frame counter. The flip of the stimulus
            % presentation is frame 0. It is already the previous frame because it already occured:
            PreviousFrame = 0;
            % I then set a frame index. It is the same as the previous
            % frame for now
            FrameIndex = PreviousFrame;
            % I then send the first trigger of the successive trigger scheme:
            if MEEG % +1 because 1st frame is 0
                sendTrig(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1),LPT_OBJECT,LPT_ADDRESS);
            end
            if ECoG && ~NO_AUDIO
                sendStimTrigsAudio(miniBlocks,miniBlockNum,tr); % Here no need to add the plus 1, because of how the sendStimTrigsAudio access the stimulus ID
            end
            if EYE_TRACKER
                if ~TOBII_EYETRACKER
                    % For the eyetracker, the stimulus category is sent
                    % first.
                    % HERE I NEED TO CHECK BACK WITH KONSTANTINOS IF I DO
                    % IT RIGHT
                    Eyelink('Message',num2str(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1)));
                    % Following the sending of the messages, I get the timestamp from the
                    % experiment computer and the estimated eyetracker time
                    % stamp. This is mimicking the video game:
                    StimComputerTimeStamp = miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr};
                    TrackerTimeStamp      =  Eyelink('TrackerTime');
                    % The first thing to send are the time stamps of the local computer and
                    % the estimated time stamp of the tracker, so that we can then align:
                    Eyelink('Message',num2str(StimComputerTimeStamp)); % Stim computer time stamp
                    Eyelink('Message',num2str(TrackerTimeStamp)); % Eyetracker time stamp
                    Eyelink('Message',num2str(TrackerTimeStamp-StimComputerTimeStamp)); % DT
                else
                    % Sending the stimID trigger:
                    tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1)};
                    StimComputerTimeStamp = GetSecs;
                    TrackerTimeStamp = tobii.get_system_time_stamp;
                    tobii_TimeCell(end+1,:) = {TrackerTimeStamp,num2str(StimComputerTimeStamp)};
                    tobii_TimeCell(end+1,:) = {TrackerTimeStamp,num2str(TrackerTimeStamp - StimComputerTimeStamp)};
                end
            end
            
            % Log the stimulus presentation in the output table
            setOutputTable('Stimulus', miniBlocks, miniBlockNum, tr, miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr}); %setting all the trial values in the output table
            
            if tr > 0 % only if not first trial: log stimuli duration and blank duration using the previous trial:
                miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr-1} = miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr} - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr-1};
                miniBlocks{miniBlockNum,TRIAL1_STIM_DUR_COL + tr-1} = miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr-1} - miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr-1};
                miniBlocks{miniBlockNum,TRIAL1_DURATION_COL + tr-1} = miniBlocks{miniBlockNum,TRIAL1_STIM_DUR_COL + tr-1} + miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr-1};
            end
            
            
            %% Time loop
            % This is perhaps the most important part of the code. This
            % is the loop controlling all the timings of the different
            % events:
            elapsedTime = 0; % time elapse through trial (since its 0 one loop iteration at least will be performed)
            % this loop runs for trial duration.
            % it receives user response for the above stimuli
            % and then presents fixation (for a duration that includes its jitter)
            while elapsedTime < TRIAL_DURATION + miniBlocks{miniBlockNum, TRIAL1_JITTER_TIME_COL + tr} - (refRate*(2/3))
                
                % In order to count the frames, I always convert the
                % time to frames by dividing it by the refresh rate:
                CurrentFrame = floor(elapsedTime/refRate);

                
                % =====================================================
                % a. Successive triggers:
                % If the current frame number is different from the
                % previous, then a new frame started so I send the new triggers:
                if CurrentFrame > PreviousFrame
                    % I set the frame index to be the previous
                    % frame plus one. This is a safety, in case
                    % something happens beforehand and that the current
                    % frame is not the one following the previous one.
                    % That way, I make sure that all the trigger are
                    % sent even if the frames counting is off
                    FrameIndex = FrameIndex +1;
                    % I can then send the different triggers:
                    % We send the MEEG trigger
                    if MEEG && FrameIndex < size(TriggerMatrix,3)
                        sendTrig(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1),LPT_OBJECT,LPT_ADDRESS);
                    end
                    % Then the eyetracker trigger
                    if EYE_TRACKER && FrameIndex + 1 < size(TriggerMatrix,3) 
                            if ~TOBII_EYETRACKER
                                Eyelink('Message',num2str(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1)));
                            else
                                tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1))};
                            end
                    end
                    
                    % If the frame counter is at DIOD_DURATION - 1, it means that the
                    % flip already occured, which means that for the
                    % next one, the photodiode trigger should be
                    % removed. It is really imporatant that this
                    % statement is in the end. The turnPhotoTrigger
                    % function waits for a flip to occur, so it
                    % basically pauses the execution until the new
                    % frame is done. So if it was above the other
                    % things, then it would delay them. Here it should
                    % be fine: SOMEONE NEEDS TO CHECK THAT
                    if PHOTODIODE && (CurrentFrame == DIOD_DURATION - 1)
                        turnPhotoTrigger('off');
                    end
                    
                    % Finally actualizing the frame: the current Frame
                    % becomes the previous one because time is ruthless
                    PreviousFrame = CurrentFrame;
                end
                % =====================================================
                
                
                % =====================================================
                % (b.) For the ECoG audio trigger, we only send the audio
                % trigger if the participant answer after the first
                % 200ms to avoid audio triggers interferences.
                % Therefore, after the
                if (ECoG && ~NO_AUDIO) && elapsedTime > RESP_TRIG_ONSET
                    CanSendAudioTrigger = TRUE;
                end
                
                
                % =====================================================
                % b. Check input
                % if there was no input yet, get input and log if it is correct
                if hasInput == FALSE
                    % The get input function takes in the pause time
                    % and spits it back out. If you didn't paused, it
                    % will not change, if you do, it will be the
                    % duration of the break
                    [key,RT,PauseTime] = getInput(PauseTime);
                    
                    % -----------------------------------------------------
                    % Restarting and interruption keys treatment
                    % If there was a pause, log it in the output table:
                    if PauseTime > 0 &&  PauseTimeLogged == FALSE
                        setOutputTable ('Pause', miniBlocks, miniBlockNum, tr, RT, PauseTime)
                        PauseTimeLogged = TRUE;
                    end
                    % If the restart key was pressed
                    if(key == RESTART_KEY)
                        %  Ask the experiment whether he really wishes
                        %  to restartq
                        if(fMRI)
                            showMessage(RESTART_MESSAGE_fMRI);
                        else
                            showMessage(RESTART_MESSAGE);
                        end
                        % Wait for answer
                        [secs, keyCode, deltaSecs] =KbWait(compKbDevice,3);
                        % Get the restart interval (the time it took
                        % the experimenter to say he/she wants to
                        % restart:
                        RestartInterval = (secs - RT) - PauseTime; % Need to take the pause time into account, otherwise, we would be counting it twice down the line!
                        
                        % If the experimenter wants to restart, log it:
                        if(keyCode(YesKey))
                            setOutputTable ('Interruption', miniBlocks, miniBlockNum, tr, secs)
                            % Now if we are in ECoG or fMRI mode, we
                            % want the experimenter to decide whether
                            % he wants to go back a block or a
                            % miniBlock:
                            if ~fMRI
                                showMessage(RESTARTBLOCK_OR_MINIBLOCK_MESSAGE)
                                % Wait for answer
                                [~, BlkOrminiBlk_keyCode, ~] =KbWait(compKbDevice,3);
                            end
                            break
                        else % Else, continue:
                            key=NO_KEY;
                        end
                    elseif (key == ABORT_KEY) % If the experiment was aborted:
                        setOutputTable ('Abortion', miniBlocks, miniBlockNum, tr, RT, PauseTime)
                        ABORTED = 1;
                        cleanExit(); % Sends an error and therefore sends you into hte catch statement at the very bottom, where things are saved
                    end
                    
                    
                    % -----------------------------------------------------
                    % Responses keys treatment (needs to be separated from
                    % above, because above can change the key input
                    % depending on what's pressed, i.e. pursuing after 
                    % clicking restart)
                    % Log the response received:
                    miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr} = key;
                    % If the participant pressed a key:
                    if key ~= NO_KEY
                        
                        % -------------------------------------------------
                        % Sending the response triggers first to get best
                        % timing:
                        % Sending response trigger for MEEG:
                        if MEEG sendResponseTrig(); end
                        
                        % Sending response trigger for ECoG (no photodiode
                        % trigger because they are yolked to frame rate by nature
                        % and responses are not: 
                        if (ECoG && ~NO_AUDIO) && CanSendAudioTrigger
                            sendResponseTrigAudio();
                            % If the response trigger was already sent,
                            % we can't send it anymore for this trial
                            CanSendAudioTrigger = FALSE;
                        end
                        
                        % Sending response trigger for the eyetracker
                        if EYE_TRACKER
                            if ~TOBII_EYETRACKER
                                Eyelink('Message',num2str(TRG_RESPONSE));
                            else
                                tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(TRG_RESPONSE)};
                            end
                        end
                        
                        % Logging the reaction time:
                        miniBlocks{miniBlockNum,TRIAL1_RESPONSE_TIME_COL + tr} = RT - miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr};
                        hasInput = TRUE; % Flagging the input
                        
                        % Logging whether the response was correct:
                        if key == TARGET_KEY % taget key was pressed
                            miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr} = isTarget(miniBlocks,miniBlockNum,tr);
                            if miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr}
                                hits = hits + 1;
                            else
                                fa = fa + 1;
                            end
                        else % other key was pressed
                            % I take any wrong key as a legitimate button press
                            miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr} = isTarget(miniBlocks,miniBlockNum,tr);
                            if miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr}
                                hits = hits + 1;
                            else
                                fa = fa + 1;
                            end
                            miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr} = WRONG_KEY;
                        end
                        % log response in journal
                        setOutputTable('Response', miniBlocks, miniBlockNum, tr, RT);
                    else % no key was pressed
                        miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr} = ~isTarget(miniBlocks,miniBlockNum,tr);
                    end
                end
                
                % If one frame passed since the response trigger and
                % there was a response and the LPT trigger is not off,
                % then turn it off
                if  MEEG && hasInput == TRUE && (GetSecs - RT) > ...
                        refRate && TriggerOFF.LPT.Response == FALSE
                    sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                    TriggerOFF.LPT.Response = TRUE;
                end
                % =====================================================
                
                
                % =====================================================
                % c. Present fixation
                % if there was no fixation yet, show fixation
                if elapsedTime >= (miniBlocks{miniBlockNum, TRIAL1_TIME_COL + tr} - refRate*(2/3)) && fixShown == FALSE
                    miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr} = showFixation('PhotodiodeOn');
                    if MEEG sendTrig(TRG_STIM_END,LPT_OBJECT,LPT_ADDRESS); end
                    %if ECoG sendTrigAudio(TRG_STIM_END); end
                    if EYE_TRACKER
                        if ~TOBII_EYETRACKER
                            Eyelink('Message',num2str(TRG_STIM_END));
                        else
                            tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(TRG_STIM_END)};
                        end
                    end
                    % log fixation in journal
                    setOutputTable('Fixation', miniBlocks, miniBlockNum, tr, miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr}); %setting all the trial values in the output table
                    fixShown = TRUE;
                end
                
                if MEEG && fixShown == TRUE && (GetSecs - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr}) > ...
                        refRate && TriggerOFF.LPT.Fixation == FALSE
                    sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                    TriggerOFF.LPT.Fixation = TRUE;
                end
                
                
                % Turning off the phototrigger n frames after the fixation:
                if PHOTODIODE && fixShown == TRUE && (GetSecs - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr}) > ...
                        (DIOD_DURATION*refRate - refRate*(2/3)) && TriggerOFF.Photo.Fixation  == FALSE
                    turnPhotoTrigger('off');
                    TriggerOFF.Photo.Fixation = TRUE;
                end
                % =====================================================
                
                
                % =====================================================
                % d. Present the jitter
                if elapsedTime > TRIAL_DURATION  - refRate*(2/3) && jitterLogged == FALSE
                    
                    JitOnset = showFixation('PhotodiodeOn');
                    if MEEG sendTrig(TRG_JITTER_START,LPT_OBJECT,LPT_ADDRESS); end
                    if EYE_TRACKER
                        if ~TOBII_EYETRACKER
                            Eyelink('Message',num2str(TRG_JITTER_START));
                        else
                            tobii_TimeCell(end+1,:) = {tobii.get_system_time_stamp,num2str(TRG_JITTER_START)};
                        end
                    end
                    % log jitter started
                    setOutputTable('Jitter', miniBlocks, miniBlockNum, tr, JitOnset);
                    jitterLogged = TRUE;
                    % For the ECoG, if we are in the jitter already,
                    % then the audio triggers for the response should
                    % not be sent anymore, to avoid conflict with the
                    % coming trigger:
                    if ECoG && ~NO_AUDIO CanSendAudioTrigger = FALSE; end
                end
                
                % Turning off the LPT trigger after the jitter
                if MEEG && jitterLogged == TRUE  && (GetSecs - JitOnset) > refRate ...
                        && TriggerOFF.LPT.Jitter == FALSE
                    sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                    TriggerOFF.LPT.Jitter = TRUE;
                end
                
                % Turning off the phototrigger after the jitter:
                if PHOTODIODE && jitterLogged == TRUE &&  (GetSecs - JitOnset) > ...
                        (DIOD_DURATION*refRate - refRate*(2/3)) && TriggerOFF.Photo.Jitter == FALSE
                    turnPhotoTrigger('off');
                    TriggerOFF.Photo.Jitter = TRUE;
                end
                % =====================================================
                
                
                % =====================================================
                % f. Updating clock:
                % update time since iteration begun. Subtract the time
                % of the pause to the elapsed time, because we don't
                % want to have it in there. If there was no pause, then
                % pause time = 0
                elapsedTime = (GetSecs) - miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr}-(PauseTime+RestartInterval);
            end % while
            
            %% End of trial
            % Here we do a few things once the trial is over:
            % If the restart key was pressed, we break
            if(key==RESTART_KEY)
                break
            end
            
            % if trial ended and no input, logs as CR or miss
            if (miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr} == TRUE) && hasInput == FALSE
                cr = cr + 1;
            elseif (miniBlocks{miniBlockNum,TRIAL1_ANSWER_COL + tr} == FALSE) && hasInput == FALSE
                misses = misses + 1;
            end
            
            %% End of miniBlock:
            % At the last trial of each mini block, save the data:
            if tr == miniBlocks{miniBlockNum,MINI_BLOCK_SIZE_COL} - 1 % on the last trial save
                % For the fMRI, a fixation is shown at the end of a miniblock:
                if (fMRI && mod(miniBlockNum,4) ~= 0 )
                    BaselineOnset=showFixation('PhotodiodeOn'); %
                    WaitSecs(MRI_BASELINE_PERIOD); %
                    setOutputTable('Baseline', miniBlocks, miniBlockNum, tr, BaselineOnset);
                end
                % update data structure for this block
                miniBlocks{miniBlockNum,MISSES_COL} = misses;
                miniBlocks{miniBlockNum,HITS_COL} = hits;
                miniBlocks{miniBlockNum,FA_COL} = fa;
                miniBlocks{miniBlockNum,CR_COL} = cr;
                
                % Show the saving message
                if(~fMRI)
                    miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr} = showMessage(SAVING_MESSAGE) - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr};
                else
                    miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr} = GetSecs - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr};
                end
                ttime = miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr} + miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr};
                % Set the output table
                setOutputTable('Save', miniBlocks, miniBlockNum, tr, ttime);
                
                if(ECoG  || (~ECoG && mod(miniBlockNum,4) == 0))
                    % Save the results to HD
                    saveBlockToHD(OUTPUT_TABLE,miniBlockNum,'Results',tr);
                    % Save the back up to HD
                    saveTrialBackupToHD(miniBlocks,1,'Backup');
                    % Save the data to excel:
                    saveBlockToExcel(OUTPUT_TABLE,miniBlockNum,tr);
                end
                
                if VERBOSE display(tr, 'In last trial loop with trial '); end
                % Save the audio trigger log to HD
                if ECoG && ~NO_AUDIO
                    saveTrigAudToHD();
                end
                
                % Save the LPT triggers log to HD
                if MEEG
                    saveTrigToHD();
                end
                
                % Showing the indices of whether the participant performed
                % well or not:
                if(mod(miniBlockNum,4) == 0 && ~ECoG)
                    NumBlockTargets=sum(and(cell2mat(OUTPUT_TABLE(find(cell2mat(OUTPUT_TABLE(2:end,BLK_COL))==Block_ctr)+1,DSRD_RESP_COL)),ismember(OUTPUT_TABLE(find(cell2mat(OUTPUT_TABLE(2:end,BLK_COL))==Block_ctr)+1,EVENT_TYPE_COL),'Stimulus')));
                    
                    BlockIndices=find(cell2mat(OUTPUT_TABLE(2:end,BLK_COL))==Block_ctr)+1;
                    BlockHitsLoc=OUTPUT_TABLE(BlockIndices,HT_COL);
                    BlockFALoc=OUTPUT_TABLE(BlockIndices,FAS_COL);
                    BlockHitsLoc(cellfun('isempty',BlockHitsLoc)) = {0};
                    BlockFALoc(cellfun('isempty',BlockFALoc)) = {0};
                    BlockHits=sum(cell2mat(BlockHitsLoc));
                    BlockFA=sum(cell2mat(BlockFALoc));
                    
                    HitsScore=(4/NumBlockTargets)*BlockHits;
                    FAScore=(-3/NumBlockTargets)*BlockFA+3;
                    FAScore(find(FAScore<0))=0;
                    
                    StimulusCategory=floor(cell2mat(OUTPUT_TABLE(BlockIndices(find(cell2mat(BlockFALoc))),TYPE_COL))/1000);
                    TARG1Category=floor(cell2mat(OUTPUT_TABLE(BlockIndices(find(cell2mat(BlockFALoc))),TARG1_COL))/1000);
                    TARG2Category=floor(cell2mat(OUTPUT_TABLE(BlockIndices(find(cell2mat(BlockFALoc))),TARG2_COL))/1000);
                    
                    BlockIrrelevantCategoryFA=sum(not(or(StimulusCategory==TARG1Category,StimulusCategory==TARG2Category)));
                    if(BlockIrrelevantCategoryFA==0)
                        IrrelevantCategoryFAScore=3;
                    else
                        IrrelevantCategoryFAScore=0;
                    end
                    TotalScore=(HitsScore+FAScore+IrrelevantCategoryFAScore)*10;
                    feedback_message_flag1=not(HitsScore/4<=0.5 || FAScore/3<=0.5 || IrrelevantCategoryFAScore<3);
                    feedback_message_flag2=HitsScore/4<=0.5;
                    feedback_message_flag3=FAScore/3<=0.5;
                    feedback_message_flag4=IrrelevantCategoryFAScore<3;
                    feedback_messages=FEEDBACK_MESSAGES(find([feedback_message_flag1 feedback_message_flag2 (feedback_message_flag3 || feedback_message_flag4)]));
                    
                end
                
                if DEBUG disp(sprintf('Saving took : %f \n',GetSecs - ttime)); end
                
                % Copying these guys to a secret location to avoid overwritting
                secretDataSaving
                % calculate the last stimulus duration and entire trial duration
                miniBlocks{miniBlockNum,TRIAL1_STIM_DUR_COL + tr} = miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr} - miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr};
                miniBlocks{miniBlockNum,TRIAL1_DURATION_COL + tr} = miniBlocks{miniBlockNum,TRIAL1_STIM_DUR_COL + tr} + miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr};
            end
        end % Trial loop
        
        % If the participant pressed the escape key, change the
        % miniblock and block ctr accordingly:
        if(key==RESTART_KEY)
            % Set the interrupt flag to 1
            InterruptFlag=1;
            % For fMRI
            if (fMRI)              
                % Compute the miniblock and block number:
                miniBlockNum=1+4*(floor((miniBlockNum-1)/4));
                Block_ctr=floor(miniBlockNum/4);
                % Save the interrupted block to HD and excel:
                saveBlockToHD(OUTPUT_TABLE,miniBlockNum,'Results',tr); % Saving the fMRI data in case of interruption
                saveBlockToExcel(OUTPUT_TABLE,miniBlockNum,tr);
                save('RestartFlag.mat','Block_ctr')
            % If not in fMRI, the experimenter had to choose between going
            % back a block or a miniblock:
            elseif BlkOrminiBlk_keyCode(MINIBLOCK_RESTART_KEY) % If the experimenter only wants to go back to the miniBlock:
                if mod(miniBlockNum,4) == 1
                    Block_ctr=floor((miniBlockNum)/4);
                end
            else BlkOrminiBlk_keyCode(BLOCK_RESTART_KEY) % If the experimenter only wants to go back to the miniBlock:
                miniBlockNum=1+4*(floor((miniBlockNum-1)/4));
                Block_ctr=floor(miniBlockNum/4);
            end
        else% Resetting the restarting flag (in case it wasn't already)
            InterruptFlag=0;
            % Actualizing the miniBlock counter
            miniBlockNum=miniBlockNum+1;
        end
        
        
    end % end of miniblock loop
%     if fMRI
%        TCPIP = tcpip('0.0.0.0', 20000, 'NetworkRole', 'server','Timeout',Inf);
%        fopen(TCPIP);
%        fread(TCPIP, 1, 'double');  
%        fclose(TCPIP);
%     end
    %% Ending the experiment
    % end of experiment, save to HD and summarize results
    showMessage(SAVING_MESSAGE);
    % Mark the time of saving onset
    ttime = GetSecs;
    % Save the data:
    saveBlockToHD(OUTPUT_TABLE,miniBlockNum-1,'Results',tr);
    % Save the back up:
    saveTrialBackupToHD(miniBlocks,1,'Backup');
    % Save the different data to excel:
    saveBlockToExcel(OUTPUT_TABLE,miniBlockNum-1,tr);
    saveSummaryToExcel(OUTPUT_TABLE, 1);
    % Save the different triggers
    if MEEG saveTrigToHD(); end
    if VERBOSE disp('To save trigAudtoHD because end of experiment'); end
    if ECoG && ~NO_AUDIO saveTrigAudToHD(); end
    
    % Copying these guys to a secret location to avoid overwritting
    secretDataSaving
    
    if DEBUG
        disp(sprintf('Saving took : %f \n',GetSecs - ttime));
    end
    
    
    %% Crashes management
    % If the experiment crashes along the way, we make sure to save the
    % results and the backup:
catch e
    try
        ttime = GetSecs;
        saveTrialBackupToHD(miniBlocks,1,'Backup');
        saveBlockToHD(OUTPUT_TABLE,miniBlockNum,'Results',tr);
        % Save the different data to excel:
        saveBlockToExcel(OUTPUT_TABLE,miniBlockNum,tr);
        saveSummaryToExcel(OUTPUT_TABLE, 1);
        if VERBOSE disp('saving because user quit'); end
        if DEBUG disp(sprintf('Saving took : %f \n',GetSecs - ttime)); end
    catch
    end
    rethrow(e);
    
end

end

