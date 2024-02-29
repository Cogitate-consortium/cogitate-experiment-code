% This function when called import the edf data of the eyetracker to the
% experiment computer. 
function importEyetrackerEDF(BlockNumber,Restart,Interrupt)

global edfFile DATA_FOLDER LAB_ID subjectNum EYETRACKER_FILE_NAMING exp_Interrupt_counter
%Closing the edf file
Eyelink('CloseFile')
if Restart
    fileName  = sprintf('%s%c%s%c%s%c%s_RESTARTED',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(BlockNumber)]);
elseif Interrupt==1
    fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(BlockNumber),'_INTERRUPTED_',num2str(exp_Interrupt_counter)]);
else
    fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(BlockNumber)]);
end

%Retrieving the edf file from eyetracker PC
try
    fprintf('Receiving data file ''%s''\n',edfFile);
    status=Eyelink('ReceiveFile',edfFile,[fileName, '.edf']);

    if status > 0
        fprintf('ReceiveFile status %d/n',status);
    end
    if 2 == exist(edfFile,'file')
        fprintf('Data file ''%s'' can be found in ''%s''\n', edfFile, pwd);
    end
catch rdf
    fprintf('Problem receiving file ''%s''\n',edfFile);
    throw(rdf);
end

end