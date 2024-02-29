%SHOWSTIMULI - shows a stimuli
% input:
% ------
% miniBlocks - the main data structure of the experiment
% blockNum - mini-block number of the stimuli to be presented
% tr - trial number of the stimuli to be presented
% parctice - in the practice, we don't want to flash the photodiode on. So
% if we are in the practice, let it off
% output:
% -------
% stimuliTiming - the time in which the stimuli was presented

function [ stimuliTiming ] = showStimuli(miniBlocks, blockNum, tr, Photodiode)

    global TRIAL1_NAME_COL PHOTODIODE
    global stimSizeLength stimSizeHeight
    global gray w center  originalHeight originalWidth;
    global trial_nrs_for_additional_log_file block_nrs_for_additional_log_file textures_for_additional_log_file

    stimSizeLength = round((stimSizeHeight/originalHeight) * originalWidth);
    
    Screen('FillRect', w, gray);

    drawFrame();

    x = transpose(center) - [stimSizeLength/2 stimSizeHeight/2];
    y = transpose(center) + [stimSizeLength/2 stimSizeHeight/2];

    %show stimuli

    texture = getTexture(miniBlocks{blockNum,tr + TRIAL1_NAME_COL});
    % Save this information to an additional log file
    
    %display(tr, 'tr');
    %display(blockNum, 'blockNum');
    %display(TRIAL1_NAME_COL);
    %display(miniBlocks{blockNum,tr + TRIAL1_NAME_COL}, 'miniBlocks{blockNum,tr + TRIAL1_NAME_COL}');
    %display(texture, 'texture');
      
    Screen('DrawTexture',w, texture,[],[x y]);

    drawFixation();
    if PHOTODIODE && nargin == 3
        drawPhotodiodBlock('on'); 
        [~,stimuliTiming] = Screen('Flip', w,[],1);
    elseif PHOTODIODE && nargin == 4
        if strcmp(Photodiode,'PhotodiodeOff')
            drawPhotodiodBlock('off'); 
            [~,stimuliTiming] = Screen('Flip', w,[],1);
        end
    else
        [~,stimuliTiming] = Screen('Flip', w, [], 1);
    end   
    
    trial_nrs_for_additional_log_file = [trial_nrs_for_additional_log_file; tr];
    block_nrs_for_additional_log_file = [block_nrs_for_additional_log_file; blockNum];
    textures_for_additional_log_file = [textures_for_additional_log_file, texture];

%     if DEBUG disp(sprintf("\n[%d,%d]\n",stimSizeHeight,stimSizeLength)); end
end
