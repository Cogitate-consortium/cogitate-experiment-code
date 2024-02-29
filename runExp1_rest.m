%% Coded by Yoav Roll yoav.roll@gmail.com

% modifed from runExp1 to run 5 min resting state with fixation & eye-tracking data
% by Ling 04/10/2020
% run as 'runExp1_rest( subNum, viewDist, REST_min)'

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
function runExp1_rest( subNum, viewDist, REST_min)
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
dfile ='log_exp1_rest.txt';
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
global viewDistance compKbDevice subjectNum END_WAIT END_OF_EXPERIMENT_MESSAGE LOADING_MESSAGE
global RUN_PRACTICE PRACTICE_START_MESSAGE PRACTICE_START_MESSAGE_fMRI LPT_CODE_START LPT_OBJECT LPT_ADDRESS PRACTICE_START_MESSAGE_ECOG ScreenHeight
global TRIGAUD_CODE_START WhereToRestart TOBII_EYETRACKER
global ExistFlag DATA_FOLDER TEMPORARY_FOLDER EXPERIMENT_NAME BEHAV_FILE_NAMING_WHOLE miniBlocksInfo

subjectNum = subNum;
compKbDevice = -1;
viewDistance = viewDist;

% Before we go further, we check whether the participant number was
% already used:
ParticipantFolder = sprintf('%s%c%s%c%s%c%s_LPTtriggers_ID%s',pwd,filesep,'data_rest',filesep,[LAB_ID,num2str(subjectNum),filesep]);

% if ~MATRIX_GENERATION
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
    DATA_FOLDER='data_rest';
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
    
    %% runMiniBlocks
    %         % This is where the experiment actually gets run:
    %
    % set resting-state time (s)
    REST_DURATION= REST_min*60;%5min
    
    runRest(REST_DURATION);
    %         miniBlocks = runMiniBlocks(miniBlocks,TriggerMatrix);
    
    %% Finishing the experiment:
    if VERBOSE disp('After runMiniBlocks'); end
    % Letting the participant that it is over:
    showMessage(END_OF_EXPERIMENT_MESSAGE);
    
    % Waiting a bit so that participant see the end message:
    WaitSecs(END_WAIT);
    %% save everything from command window
    Str = CmdWinTool('getText');
    dlmwrite(dfile,Str,'delimiter','');
    %     else % If we are in MATRIX generation mode, run the createMiniBlocks function
    %         [miniBlocks, TriggerMatrix] = createMiniBlocks();
    %     end
    % safe finish, closing Psychtoolbox, ends the recording methods:
    safeExit();
    
catch e % When the program has error, it safely ends, closing psychtoolbox
    Str = CmdWinTool('getText');
    dlmwrite(dfile,Str,'delimiter','');
    safeExit();
    rethrow(e);
    
end % try

end % function

%% runRest
function runRest(REST_DURATION)


global output_table_cntr miniBlocksInfo BLOCK_NUM_COL MINIBLK_COL TRIAL1_ANSWER_COL TRIAL1_TIME_COL MINI_BLOCK_SIZE_COL EVENT_TYPE_COL compKbDevice TRIAL1_RESPONSE_TIME_COL   FALSE TRUE TRIAL_DURATION
global TRIAL1_BLANK_DUR_COL TRIAL1_STIM_DUR_COL refRate SAVING_MESSAGE NO_KEY TRIAL1_JITTER_TIME_COL TRIAL1_STIM_END_TIME_COL TRIAL1_BUTTON_PRESS_COL  TRIAL1_START_TIME_COL TRIAL1_DURATION_COL
global MISSES_COL HITS_COL FA_COL CR_COL WRONG_KEY RESTART_KEY TARGET_KEY Behavior ExistFlag YesKey MINIBLOCK_RESTART_KEY BLOCK_RESTART_KEY
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
    Block_ctr = 0;
    miniBlockNum=1;
    RestartFlag = 0; % This is a flag to know if we restarted, 0 means no
    
    %% runs the mini-blocks
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
    
    
    
    %% Miniblocks loop:
    % Running through miniBlocks. Use of a while so that we can go back
    % if the experimenter wants to (for loop don't go back)
    %     while miniBlockNum <= 1
    if VERBOSE display(miniBlockNum, 'now we are just starting miniBlock nr: '); end
    
    
    if EYE_TRACKER && miniBlockNum == 1
        % Starting the eyetracker recording for the first block
        % Calibrate or drift correction of the eye tracker
        if(~fMRI)
            if ECoG
                showMessages(EYETRACKER_CALIBRATION_MESSAGE_ECOG, [0,2], [round(ScreenHeight*(1/6)), round((ScreenHeight*(5/6)))]);
                KbWait(compKbDevice,3);
            else
                showMessage(EYETRACKER_CALIBRATION_MESSAGE);
                KbWait(compKbDevice,3);
            end
        else
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
        if ~TOBII_EYETRACKER
            % Starting the eyetracker recording for the first block
            % Calibrate or drift correction of the eye tracker
            EyelinkDoTrackerSetup(el);
            % Starting the recording
            Eyelink('StartRecording');
        else
            tobiiCalibration; % Perform the calibration
            % And then, we start the recording anew:
            GazeData = tobii_eyetracker.get_gaze_data();
            tobii_TimeCell = {'system_time_stamp', 'point_description'};
        end
    end % eye tracker
    
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
    
    
    
    
    % For the fMRI crew, before starting a mini-block, they
    % need to wait for the TR trigger of the other matlab
    % session
    if(fMRI)
        REST_START_MESSAGE_fMRI = 'Please stay still and keep your eyes fixed\n\n on the center of the screen.\n\n\n\n Waiting for scanner.\n\n\n REST Min:';
        start_of_rest_message = sprintf(strcat(REST_START_MESSAGE_fMRI, '\n\n%d/8'), REST_DURATION/60);
        showMessage(start_of_rest_message);
        scannerListener()
        %                 TCPIP = tcpip('0.0.0.0', 30000, 'NetworkRole', 'server','Timeout',Inf);
        %                 fopen(TCPIP);
        %                 fread(TCPIP, 1, 'double');
        %                 Receiving_Time=GetSecs;
        %                 fclose(TCPIP);
        %                 save('Receiving_Time','Receiving_Time');
        %                 RunOnset=showFixation('PhotodiodeOn');
        WaitSecs(1);
        %             end
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
    
    %% rest has only 1 trial,start from 0
    tr=0;
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
    if fMRI
        switch LAB_ID
            case 'SC'
                bitsi_buttonbox.clearResponses();
            case 'SD'
                
        end
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
        %                 if (fMRI && mod(miniBlockNum,4) == 1)
        %                 setOutputTable('RunOnset', miniBlocks, miniBlockNum,tr,RunOnset);
        %                 end
        %                 % log fixation in journal
        %                 setOutputTable('Fixation', miniBlocks, miniBlockNum, tr, fixOnset); %4 %setting all the trial values in the output table
        %                 % Starting the while loop for the jitter of the first
        % trial. Not relying on waitsec because it disables the
        % possibility to turn off the photodiode and LPT triggers:
        
        elapsedTime = 0;
        while elapsedTime<(REST_DURATION) - refRate*(2/3)
            
            % If the LPT trigger was sent, the port needs to be
            % set back to 0 after a frame
            if MEEG && floor(elapsedTime/refRate) >= 1 && firstTrialTriggerOFF.LPT.Fixation == FALSE
                sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
                firstTrialTriggerOFF.LPT.Fixation = TRUE;
            end
            
            elapsedTime = GetSecs - fixOnset;
        end
        
        
        %% End of miniBlock:
        
        
        if VERBOSE display(tr, 'In last trial loop with trial '); end
        % Save the audio trigger log to HD
        if ECoG && ~NO_AUDIO
            saveTrigAudToHD();
        end
        % Save the LPT triggers log to HD
        if MEEG
            saveTrigToHD();
        end
        if DEBUG disp(sprintf('Saving took : %f \n',GetSecs - ttime)); end
        
    end % Trial loop
    
    
    
    Block_ctr=Block_ctr+1;
    %     if fMRI
    %        TCPIP = tcpip('0.0.0.0', 20000, 'NetworkRole', 'server','Timeout',Inf);
    %        fopen(TCPIP);
    %        fread(TCPIP, 1, 'double');
    %        fclose(TCPIP);
    %     end
    %% Ending the experiment
    % end of experiment, save to HD and summarize results
    showMessage(SAVING_MESSAGE);
    KbWait(compKbDevice,3);
    % Mark the time of saving onset
    ttime = GetSecs;
    % Save the different triggers
    
    if MEEG saveTrigToHD(); end
    if VERBOSE disp('To save trigAudtoHD because end of experiment'); end
    if ECoG && ~NO_AUDIO saveTrigAudToHD(); end
    
    %     % Copying these guys to a secret location to avoid overwritting
    %     secretDataSaving
    
    if DEBUG
        disp(sprintf('Saving took : %f \n',GetSecs - ttime));
    end
    
    
    %% Crashes management
    % If the experiment crashes along the way, we make sure to save the
    % results and the backup:
catch e
    try
        ttime = GetSecs;
        if VERBOSE disp('saving because user quit'); end
        if DEBUG disp(sprintf('Saving took : %f \n',GetSecs - ttime)); end
    catch
    end
    rethrow(e);
    
end

end

