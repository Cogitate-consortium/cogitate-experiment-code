%% AUDIO TRIGGER
%SENDSTIMTRIGSAUDIO for audio triggers
% This function sends triggers based on the type of stimuli presented. It sends 4 triggers for type, orientation, relevancy and duration. Between each trigger there is very short time interval.
function [] = sendStimTrigsAudio(miniBlocks, miniBlockNum, tr)

    global TRIAL1_TIME_COL TRIAL1_NAME_COL
    global VERBOSE NO_AUDIO STIM_DURATION
    
    if VERBOSE
        disp('Welcome to sendStimTrigsAudio')
    end
    
    % TRIAL1_NAME_COL is the column where the stimulus IDs are stored for each trial. You then add tr which is a counter for
    % the trial number. TRIAL1_TIME_COL is keeping track of the durations
    stimNum = miniBlocks{miniBlockNum,TRIAL1_NAME_COL + tr};

    if VERBOSE
        display(stimNum, 'stimNum');
    end
    
    % Map the properties of the stimulus to numbers to feed into the
    % getStimTrigAudBitCode function, from where we get a bit code 
    % for the stimulus to send to sendTrigAud
    cat = 0;
    rel = 0;
    ori = 0;
    dur = 0;
    
    % category
    cat = uint8((stimNum - mod(stimNum,1000))/1000);
    if VERBOSE display(cat, 'cat'); end
    
    % relevance
    
    if isTarget(miniBlocks,miniBlockNum,tr)
        rel = 1;
    elseif isRelevant(miniBlocks,miniBlockNum,tr)
        rel = 2;
    else
        rel = 3;
    end
    
    if VERBOSE display(rel, 'rel'); end
    
    % orientation
    ori = uint8((mod(stimNum,1000) - mod(stimNum,100))/100);
    if VERBOSE display(ori, 'ori'); end
      
    % duration
    
    duration = miniBlocks{miniBlockNum, TRIAL1_TIME_COL + tr};
      
    if (round(duration*10) == round(STIM_DURATION(1)*10))
        dur = 1;
    elseif (round(duration*10) == round(STIM_DURATION(2)*10))
         dur = 2;
    elseif (round(duration*10) == round(STIM_DURATION(3)*10))
         dur = 3;
    end
    
    if VERBOSE display(dur, 'dur'); end
      
    % Convert to 8 bit code
    bit_code = getStimTrigAudBitCode(cat, rel, ori, dur);
      
    % send trigger:
    if ~NO_AUDIO
        sendTrigAudio(bit_code);
    end  
end % end sendStimTrigsAudio