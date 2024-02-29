% The trial number within the mini-block is being sent at the beginning of the trial, 
% running between 1 to 38. To make sure there is no confusion with actual triggers 
% that are sent, trial number is preceded by a 253 trigger, and followed by a 254 trigger.
% This function is not in use
function [] = sendMbIDTrigAudio(id)

    global VERBOSE NO_AUDIO
    
    if VERBOSE disp('sendMbIDTrigAudio'); end
    
    if VERBOSE display(id); end
   
    % convert to binary:
    bit_code = dec2bin(id,5);
    
    if VERBOSE display(bit_code); end
    
    if ~NO_AUDIO
        sendTrigAudio(bit_code);
    end
end % function