%% This script includes the basic checks of the Exp1 logfile that the fMRI team need to do

clc; clear all; close all;
diary('log_fMRITest.txt')

% Things you need to setup:
addpath '....=GitHub\Experiment1Development\Pre-acquisition tests' %analysis script
% addpath audioTriggers
addpath '.' % assuming you are in the tests folder
%% Recording modalities
global    fMRI font_size
fMRI = 1; % Set to 1 if recording with fMRI
font_size = 18;
%%
CHECK_TRIAL_BALANCE = 1; % If you want to perform the check of trial balance. It prints many things to your command window so you might want to not do it sometimes
SITE = 'SA';%'MPI'; 
VERBOSE = 0;
dataPath = 'C:\Users\csaba\OneDrive\Documents\GitHub\Experiment1Development\data'; %exp logs
participantPath = 'SA101'; % Path to the data of the specific participant
%participantPath = 'SE193'; % Path to the data of the specific participant
cd(fullfile(dataPath,participantPath)) % Change the directory
frameDuration = 8;
%% Reading the log files in:
logFilesList = dir('*RawDur*.csv');
fullLogs = [];
for i = 1:length(logFilesList)
    Log = readtable(logFilesList(i).name);
    fullLogs = [fullLogs;Log];
end

% Cleaning up the full logs: 
cleanLogs = fullLogs(~ismember(fullLogs.eventType,'Save'),:);

%Stimulus count
StimCount = height(cleanLogs(ismember(cleanLogs.eventType,'Stimulus'),:));

%Response count
RespCount = height(cleanLogs(ismember(cleanLogs.eventType,'Response'),:));


%% Timings test
stats = checkDurations(cleanLogs,'PTB log',0,frameDuration)
        [stim_dur_inacc_percent_1_frame, jitter_dur_inacc_percent_1_frame, stimDur, stimDurAccuracy, JitterDur, JitterDurAccuracy, StimDurAccuracy_center, StimDurAccuracy_right, StimDurAccuracy_left, StimDurAccuracy_face, StimDurAccuracy_object, StimDurAccuracy_letter, StimDurAccuracy_false, StimDurAccuracy_05, StimDurAccuracy_10, StimDurAccuracy_15, StimDurAccuracy_target, StimDurAccuracy_non_target, StimDurAccuracy_irrelevant] = ECoG_Check_diff_between_logged_and_planned_durations(cleanLogs,'PTB',frameDuration, participantPath);

%% Checking trials balance:
if CHECK_TRIAL_BALANCE 
    TrialBalanceControl(cleanLogs);
end


