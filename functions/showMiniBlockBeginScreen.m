
%SHOWBLOCKBEGINSCREEN shows the block begin screen and returns a timestap
% input:
% ------
% miniBlocks - the complete data structure of the
% blockNum - the mini-block number.
%
% output:
% -------
% time - the exact time in which it was presented

function [ time ] = showMiniBlockBeginScreen( miniBlocks, blockNum )

    global gray PRESS_SPACE PRESS_ANY_BUTTON ScreenWidth ScreenHeight stimSizeHeight RIGHT originalWidth originalHeight stimSizeLength LEFT CENTER TARGET1_COL TARGET2_COL MINIBLOCK_TEXT MINIBLOCK_TEXT_ECOG text fifthStimPosition sixthStimPosition w  fourthStimPosition firstStimPosition secondStimPosition thirdStimPosition PHOTODIODE
    global fMRI ECoG MEEG

    Screen('FillRect', w, gray);

if (MEEG)
    % This will scale the stimuli length by the change in height
    stimSizeLength = round((stimSizeHeight/originalHeight) * originalWidth);

    % stimuli location in block splash screen (instruction screen between blocks)
    firstStimPosition = round([ScreenWidth*(1/4), ScreenHeight*(1/3)] - [stimSizeLength/2 , stimSizeHeight/2]);
    secondStimPosition = round([ScreenWidth*(2/4), ScreenHeight*(1/3)] - [stimSizeLength/2, stimSizeHeight/2]);
    thirdStimPosition = round([ScreenWidth*(3/4), ScreenHeight*(1/3)] - [stimSizeLength/2, stimSizeHeight/2]);
    fourthStimPosition = round([ScreenWidth*(1/4), ScreenHeight*(2/3)] - [stimSizeLength/2 , stimSizeHeight/2]);
    fifthStimPosition = round([ScreenWidth*(2/4), ScreenHeight*(2/3)] - [stimSizeLength/2, stimSizeHeight/2]);
    sixthStimPosition = round([ScreenWidth*(3/4), ScreenHeight*(2/3)] - [stimSizeLength/2, stimSizeHeight/2]);
 
    DrawFormattedText(w, textProcess(MINIBLOCK_TEXT), 'center', round(ScreenHeight*(1/15)), text.Color);
    
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET1_COL} + RIGHT),[],[firstStimPosition, firstStimPosition + [stimSizeLength stimSizeHeight]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET1_COL} + CENTER),[],[secondStimPosition, secondStimPosition + [stimSizeLength stimSizeHeight]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET1_COL} + LEFT),[],[thirdStimPosition, thirdStimPosition + [stimSizeLength stimSizeHeight]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET2_COL} + RIGHT),[],[fourthStimPosition, fourthStimPosition + [stimSizeLength stimSizeHeight]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET2_COL} + CENTER),[],[fifthStimPosition, fifthStimPosition + [stimSizeLength stimSizeHeight]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET2_COL} + LEFT),[],[sixthStimPosition, sixthStimPosition + [stimSizeLength stimSizeHeight]]);
        
    DrawFormattedText(w, textProcess(PRESS_SPACE), 'center', round((ScreenHeight*(5/6))), text.Color);
    
else
    
    stimSizeHeightfMRI=0.7* stimSizeHeight;
    % This will scale the stimuli length by the change in height
    stimSizeLengthfMRI = round((stimSizeHeightfMRI/originalHeight) * originalWidth);
    % stimuli location in block splash screen (instruction screen between blocks)
    firstStimPosition = round([ScreenWidth*(1/4), ScreenHeight*(1/3)] - [stimSizeLengthfMRI/2 , stimSizeHeightfMRI/2]);
    secondStimPosition = round([ScreenWidth*(2/4), ScreenHeight*(1/3)] - [stimSizeLengthfMRI/2, stimSizeHeightfMRI/2]);
    thirdStimPosition = round([ScreenWidth*(3/4), ScreenHeight*(1/3)] - [stimSizeLengthfMRI/2, stimSizeHeightfMRI/2]);
    fourthStimPosition = round([ScreenWidth*(1/4), ScreenHeight*(2/3)] - [stimSizeLengthfMRI/2 , stimSizeHeightfMRI/2]);
    fifthStimPosition = round([ScreenWidth*(2/4), ScreenHeight*(2/3)] - [stimSizeLengthfMRI/2, stimSizeHeightfMRI/2]);
    sixthStimPosition = round([ScreenWidth*(3/4), ScreenHeight*(2/3)] - [stimSizeLengthfMRI/2, stimSizeHeightfMRI/2]);
    
    if ECoG
        DrawFormattedText(w, textProcess(MINIBLOCK_TEXT_ECOG), 'center', round(ScreenHeight*(1/15)), text.Color);
    else
        DrawFormattedText(w, textProcess(MINIBLOCK_TEXT), 'center', round(ScreenHeight*(1/15)), text.Color);
    end
    
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET1_COL} + RIGHT),[],[firstStimPosition, firstStimPosition + [stimSizeLengthfMRI stimSizeHeightfMRI]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET1_COL} + CENTER),[],[secondStimPosition, secondStimPosition + [stimSizeLengthfMRI stimSizeHeightfMRI]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET1_COL} + LEFT),[],[thirdStimPosition, thirdStimPosition + [stimSizeLengthfMRI stimSizeHeightfMRI]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET2_COL} + RIGHT),[],[fourthStimPosition, fourthStimPosition + [stimSizeLengthfMRI stimSizeHeightfMRI]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET2_COL} + CENTER),[],[fifthStimPosition, fifthStimPosition + [stimSizeLengthfMRI stimSizeHeightfMRI]]);
    Screen('DrawTexture',w, getTexture(miniBlocks{blockNum,TARGET2_COL} + LEFT),[],[sixthStimPosition, sixthStimPosition + [stimSizeLengthfMRI stimSizeHeightfMRI]]);
    
    if ECoG
        Screen('TextStyle', w, 2);
        DrawFormattedText(w, textProcess(PRESS_ANY_BUTTON), 'center', round((ScreenHeight*(5/6))), text.Color);
        Screen('TextStyle', w, 0);
    end
    
end
if PHOTODIODE
    drawPhotodiodBlock('off');
end
[~, time] = Screen('Flip', w);
end