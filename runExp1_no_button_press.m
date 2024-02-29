%% Coded by Alex LePauvre, Katarina Bendtz, Yoav Roll, Csaba Kozma and Aya Khalif
    % Dr. Liad Mudrik's Lab, Tel-Aviv University
    % Last changed: 12/03/20 by Yoav Roll
    % Updated by Dr. Katarina Bendtz 02/03/20 katarina@bendtz.se
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
function runExp1_no_button_push( subNum, viewDist)
    %% House-keeping:
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
    % This function sets all the parameters for the recording modalities
    % and code. Gives back the VIEW_DISTANCE, which is the default viewing
    % distance. 
    initRuntimeParameters;
    
    %% Setting parameters in case of missing inputs:
    % If there are less than two inputs in the runExp1 function
    if nargin < 2
        % If debug mode, replace the subNum by 999
        if DEBUG
            subNum = 999;
        end
        % Since the experimenter didn't set the viewing distance, we take
        % the default
        viewDist = VIEW_DISTANCE; % default value in centimeters
        Screen('Preference', 'SkipSyncTests', 1);
    end

    if nargin < 1 && DEBUG == 0     % Check if all needed parameters given:
        error('Must provide required input parameters %d!',subNum);
    end

    %% Checking if participant already exists:
    global viewDistance compKbDevice subjectNum END_WAIT END_OF_EXPERIMENT_MESSAGE LOADING_MESSAGE INSTRUCTIONS2 INSTRUCTIONS1 INSTRUCTIONS3 
    global PRACTICE_START_MESSAGE PRACTICE_START_MESSAGE_fMRI EXPERIMET_START_MESSAGE EXPERIMET_START_MESSAGE_fMRI LPT_CODE_START LPT_OBJECT LPT_ADDRESS TRG_EXP_START_MSG TRG_EXP_END_MSG
    global TRIGAUD_CODE_START WhereToRestart TRG_EXP_START_MSG_AUD TRG_EXP_END_MSG_AUD
    global ExistFlag DATA_FOLDER TEMPORARY_FOLDER EXPERIMENT_NAME BEHAV_FILE_NAMING_WHOLE miniBlocksInfo
    subjectNum = subNum;

    compKbDevice = -1;
    viewDistance = viewDist;

    % Before we go further, we check whether the participant number was
    % already used:
    ParticipantFolder = sprintf('%s%c%s%c%s%c%s_LPTtriggers_ID%s',pwd,filesep,'data',filesep,[LAB_ID,num2str(subjectNum),filesep]);
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
            % Waiting 10ms to turn it off:
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
            turnPhotoTrigger('on');
            WaitSecs(refRate*(DIOD_DURATION-0.5));
            turnPhotoTrigger('off');
            WaitSecs(refRate*(DIOD_DURATION-0.5));
            turnPhotoTrigger('on');
            WaitSecs(refRate*(DIOD_DURATION-0.5));
            turnPhotoTrigger('off');
            WaitSecs(refRate*(DIOD_DURATION-0.5));
            turnPhotoTrigger('on');
            WaitSecs(refRate*(DIOD_DURATION-0.5));
            turnPhotoTrigger('off');
        end
        
        
        % Initialize the eyetracker
        if EYE_TRACKER 
            initEyetracker(1); % ALEX: HERE THE 1 WILL NEED TO BE CHANGED IF WE RESTARTED THE EXPERIMENT!!!!
        end
        

        %% Starting and setting up presentation:

        showMessage(LOADING_MESSAGE);
        % Saves a copy of code to disk
        saveCode();

        % Loads all textures from hard drive before the experiment runs
        loadTexturesFromHD();
     
        %% Restarting contingencies:
        
        % If the experiment wasn't restarted, then:
        if ~ExistFlag
            % Create all trials before expriement runs
            [miniBlocks, TriggerMatrix] = createMiniBlocks();
            
            % displays all instruction screens
            showInstructions(INSTRUCTIONS1);
            %KbWait(compKbDevice,3);
            WaitSecs(4);
            showInstructions(INSTRUCTIONS2);
            %KbWait(compKbDevice,3);
            WaitSecs(4);
            showInstructions(INSTRUCTIONS3);
            %KbWait(compKbDevice,3);
            WaitSecs(4);
            
            % runs one practice run, with specific stimuli
            if ~NO_PRACTICE
                if(~fMRI)
                    showMessage(PRACTICE_START_MESSAGE);
                    %KbWait(compKbDevice,3);
                    WaitSecs(4);
                else
                    showMessage(PRACTICE_START_MESSAGE_fMRI);
                    WaitSecs(2);
                end
                runPractice();
            end
            
            % runs the mini-blocks
            if(~fMRI)
                showMessage(EXPERIMET_START_MESSAGE);
                %KbWait(compKbDevice,3);
                WaitSecs(4);
                
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
            
        else % If the experiment is being restarted, we need to log things back up:
            %             load('miniBlocksInfo')
            BackupFile  = sprintf('%s%c%s%c%s%c%s%c%s_%s_ID%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,EXPERIMENT_NAME,'Backup',[LAB_ID,num2str(subjectNum)]);
            load(BackupFile)
            TriggerFile  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,'TriggerMatrix');
            load(TriggerFile)
            WholeFile = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING_WHOLE]);
            miniBlocksInfo=load(WholeFile);
            % Also, changing the name of the file where to save the data:
            
            
            % displays all instruction screens
            showInstructions(INSTRUCTIONS1);
            %KbWait(compKbDevice,3);
            WaitSecs(4);
            showInstructions(INSTRUCTIONS2);
            %KbWait(compKbDevice,3);
            WaitSecs(4);
            showInstructions(INSTRUCTIONS3);
            %KbWait(compKbDevice,3);
            WaitSecs(4);
            
            % runs the mini-blocks
            if(~fMRI)
                showMessage(EXPERIMET_START_MESSAGE);
                %KbWait(compKbDevice,3);
                WaitSecs(4);
                
            else
                showMessage(EXPERIMET_START_MESSAGE_fMRI);
                %WaitSecs(2);
                WaitSecs(4);
            end
            
            if MEEG
                sendTrig(TRG_EXP_START_MSG,LPT_OBJECT,LPT_ADDRESS);
                % Waiting 10ms to turn it off:
                WaitSecs(0.01);
                sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
            end
            
            if REDO_PRACTICE % If the experimenter wants to redo the practice, then it will happen. But otherwise,it won't
                if(~fMRI)
                    showMessage(PRACTICE_START_MESSAGE);
                    %KbWait(compKbDevice,3);
                    WaitSecs(4);
                else
                    showMessage(PRACTICE_START_MESSAGE_fMRI);
                    WaitSecs(2);
                end
                runPractice();
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
 
        % safe finish, closing Psychtoolbox, ends the recording methods:
        safeExit();
       
    catch e % When the program has error, it safely ends, closing psychtoolbox 
        Str = CmdWinTool('getText');
        dlmwrite(dfile,Str,'delimiter','');
        safeExit();
        rethrow(e);       
    end % try
    
    %% save everything from command window
    Str = CmdWinTool('getText');   
    dlmwrite(dfile,Str,'delimiter','');
end % function
    


%% runMiniBlocks 
% this function runs the experiment given a block with all miniblocks
% input:
% ------
% miniBlocks - the mini-blocks to run

function [ miniBlocks ] = runMiniBlocks( miniBlocks,TriggerMatrix )
    global output_table_cntr miniBlocksInfo BLOCK_NUM_COL MINIBLK_COL TRIAL1_ANSWER_COL TRIAL1_TIME_COL MINI_BLOCK_SIZE_COL compKbDevice TRIAL1_RESPONSE_TIME_COL   FALSE TRUE    TRIAL_DURATION
    global TRIAL1_BLANK_DUR_COL TRIAL1_STIM_DUR_COL refRate SAVING_MESSAGE NO_KEY TRIAL1_JITTER_TIME_COL TRIAL1_STIM_END_TIME_COL TRIAL1_BUTTON_PRESS_COL  TRIAL1_START_TIME_COL TRIAL1_DURATION_COL
    global MISSES_COL HITS_COL FA_COL CR_COL WRONG_KEY RESTART_KEY TARGET_KEY Behavior ExistFlag YesKey MINIBLOCK_RESTART_KEY BLOCK_RESTART_KEY
    global END_OF_BLOCK_MESSAGE END_OF_BLOCK_MESSAGE_fMRI BLOCK_START_MESSAGE_fMRI BREAK_MESSAGE_fMRI MEG_BREAK_MESSAGE RESTART_MESSAGE RESTART_MESSAGE_fMRI RESTARTBLOCK_OR_MINIBLOCK_MESSAGE DEBUG OUTPUT_TABLE MRI_BASELINE_PERIOD MEGbreakKey EYETRACKER_CALIBRATION_MESSAGE GENERAL_BREAK_MESSAGE
    global MEEG fMRI ECoG  LPT_OBJECT LPT_ADDRESS TRG_RESPONSE TRG_STIM_END TRG_JITTER_START TRG_MB_ADD
    global el EYE_TRACKER DIOD_DURATION RESP_TRIG_ONSET% This is a parameter generated when initializing the eyetracker, which is required for the calibratio
    global VERBOSE
    global PHOTODIODE ABORT_KEY Block_ctr WhereToRestart RestartFlag
    global NO_AUDIO triggers TRIGGER_ARRAY_SIZE triggsCounter triggersAudio triggsAudioCounter TRG_MBONSET_AUD
    global ABORTED % This is a flag that is turned to 1 if the experiment was aborted to store the data accordingly
    if VERBOSE
        disp('WELCOME TO runMiniBlocks') 
    end
    
    try
        % First things first, setting the aborted flag to 0:
        ABORTED = 0;
        
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
            OUTPUT_TABLE=miniBlocksInfo.miniBlocks; % The output table was loaded back up to continue filling it from where we left off
            miniBlockIndices=find(cell2mat(OUTPUT_TABLE(2:end,MINIBLK_COL)))+1;   % The miniBlock index is fecthed back +1 to account for the table header
            miniBlockNum=cell2mat(OUTPUT_TABLE(miniBlockIndices(end),MINIBLK_COL)); % The Block number is also fetched back 
            if(fMRI) % If we are in fMRI. we want to restart from the beginning of the block 
                miniBlockNum=1+4*floor((miniBlockNum-1)/4);
                Block_ctr=floor(miniBlockNum/4);
            elseif strcmp(WhereToRestart,'m')% If the Experimenter wants to, he/she can restart at the last miniBlock
                if mod(miniBlockNum,4) == 1 
                    Block_ctr=floor((miniBlockNum)/4); % If the experiment stopped in the first miniblock of a block, need to have block counter be -1 because it will go into +1 right after
                else
                    Block_ctr=1+floor((miniBlockNum-1)/4);
                end
            elseif strcmp(WhereToRestart,'b')% Or at the last Block
                miniBlockNum=1+4*floor((miniBlockNum-1)/4);
                Block_ctr=floor(miniBlockNum/4);
            end
            output_table_cntr=find(cell2mat(OUTPUT_TABLE(2:end,MINIBLK_COL))==miniBlockNum,1,'last')+2; % Fetching where to restart from when filling the output table. We append to the table rather than overwritting
            tr = 0; % This is necessary for things not to crash: when restarting, the tr will be called before being defined otherwise
            clear miniBlocksInfo
            % Since we are restarting, we should calibrate the eyetracker.
            % Except if we are in the first miniBlock, because in that
            % case, it will be done anyways down the line. Same if the block is restarted:
            if EYE_TRACKER && (miniBlockNum ~= 1 || mod(miniBlockNum,4) ~= 1)
                % Starting the eyetracker recording for the first block
                    % Calibrate or drift correction of the eye tracker
                    EyelinkDoTrackerSetup(el);
                    % Starting the recording
                    Eyelink('StartRecording');
            end % eye tracker
        end

        %% Miniblocks loop:
        % Running through miniBlocks. Use of a while so that we can go back
        % if the experimenter wants to (for loop don't go back)
        while miniBlockNum <= size(miniBlocks,1)
            if VERBOSE display(miniBlockNum, 'now we are just starting miniBlock nr: '); end
            
            if EYE_TRACKER && miniBlockNum == 1
                    % Starting the eyetracker recording for the first block
                    % Calibrate or drift correction of the eye tracker
                    EyelinkDoTrackerSetup(el);
                    % Starting the recording
                    Eyelink('StartRecording');
            end % eye tracker

            % keep track of block number (4 miniblocks = block)
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
                    
                    % If we are in MEEG recording, the experimenter needs
                    % to press the MEGbreakKey to leave time to save the
                    % MEEG data, as these needs to be saved every couple
                    % of 20 minutes
                    if MEEG 
                        % Resetting the triggers log file, only every two blocks:
                        % ALEX: HERE, WE NEED TO MAKE SURE OF WHAT HAPPENS
                        % WHEN WE RESTART THE EXPERIMENT!
                        if miniBlockNum > 1 && floor((miniBlockNum-1)/8)+1 ~= floor((miniBlockNum-2)/8)+1
                            triggers = cell(TRIGGER_ARRAY_SIZE,3);
                            % Restting the trigger counter:
                            triggsCounter = 0;
                        end
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
                        % ALEX: HERE, WE NEED TO MAKE SURE OF WHAT HAPPENS
                        % WHEN WE RESTART THE EXPERIMENT!
                        if miniBlockNum > 1 && floor((miniBlockNum-1)/4)+1 ~= floor((miniBlockNum-2)/4)+1
                            % Resetting the audio triggers for the saving
                            triggersAudio = [];
                            triggsAudioCounter = 1;
                        end
                        end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE, '\n\n%d/5'), Block_ctr);
                        showMessage(end_of_this_block_message);
                        %KbWait(compKbDevice,3);
                        WaitSecs(4);
                    end
                    
                    %%%You can take a break message
                    if (MEEG||Behavior||ECoG)
                        if ECoG
                            end_of_this_block__take_a_break_message = sprintf(strcat(GENERAL_BREAK_MESSAGE, '\n\n%d/5'), Block_ctr);
                        else 
                            end_of_this_block__take_a_break_message = sprintf(strcat(GENERAL_BREAK_MESSAGE, '\n\n%d/10'), Block_ctr);
                        end
                        showMessage(end_of_this_block__take_a_break_message);
                        %KbWait(compKbDevice,3);%
                        WaitSecs(4);
                    end
                    
                    if fMRI % If in fMRI mode, the experiment will proceed on its own after a set amount of time
                        % Generate the end of block message
                        end_of_this_block_message = sprintf(strcat(END_OF_BLOCK_MESSAGE_fMRI, '\n\n%d/8'), Block_ctr-1);
                        % Show the message
                        showMessage(end_of_this_block_message);
                        % Wait 2 seconds
                        WaitSecs(2);
                        % Show the BREAK_MESSAGE_fMRI
                        showMessage(BREAK_MESSAGE_fMRI);
                        % Wait for the participant to press a key:
                        KbWait(compKbDevice,3);
                        % WaitSecs(2);
                    end
                    
                    % If we are recoding with the eyetracker, now is time
                    % to do the calibration or drift correction
                    if EYE_TRACKER
                        % Stopping the eyetracker recording at the end of each
                        % block
                        Eyelink('StopRecording');
                        % The files then get imported from the tracker to
                        % the experiment computer:
                        % If we are not in MEEG, we do so every block
                        if ~MEEG
                            if ExistFlag && RestartFlag
                                importEyetrackerEDF(Block_ctr-1,1)
                                RestartFlag = 0; % Setting the tracker restart flag back to 0
                            else
                                importEyetrackerEDF(Block_ctr-1,1) % If the tracker restart flag is 0, then we don't need to worry about it!
                            end
                           initEyetracker(Block_ctr) % Then, initialize the tracker again, with new file name:
                        else % Otherwise, every second block:
                            if mod(Block_ctr,2) == 1
                                DoubleBlockNumber = floor((Block_ctr-1)/2);
                                if ExistFlag && RestartFlag
                                    importEyetrackerEDF(DoubleBlockNumber,1)
                                    RestartFlag = 0;
                                else
                                    importEyetrackerEDF(DoubleBlockNumber,1)
                                end
                            end
                        end
                        % Before the begining of a new block, performing the
                        % eyetracker calibration
                        showMessage(EYETRACKER_CALIBRATION_MESSAGE);
                        %KbWait(compKbDevice,3);
                        WaitSecs(4);
                        EyelinkDoTrackerSetup(el);
                        % Starting the eyetracker recording again
                        Eyelink('StartRecording');
                    end
                end
                
                % For the fMRI crew, before starting a mini-block, they
                % need to wait for the TR trigger of the other matlab
                % session
                if(fMRI)
                    start_of_this_block_message = sprintf(strcat(BLOCK_START_MESSAGE_fMRI, '\n\n%d/8'), Block_ctr);
                    showMessage(start_of_this_block_message);
                    Trigger=0;
                    ServerFlag=1;
                    save('ServerFlag','ServerFlag') 
                    while(Trigger~=5)   
                    TCPIP = tcpip('0.0.0.0', 30000, 'NetworkRole', 'server','Timeout',300);
                    fopen(TCPIP);
                    Trigger = fread(TCPIP, 1, 'double');
                    Receiving_Time=GetSecs;
                    fclose(TCPIP)
                    save('Receiving_Time','Receiving_Time');
                    showFixation('PhotodiodeOn');
                    WaitSecs(2) ;
                    end
                end
            end
            
            miniBlocks{miniBlockNum,BLOCK_NUM_COL} = Block_ctr; % add block_nr info to miniBlocks

            % for each miniblock count the number of HITS, MISSES, FA and CR
            misses = 0;
            hits = 0;
            fa = 0;
            cr = 0; % correct rejection
            
            % Showing the miniblock begin screen. This is the target screen
            showMiniBlockBeginScreen(miniBlocks, miniBlockNum);

            if(~fMRI) % For the ECOG and MEEG or Behavior, wait for the participant to press a key to proceed
                %KbWait(compKbDevice,3);
                WaitSecs(4);
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
                Eyelink('Message',num2str(TRG_MB_ADD+miniBlockNum)); 
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
                
                % For ECoG, to avoid audio triggers conflict with the
                % response, we only send it if the response is in a window
                % of 200ms to 2000ms after stimulus onset. 200ms to avoid
                % conflict with the stim trigger
                if ECoG CanSendAudioTrigger = FALSE;end
                
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
                    if EYE_TRACKER Eyelink('Message',num2str(TRG_STIM_END)); end
                    if ECoG sendTrigAudio(dec2bin(TRG_MBONSET_AUD+Block_ctr,7)); end % Sending an audio trigger to mark onset of mb
                    
                    % log fixation in journal
                    setOutputTable('Fixation', miniBlocks, miniBlockNum, tr, fixOnset); %4 %setting all the trial values in the output table
                    % Starting the while loop for the jitter of the first
                    % trial. Not relying on waitsec because it disables the
                    % possibility to turn off the photodiode and LPT triggers:
                    
                    elapsedTime = 0;
                    while elapsedTime<((TRIAL_DURATION - miniBlocks{miniBlockNum, TRIAL1_TIME_COL}) ...
                            + miniBlocks{miniBlockNum, TRIAL1_JITTER_TIME_COL + tr}) - refRate/2
                        
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
                            % Show the first jitter
                            
                            JitOnset = showFixation('PhotodiodeOff'); % 6
                            
                            % log jitter started
                            setOutputTable('Jitter', miniBlocks, miniBlockNum, tr, JitOnset);
                            if MEEG sendTrig(TRG_JITTER_START,LPT_OBJECT,LPT_ADDRESS); end
                            %if ECoG sendTrigAudio(TRG_JITTER_START); end % 7
                            if EYE_TRACKER Eyelink('Message',num2str(TRG_JITTER_START)); end
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
                if ECoG 
                    sendStimTrigsAudio(miniBlocks,miniBlockNum,tr); % Here no need to add the plus 1, because of how the sendStimTrigsAudio access the stimulus ID
                end
                if EYE_TRACKER 
                    % For the eyetracker, the stimulus category is sent
                    % first.
                    % HERE I NEED TO CHECK BACK WITH KONSTANTINOS IF I DO
                    % IT RIGHT
                    Eyelink('Message',num2str(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1))); 
                    % Following the sending of the messages, I get the timestamp from the
                    % experiment computer and the estimated eyetracker time
                    % stamp. This is mimicking the video game:
                    StimComputerTimeStamp = miniBlocks{miniBlockNum,TRIAL1_START_TIME_COL + tr};
                    TrackerTimeStamp      = Eyelink('Command','eyelink_tracker_double_usec');
                    % The first thing to send are the time stamps of the local computer and
                    % the estimated time stamp of the tracker, so that we can then align:
                    Eyelink('Message',num2str(StimComputerTimeStamp)); % Stim computer time stamp
                    Eyelink('Message',num2str(TrackerTimeStamp)); % Eyetracker time stamp
                    Eyelink('Message',num2str(TrackerTimeStamp-StimComputerTimeStamp)); % DT
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
                while elapsedTime < TRIAL_DURATION + miniBlocks{miniBlockNum, TRIAL1_JITTER_TIME_COL + tr} - (refRate/2)
                    
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
                            Eyelink('Message',num2str(TriggerMatrix(miniBlockNum,tr+1,FrameIndex+1)));
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
                    if ECoG && elapsedTime > RESP_TRIG_ONSET
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
                        
                        % If there was a pause, log it in the output table:
                        if PauseTime > 0
                            setOutputTable ('Pause', miniBlocks, miniBlockNum, tr, RT, PauseTime)
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
                            cleanExit();
                        end
                        % Log the response received
                        miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr} = key;
                        % If the participant pressed a key:
                        if key ~= NO_KEY
                            % Sending response trigger for MEEG
                            if MEEG sendResponseTrig(); end
                            % Sending response trigger for ECoG
                            if ECoG && CanSendAudioTrigger
                                sendResponseTrigAudio();
                                % If the response trigger was already sent,
                                % we can't send it anymore for this trial
                                CanSendAudioTrigger = FALSE;
                            end
                            % Sending response trigger for the eyetracker
                            if EYE_TRACKER Eyelink('Message',num2str(TRG_RESPONSE)); end
                            
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
                    if  MEEG && hasInput == TRUE && (GetSecs - miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr}) > ...
                            refRate && TriggerOFF.LPT.Response == FALSE
                            sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                            TriggerOFF.LPT.Response = TRUE;
                    end
                
                    % Turning off the response trigger after participants
                    % response, after the set amount of frames has passed:
                    if PHOTODIODE && hasInput == TRUE && (GetSecs - miniBlocks{miniBlockNum,TRIAL1_BUTTON_PRESS_COL + tr}) > ...
                            (DIOD_DURATION*refRate - refRate/2) && TriggerOFF.Photo.Response == FALSE
                        turnPhotoTrigger('off');
                        TriggerOFF.Photo.Response = TRUE;
                    end
                    % =====================================================
                    
                    
                    % =====================================================
                    % c. Present fixation
                    % if there was no fixation yet, show fixation
                    if elapsedTime >= (miniBlocks{miniBlockNum, TRIAL1_TIME_COL + tr} - refRate/2) && fixShown == FALSE
                        miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr} = showFixation('PhotodiodeOn');
                        if MEEG sendTrig(TRG_STIM_END,LPT_OBJECT,LPT_ADDRESS); end
                        %if ECoG sendTrigAudio(TRG_STIM_END); end
                        if EYE_TRACKER Eyelink('Message',num2str(TRG_STIM_END)); end
                        % log fixation in journal
                        setOutputTable('Fixation', miniBlocks, miniBlockNum, tr, miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr}); %setting all the trial values in the output table
                        fixShown = TRUE;
                    end
                    
                    if MEEG && fixShown == TRUE && (GetSecs - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr}) > ...
                            refRate && TriggerOFF.LPT.Fixation == FALSE
                        sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                        TriggerOFF.LPT.Fixation = TRUE;
                    end
               

                    % Turning off the phototrigger one frame after the fixation:
                    if PHOTODIODE && fixShown == TRUE && (GetSecs - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr}) > ...
                            (DIOD_DURATION*refRate - refRate/2) && TriggerOFF.Photo.Fixation  == FALSE
                        turnPhotoTrigger('off');
                        TriggerOFF.Photo.Fixation = TRUE;
                    end
                    % =====================================================
                    
                    
                    % =====================================================
                    % d. Present the jitter
                    if elapsedTime > TRIAL_DURATION  - refRate/2 && jitterLogged == FALSE

                        JitOnset = showFixation('PhotodiodeOn');
                        if MEEG sendTrig(TRG_JITTER_START,LPT_OBJECT,LPT_ADDRESS); end
                        if EYE_TRACKER Eyelink('Message',num2str(TRG_JITTER_START)); end                        
                        % log jitter started
                        setOutputTable('Jitter', miniBlocks, miniBlockNum, tr, JitOnset);
                        jitterLogged = TRUE;
                        % For the ECoG, if we are in the jitter already,
                        % then the audio triggers for the response should
                        % not be sent anymore, to avoid conflict with the
                        % coming trigger:
                        if ECoG CanSendAudioTrigger = FALSE; end
                    end
                    
                    % Turning off the LPT trigger after the jitter
                    if MEEG && jitterLogged == TRUE  && (GetSecs - JitOnset) > refRate ...
                             && TriggerOFF.LPT.Jitter == FALSE
                        sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                        TriggerOFF.LPT.Jitter = TRUE;
                    end
                    
                    % Turning off the phototrigger after the jitter:
                    if PHOTODIODE && jitterLogged == TRUE &&  (GetSecs - JitOnset) > ...
                            (DIOD_DURATION*refRate - refRate/2) && TriggerOFF.Photo.Jitter == FALSE
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
                    % update data structure for this block
                    miniBlocks{miniBlockNum,MISSES_COL} = misses;
                    miniBlocks{miniBlockNum,HITS_COL} = hits;
                    miniBlocks{miniBlockNum,FA_COL} = fa;
                    miniBlocks{miniBlockNum,CR_COL} = cr;
                    
                    % Show the saving message 
                    miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr} = showMessage(SAVING_MESSAGE) - miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr};
                    ttime = miniBlocks{miniBlockNum,TRIAL1_BLANK_DUR_COL + tr} + miniBlocks{miniBlockNum,TRIAL1_STIM_END_TIME_COL + tr};
                    % Save the results to HD
                    saveBlockToHD(OUTPUT_TABLE,miniBlockNum,'Results',tr);
                    % Save the back up to HD
                    saveTrialBackupToHD(miniBlocks,1,'Backup');
                    % Save the data to excel:
                    saveBlockToExcel(OUTPUT_TABLE,miniBlockNum,tr);
                    
                    if VERBOSE display(tr, 'In last trial loop with trial '); end
                    % Save the audio trigger log to HD
                    if ECoG && ~NO_AUDIO 
                        saveTrigAudToHD(); 
                        RestartFlag = 0;
                    end
                    % Save the LPT triggers log to HD
                    if MEEG 
                        saveTrigToHD(); 
                        RestartFlag = 0;
                    end
                    % Set the output table
                    setOutputTable('Save', miniBlocks, miniBlockNum, tr, ttime);
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
                if (fMRI)
                    miniBlockNum=1+4*(floor((miniBlockNum-1)/4));
                    Block_ctr=floor(miniBlockNum/4);
                    saveBlockToHD(OUTPUT_TABLE,miniBlockNum,'Results',tr); % Saving the fMRI data in case of interruption
                    saveBlockToExcel(OUTPUT_TABLE,miniBlockNum,tr);
                    save('RestartFlag.mat','Block_ctr')
                elseif BlkOrminiBlk_keyCode(MINIBLOCK_RESTART_KEY) % If the experimenter only wants to go back to the miniBlock:
                    if mod(miniBlockNum,4) == 1
                        Block_ctr=floor((miniBlockNum)/4);
                    end
                else BlkOrminiBlk_keyCode(BLOCK_RESTART_KEY) % If the experimenter only wants to go back to the miniBlock:
                    miniBlockNum=1+4*(floor((miniBlockNum-1)/4));
                    Block_ctr=floor(miniBlockNum/4);
                end
            else
                % Actualizing the miniBlock counter
                miniBlockNum=miniBlockNum+1;
                % For the fMRI, a fixation is shown at the end of a miniblock:
                if fMRI
                    showFixation('PhotodiodeOn'); %
                    WaitSecs(MRI_BASELINE_PERIOD); %
                end
            end
            
            
        end % end of miniblock loop
        
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

