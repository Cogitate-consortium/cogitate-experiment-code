
%SHOWINSTRUCTIONS - presents the instructions slide
% input:
% ------
% background - the pointer to the instructions image

function [ ] = showInstructions(background)

    global gray w ScreenWidth ScreenHeight INSTRUCTIONS_FOLDER PHOTODIODE;
    Screen('FillRect', w, gray);

    %show main stimuli
    x = Screen('MakeTexture', w, imread(fullfile(pwd, INSTRUCTIONS_FOLDER, background)));
    Screen('DrawTexture',w, x, [], [0 0 ScreenWidth ScreenHeight]);
    if PHOTODIODE
            drawPhotodiodBlock('off')
    end
    Screen('Flip', w);
end
