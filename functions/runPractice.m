%RUNPRACTICE
% This runs the practice mini block. This function does not recieve any
% input besided ESCAPE. It does not record any information regarding this
% block. It takes the practice trial list practice.mat (which is always constant) and the first two
% pictures in the practice folder in the stimuli folder as targets.
function [ ] = runPractice()

disp('WELCOME TO runPractice')

global LAB_ID MINI_BLOCK_SIZE_COL TRIAL1_NAME_COL compKbDevice FALSE TRUE TRIAL_DURATION
global refRate NO_KEY
global RESTART_KEY RESTART_MESSAGE_fMRI RESTART_MESSAGE ABORT_KEY YesKey TARGET_KEY
global STIM_DURATION fMRI PRACTICE_MINIBLOCK_MAT
global RUN_PRACTICE PracticeHits PracticeFalseAlarms PracticeFalseAlarms_Irrelevant MinPracticeHits MaxPracticeHits MaxPracticeFalseAlarms MaxPracticeFalseAlarms_Irrelevant MinPracticeHits_fMRI MaxPracticeHits_fMRI MaxPracticeFalseAlarms_fMRI MaxPracticeFalseAlarms_Irrelevant_fMRI TotalScore FEEDBACK_MESSAGES_PRACTICE PRACTICE_FEEDBACK_MESSAGES
global bitsi_buttonbox ECoG
load(PRACTICE_MINIBLOCK_MAT); % <- loads the practice "miniBlocks" variable



miniBlockNum = size(miniBlocks,1);

reduce_trials = 0;
if fMRI %|| ECoG % if MRI or ECoG, reduce the number of trials by 50%
    reduce_trials = round(miniBlocks{miniBlockNum,MINI_BLOCK_SIZE_COL}/2);
end

% Initializing trial:
tr =0;
% Initializing number of hits and false alarms
PracticeHits=0;
PracticeFalseAlarms=0;
PracticeFalseAlarms_Irrelevant=0;
TotalScore=0;
% loop through all trials; trials start from 0, so that trial 0 is
% actually trial 1, 1 is 2 etc.
while tr <= miniBlocks{miniBlockNum,MINI_BLOCK_SIZE_COL} - 1 - reduce_trials
    if strcmp(LAB_ID,'SC')  
    bitsi_buttonbox.clearResponses()
    end
    %get some random values for practice
    jitter = getJitter(1);
    trial_time = STIM_DURATION(ceil(rand()*length(STIM_DURATION)));
    
    hasInput = FALSE; % input flag
    fixShown = FALSE; % fixation flag
    jitterLogged = FALSE; % logging jitter flag
    PauseTime = 0;
    RestartInterval = 0;
    
    if tr == 0
        showMiniBlockBeginScreen(miniBlocks, miniBlockNum);
        if(~fMRI)
            KbWait(compKbDevice,3);
        else
            WaitSecs(5);
        end
        % Sending the first fixation with jitter of each mini-block. Here the participant cannot respond
        fixOnset = showFixation('PhotodiodeOff');
        % log fixation in journal
        WaitSecs(TRIAL_DURATION - trial_time - refRate/2); % the fixation wait
        JitOnset = showFixation('PhotodiodeOff');
        % log jitter started
        WaitSecs(jitter - refRate/2); % the jitter wait
    end
    
    % present first stimuli and start the clock on the experiment
    startTime = showStimuli(miniBlocks, miniBlockNum, tr, 'PhotodiodeOff');
    elapsedTime = GetSecs - startTime;
   
    while elapsedTime < TRIAL_DURATION + jitter
        % if there was no input yet, get input and log if it is correct
        if hasInput == FALSE
            [key,RT,PauseTime] = getInput(PauseTime);
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
                    tr = 0;
                    break
                else % Else, continue:
                    key = NO_KEY;
                end
            elseif (key == ABORT_KEY) % If the experiment was aborted:
                cleanExit();
            elseif key ~= NO_KEY
                if ( key == TARGET_KEY && isTarget(miniBlocks,miniBlockNum,tr))
                    PracticeHits = PracticeHits + 1;
                elseif ( key ~= TARGET_KEY && isTarget(miniBlocks,miniBlockNum,tr)) && ECoG
                    PracticeHits = PracticeHits + 1;
                else
                    PracticeFalseAlarms = PracticeFalseAlarms + 1;
                    stim = miniBlocks{miniBlockNum, TRIAL1_NAME_COL + tr};
                    if (floor(stim/1000)==1 || floor(stim/1000)==2)
                        PracticeFalseAlarms_Irrelevant = PracticeFalseAlarms_Irrelevant + 1;
                    end
                    
                end
                hasInput = TRUE;
            end
        end

        
        % if there was no fixation yet, show fixation
        if elapsedTime >= (trial_time - refRate/2) && fixShown == FALSE
            showFixation('PhotodiodeOff');
            % log fixation in journal
            fixShown = TRUE;
        end
        
        % if within jitter, log that it has begun
        if elapsedTime > TRIAL_DURATION && jitterLogged == FALSE
            showFixation('PhotodiodeOff');
            % log jitter started
            jitterLogged = TRUE;
        end
        
        % update time since iteration begun
        elapsedTime = (GetSecs - startTime) - (PauseTime + RestartInterval);
    end
    if (key == RESTART_KEY) % If there was a restart, tr = 0
        tr = 0;
    else % else, we simply continue
        tr =  tr + 1;
    end
end


                
if ~fMRI
    if(PracticeHits>=MinPracticeHits && PracticeFalseAlarms<=MaxPracticeFalseAlarms && PracticeFalseAlarms_Irrelevant==MaxPracticeFalseAlarms_Irrelevant)
       RUN_PRACTICE=0; 
    else
       RUN_PRACTICE=1;
    end
       HitsScore=(4/MaxPracticeHits)*PracticeHits;
       FAScore=(-3/MaxPracticeHits)*PracticeFalseAlarms+3;
       FAScore(find(PracticeFalseAlarms<0))=0;
       BlockIrrelevantCategoryFA=PracticeFalseAlarms_Irrelevant;
else
    if(PracticeHits>=MinPracticeHits_fMRI && PracticeFalseAlarms<=MaxPracticeFalseAlarms_fMRI  && PracticeFalseAlarms_Irrelevant==MaxPracticeFalseAlarms_Irrelevant_fMRI)
       RUN_PRACTICE=0; 
    else
       RUN_PRACTICE=1; 
    end
       HitsScore=(4/MaxPracticeHits_fMRI)*PracticeHits;
       FAScore=(-3/MaxPracticeHits_fMRI)*PracticeFalseAlarms+3;
       FAScore(find(PracticeFalseAlarms<0))=0;
       BlockIrrelevantCategoryFA=PracticeFalseAlarms_Irrelevant;
end
                 if(BlockIrrelevantCategoryFA==0)
                 IrrelevantCategoryFAScore=3;
                 else
                 IrrelevantCategoryFAScore=0;    
                 end
                 TotalScore=(HitsScore+FAScore+IrrelevantCategoryFAScore)*10;
                 feedback_message_flag1=not(HitsScore/4<0.5 || FAScore/3<0.5 || IrrelevantCategoryFAScore<3);
                 feedback_message_flag2=HitsScore/4<0.5;
                 feedback_message_flag3=FAScore/3<0.5;
                 feedback_message_flag4=IrrelevantCategoryFAScore<3;
                 PRACTICE_FEEDBACK_MESSAGES=FEEDBACK_MESSAGES_PRACTICE(find([feedback_message_flag1 feedback_message_flag2 (feedback_message_flag3 || feedback_message_flag4)]));
                 
                 
end % end function RunPractice