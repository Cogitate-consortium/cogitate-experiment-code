function [] = endECoG()

    global TRG_EXP_END_MSG_AUD VERBOSE NO_AUDIO DIOD_DURATION refRate PHOTODIODE
    
    if VERBOSE disp('inside endECoG'), end
    %sendTrigAudio(TRG_MINIBLOCK_ENDED);
    if ~NO_AUDIO
        saveTrigAudToHD();
    end
    if ~NO_AUDIO
        try 
            sendTrigAudio(dec2bin(TRG_EXP_END_MSG_AUD));
        catch ME
            warning(ME.message)
        end
    end
    
    % Flashing the photodiode 3 times to mark the end of the experiment.
    % Try statement in case the screen is already closed, to avoid crashing
    try
        if PHOTODIODE
            multiplePhotodiodeFlashes(3)
        end
    catch ME
            warning(ME.message)
    end
    
end