% This scripts analyse the blank recordings: revised from ECoGTest.m for
% MEEG test


%% Parameters %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc; clear all; close all;
diary('log_MEEGTest.txt')
ft_defaults %load fieldtrip

% Setting up the path to the test script:
addpath 'C:\Users\ferranto\DataShare\UoB\PROJECTS\02 Consciousness\Experiments\Exp 1' %analysis script

% Site code
SITE = 'SA';

% Setting up the path to the log files:
dataPath = 'Z:\Real data\Exp1\SA101\exp_logs\Experiment1Development-Last_checks\data'; %exp logs
participantPath = 'SA101'; % Path to the data of the specific participant
cd(fullfile(dataPath,participantPath)) % Change the directory

% Setting up the path to the MEG data
addpath 'Z:\Real data\Exp1\SA101\meg\20201015_b59b\201015'
dataFilenames = {'SA101_run1.fif', 'SA101_run2.fif', 'SA101_run3.fif', 'SA101_run4.fif', 'SA101_run5.fif'};

% Selecting the test(s) to be runned
PHOTODIODE = 1; % If you want to analyze photodiode signal
LPT = 1; % If you want to analyze audio signal
COMPARE_T0_TRIGGER_LOGS = 1;
COMPARE_T0_RAW_LOGS = 1;
CHECK_TRIAL_BALANCE = 0; % If you want to perform the check of trial balance. It prints many things to your command window so you might want to not do it sometimes
STAT_DURATIONS = 0; % Do statistics for 

% Photo-diode settings
if PHOTODIODE
    if strcmp(SITE, 'SA') %Birmingham
        photodiode_channel = 'MISC004';
    elseif strcmp(SITE, 'SB') %Peking
        photodiode_channel = 'MISC001';
    end
    photoDiodeThreshold = 0.055; % The threshold for photodiode peak detection. You need to change it according to your system
    frameDuration = 8; % Here, enter the frame duration, required for the plots
    drift_correction = 0; % 0(false) or 1(true); reverse the signal to corrected for slow drift
end


%% Run %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%% Loading and preparing the MEG data:

%% read raw trigger and photodiode channels
for i=1:length(dataFilenames)
    cfg = [];
    cfg.channel = {'STI001', 'STI002', 'STI003', 'STI004', ...
               'STI005', 'STI006', 'STI007', 'STI008', ...
               'STI101', photodiode_channel};
    cfg.dataset = dataFilenames{i};
    raw_temp = ft_preprocessing(cfg);
    if i == 1
        raw = ft_appenddata([], raw_temp);
    else
        raw = ft_appenddata([], raw, raw_temp);
    end
end
raw = rmfield(raw,'sampleinfo');

% Get trigger time and trigger codes
trigger_time = [];
main_trigger = [];
for i=1:length(dataFilenames)
    rawtime_temp = raw.time{i};
    if i > 1
        prev_rawtime_temp_end = trigger_time(end);
        rawtime_temp = rawtime_temp + prev_rawtime_temp_end + 1;
    end
    trigger_time = [trigger_time, rawtime_temp];
    main_trigger = [main_trigger, raw.trial{i}(strcmp(raw.label, 'STI101'),:)];
end
% figure; plot(trigger_time, main_trigger);

% Fix response triggers (ONLY SA)
% The response boxes are directly connected to the trigger channels on an
% indipendent set of bits (STIM009-STIM013). Each time a response is given,
% those bit sum up to the standard bits used by the presentation program
% giving uncorrected values for STIM101. In order to get the correct triger
% values, here we filter the response boxes bits out.
if strcmp(SITE, 'SA')
    resp_trigger = main_trigger(main_trigger >255);
    resp_trigger_bin = dec2bin(resp_trigger);
    resp_trigger_bin2 = resp_trigger_bin(:,6:end);
    resp_trigger2 = bin2dec(resp_trigger_bin2);
    main_trigger(main_trigger >255) = resp_trigger2;
end

% Check if STI101 (decimal code) contains the same information as the
% single trigger bits (binary code)
code_triggers = [];
for i=1:length(dataFilenames)
    triggers = raw.trial{i}(~strcmp(raw.label, 'STI101'),:); %STI001-STI008
    code_triggers_temp = (triggers(2,:) + triggers(3,:)*2 + triggers(4,:)*4 + triggers(5,:)*8 + ...
        triggers(6,:)*16 + triggers(7,:)*32 + triggers(8,:)*64 + triggers(9,:)*128) / 5;
    code_triggers = [code_triggers, code_triggers_temp];
end
figure; plot(trigger_time, code_triggers-main_trigger);

% See below ONLY if you see non-zeros in the plot above (meaning that some STI101 
% decimal values are not equal to the binary sum of STIM001-008 (bits))!!!
% %% Check if any LPT bug need fixed first. For each trigger, there 
% % will be several repeat sample of the trigger before it reset to zero, in rare
% % cases, the first sample of the trigger may be small than the real trigger
% % (e.g. 0 0 112 127 127 127 ... 127 0 0 or 0 0 152 153 153 ... 153 0 0 ),
% % as some trigger channel may not open correctly,so need fixed this bug
% % before other analysis.
% % Fix LPT trigger
% main_trigger(775124)=153;
% main_trigger(180395)=127;

% Get the photo-diode signal
main_photodiote = [];
for i=1:length(dataFilenames)
    main_photodiote_temp = raw.trial{i}(strcmp(raw.label, photodiode_channel),:);
    main_photodiote = [main_photodiote, main_photodiote_temp];
end
% figure; plot(main_photodiote); yline(photoDiodeThreshold,'r'); title('Raw photo-diode signal with onset threshold')

%corrected for slow drift reverse the signal
if drift_correction
    main_photodiote_c = -1*(main_photodiote-smooth(main_photodiote,100)');
    figure; plot(main_photodiote); yline(photoDiodeThreshold,'r'); title('Drift corrected photo-diode signal with onset threshold')
else
    main_photodiote_c = main_photodiote;
end

%% Reading the log files:
logFilesList = dir('*RawDur*.csv');
fullLogs = [];
for i = 1:length(logFilesList)
    Log = readtable(logFilesList(i).name);
    fullLogs = [fullLogs;Log];
end

% Cleaning up the full logs: 
cleanLogs = fullLogs(~ismember(fullLogs.eventType,'Save'),:);

% There is the issue that the first fixation and jitters of a trial are
% logged but no photodiode trigger was sent. So I remove them from the log
% file. This unlogged entries always occur at the beginning of a new
% miniblock, so I need to find the first entry of each block:
for ii = 1:2
    for i = 2:height(cleanLogs)- max(cleanLogs.miniBlock) %MEG has 40 mini-blocks, ECoG 20
        if cleanLogs.miniBlock(i,1) > cleanLogs.miniBlock(i-1,1)
            cleanLogs(i,:) = [];
        end
    end
end

% Removing the ones of the first block too:
cleanLogs(1:2,:) = [];

% Counting the number of stimuli and targets, just to make sure:
StimCount = height(cleanLogs(ismember(cleanLogs.eventType,'Stimulus'),:));


%% Photodiode:

if PHOTODIODE
    % Compare the photo triggers to the log file:
    % Get the onset of the photodiode:
    PhotodiodeSig = main_photodiote_c;
    
    % Threshold the photo signal:
    PhotodiodeSigBin = PhotodiodeSig>photoDiodeThreshold;
    %plot
    figure; plot(PhotodiodeSig);
    hold on;
    plot(PhotodiodeSigBin*photoDiodeThreshold)
    yline(photoDiodeThreshold,'r')
    
    % Do the diff:
    diffPhotoSig = diff(PhotodiodeSigBin);
    
    % Get the onsets
    photoDiodeOnset = find(diffPhotoSig == 1);
    %figure; plot(photoDiodeOnset)
    
    % Compare the time stamps:
    intervalPhotodiode = diff(photoDiodeOnset);
    intervalLogs = diff(cleanLogs.time(~ismember(cleanLogs.eventType,'Response')));
    intervalLogs = intervalLogs'*1000; % to ms
    
    % Compare the log files and the photodiode recording
    figure
    plot(intervalLogs,'r')
    hold on 
    plot(intervalPhotodiode,'b')
    legend({'Photodiode' 'Log'})
    title('Photodiode vs log files interval between triggers')
    ylabel('Delta t between triggers')
    xlabel('samples')
    
    % Comparing the inaccuracies in the PTB timestamps (only potential
    % jitters, not delays):
    PTBInaccuracy = intervalPhotodiode(1:length(intervalLogs))-(intervalLogs); %MEG: removed the transposition from (intervalLogs*1000)
    % remove the huge jitters procuded when concatenating MEG recordings
    PTBInaccuracy(PTBInaccuracy < -10000) = 0;
    %plot
    figure(1)
    histogram(PTBInaccuracy)
    xlabel('Jitter (msec)')
    ylabel('Number of samples')
    title('PTB timestamping jitter (msec)')
    
    % Find outlier triggers with more than 1 frame of jitter
    outliers = sum(PTBInaccuracy > frameDuration | PTBInaccuracy < -frameDuration);
    outliers_ratio = outliers/ length(PTBInaccuracy);
    figure(1)
    text1 = sprintf('%.2f%% of the jitter are between -8 and 8 ms (1 frame duration)',  (1-outliers_ratio)*100);
    text( 0, 1300, text1, 'FontSize', 10);

    % Replacing the log files time stamps by the photodiode ones, so that I
    % can then check the accuracy of durations based on what really
    % happened on screen
    % I get rid of the response first:
    cleanLogsPhotodiode = cleanLogs(~ismember(cleanLogs.eventType,'Response'),:);
    % I then replace the timestamps by the photodiode ones:
    cleanLogsPhotodiode.time = photoDiodeOnset(1:height(cleanLogsPhotodiode))';
    
    % Checking the trial and stimuli durations based on the photodiode. The
    % stats structure contains estimations of percentage of skipped frames:
    statsDurationPhotodiode = checkDurations(cleanLogsPhotodiode,'Photodiode',1,frameDuration);

end


%% LPT trigger


if LPT
    % Retrieving LPT signal:
    % with trigger logs cvs file
    if COMPARE_T0_TRIGGER_LOGS
        %reading trigger logs from csv file
        lpt_trig_log_list = dir('*TrigDurR*.csv');
        lpt_trig_logs = [];
        for i = 1:length(lpt_trig_log_list)
            lptlog = readtable(lpt_trig_log_list(i).name);
            lpt_trig_logs = [lpt_trig_logs; lptlog];
        end
        
        lpt_trig_logs_codes = table2cell(lpt_trig_logs(:,1));
        
        %first to check of trigger failer
        failerLPTLogs = lpt_trig_logs(ismember(lpt_trig_logs.TriggerStatus,'TRIGGER_FAILED'),:);
        figure
        plot(failerLPTLogs.TimeStamp, failerLPTLogs.Trigger,'r*')
        title('Trigger Failed')
        xlabel('Sample Time')
        ylabel('Trigger Code')
        
        %miniblock start trigger confused with trial end and jitter, keep only
        %trial related trigger
        miniblockIndex= find(lpt_trig_logs.Trigger>=161 & lpt_trig_logs.Trigger<=200);
        lpt_trig_logs_raw=lpt_trig_logs;
        lpt_trig_logs.Trigger(miniblockIndex+2)=0;%remove miniblock related 96
        lpt_trig_logs.Trigger(miniblockIndex+4)=0;%remove miniblock related 97
        %% break make 97 missing with 83
        %% fix this before other analysis
        %breakendIndex= find(lpt_trig_logs.Trigger==83);
        %lpt_trig_logs.Trigger(breakendIndex(1))=97;
        
        % I then extract only the stimuli triggers:(here use the stimulus type)
        stimLPTTriggers = lpt_trig_logs(lpt_trig_logs.Trigger>=1 & lpt_trig_logs.Trigger<=80,:);
        
        %% trigger timeline analysis
        % for each trial, there are 7 trigger plus a possible response trigger
        % Here only analysis of the fixed trial trigger
        % 1st stimulus type; 2 orienation; 3 duration; 4 task relevance; 5
        % trial ID; 6 target offset; 7 jitter onset
        LPTtrialT1=stimLPTTriggers;
        LPTtrialT2=lpt_trig_logs(lpt_trig_logs.Trigger>=101&lpt_trig_logs.Trigger<=103,:);
        LPTtrialT3=lpt_trig_logs(lpt_trig_logs.Trigger>=151&lpt_trig_logs.Trigger<=153,:);
        LPTtrialT4=lpt_trig_logs(lpt_trig_logs.Trigger>=201&lpt_trig_logs.Trigger<=203,:);
        LPTtrialT5=lpt_trig_logs(lpt_trig_logs.Trigger>=111&lpt_trig_logs.Trigger<=148,:);
        LPTtrialT6=lpt_trig_logs(lpt_trig_logs.Trigger==96,:);
        LPTtrialT7=lpt_trig_logs(lpt_trig_logs.Trigger==97,:);
        
        % plot the number of each type of trigger
        figure
        bar([height(LPTtrialT1),height(LPTtrialT2),height(LPTtrialT3),height(LPTtrialT4),height(LPTtrialT5),height(LPTtrialT6),height(LPTtrialT7)]);
        
        % compute the time difference between the first trigger and the
        % latter ones
        TrialScode(:,1)=LPTtrialT2.TimeStamp-LPTtrialT1.TimeStamp;
        TrialScode(:,2)=LPTtrialT3.TimeStamp-LPTtrialT1.TimeStamp;
        TrialScode(:,3)=LPTtrialT4.TimeStamp-LPTtrialT1.TimeStamp;
        TrialScode(:,4)=LPTtrialT5.TimeStamp-LPTtrialT1.TimeStamp;
        TrialScode(:,5)=LPTtrialT6.TimeStamp-LPTtrialT1.TimeStamp;
        TrialScode(:,6)=LPTtrialT7.TimeStamp-LPTtrialT1.TimeStamp;
        
        % I then compute the time diff between consecutive triggers
        % I can then compare the timestamps:
        stimLogs = cleanLogs(ismember(cleanLogs.eventType,'Stimulus'),:);
        diffLPTLog = diff(stimLogs.time);
        diffLPTSig = diff(stimLPTTriggers.TimeStamp);
        
        % Responses:
        % Checking the responses:
        RespLPTTriggers = lpt_trig_logs(lpt_trig_logs.Trigger==255,:);
        
        %     RespLPTTriggers = array2table(audioTriggers(audioTriggers(:,1) == 110,:),'VariableNames',{'Triggers' 'TimeStamp' 'TriggerDuration',  'BitCode'});
        
        % I then compute the diff to get the
        % I can then compare the timestamps:
        RespLogs = cleanLogs(ismember(cleanLogs.eventType,'Response'),:);
        
        
        % find response trial index from RespLogs's miniBlock & trial in
        % StimLogs' miniBlock & trial
        for j = 1: height(RespLogs)
            respIndex(j)=find((stimLogs.miniBlock==RespLogs.miniBlock(j)) &(stimLogs.trial==RespLogs.trial(j)));
        end
        
        %8 response trigger
        LPTtrialT8=LPTtrialT7;
        LPTtrialT8.Trigger=nan(height(LPTtrialT7),1);
        LPTtrialT8.TimeStamp=nan(height(LPTtrialT7),1);
        LPTtrialT8(respIndex,:)=RespLPTTriggers;
        
        TrialScode(:,7)=LPTtrialT8.TimeStamp-LPTtrialT1.TimeStamp;
        
        
        
        figure
        plot(zeros(height(LPTtrialT1),1),1:1:height(LPTtrialT1),'k*');
        hold on
        plot(TrialScode(:,1),1:1:height(LPTtrialT1),'r*');
        hold on
        plot(TrialScode(:,2),1:1:height(LPTtrialT1),'g*');
        hold on
        plot(TrialScode(:,3),1:1:height(LPTtrialT1),'b*');
        hold on
        plot(TrialScode(:,4),1:1:height(LPTtrialT1),'m*');
        hold on
        plot(TrialScode(:,5),1:1:height(LPTtrialT1),'b.');
        hold on
        plot(TrialScode(:,6),1:1:height(LPTtrialT1),'k.');
        hold on
        plot(TrialScode(:,7),1:1:height(LPTtrialT1),'ro');
        
        legend({'1 Stimulus type' '2 Stimulus orientation' '3 Stimulus duration' '4 Task relevance' '5 Trial ID' '6 Offset' '7 Jitter' '8 Response'},'FontSize',12)
        title('LPT stimulus triggers timeline','FontSize',12)
        xlabel('Delta t between triggers','FontSize',12)
        ylabel('trial number','FontSize',12)
        xlim([-0.2,3])
        
        
        % trial failed plot
        % only plot failed trials
        fTrialScode=TrialScode(ismember(LPTtrialT8.TriggerStatus,'TRIGGER_FAILED'),:);
        
        figure
        plot(zeros(height(failerLPTLogs),1),1:1:height(failerLPTLogs),'k*');
        hold on
        plot(fTrialScode(:,1),1:1:height(failerLPTLogs),'r*');
        hold on
        plot(fTrialScode(:,2),1:1:height(failerLPTLogs),'g*');
        hold on
        plot(fTrialScode(:,3),1:1:height(failerLPTLogs),'b*');
        hold on
        plot(fTrialScode(:,4),1:1:height(failerLPTLogs),'m*');
        hold on
        plot(fTrialScode(:,5),1:1:height(failerLPTLogs),'b.');
        hold on
        plot(fTrialScode(:,6),1:1:height(failerLPTLogs),'k.');
        hold on
        plot(fTrialScode(:,7),1:1:height(failerLPTLogs),'ro');
        
        legend({'1 Stimulus type' '2 Stimulus orientation' '3 Stimulus duration' '4 Task relevance' '5 Trial ID' '6 Offset' '7 Jitter' '8 Response'},'FontSize',20)
        title('LPT stimulus triggers timeline (Failed trial) ','FontSize',20)
        xlabel('Delta t between triggers','FontSize',20)
        ylabel('trial number','FontSize',20)
        xlim([-0.2,3])
        ylim([0, height(failerLPTLogs)+1])
        
        
        % If things are misaligned, you will want to remove entries from the
        % log. Please enter
        %     happy = 0;
        %     figure
        % Plotting the intervals:;
        %     while ~happy
        figure
        plot(diffLPTLog, 'r')
        hold on
        plot(diffLPTSig, 'b')
        legend({'Logs' 'LPT'})
        title('LPT vs log files interval between stimuli triggers')
        ylabel('Delta t between triggers')
        xlabel('samples')
        %         happyInput = input('Are things aligned? y/n [y]:','s');
        %         if strcmp(happyInput,'y')
        %             happy = 1;
        %         elseif strcmp(happyInput,'n')
        %             idxMisaligned = input('What is the index of the first misaligned sample? ');
        %             stimLPTTriggers = [stimLPTTriggers(1:idxMisaligned-1,:); stimLPTTriggers(idxMisaligned+1:end,:)];
        %             diffLPTSig = diff(stimLPTTriggers.TimeStamp);
        %             close
        %         end
        %     end
        
        
        % Plotting the diffs:
        figure
        plot(diff(RespLogs.time))
        hold on
        plot(diff(RespLPTTriggers.TimeStamp))
        legend({'Logs' 'LPT'})
        title('LPT vs log files interval between response triggers')
        ylabel('Delta t between triggers')
        xlabel('samples')
        
    end
    
     % with trigger logs cvs file
     if COMPARE_T0_TRIGGER_FIF
         
         %miniblock start trigger confused with trial end and jitter, keep only
         %     %trial related trigger
         %     miniblockIndex= find(main_trigger>=161&main_trigger<=200);
         %     lpt_trig_fif_raw=main_trigger;
         %     lpt_trig_logs.Trigger(miniblockIndex+2)=0;%remove miniblock related 96
         %     lpt_trig_logs.Trigger(miniblockIndex+4)=0;%remove miniblock related 97
         % %         %% break make 97 missing with 83
         % %         % fix this before other analysis
         % %     breakendIndex= find(lpt_trig_logs.Trigger==83);
         % %     lpt_trig_logs.Trigger(breakendIndex(1))=97;
         
         % I fisrt extract only the stimuli triggers:(here use the stimulus type)
         %get the trigger onset : trigger change from 0 to no-zero
         %first set all non-zero to 1
         temp=main_trigger;
         %set response to 0
         temp(main_trigger==255)=0;
         
         strigOnset=find(diff(temp)>0);
         %timepoint of trigger Onset
         strigOnset_time=trigger_time(strigOnset+1);
         %trigger code of the trigger onset
         strigOnset_code=main_trigger(strigOnset+1);
         
         %find response
         temp2=main_trigger;
         %set non-response to 0;
         temp2(main_trigger~=255)=0;
         %find response trigger
         rtrigOnset=find(diff(temp2)>0);
         %timepoint of trigger Onset
         rtrigOnset_time=trigger_time(rtrigOnset+1);
         %trigger code of the trigger onset
         rtrigOnset_code=main_trigger(rtrigOnset+1);
         
         
         
         %     % then find trigger onset index
         %     trigOnset=find(diff(temp)==1|diff(temp)==10|diff(temp)==9);
         %
         trigOnset_code2=strigOnset_code;
         trigOnset_time2=strigOnset_time;
         miniblockIndex= find(trigOnset_code2>=161&trigOnset_code2<=200);
         trigOnset_code2(miniblockIndex+1)=0;%remove miniblock related 96
         trigOnset_code2(miniblockIndex+2)=0;%remove miniblock related 97
         
         
         %find stim code
         stimLPTTrigfif_Index = strigOnset_code>=1&strigOnset_code<=80;
         stimLPTTrigfif_time=strigOnset_time(stimLPTTrigfif_Index);
         
         
         %% trigger timeline analysis
         % for each trial, there are 7 trigger plus an possible response trigger
         % Here only analysis of the fixed trial trigger
         % 1st stimulus type; 2 orienation; 3 duration; 4 task relevance; 5
         % trial ID; 6 target offset; 7 jitter onset
         LPTtrialT1fif=stimLPTTrigfif_time;
         LPTtrialT2fif=strigOnset_time(strigOnset_code>=101&strigOnset_code<=103);
         LPTtrialT3fif=strigOnset_time(strigOnset_code>=151&strigOnset_code<=153);
         LPTtrialT4fif=strigOnset_time(strigOnset_code>=201&strigOnset_code<=203);
         LPTtrialT5fif=strigOnset_time(strigOnset_code>=111&strigOnset_code<=148);
         LPTtrialT6fif=trigOnset_time2(trigOnset_code2==96);
         LPTtrialT7fif=trigOnset_time2(trigOnset_code2==97);
         
         figure
         plot(LPTtrialT1fif,'.')
         hold on
         plot(LPTtrialT2fif,'.')
         plot(LPTtrialT3fif,'.')
         plot(LPTtrialT4fif,'.')
         plot(LPTtrialT5fif,'.')
         plot(LPTtrialT6fif,'.')
         plot(LPTtrialT7fif,'.')
         
         %
         % plot the number of each type of trigger
         figure
         bar([length(LPTtrialT1fif),length(LPTtrialT2fif),length(LPTtrialT3fif),length(LPTtrialT4fif),length(LPTtrialT5fif),length(LPTtrialT6fif),length(LPTtrialT7fif)]);
         
         TrialScodefif(:,1)=LPTtrialT2fif-LPTtrialT1fif;
         TrialScodefif(:,2)=LPTtrialT3fif-LPTtrialT1fif;
         TrialScodefif(:,3)=LPTtrialT4fif-LPTtrialT1fif;
         TrialScodefif(:,4)=LPTtrialT5fif-LPTtrialT1fif;
         TrialScodefif(:,5)=LPTtrialT6fif-LPTtrialT1fif;
         TrialScodefif(:,6)=LPTtrialT7fif-LPTtrialT1fif;
         
         
         %
         %     stimLPTTriggers = lpt_trig_logs(lpt_trig_logs.Trigger<=80&lpt_trig_logs.Trigger>0,:);
         %     % I then compute the diff to get the
         %     % I can then compare the timestamps:
         %     stimLogs = cleanLogs(ismember(cleanLogs.eventType,'Stimulus'),:);
         %     diffLPTLog = diff(stimLogs.time);
         diffLPTSigfif = diff(stimLPTTrigfif_time);
         
         
         %     % Responses:
         %     % Checking the responses:
         RespLPTTriggers = lpt_trig_logs(lpt_trig_logs.Trigger==255,:);
         
         %     RespLPTTriggers = array2table(audioTriggers(audioTriggers(:,1) == 110,:),'VariableNames',{'Triggers' 'TimeStamp' 'TriggerDuration',  'BitCode'});
         %
         %     % I then compute the diff to get the
         %     % I can then compare the timestamps:
         RespLogs = cleanLogs(ismember(cleanLogs.eventType,'Response'),:);
         
         %     % find response trial index from RespLogs's miniBlock & trial in
         % StimLogs' miniBlock & trial
         for j = 1: height(RespLogs)
             respIndex(j)=find((stimLogs.miniBlock==RespLogs.miniBlock(j)) &(stimLogs.trial==RespLogs.trial(j)));
         end
         
         %8 response trigger
         LPTtrialT8fif=LPTtrialT7fif;
         LPTtrialT8fif=nan(1,length(LPTtrialT7fif));
         LPTtrialT8fif(respIndex)=rtrigOnset_time(rtrigOnset_code==255);
         
         TrialScodefif(:,7)=LPTtrialT8fif-LPTtrialT1fif;
         
         
         
         figure
         plot(zeros(length(LPTtrialT1fif),1),1:1:length(LPTtrialT1fif),'k*');
         hold on
         plot(TrialScodefif(:,1),1:1:height(LPTtrialT1),'r*');
         hold on
         plot(TrialScodefif(:,2),1:1:height(LPTtrialT1),'g*');
         hold on
         plot(TrialScodefif(:,3),1:1:height(LPTtrialT1),'b*');
         hold on
         plot(TrialScodefif(:,4),1:1:height(LPTtrialT1),'m*');
         hold on
         plot(TrialScodefif(:,5),1:1:height(LPTtrialT1),'b.');
         hold on
         plot(TrialScodefif(:,6),1:1:height(LPTtrialT1),'k.');
         hold on
         plot(TrialScodefif(:,7),1:1:height(LPTtrialT1),'ro');
         
         legend({'1 Stimulus type' '2 Stimulus orientation' '3 Stimulus duration' '4 Task relevance' '5 Trial ID' '6 Offset' '7 Jitter' '8 Response'},'FontSize',20)
         title('LPT stimulus triggers timeline','FontSize',20)
         xlabel('Delta t between triggers','FontSize',20)
         ylabel('trial number','FontSize',20)
         xlim([-0.2,3])
         
     end
    
    
    %compare the log file and the fif file
    if COMPARE_T0_TRIGGER_fif && COMPARE_T0_TRIGGER_LOGS
        
        figure
        plot(zeros(length(LPTtrialT1fif),1),1:1:length(LPTtrialT1fif),'k*');
        hold on
        plot(1000*(TrialScodefif(:,1)-TrialScode(:,1)),1:1:height(LPTtrialT1),'r*');
        hold on
        plot(1000*(TrialScodefif(:,2)-TrialScode(:,2)),1:1:height(LPTtrialT1),'g*');
        hold on
        plot(1000*(TrialScodefif(:,3)-TrialScode(:,3)),1:1:height(LPTtrialT1),'b*');
        hold on
        plot(1000*(TrialScodefif(:,4)-TrialScode(:,4)),1:1:height(LPTtrialT1),'m*');
        hold on
        plot(1000*(TrialScodefif(:,5)-TrialScode(:,5)),1:1:height(LPTtrialT1),'b.');
        hold on
        plot(1000*(TrialScodefif(:,6)-TrialScode(:,6)),1:1:height(LPTtrialT1),'k.');
        hold on
        plot(1000*(TrialScodefif(:,7)-TrialScode(:,7)),1:1:height(LPTtrialT1),'ro');
        
        title('LPT stimulus triggers difference in fif vs in csv','FontSize',20)
        xlabel('Delta t (ms) between triggers','FontSize',20)
        ylabel('trial number','FontSize',20)
        
        xlim([-100,100])
        
    end

    % Now, I compare the LTP triggers conditions to what we have in the log
    % files:
    category_vec = ["face", "object", "letter", "false"];
    relevance_vec = ["target", "non-target", "irrelevant"];
    orientation_vec = ["center", "left", "right"];
    duration_vec = ["0.5", "1.0", "1.5"];
    
    % First, I need to make the format of the conditions match. To do so, I
    % transform the log file to match the one in the triggers
    for i = 1:height(stimLogs)
        Category = floor(stimLogs.event(i)/1000);
        orientation= mod(floor(stimLogs.event(i)/100),10);
        if stimLogs.plndStimulusDur(i)<0.6
            duration = 1;
        elseif stimLogs.plndStimulusDur(i)>0.6 && stimLogs.plndStimulusDur(i)<1.1
            duration = 2;
        elseif stimLogs.plndStimulusDur(i)>1.1 && stimLogs.plndStimulusDur(i)<1.6
            duration = 3;
        end
        relevance = relevanceDecode(stimLogs.event(i),stimLogs.targ1(i),stimLogs.targ2(i),stimLogs.miniBlockType(i));
        
        % Now that we have all the info, we can convert:
        stimLogNew.Category(i,1) = category_vec(Category);
        stimLogNew.orientation(i,1) = orientation_vec(orientation);
        stimLogNew.duration(i,1) = duration_vec(duration);
        stimLogNew.relevance(i,1) = relevance_vec(relevance);
    end
    % Converting it to a table:
    %stimLogNew
    stimLogNew = struct2table(stimLogNew);
    
    
    % Then transform the trigger code
    % decode the trigger
    for i = 1:height(stimLPTTriggers)
        %category code
        % Triggers for faces are 1-20
        if LPTtrialT1.Trigger(i)>0 && LPTtrialT1.Trigger(i)<21 
        Category = 1;
        % Triggers for objects are 21-40
        elseif LPTtrialT1.Trigger(i)>20 && LPTtrialT1.Trigger(i)<41
        Category = 2;
        % Triggers for real fonts are 41-60
        elseif LPTtrialT1.Trigger(i)>40 && LPTtrialT1.Trigger(i)<61
        Category = 3;
        % Triggers for false fonts are 61-80
        elseif LPTtrialT1.Trigger(i)>60 && LPTtrialT1.Trigger(i)<81
        Category = 4;
        end
        %orientation code
        if LPTtrialT2.Trigger(i)==101
            orientation=1;
        elseif LPTtrialT2.Trigger(i)==102
            orientation=2;
        elseif LPTtrialT2.Trigger(i)==103
            orientation=3;
        end
        % duration code
        if LPTtrialT3.Trigger(i)==151
            duration=1;
        elseif LPTtrialT3.Trigger(i)==152
            duration=2;
        elseif LPTtrialT3.Trigger(i)==153
            duration=3;
        end
        %relevance code
        if LPTtrialT4.Trigger(i)==201
            relevance=1;
        elseif LPTtrialT4.Trigger(i)==202
            relevance=2;
        elseif LPTtrialT4.Trigger(i)==203
            relevance=3;
        end
        
         % Now that we have all the info, we can convert:
        stimLPTNew.Cate(i,1) = category_vec(Category);
        stimLPTNew.Or(i,1) = orientation_vec(orientation);
        stimLPTNew.Dur(i,1) = duration_vec(duration);
        stimLPTNew.Rel(i,1) = relevance_vec(relevance);
    end
   % Converting it to a table:
   %stimLPTNew
   stimLPTNew = struct2table(stimLPTNew);

    
   % Now that we have things in the same format, we can compare every entry we
   % have in the logs and in the LPT triggers
   for i = 1:height(stimLogNew)
       
       if strcmp(stimLogNew.Category(i),stimLPTNew.Cate(i)) && strcmp(stimLogNew.orientation(i),stimLPTNew.Or(i)) && ...
               strcmp(stimLogNew.duration(i),stimLPTNew.Dur(i)) && strcmp(stimLogNew.relevance(i),stimLPTNew.Rel(i))
           condComp(i) = 1;
       else
           condComp(i) = 0;
       end
   end
    
    
   a = find(condComp == 0);
   
   if ~isempty(a)
       warning('The LPT and log files are not fully consistent, go check the following matrices: stimLogNew, stimLog, stimTrigger at the following lines to figure out whats going on:')
       disp(a)
       disp('Go check the figure 2 at the following time stamps')
       disp(stimLPTTriggers.TimeStamp(a))
   else
       disp('Your log and LPT are fully consistent: things work fine')
   end
    

%% Comparing the logged duration vs the expected duration:
% This is the script written by Katarina, checking the timestamps from the
% log file against the expected duration


if STAT_DURATIONS
    
    % from LTP file
    StimDur=TrialScode(:,5); 
    
    figure
    histogram(StimDur)
    ylabel('Number of trials')
    xlabel('Measured stimulus duration')
    title(sprintf('Histogram of the measured stimuli durations %s', 'LTP'))
    
    % But then, there is something else we want to make sure: do they last as
    % long as what was planned?
    % To get this information, I subtract the planned duration to the actual
    % duration, and I make a scatter plot of that:
    StimDurAccuracy = StimDur-stimLogs.plndStimulusDur;
   % Count the number of missed or skipped frames
    stats.PercentSkippedFrameStimuli = sum(StimDurAccuracy>=frameDuration)/length(StimDurAccuracy);
    
    % Ithen plot it:
    figure
    scatter(1:length(StimDurAccuracy),StimDurAccuracy)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Stimulus duration error (msec)')
    title(sprintf('Stimuli durations inaccuracies (%s)', 'LTP'))
 
    % -------------------------------------------------------------------------
    % 2. Trials:
    % The other things we want to check is whether the overall trial duration
    % matches the planned 2 seconds. We follow the same procedure:
    TrialDur =  TrialScode(:,6); 
    
    % I then plot the histogram
    figure
    histogram(TrialDur)
    ylabel('Number of trials')
    xlabel('Measured trial duration')
    title(sprintf('Histogram of the measured trial durations (%s)', 'LTP'))
    % But then, there is something else we want to make sure: do they last as
    % long as what was planned?
    % To get this information, I subtract the planned duration to the actual
    % duration, and I make a scatter plot of that:
    TrialDurAccuracy =TrialScode(:,6) - 2;
    
    stats.PercentSkippedFrameTrial = sum(TrialDurAccuracy>=frameDuration)/length(TrialDurAccuracy);
    % I then plot it:
    figure
    scatter(1:length(TrialDurAccuracy),TrialDurAccuracy)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Trial duration error (msec)')
    title(sprintf('Trial durations inaccuracies (%s)','LTP'))
    
    
    % from matlab log file
    miniBlocks =fullLogs;
    % First, I clean the miniBlocks and get rid of everything that is not
    % fixation, jitter and stimuli
    miniBlocks = miniBlocks(ismember(miniBlocks.eventType,'Stimulus') |...
    ismember(miniBlocks.eventType,'Fixation') | ...
    ismember(miniBlocks.eventType,'Jitter'),:);


    % There are several things to look at for the timings:
    % 1. Stimuli:
    % First, did the stimuli lasted for the duration they were supposed to.
    % There are several ways to look at that:
    % First, I make a histogram of the duration of all stimuli, there should be
    % three bars: 1 at 0.5, 1 at 1 and 1 a 1.5
    % Getting the duration of the stimuli:
    % I first remove the responses and the save events, because they might come
    % inbetween and we don't need them
    % I then get the indices of the stimulus presentation begining
    idxStimulus2 = find(ismember(miniBlocks.eventType,'Stimulus'));
    
    % To get the duration of the presentation, one need to subtract the
    % timestamp of the begining of the stimulus presentation to the timestamp
    % of the begining of the fixation. Since I removed the responses, the
    % fixation always directly follows the stimulus, so I can do it as follows:
    
    StimDur2 = [miniBlocks.time(idxStimulus2+1)] - [miniBlocks.time(idxStimulus2)];
    
    % I then plot the histogram
    figure
    histogram(StimDur2)
    ylabel('Number of trials')
    xlabel('Measured stimulus duration')
    title(sprintf('Histogram of the measured stimuli durations %s', 'Logs'))
    
    % But then, there is something else we want to make sure: do they last as
    % long as what was planned?
    % To get this information, I subtract the planned duration to the actual
    % duration, and I make a scatter plot of that:
    StimDurAccuracy2 = (miniBlocks.time(idxStimulus2+1) - miniBlocks.time(idxStimulus2)) - ...
        miniBlocks.plndStimulusDur(idxStimulus2) ;
    
    % Count the number of missed or skipped frames
    stats.PercentSkippedFrameStimuli = sum(StimDurAccuracy2>=frameDuration)/length(StimDurAccuracy2);
    
    % Ithen plot it:
    figure
    scatter(1:length(StimDurAccuracy2),StimDurAccuracy2)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Stimulus duration error (msec)')
    title(sprintf('Stimuli durations inaccuracies (%s)', 'Logs'))
    
    % -------------------------------------------------------------------------
    % 2. Trials:
    % The other things we want to check is whether the overall trial duration
    % matches the planned 2 seconds. We follow the same procedure:
    TrialDur2 =  miniBlocks.time(idxStimulus2+2) - miniBlocks.time(idxStimulus2);
    
    % I then plot the histogram
    figure
    histogram(TrialDur2)
    ylabel('Number of trials')
    xlabel('Measured trial duration')
    title(sprintf('Histogram of the measured trial durations (%s)', 'Logs'))
    % But then, there is something else we want to make sure: do they last as
    % long as what was planned?
    % To get this information, I subtract the planned duration to the actual
    % duration, and I make a scatter plot of that:
    TrialDurAccuracy2 =(miniBlocks.time(idxStimulus2+2) - miniBlocks.time(idxStimulus2)) - 2;
    
    stats.PercentSkippedFrameTrial = sum(TrialDurAccuracy2>=frameDuration)/length(TrialDurAccuracy2);
    % I then plot it:
    figure
    scatter(1:length(TrialDurAccuracy2),TrialDurAccuracy2)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Trial duration error (msec)')
    title(sprintf('Trial durations inaccuracies (%s)','Logs'))


end


%% Checking trials balance:
if CHECK_TRIAL_BALANCE 
    TrialBalanceControl(fullLogs);
end


%% Check relevance:

% This function gets the task relevant condition of a given stim
function taskRelevance = relevanceDecode(stim,Target1,Target2,MiniBlockType)

switch string(MiniBlockType)
    case 'face & object'
        if floor(stim/1000) == 1 || floor(stim/1000) == 2
            if (floor(stim/1000) == 1 && mod(stim,100) == mod(Target1,100)) ||...
                   (floor(stim/1000) == 2 && mod(stim,100) == mod(Target2,100))
                taskRelevance = 1;
            else
                taskRelevance = 2;
            end
        else
            taskRelevance = 3;
        end
        
    case 'letter & false'
        if floor(stim/1000) == 3 || floor(stim/1000) == 4
            if (floor(stim/1000) == 3 && mod(stim,100) == mod(Target1,100)) ||...
                   (floor(stim/1000) == 4 && mod(stim,100) == mod(Target2,100))
                taskRelevance = 1;
            else
                taskRelevance = 2;
            end
        else
            taskRelevance = 3;
        end
end


end

