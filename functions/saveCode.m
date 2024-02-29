
% SAVECODE saves the code into the code folder
% output:
% -------
% This code file is saved into the code folder ("/data/code/").
function [ ] = saveCode()

    global subjectNum CODE_FOLDER DATA_FOLDER LAB_ID FUNCTIONS_FOLDER%subject number
    try
        fileStruct = dir('*.m');
        
        if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER),'dir')
            mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER));
        end
        % We cannot save the date
        %prf1 = sprintf('%d-%s',subjectNum, date);
        for i = 1 : length(fileStruct)
            k = 0;
            k = strfind(fileStruct(i).name,'.m');
            if (k ~= 0)
                fileName = fileStruct(i).name;
                source = fullfile(pwd,fileName);
                destination = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER,strcat(LAB_ID,num2str(subjectNum),fileName));
                copyfile(source,destination);
            end
        end
        % Saving the log file:
        logfileName = 'log_exp1.txt';
        logsource = fullfile(pwd,logfileName);
        logdestination = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER,strcat(LAB_ID,num2str(subjectNum),logfileName));
        copyfile(logsource,logdestination);
        
        % Saving the helper functions
        helperFunctionFile = fullfile(pwd,FUNCTIONS_FOLDER);
        destination = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER,strcat(FUNCTIONS_FOLDER,LAB_ID,num2str(subjectNum)));
        copyfile(helperFunctionFile,destination)
        
    catch
        fileStruct = dir('*.m');
        
        if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER),'dir')
            mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER));
        end
        % We cannot save the date
        %prf1 = sprintf('%d-%s',subjectNum, date);
        for i = 1 : length(fileStruct)
            k = 0;
            k = strfind(fileStruct(i).name,'.m');
            if (k ~= 0)
                fileName = fileStruct(i).name;
                source = fullfile(pwd,fileName);
                destination = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER,strcat(LAB_ID,num2str(subjectNum),fileName));
                copyfile(source,destination);
            end
        end
        
        % Saving the log file:
        logfileName = 'log_exp1.txt';
        logsource = fullfile(pwd,logfileName);
        logdestination = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER,strcat(LAB_ID,num2str(subjectNum),logfileName));
        copyfile(logsource,logdestination);
        
        % Saving the helper functions
        helperFunctionFile = fullfile(pwd,FUNCTIONS_FOLDER);
        destination = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],CODE_FOLDER,strcat(FUNCTIONS_FOLDER,LAB_ID,num2str(subjectNum)));
        copyfile(helperFunctionFile,helperFunctionFile) % XXX commented out as it crashed
    end
end