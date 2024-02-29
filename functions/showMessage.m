
% SHOWMESSAGE - shows a message
% input:
% ------
% message - the message to show
%
% output:
% -------
% message_time - the time in which the message was displayed

function [ message_time ] = showMessage( message )

    global gray w text PHOTODIODE

    Screen('FillRect', w, gray);

    DrawFormattedText(w, textProcess(message), 'center', 'center', text.Color);
    
    if PHOTODIODE
            drawPhotodiodBlock('off')
    end
    [~, message_time] = Screen('Flip', w) ;

end