% This scripts analyse the blank recordings:

clc; clear all; close all;
diary('log_ECOGTest.txt')

% Things you need to setup:
%addpath C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1Development\tests
addpath audioTriggers
addpath '.' % assuming you are in the tests folder
%% Recording modalities
global MEEG Behavior ECoG fMRI
MEEG = 0; % Set to 1 if recording with MEEG
fMRI = 0; % Set to 1 if recording with fMRI
ECoG = 1; % ; Set to 1 if recording with ECoG
Behavior = 0; %Set to 1 if recording with Behavior only
%%
%addpath C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1Development\tests\audioTriggers
PHOTODIODE = 1; % If you want to analyze photodiode signal
AUDIO = 1; % If you want to analyze audio signal
COMPARE_T0_TRIGGER_LOGS = 0;
CHECK_TRIAL_BALANCE = 1; % If you want to perform the check of trial balance. It prints many things to your command window so you might want to not do it sometimes
STAT_DURATIONS = 1; % Do statistics for 
SITE = 'MPI'; 
%SITE = 'Harvard';
VERBOSE = 0;
%SITE = 'Harvard';
audioSR = 1000; % will be overwritten if MPI
%audioSR = 500;
% How many ms are the triggers allowed to exceed the theoretical duration
% of a trigger in order for not being defined as too long
uncertainty_audio = 20; % 20 for MPI for 1000 Hz, 8 for Harvard for 500 Hz
%uncertainty_audio = 8; %

photoDiodeThreshold = 3*10^4; % The threshold for photodiode peak detection. You need to change it according to your system
frameDuration = 16; % Here, enter the frame duration, required for the plots
% Setting up the path to the participant:
% MPI debugging:
dataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1DVT test\SE101\data Kat';
recordingDataPath = dataPath;
recordingFileName = 'Exp1Run1NoResp.vhdr';

% MPI:
%dataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1DVT test';
%recordingDataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1DVT test'; 
%recordingFileName = 'Exp1Run2Resp.vhdr';

% Harvard:
%dataPath = '/Users/katarinabendtz/Dropbox/Research/Conciousness/Experiment_1_preacq_tests'; 
%recordingDataPath = '/Users/katarinabendtz/Dropbox/Research/Conciousness/Experiment_1_preacq_tests/Blackrock_data'; 
%recordingFileName = 'datafile002_subject193.mat'; 

participantPath = ''; % Path to the data of the specific participant
%participantPath = 'SE193'; % Path to the data of the specific participant
cd(fullfile(dataPath,participantPath)) % Change the directory



%% Loading and preparing the data:

% Loading signal data:
cfg = [];
data_eeg = [];
if strcmp(SITE, 'MPI')
    cfg.dataset = recordingFileName;
    data_eeg = ft_preprocessing(cfg);
end

% Reading the log files in:
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
    for i = 2:height(cleanLogs)-20
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
    % Then, we can move on to compare the photo triggers to the log file:
    % But we still need to get the onset of the photodiode:
    photodiodeIdx = find(ismember(data_eeg.label,'Photodiode'));
    PhotodiodeSig = data_eeg.trial{1,1}(photodiodeIdx,:);
    
    % I can now threshold the photo signal:
    PhotodiodeSigBin = PhotodiodeSig > photoDiodeThreshold;
    
    % Now, I do the diff:
    diffPhotoSig = diff(PhotodiodeSigBin);
    
    % Now, we have the onsets
    photoDiodeOnset = find(diffPhotoSig == 1);
    % Need to remove the extra flashes in the beginning first:
    photoDiodeOnset = photoDiodeOnset(7:end);
    
    % Now, we can compare the time stamps:
    intervalPhotodiode = diff(photoDiodeOnset);
    intervalLogs = diff(cleanLogs.time(~ismember(cleanLogs.eventType,'Response')));
    
    % Comparing the log files and the photodiode
    figure(1) 
    plot(intervalPhotodiode,'b')
    hold on
    plot(intervalLogs*1000,'r')
    legend({'Photodiode' 'Log'})
    title('Photodiode vs log files interval between triggers')
    ylabel('Delta t between triggers')
    xlabel('samples')
    
    % Comparing the inaccuracies in the PTB timestamps (only potential
    % jitters, not delays):
    PTBInaccuracy = intervalPhotodiode(1:length(intervalLogs))-(intervalLogs'*1000);
    figure()
    histogram(PTBInaccuracy)
    xlabel('Jitter (msec)')
    ylabel('Number of samples')
    title('PTB timestamping jitter (msec)')
    
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

%% Audio triggers:

if AUDIO
    % Retrieving audio signal:
    AudioSig = [];
    if strcmp(SITE,'MPI')
        AudioIdx = find(ismember(data_eeg.label,'AudioSensor')); % KAT: THIS WHERE I GET THE INDEX OF THE AUDIO CHANNEL
        AudioSig = data_eeg.trial{1,1}(AudioIdx,:);  % THIS IS THE AUDIO SIGNAL dim: 1*nSamples;  
    elseif strcmp(SITE,'Harvard')
        load(fullfile(recordingDataPath, recordingFileName)); 
        AudioSig = double(NS1.Data); 
    end % end initialize
    
    % Fetching the triggers from it:
    nBits = 7; % without paddiing
    bitDurationMs = 20;
    Plotting = 'on'; % Whether you want to plot stuffs
    Volume = 0.5; % Volume set in the experiement 1 script
    if strcmp(SITE, 'MPI')
        audioSR = data_eeg.hdr.Fs; % Sampling rate  
    end
    
    % Getting the audio triggers:
    audioTriggers = audioBitDecoder(AudioSig',nBits,bitDurationMs,audioSR,Volume,Plotting);
    
    % Checking the length of the audio triggers. In some cases, the triggers
    % are too long or too short. This can happen for different reasons, but the
    % most standard is that two triggers are too close in time and parsed as
    % one.
    TooLongTrig = find(audioTriggers(:,3)>(bitDurationMs*(nBits+2)+uncertainty_audio));
    TooShortTrig = find(audioTriggers(:,3)<(bitDurationMs*(nBits+2)-uncertainty_audio));
    
    % Warning the experimenter that things were wrong
    if ~isempty(TooLongTrig)
        warning('The following triggers were too long:')
        disp(TooLongTrig)
    end
    
    if ~isempty(TooShortTrig)
        warning('The following triggers were too short:')
        disp(TooShortTrig)
    end
    
    if COMPARE_T0_TRIGGER_LOGS
        % Compare to the trigger log files
        audio_trig_log_list = dir('*TrigDur*.csv');
        audio_trig_logs = [];
        length(audio_trig_log_list)
        size(audioTriggers)
        for i = 1:length(audio_trig_log_list)
            log = readtable(audio_trig_log_list(i).name, 'Format', '%s%s');
            audio_trig_logs = [audio_trig_logs; log];
        end
        
        audio_trig_logs_bit_codes = table2cell(audio_trig_logs(:,1));
        
        if VERBOSE
            disp('The size of audio_trig_logs_bit_codes is: ')
            size(audio_trig_logs_bit_codes,1)
            disp('The size of audioTriggers is: ')
            size(audioTriggers)
        end

        %for e=1:size(audio_trig_logs_bit_codes,1)
        for e=1:size(audioTriggers,1)

            % convert the bit codes in audioTriggers to the same format as in the
            % logs
            audioTriggers_record_full_code = int2str(audioTriggers(e,4));

            audioTriggers_record = audioTriggers_record_full_code(2:nBits+1);

            if ~strcmp(audio_trig_logs_bit_codes{e},audioTriggers_record)
                disp('NOTE! Mismatch between audio trigger log files and audio triggers decoded from recording at position ')
                if VERBOSE
                    e
                    disp('The log file says: ') 
                    audio_trig_logs_bit_codes(e) 
                    disp('But the audio trigger recording says: ')
                    audioTriggers_record
                    disp('And we are at position ')
                    audioTriggers(e,2)
                    disp('in the audio_triggers')
                end
            end % end if mismatch

        end % for all entries in audio_trig_bit_codes
        
    end % end if comparing to audio trigger logs
    
    % I then extract only the stimuli triggers:
    stimAudioTriggers = array2table(audioTriggers(audioTriggers(:,1)<=108,:),'VariableNames',{'Triggers' 'TimeStamp' 'TriggerDuration',  'BitCode'});
    
    % I then compute the diff to get the
    % I can then compare the timestamps:
    stimLogs = cleanLogs(ismember(cleanLogs.eventType,'Stimulus'),:);
    diffAudioLog = diff(stimLogs.time)*1000;
    diffAudioSig = diff(stimAudioTriggers.TimeStamp);
    
    % If things are misaligned, you will want to remove entries from the
    % log. Please enter 
    happy = 0;
    % Plotting the intervals:;
    while ~happy
        figure
        plot(diffAudioLog)
        hold on
        plot(diffAudioSig)
        legend({'Logs' 'Audio'})
        title('Audio vs log files interval between stimuli triggers')
        ylabel('Delta t between triggers')
        xlabel('samples')
        happyInput = input('Are things aligned? y/n [y]:','s');
        if strcmp(happyInput,'y')
            happy = 1;
        elseif strcmp(happyInput,'n')
            idxMisaligned = input('What is the index of the first misaligned sample in the audio signal? ');
            idxPlusMin = input('Is there a sample too much or too few in the audio signal? [p for too many or m for too few]','s');
            if strcmp(idxPlusMin,'p')
                stimAudioTriggers = [stimAudioTriggers(1:idxMisaligned-1,:); stimAudioTriggers(idxMisaligned+1:end,:)];
                diffAudioSig = diff(stimAudioTriggers.TimeStamp);
            elseif strcmp(idxPlusMin,'m')
                stimAudioTriggers = [stimAudioTriggers(1:idxMisaligned,:); stimAudioTriggers(idxMisaligned:end,:)];
                diffAudioSig = diff(stimAudioTriggers.TimeStamp);
            end
            close
        end
    end
    % Responses:
    % Checking the responses:
    RespAudioTriggers = array2table(audioTriggers(audioTriggers(:,1) == 110,:),'VariableNames',{'Triggers' 'TimeStamp' 'TriggerDuration',  'BitCode'});
    
    % I then compute the diff to get the
    % I can then compare the timestamps:
    RespLogs = cleanLogs(ismember(cleanLogs.eventType,'Response'),:);
    
    % Plotting the diffs:
    figure
    plot(diff(RespLogs.time)*1000)
    hold on
    plot(diff(RespAudioTriggers.TimeStamp))
    legend({'Logs' 'Audio'})
    title('Audio vs log files interval between response triggers')
    ylabel('Delta t between triggers')
    xlabel('samples')
    
    
    %% Comparing log entries to audio triggers:
    % First, I generate the matrix_LUT
    [Matrix_LUT,inverse_Matrix_LUT] = generateMatLUT();
    
    % I then loop through all the triggers that were detected to decode the
    % experimental conditions
    for i = 1:height(stimAudioTriggers)
        [category, relevance, orientation, duration] = getCatRelOriDur(dec2bin(stimAudioTriggers.Triggers(i),7),inverse_Matrix_LUT);
        stimAudioTriggers.Cate(i) = category;
        stimAudioTriggers.Rel(i) = relevance;
        stimAudioTriggers.Or(i) = orientation;
        stimAudioTriggers.Dur(i) = duration;
    end
    
    
    % Now, I compare the audio triggers conditions to what we have in the log
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
    stimLogNew = struct2table(stimLogNew);
    
    
    
    % Now that we have things in the same format, we can compare every entry we
    % have in the logs and in the triggers
    for i = 1:height(stimLogNew)
        
        if strcmp(stimLogNew.Category(i),stimAudioTriggers.Cate(i)) && strcmp(stimLogNew.orientation(i),stimAudioTriggers.Or(i)) && ...
                strcmp(stimLogNew.duration(i),stimAudioTriggers.Dur(i)) && strcmp(stimLogNew.relevance(i),stimAudioTriggers.Rel(i))
            condComp(i) = 1;
        else
            condComp(i) = 0;
        end
    end
    
    
    a = find(condComp == 0);
    
    if ~isempty(a)
        warning('The audio and log files are not fully consistent, go check the following matrices: stimLogNew, stimLog, stimAudioTrigger at the following lines to figure out whats going on:')
        disp(a)
        disp('Go check the figure 2 at the following time stamps')
        disp(stimAudioTriggers.TimeStamp(a))
    else
        disp('Your log and audio are fully consistent: things work fine')
    end
end

%% Comparing photo and audio timeStamps:
% This enables the quantification of audio delays and jitters:
if AUDIO && PHOTODIODE
    % First things first, I plot them:
    figure
    plot(PhotodiodeSigBin)
    hold on
    scatter(stimAudioTriggers.TimeStamp,ones(height(stimAudioTriggers),1))
    legend({'Photodiode' 'Audio'})
    title('Photodiode vs audio signal')
    
    % Now, I can compare the time stamps directly, by taking every third
    % photodiode time stamps:
    StimPhotodiode = photoDiodeOnset(1,1:3:end-3);
    disp('CHECK THIS')
    size(StimPhotodiode)
    size(stimAudioTriggers.TimeStamp)
    % I can now compare them:
    PhotoVsAudio = stimAudioTriggers.TimeStamp - StimPhotodiode';
    
    figure()
    histogram(PhotoVsAudio)
    title('Distribution of audio delays (photodiode as reference)')
    
    % Computing the mean delays and standard error
    meanAudioDelay = mean(PhotoVsAudio);
    stdAudioDelay  = std(PhotoVsAudio);
    
    % To get a better sense of the delays, I centre them around zero and find
    % the max and min, to get a sense of the jitter:
    centeredAudioDelays = PhotoVsAudio - meanAudioDelay;
    maxAudioDelay = max(centeredAudioDelays);
    minAudioDelay = min(centeredAudioDelays);
    % Then, I find the 95% quartiles:
    y = [quantile(centeredAudioDelays,0.025) quantile(centeredAudioDelays,0.975)];
    
    meanDelayMessage = sprintf('The mean delays is %d',meanAudioDelay);
    stdDelayMessage = sprintf('The delay standard error is %d',stdAudioDelay);
    delaysPrctile = sprintf('95 percent of the audio jitters are found between %d and %dms around the mean delay',y);
    disp(meanDelayMessage)
    disp(stdDelayMessage)
    disp(delaysPrctile)
end


%% Comparing the logged duration vs the expected duration:
% This is the script written by Katarina, checking the timestamps from the
% log file against the expected duration
if STAT_DURATIONS
    Check_diff_between_logged_and_planned_durations(fullLogs)
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

