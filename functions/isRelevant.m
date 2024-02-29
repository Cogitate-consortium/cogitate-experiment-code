function [ answer ] = isRelevant(miniBlocks,blockNum,tr)
    global VERBOSE
    
    if VERBOSE
        disp('')
        disp('WELCOME TO isRelevant')
        disp('')
    end
    global TRUE FALSE TARGET1_COL TARGET2_COL TRIAL1_NAME_COL

    answer = FALSE;

    % gets the targets of the miniblock
    target1 = miniBlocks{blockNum, TARGET1_COL};
    target2 = miniBlocks{blockNum, TARGET2_COL};
    % gets the stimuli
    stim = miniBlocks{blockNum, TRIAL1_NAME_COL + tr};

    if floor(target1/1000) == floor(stim/1000) % checks if same type as target 1: face, obj etc.
        answer = TRUE;
    end
    if floor(target2/1000) == floor(stim/1000) % the same as above for target 2
        answer = TRUE;
    end
end