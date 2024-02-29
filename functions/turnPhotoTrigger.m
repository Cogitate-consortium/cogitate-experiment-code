% turnPhotoTrigger: this function turns the photodiode square on or off.
% The drawPhotodiodBlock function does exactly the same but doesn't flip
% the screen in the end. The current function does. This function should be
% used only if you want to modifiy the photodiode INDEPENDENTLY from
% another event, so for example when you want to turn the photodiode off
% after a few frames when it was turned on and so on. But if you want to
% turn on the photodiode together with a stimulus, use drawPhotodiodBlock
% function instead
% input: 
% command - 'on' turn the photodiode on, 'off' turns it on
% output:
% Turn off the phototrigger
% This function turns off the photodiode trigger:
function [flipTime] = turnPhotoTrigger(command)
global w ScreenWidth ScreenHeight DIOD_ON_COLOUR DIOD_OFF_COLOUR DIOD_SIZE

corner = [ScreenWidth; ScreenHeight];
xDiode = transpose(corner) - [DIOD_SIZE/2 DIOD_SIZE/2];
yDiode = transpose(corner);

if strcmp(command,'on')
    Screen('FillRect', w, DIOD_ON_COLOUR, [xDiode yDiode]);
    
elseif strcmp(command,'off')
    Screen('FillRect', w, DIOD_OFF_COLOUR, [xDiode yDiode]);
    
end

% Finally flip the screen
flipTime = Screen('Flip', w,[],1);

end