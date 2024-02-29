%SAVETRIALBACKUPTOHD Saves the basic data structure of the experiment as
%backup
function [ ] = saveTrialBackupToHD( miniBlocks , miniBlockNumber, name)

    global DATA_FOLDER EXPERIMENT_NAME subjectNum excelFormat LAB_ID TEMPORARY_FOLDER%subject number
    if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]),'dir')
        mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]));
    end
    % We cannot save the date
    %prf1 = sprintf('%d-%s',subjectNum, date);
    %fileName  = sprintf('%s%c%s%c%d%c%s_%s_S%d_b%d.mat',pwd,filesep,DATA_FOLDER,filesep,subjectNum,filesep,EXPERIMENT_NAME,name,subjectNum, miniBlockNumber);
    fileName  = sprintf('%s%c%s%c%s%c%s%c%s_%s_ID%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,EXPERIMENT_NAME,name,[LAB_ID,num2str(subjectNum)]);
    try
        save([fileName,'.mat'],'miniBlocks');
        writetable(cell2table(miniBlocks),[fileName,excelFormat])
    catch
        try % Trying to save again
            save([fileName,'.mat'],'miniBlocks');
            writetable(cell2table(miniBlocks),[fileName,excelFormat])
        catch ME % Now if this fail, sending a warning to the experimenter
            warning(ME.message)
        end
    end
end