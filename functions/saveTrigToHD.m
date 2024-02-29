
function saveTrigToHD()

    global triggers excelFormat TRIGGER_TABLE_HEADER ABORTED Block_ctr RestartFlag
    global DATA_FOLDER LAB_ID subjectNum TRIG_LOG_FILE_NAMING TEMPORARY_FOLDER
    if ABORTED % If the experiment was aborted by the experimenter
        BlockNum = Block_ctr; % Get the block number
        DoubleBlockNumber = floor((BlockNum-1)/2)+1; % For MEEG, we save the data every two blocks. So we have the number corresponding, to go from 1 to 5 in the log files names
        % Creating the log files names. 1 for the final data for XNAT, one
        % temporary in matlab format
        trigMatfile  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'_ABORTED',excelFormat]);
        trigMatfileTemp  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'_ABORTED','.mat']);
        % Deleting the files without aborted appended, because if we abort,
        % we will save the data that were already saved again, so if we
        % don't delete the other, we will have double!
        if isfile(sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),excelFormat]))
            delete(sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),excelFormat]))
        end
        % Same for the temporary: 
        if isfile(sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'.mat']))
            delete(sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'.mat']))
        end
        
        % Checking if there is already an aborted file. If it already
        % exists, we don't want to overwrite it, so we will append a number
        % to differentiate them: 
        FileExist = 1;
        numAdd = 1; 
        while FileExist % As long as the file already exists, we add a new number to mark it:
            status = isfile(trigMatfile); % If the file exists
            if status
                numAdd = numAdd + 1; % Add 1 to the file name
                trigMatfile = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'_ABORTED_',num2str(numAdd),excelFormat]); % Rename the file
            else
                FileExist = 0; % If the file doesn't exist, mark it as such, so that we exit the while loop
            end
        end
        % Doing the same for the temp file
        % Checking if there is already an aborted file
        FileExist = 1;
        numAdd = 1; 
        while FileExist % As long as the file already exists, we add a new number to mark it:
            status = isfile(trigMatfileTemp);
            if status
                numAdd = numAdd + 1;
                trigMatfileTemp = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'_ABORTED_',num2str(numAdd)]);
            else
                FileExist = 0;
            end
        end
         % In case of a restarting:
    elseif RestartFlag
        BlockNum = Block_ctr; % We also compute the double block number
        DoubleBlockNumber = floor((BlockNum-1)/2)+1; % Double block number
        trigMatfile  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'_RESTART',excelFormat]); % Rename the file 
        trigMatfileTemp  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'_RESTART','.mat']); % Rename the temp file
    else % Else, if we are in the normal case:
        BlockNum = Block_ctr; % Compute the double block number
        DoubleBlockNumber = floor((BlockNum-1)/2)+1;
        trigMatfile  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),excelFormat]); % Name the files accordingly
        trigMatfileTemp  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(DoubleBlockNumber),'.mat']);
    end
    
    try
            % Now try and save the data:
            save(trigMatfileTemp,'triggers');
            triggersTable = cell2table(triggers(~isnan([triggers{:,1}]),:),'VariableNames',TRIGGER_TABLE_HEADER); % Convert the triggers to table 
            writetable(triggersTable,trigMatfile) % Save the table as csv
    catch ME% If things crash
        warning(ME.message) % Display the error message as a warning, so that things don't completely abort
        try % Try to save the data once more, but only in matlab format, as it is less likely to crash due to external factors
            save(trigMatfileTemp,'triggers');
        catch ME2
            warning(ME2.message)
        end
    end
  end