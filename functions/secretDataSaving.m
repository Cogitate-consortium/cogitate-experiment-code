% This function copies the saved data to a secret location at the end of
% each block, to make sure nothing is deleted by mistake

function secretDataSaving
global DATA_FOLDER LAB_ID subjectNum SECRET_FOLDER

try
% Making the dir of the files to save
if ~exist(fullfile(pwd,SECRET_FOLDER,[LAB_ID,num2str(subjectNum)]),'dir')
    mkdir(fullfile(pwd,SECRET_FOLDER,[LAB_ID,num2str(subjectNum)]));
end
Source       = fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]);
Destination  = fullfile(pwd,SECRET_FOLDER,[LAB_ID,num2str(subjectNum)]);
fileattrib(Destination,'+w +h')
copyfile(Source, Destination)
catch
end

end