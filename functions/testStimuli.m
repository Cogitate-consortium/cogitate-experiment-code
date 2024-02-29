
function [bool] = testStimuli(stim)
     global FACE OBJECT LETTER FALSE_FONT NUM_OF_STIM_TYPE_PER_MINIBLOCK NO_ERROR VERBOSE
     if VERBOSE
        disp('WELCOME to testStimuli')
     end
   
    
    bool = true;
    
    faces = stim(getType(stim) >= FACE & getType(stim) < OBJECT);
    objects = stim(getType(stim) >= OBJECT & getType(stim) < LETTER);
    chars = stim(getType(stim) >= LETTER & getType(stim) < FALSE_FONT);
    falses = stim(getType(stim) >= FALSE_FONT & getType(stim) < 10000);
    
    if size(faces,2) ~= NUM_OF_STIM_TYPE_PER_MINIBLOCK ...
       || size(objects,2) ~= NUM_OF_STIM_TYPE_PER_MINIBLOCK ...
       || size(chars,2) ~= NUM_OF_STIM_TYPE_PER_MINIBLOCK ...
       || size(falses,2) ~= NUM_OF_STIM_TYPE_PER_MINIBLOCK
        bool = false;
        warning('\n%s %d %d %d %d\n','Wrong number of stimuli!',size(faces,2),size(objects,2),size(chars,2),size(falses,2));
        if ~NO_ERROR error('\n%s %d %d %d %d\n','Wrong number of stimuli!',size(faces,2),size(objects,2),size(chars,2),size(falses,2)); end
    end     
end