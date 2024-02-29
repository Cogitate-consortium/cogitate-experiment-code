%% EYE_TRACKER TRIGGER FUCNTIONS

%ENDTRIG
% The function sends end of experiment signals to the eyetracker and
% saves it in the data folder.
function [] = endEyeTracker()
global edfFile DATA_FOLDER LAB_ID subjectNum EYETRACKER_FILE_NAMING Block_ctr MEEG ABORTED RestartFlag
%Stopping the eyetracker recording
Eyelink('StopRecording')
%Closing the edf file
Eyelink('CloseFile')

% Getting the block number to save the file:
FileNameBlockNumber = Block_ctr;

% If in MEEG, the saving number has to be changed to go from 1 2 3 4 and so
% on:
if MEEG
    FileNameBlockNumber = floor((FileNameBlockNumber - 1)/2) + 1;
end

if ABORTED
    % Generating the file name to store the edf data:
    fileName = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(FileNameBlockNumber),'_ABORTED']);
    % Mutliple interruptions contingencies:
    FileExist = 1;
    numAdd = 1;
    while FileExist % As long as the file already exists, we add a new number to mark it:
        status = isfile(fileName);
        if status
            numAdd = numAdd + 1;
            fileName = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(FileNameBlockNumber),'_ABORTED_',num2str(numAdd)]);
        else
            FileExist = 0;
        end
    end
elseif RestartFlag
    % Generating the file name to store the edf data:
    fileName = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(FileNameBlockNumber),'_RESTARTED']);
else
    fileName = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(FileNameBlockNumber)]);
end

%Retrieving the edf file from eyetracker PC
try
    fprintf('Receiving data file ''%s''\n',edfFile);
    status=Eyelink('ReceiveFile',edfFile, [fileName, '.edf']);
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

% Restoring the Eyetracker to the state we found it in:
Eyelink('Command','include "PHYSICAL.INI"')
Eyelink('Command','include "FINAL.INI"')
Eyelink('Command','include "PARSER.INI"')
% Shutting down the Eyelink
Eyelink('Shutdown')

end