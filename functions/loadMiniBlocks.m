% This function loads the miniblocks and trigger matrices:
function [data,TriggerMatrix] = loadMiniBlocks()
global subjectNum LAB_ID DATA_FOLDER TEMPORARY_FOLDER
TrialMatricesDirectory = fullfile(pwd,'TrialMatrices',LAB_ID);
load(fullfile(TrialMatricesDirectory,[LAB_ID,num2str(subjectNum),'_TrialMatrix.mat']))
load(fullfile(TrialMatricesDirectory,[LAB_ID,num2str(subjectNum),'_TriggerMatrix.mat']))

% Now these matrices need to be copied into the participant folder:
% Preparing the saving of the trigger matrix, in case we need to fetch
% it when restarting
TriggerFile  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,'TriggerMatrix');
% Making the dir:
if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],TEMPORARY_FOLDER),'dir')
    mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],TEMPORARY_FOLDER));
end

save(TriggerFile,'TriggerMatrix')

end