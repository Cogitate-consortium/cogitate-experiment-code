%DRAWPHOTODIOD
% draws the photodiod black squere in the corner.
function [fixation_time] = drawPhotodiodBlock (command)

    global w ScreenWidth ScreenHeight DIOD_ON_COLOUR DIOD_OFF_COLOUR DIOD_SIZE 

    corner = [ScreenWidth; ScreenHeight];
    xDiode = transpose(corner) - [DIOD_SIZE/2 DIOD_SIZE/2];
    yDiode = transpose(corner);
    if strcmp(command,'on') 
        Screen('FillRect', w, DIOD_ON_COLOUR, [xDiode yDiode]);
        % I then log that the phototrigger was drawn:
    elseif strcmp(command,'off')
        Screen('FillRect', w, DIOD_OFF_COLOUR, [xDiode yDiode]);
        % I then log that the phototrigger was drawn off:
    end
    
    
end