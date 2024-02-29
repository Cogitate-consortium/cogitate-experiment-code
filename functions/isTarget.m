% ISTARGET identifies if the stimuli of a certain trial in a certain
% miniblock is a target, or just a stimuli
% input:
% ------
% miniBlocks - the main data structure of the experiment
% blockNum - the mini-block number
% tr - the trial number
%
% output:
% -------
% answer - TRUE or FALSE answer if the stimuli is indeed a target
function [ answer ] = isTarget(miniBlocks,blockNum,tr)

    global TRUE FALSE TARGET1_COL TARGET2_COL TRIAL1_NAME_COL

    answer = FALSE;

    % gets the targets of the miniblock
    target1 = miniBlocks{blockNum, TARGET1_COL};
    target2 = miniBlocks{blockNum, TARGET2_COL};
    % gets the stimuli
    stim = miniBlocks{blockNum, TRIAL1_NAME_COL + tr};

    if floor(target1/1000) == floor(stim/1000) % checks if same type as target 1: face, obj etc.
        if rem(target1,100) == rem(stim,100) % checks if same stimuli in the type
            answer = TRUE;
        end
    end
    if floor(target2/1000) == floor(stim/1000) % the same as above for target 2
        if rem(target2,100) == rem(stim,100)
            answer = TRUE;
        end
    end
end