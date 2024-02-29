
% This function create for each trial a trigger vector, containing the
% trigggers to be sent for each trial. We are using successive trigger
% scheme, so for each trial, we have one trigger for the category, one for
% the orientation, one for the 
% Input:
% miniBlocks: Contain the trials
% targetsType: contains the target types
% Contains the targets ID
% times: Contain the stimuli durations:
% ----------
% Output:
% Trigger vectors: 3D matrix: each row is a miniblock, each column a trial
% and each cell along the third dimension is a frame for which the
% corresponding trigger can be found:
function [TriggerMatrix] = createTriggersVector(data)
global TRIAL1_NAME_COL MAX_NUM_OF_TRIALS_PER_MINI_BLOCK TRIAL1_TIME_COL 
global FACE OBJECT LETTER FALSE_FONT LEFT RIGHT CENTER
global TRG_STIM_ADD TRG_LEFT TRG_RIGHT TRG_CENTER TRG_DUR_500 TRG_DUR_1000 TRG_DUR_1500 TRG_TASK_RELEVANT TRG_TASK_RELEVANT_NON_TARGET
global TRG_TASK_IRRELEVANT TRG_TRIAL_ADD STIM_DURATION
      
% I start looping through each miniblock
for i = 1 : size(data,1)
    % I then loop through each trial in the miniblock
    % I set a counter to add things as I go:
    TrialCnt = 1;
    for ii = TRIAL1_NAME_COL:(TRIAL1_NAME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK-1)
        % I extract the stimulus ID from the data:
        stimNum = data{i,ii};
        % I then get the stimulus category triggers:
        switch (stimNum - mod(stimNum,1000))
            % Faces triggers
            case FACE
                % Triggers for faces are 1-20
                CategoryTrigger(i,TrialCnt) = TRG_STIM_ADD*0 + mod(stimNum,100);
                % Object triggers
            case OBJECT
                % Triggers for objects are 21-40
                CategoryTrigger(i,TrialCnt) = TRG_STIM_ADD*1 + mod(stimNum,100);
                % Letter triggers
            case LETTER
                % Triggers for real fonts are 41-60
                CategoryTrigger(i,TrialCnt) = TRG_STIM_ADD*2 + mod(stimNum,100);
                % False font triggers
            case FALSE_FONT
                % Triggers for false fonts are 61-80
                CategoryTrigger(i,TrialCnt) = TRG_STIM_ADD*3 + mod(stimNum,100);
                % I stimNum is a nan, it means we are at the end of the
                % miniblock. For consistency, we keep the nan, otherwise
                % there will be differences in dimensions:
            otherwise
                CategoryTrigger(i,TrialCnt) = nan;
        end
        
        % I then get the stimulus orientation triggers:
        % If the second number of the stimulus ID is equal to the left
        % orientation, then trigger left and so on for the differnet
        % orientations
        if (mod(stimNum,1000) >= LEFT && mod(stimNum,1000) < RIGHT) % is Left | get the 100s, left is 200 and right is 300
            OrientationTrigger(i,TrialCnt) = TRG_LEFT;
        elseif (mod(stimNum,1000) >= RIGHT) % (RIGHT)
            OrientationTrigger(i,TrialCnt) = TRG_RIGHT;
        elseif (mod(stimNum,1000) >= CENTER) % CENTER
            OrientationTrigger(i,TrialCnt) = TRG_CENTER;
        elseif isnan(stimNum) % If it is a nan, then we enter it as a nan for dimension consistency:
            OrientationTrigger(i,TrialCnt) = nan;
        end
       
        % The task relevance:
        % In that case, I rely on the isTarget and isRelevant functions
        % So if it is a target, then we have the target trigger
        if isTarget(data,i,TrialCnt-1)
            relTrigger(i,TrialCnt) = TRG_TASK_RELEVANT;
            % Otherwise we have the non target trigger
        elseif isRelevant(data,i,TrialCnt-1)
            relTrigger(i,TrialCnt) = TRG_TASK_RELEVANT_NON_TARGET;
            % And if it is not a nan but was not one of the above, it is a
            % task irrelevant
        elseif ~isnan(stimNum)
            relTrigger(i,TrialCnt) = TRG_TASK_IRRELEVANT;
        else % Finally, if it is a nan we keep it
            relTrigger(i,TrialCnt) = nan;
        end
        
        % Finally, the trial number:
        TrialNumTrigger(i,TrialCnt) = TrialCnt+TRG_TRIAL_ADD;
        
        TrialCnt = TrialCnt + 1;
    end
    TrialCnt = 1;
    
    % The trial duration is found in another set of column and to avoid to
    % mix things up, I do it in a spearated loop
    for ii = TRIAL1_TIME_COL:(TRIAL1_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK-1)
        % I first get the durations
        dur = data{i,ii};
        % Then, depending on what the duration is, we give the
        % corresponding trigger
        if (dur == STIM_DURATION(1))
            DurationTriggers(i,TrialCnt) = TRG_DUR_500;
        elseif (dur == STIM_DURATION(2))
            DurationTriggers(i,TrialCnt) = TRG_DUR_1000;
        elseif (dur == STIM_DURATION(3))
            DurationTriggers(i,TrialCnt) = TRG_DUR_1500;
        elseif isnan(dur) 
            DurationTriggers(i,TrialCnt) = nan;
        else
            error('Issue in LPT duration triggers attribution!!!')
        end
% Finally, we actualize the counter:
        TrialCnt = TrialCnt + 1;
    end
end

% We also need to have blanks in between, where the triggers will be
% resetted:
ZeroTriggers = zeros(size(CategoryTrigger,1),size(CategoryTrigger,2));
% I can then stack them along the third dimension and we have it:
TriggerMatrix = cat(3,CategoryTrigger,ZeroTriggers,OrientationTrigger,ZeroTriggers,...
    DurationTriggers,ZeroTriggers,relTrigger,ZeroTriggers,TrialNumTrigger,ZeroTriggers);

end