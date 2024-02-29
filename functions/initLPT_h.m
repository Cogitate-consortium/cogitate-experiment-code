%% MEEG TRIGGER FUCNTION

%INIT_LPT
% MEEG trigger function, initializes all the parameters for sending triggers
function [] = initLPT_h()

    global NUMBER_OF_TOTAL_TRIALS triggers trigMatName LPT_ADDRESS LPT_OBJECT LAB_ID
    global DEBUG triggsStart DATA_FOLDER triggsCounter subjectNum TRIGGER_ARRAY_SIZE TRIG_LOG_FILE_NAMING

    % We cannot save the date
    %prf1 = sprintf('%d-%s',subjectNum, date);
    
    
    TRIGGER_ARRAY_SIZE = NUMBER_OF_TOTAL_TRIALS * 10;
    triggers = cell(TRIGGER_ARRAY_SIZE,3);
    triggers(:,:) = {nan};
    triggsCounter = 0;
    triggsStart = GetSecs;

    try
        [LPT_OBJECT, LPT_ADDRESS]=init_LPT;
    catch e
        warning('BioSemi connection initiation failed! Triggers will not be sent!');
        if ~DEBUG throw(e); end
    end
end