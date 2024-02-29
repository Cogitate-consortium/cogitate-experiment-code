% CLEANEXIT safely exits the program.
function [] = cleanExit( )
global CLEAN_EXIT_MESSAGE ECoG DIOD_DURATION refRate

if ECoG
    turnPhotoTrigger('on');
    WaitSecs(refRate*DIOD_DURATION-0.5*refRate);
    turnPhotoTrigger('off');
    WaitSecs(refRate*DIOD_DURATION-0.5*refRate);
    turnPhotoTrigger('on');
    WaitSecs(refRate*DIOD_DURATION-0.5*refRate);
    turnPhotoTrigger('off');
    WaitSecs(refRate*DIOD_DURATION-0.5*refRate);
    turnPhotoTrigger('on');
    WaitSecs(refRate*DIOD_DURATION-0.5*refRate);
    turnPhotoTrigger('off');
    
end

error(CLEAN_EXIT_MESSAGE);
 
end