
% SHOWMESSAGE - shows a message
% input:
% ------
% message - the message to show
%
% output:
% -------
% message_time - the time in which the message was displayed

function [ message_time ] = showMessageItalics( message )

    global gray w text PHOTODIODE

    Screen('FillRect', w, gray);
    % Make it italics
    Screen('TextStyle', w, 2);

    DrawFormattedText(w, textProcess(message), 'center', 'center', text.Color);
    
    % Put the default back to normal style
    Screen('TextStyle', w, 0);
    
    if PHOTODIODE
            drawPhotodiodBlock('off')
    end
    [~, message_time] = Screen('Flip', w) ;

end