% SENDTRIGAUDIO
% Send a bit code trigger through the adio port
function [] = sendTrigAudio()


    % These variables are for debugging
    global VERBOSE
    global pahandle
    if VERBOSE disp('Into sendTrigAudio'); end
    
    % Send the audio trigger that is already loaded in the buffer:
    AudioTimeStamp = PsychPortAudio('Start',pahandle, 1, 0);
   
end
