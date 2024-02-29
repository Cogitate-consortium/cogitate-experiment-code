
function saveTrigAudToHD()

global trigAudMatName triggersAudio excelFormat ABORTED Block_ctr RestartFlag DATA_FOLDER LAB_ID subjectNum TEMPORARY_FOLDER TRIG_LOG_FILE_NAMING
disp('into saveTrigAudToHD');
if ~exist(sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER))
    mkdir(sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER))
end
try
    % Create the name of the data to save in temp
    triAudTempMatName = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING]);
    BlockNum = Block_ctr; % Get the block counter
    if ABORTED % If the experiment was aborted:
        % If there was already a trigger file that was saved previously (if we are further than the first miniblock of a block), need to remove it 
        ExisitingFileName = [trigAudMatName,num2str(BlockNum),excelFormat];
        if isfile(ExisitingFileName)
            delete(ExisitingFileName)
        end
        % Same for the temporary file:
        ExisitingFileNameTemp = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(BlockNum),'.mat']);
        if isfile(ExisitingFileNameTemp)
            delete(ExisitingFileNameTemp)
        end
        audTrigMatfile = [trigAudMatName,num2str(BlockNum),'_ABORTED_1',excelFormat]; % Change the name of the data
        audTrigMatfileTemp  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(BlockNum),'_ABORTED_1','.mat']); % Same for the temp file
        
        % Checking if there is already an aborted file
        FileExist = 1;
        numAdd = 1;
        while FileExist % As long as the file already exists, we add a new number to mark it:
            status = isfile(audTrigMatfile); % Is there already an aborted file under the same name
            if status % If yes
                numAdd = numAdd + 1; % Add 1 to the name of the matrix and change the name of the files accordingly:
                audTrigMatfile = [trigAudMatName,num2str(BlockNum),'_ABORTED_',num2str(numAdd),excelFormat];
                audTrigMatfileTemp  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),TRIG_LOG_FILE_NAMING,num2str(BlockNum),'_ABORTED_',num2str(numAdd),'.mat']);
            else % If the file doesn't already exists, exit the while loop
                FileExist = 0;
            end
        end
        try % Try to save the data:
            writetable(cell2table(triggersAudio),audTrigMatfile);
            save(audTrigMatfileTemp,'triggersAudio');
        catch ME2 % If it fails, send the error as warning and save as matlab only, less likely to fail
            warning(ME2.message)
            save(audTrigMatfileTemp,'triggersAudio'); % If this statement fails, it will get caught by the upper try catch
        end
    elseif RestartFlag % If the experiment was restarted:
        try
            writetable(cell2table(triggersAudio),[trigAudMatName,num2str(BlockNum),'_RESTARTED',excelFormat]); % Simply write the table with _RESTARTED
            save([triAudTempMatName,num2str(BlockNum),'_RESTARTED'],'triggersAudio');
        catch ME2
            warning(ME2.message)
            save([triAudTempMatName,num2str(BlockNum),'_RESTARTED'],'triggersAudio'); % If this statement fails, it will get caught by the upper try catch
        end
    else
        try % Try to save the data:
            writetable(cell2table(triggersAudio),[trigAudMatName,num2str(BlockNum),excelFormat]);
            save([triAudTempMatName,num2str(BlockNum)],'triggersAudio');
        catch ME2 % If it fails, send the error as warning and save as matlab only, less likely to fail
            warning(ME2.message)
            save([triAudTempMatName,num2str(BlockNum)],'triggersAudio'); % If this statement fails, it will get caught by the upper try catch
        end
    end
catch ME % If things fail altogether, then send a warning to let the experimenter know!
    warning(ME.message)
end
end % function