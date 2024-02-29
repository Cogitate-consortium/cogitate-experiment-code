
function [] = initAudioTrigger()

    global NUMBER_OF_TOTAL_TRIALS triggersAudio trigAudMatName BIT_DURATION SAMPLE_RATE squarewave zerowave
    global triggsAudioStart DATA_FOLDER triggsAudioCounter subjectNum TRIGGER_ARRAY_SIZE LAB_ID TRIG_LOG_FILE_NAMING 

    % We cannot save the date
    %prf1 = sprintf('%d-%s',subjectNum, date);
    trigAudMatName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING]);

    TRIGGER_ARRAY_SIZE = NUMBER_OF_TOTAL_TRIALS * 10;
    %triggersAudio = nan(TRIGGER_ARRAY_SIZE,2);
    triggersAudio = [];
    triggsAudioCounter = 1;
    triggsAudioStart = GetSecs;
    
    % Creating the square wave and zero way ahead of time to spare some
    % processing power when sending the actual triggers:
    original_nr_of_samples_one_bit = BIT_DURATION/(1/SAMPLE_RATE);
    updated_nr_of_samples_one_bit = original_nr_of_samples_one_bit;
    squarewave = [ones(updated_nr_of_samples_one_bit/2, 2); -1*ones(updated_nr_of_samples_one_bit/2,2)];
    squarewave(:,2) = 0;
    zerowave = zeros(updated_nr_of_samples_one_bit,2);


end % function
