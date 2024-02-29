
% SHOWMESSAGE - shows a message
% input:
% ------
% message - the message to show
%
% output:
% -------
% message_time - the time in which the message was displayed

function [ message_time ] = showMessages( messages, styles, positions )

    global gray w text PHOTODIODE

    Screen('FillRect', w, gray);
    
    disp(length(messages))

    for i=1:length(messages)
        message = messages{i};
        style = styles(i);
        position = positions(i);
        
        % Make it the wanted style
        Screen('TextStyle', w, style);
        DrawFormattedText(w, textProcess(message), 'center', position, text.Color);
    end
    
    % Put the default back to normal style
    Screen('TextStyle', w, 0);
    
    if PHOTODIODE
            drawPhotodiodBlock('off')
    end
    [~, message_time] = Screen('Flip', w) ;

end