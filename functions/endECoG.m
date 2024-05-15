function [] = endECoG()

    global VERBOSE PHOTODIODE
    
    if VERBOSE disp('inside endECoG'), end
    
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