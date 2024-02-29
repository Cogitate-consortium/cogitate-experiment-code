%SHOWERROR shows an error message
% input:
% ------
% message - the error message
%
% output:
% -------
% time - the exact time in which it was presented

function [ message_time ] = showError( message )
global PHOTODIODE
    global gray w text

    Screen('FillRect', w, gray);

    DrawFormattedText(w, message, 'center', 'center', text.Color);
    
    if PHOTODIODE
        drawPhotodiodBlock('off');
    end
    [~, message_time] = Screen('Flip', w) ;

    pause(2);
end