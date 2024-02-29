% Written by Alex LePauvre and Katarina Bendtz, 2020.
% Audio only works for one experiment (recording of one experiment)

clc; clear all; close all;
diary('log_ECOGTest.txt')

% Things you need to setup:
%addpath C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1Development\tests
%addpath C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1Development\tests
addpath '.' % assuming you are in the tests folder
addpath './audioTriggers'
%% Recording modalities and plotting and saving
global MEEG Behavior ECoG fMRI font_size save_plots signalSR VERBOSE show_only_plots_that_are_not_saved
MEEG = 0; % Set to 1 if recording with MEEG
fMRI = 0; % Set to 1 if recording with fMRI
ECoG = 1; % ; Set to 1 if recording with ECoG
Behavior = 0; %Set to 1 if recording with Behavior only
% Plotting:
font_size = 18; % Plots SLAB
save_plots = 1; % SLAB
show_only_plots_that_are_not_saved = 1;


%% What analyses to perform
%addpath C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1Development\tests\audioTriggers
PHOTODIODE = 1; % If you want to analyze photodiode signal
AUDIO = 1; % If you want to analyze audio signal
COMPARE_AUDIO_T0_TRIGGER_LOGS = 0;
CHECK_TRIAL_BALANCE = 1; % If you want to perform the check of trial balance. It prints many things to your command window so you might want to not do it sometimes
%LOG_DURATIONS = 1; % Removed this bc durations are better measured by the photodiode and the logs are better checked against the photodiode then to itself % SLAB

%% Site
%SITE = 'MPI'; 
SITE = 'MPI';
VERBOSE = 0;

%% Recording signal properties
% If you have a sampling rate that is higher than 1000, you might end up in
% problems with the photodiode decoder as it is constructed right now
% because you might have an a 'fury' structure of your signal making
% the signal pass your threshold several times per photosignal. SLAB
% In that case you might want to downsample to 1000. If the signalSR_orig >
% signalSR, this downsamoling will be carried out.
signalSR_orig = 2048;
% This is the sampling rate that we will use.
signalSR = 2048;

%% Audio trigger settings
% How many ms are the triggers allowed to exceed the theoretical duration
% of a trigger in order for not being defined as too long
%uncertainty_audio = 20; % 20 for MPI for 1000 Hz, 8 for Harvard for 500 Hz
uncertainty_audio = 8; %
%noise_threshold = 3*10^6; % MPI
noise_threshold_high_pass_audio = 2000;
% Cutting away initial noise (you have to inspect your signal to set this): 
%start_sample = 7700;
start_sample_audio = 7700*2; 
% For peprocessing of denoiseaudio (variable used in audioBitDecoder). How many zeros in a row can turn up between the ones and minus ones in
% denoiseaudio (check the denoiseaudio before preprocessing)
max_intermediate_noise_zeros_audio = 2; 
nBits = 7; % without paddiing
bitDurationMs = 20;

%% Photodiode trigger settings
photoDiodeThreshold = 3*10^6; % 
start_sample_photo_diode = 18000; % At experiment onset, the screens turns on and off a few times, 
% which leads to photodiode detection you want to remove. Check your signal
% to set this value. SLAB
%photoDiodeThreshold = 3*10^4; % The threshold for photodiode peak
%detection. You need to change it according to your system. 
frameDuration = 16; % Here, enter the frame duration, required for the plots

%% Path and file settings
% Setting up the path to the participant:
% MPI debugging:
%dataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1DVT test\SE101\data Kat';
%recordingDataPath = dataPath;
%recordingFileName = 'Exp1Run1NoResp.vhdr';

% MPI:
%dataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1DVT test';
%recordingDataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Experiment1DVT test'; 
%recordingFileName = 'Exp1Run2Resp.vhdr';

% Harvard:
%dataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Kat test'; 
%recordingDataPath = 'C:\Users\alexander.lepauvre\Seafile\TWCF_Project\Kat test'; 
dataPath = 'C:\Users\alexander.lepauvre\Seafile\[ECoG Team]\Pre-acquisition test reports\NYU\Pre_acqTestData_7_08_2020';
recordingDataPath = 'C:\Users\alexander.lepauvre\Seafile\[ECoG Team]\Pre-acquisition test reports\NYU\Pre_acqTestData_7_08_2020';
recordingFileName = 'exp1_multipleRun_part1_8.6.20.edf'; 
%recordingFileName = 'Blackrock_recording_only_photodiode_July_16_2020_30KHz_subject_901_to_921';
audio_channel = 2; % the channel for audio trigger in the recording data SLAB
%audio_channel = 1; 
photo_channel = 1; % the channel for photo trigger in the recording data

%participantPath = 'SE193'; % Path to the data of the specific participant
participantPaths = {'SE1 - Copy'};% 'SE602' 'SE603' 'SE604' 'SE605' 'SE606' 'SE607' 'SE608' 'SE609' 'SE610' 'SE611' 'SE612' 'SE613' 'SE614' 'SE615' 'SE616' 'SE617' 'SE618' 'SE619' 'SE620' 'SE621'}; % Path to the data of the specific participant
%participantPaths = {'SE901' 'SE902' 'SE903' 'SE904' 'SE905' 'SE906' 'SE907' 'SE908' 'SE909' 'SE910' 'SE911' 'SE912' 'SE913' 'SE914' 'SE915' 'SE916' 'SE917' 'SE918' 'SE919' 'SE920' 'SE921'};
%participantPaths = {'SE805'}; % Use this if one experiment only

%% Several experiments in one recording:
several_experiments_in_the_recording = 0; % Set this to one 
plot_save_path_all_exp = '/Users/katarinabendtz/Dropbox/Research/Conciousness/Experiment_1_preacq_tests/Plots_SE601_to_SE621';
%plot_save_path_all_exp = '/Users/katarinabendtz/Dropbox/Research/Conciousness/Experiment_1_preacq_tests/Plots_SE901_to_SE921';

if AUDIO && several_experiments_in_the_recording
    disp('Sorry, it is not supported to run audio checks for recordings with several experiments');
    return
end


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%% Loading and preparing the data:
cleanLogs_vector = {};
ctr = 1;
for i=1:length(participantPaths)
    
    participantPath = participantPaths{i};
    % Loading signal data:
    cd(fullfile(dataPath,participantPath)) % Change the directory
    cfg = [];
    data_eeg = [];
    if strcmp(SITE, 'MPI')
        cfg.dataset = recordingFileName;
        data_eeg = ft_preprocessing(cfg);
    end

    % Reading the log files in:
    logFilesList = dir('*RawDur*.csv');
    fullLogs = [];
    for j = 1:length(logFilesList)
        disp(sprintf('Loading %s', logFilesList(j).name));
        Log = readtable(logFilesList(j).name);
        fullLogs = [fullLogs;Log];
     
    end

    % Cleaning up the full logs from events that we are not interested in: 
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
    
    cleanLogs_vector{ctr} = cleanLogs;
    ctr = ctr + 1;
    
    cd(fullfile(dataPath));
end

%% Preparing photo diode data %%

photoDiodeOnset_vector = {};

if PHOTODIODE
    
    disp('Preparing photodiode data');
    
    % Fetch the data
    if strcmp(SITE, 'MPI')
        photodiodeIdx = 131;
        PhotodiodeSig = data_eeg.trial{1,1}(photodiodeIdx,:);
    else
        load(fullfile(recordingDataPath, recordingFileName)); 
        PhotodiodeSig = double(NS6.Data(photo_channel,:));
        %PhotodiodeSig = double(NS1.Data(photo_channel,:)); 
    end
    
    % Downsample, otherwise the signal is too noisy.
    if signalSR_orig > signalSR
        PhotodiodeSig = resample(PhotodiodeSig,signalSR, signalSR_orig);
    end
    
    % NOTE: Removing this bc using the three close flashes instead
    % Cut away the first part where PTB is testing the frame
    %%PhotodiodeSig(1:start_sample_photo_diode) = PhotodiodeSig(start_sample_photo_diode);
    % I can now threshold the photo signal:
    PhotodiodeSigBin = PhotodiodeSig > photoDiodeThreshold;

    % Get the onset of the photodiode.
    % First we do the diff:
    diffPhotoSig = diff(PhotodiodeSigBin);
    
    % Now, we have the onsets
    photoDiodeOnset = find(diffPhotoSig == 1);

    % Need to remove the 3 flashes in the beginning first and the 3 at the end:
    %%photoDiodeOnset = photoDiodeOnset(4:end);
    %%photoDiodeOnset = photoDiodeOnset(1:end -3);
    
    % Cut out the photosignals corresponding to the experiment
    % Time window should be frameduration * 3 (nr of triggers) * 3 (nr flashes it is on)  * 2 +
    % (on and off) + 1 * conversion from ms to # samples
    experiment_start = [44116];
    experiment_end = [4083949];
%     sample_window_for_the_3_consecutive_start_end_triggers = (frameDuration * 3 * 3 * 2 +1)*(signalSR/1000.);
%     i=1;
%     while i<length(photoDiodeOnset) -1
%         
%         three_consecutive_onset_times = photoDiodeOnset(i:i+2)*(1000./signalSR);
%         
%         if (three_consecutive_onset_times(3) - three_consecutive_onset_times(1)) < sample_window_for_the_3_consecutive_start_end_triggers 
%             %disp('Inside found an experiment start!')
%            
%             if length(experiment_start) == length(experiment_end)
%                 experiment_start = [experiment_start photoDiodeOnset(i+3)];
%                 %disp('adding this to experiment start: ')
%                 %photoDiodeOnset(i+3)
%             else
%                 experiment_end = [experiment_end photoDiodeOnset(i-1)];
%                 %disp('adding this to experiment end: ')
%                 %photoDiodeOnset(i-1)
%             end % assign        
%             i = i + 2;
%         end % into window 
%         i = i + 1;
%     end % end photoDiodeOnset
    
    % Plot the photodiode signal
    figure
    title('Raw photodiode signal and onsets', 'FontSize', font_size);
    xlabel('Sample #', 'FontSize', font_size);
    ylabel('Signal amplitude', 'FontSize', font_size);
    hold on;
    plot(PhotodiodeSig);
    hold on;
    plot(photoDiodeOnset,ones(length(photoDiodeOnset),1)*photoDiodeThreshold,'k+');
    hold on;
    plot(experiment_start,ones(length(experiment_start),1)*1.1*photoDiodeThreshold, 'g*');
    hold on;
    plot(experiment_end,ones(length(experiment_end),1)*1.1*photoDiodeThreshold, 'rp');
    hold on;
    
    lgd = legend({'Photodiode signal' 'Detected onsets' 'Detected exp start' 'Detected exp end'});
    lgd.FontSize = font_size;
    
    disp(sprintf('I found %i experiments', length(experiment_start)));
    
    % Store the experiment signals:
    ctr = 1;
    for i=1:length(participantPaths)
        
        exp_start = experiment_start(i);
        exp_end = experiment_end(i);
        photoDiodeOnset_vector{ctr} = photoDiodeOnset( photoDiodeOnset >= exp_start & photoDiodeOnset <= exp_end);
        ctr = ctr + 1;
        
    end
    
end

%% Preparing audio data
     
%AudioSig_all_exp = {};
if AUDIO
    
    disp('Preparing audio trigger data')
    
    % Retrieving audio signal:
    AudioSig = [];
    if strcmp(SITE,'MPI')
        AudioIdx = 130; 
        AudioSig = data_eeg.trial{1,1}(AudioIdx,:);  % THIS IS THE AUDIO SIGNAL dim: 1*nSamples;  
    elseif strcmp(SITE,'Harvard')
        load(fullfile(recordingDataPath, recordingFileName)); 
        %audioSig = double(NS1.Data(audio_channel,:)); 
        audioSig = double(NS6.Data(audio_channel,:)); 
    end % end initialize
    
    % Resample 
    if signalSR_orig > signalSR
        disp('resampling audio data');
        AudioSig = resample(audioSig, signalSR , signalSR_orig);
    end
    
    
end % end if audio
  
% Compare the photo triggers to the log file etc   

if PHOTODIODE
    
    disp('Photodiode analysis');
    
    PTBInaccuracy_all_exp = [];
    
    stim_dur_inacc_percent_1_frame_all_exp = [];
    jitter_dur_inacc_percent_1_frame_all_exp = [];
    stimDur_all_exp = [];
    stimDurAccuracy_all_exp = [];
    JitterDur_all_exp = [];
    JitterDurAccuracy_all_exp = [];
    StimDurAccuracy_center_all_exp = [];
    StimDurAccuracy_right_all_exp = [];
    StimDurAccuracy_left_all_exp = [];
    StimDurAccuracy_face_all_exp = [];
    StimDurAccuracy_object_all_exp = [];
    StimDurAccuracy_letter_all_exp = [];
    StimDurAccuracy_false_all_exp = [];
    StimDurAccuracy_05_all_exp = [];
    StimDurAccuracy_10_all_exp = [];
    StimDurAccuracy_15_all_exp = [];
    StimDurAccuracy_target_all_exp = [];
    StimDurAccuracy_non_target_all_exp = [];
    StimDurAccuracy_irrelevant_all_exp = [];
    
    for e=1:length(participantPaths)
        
        photoDiodeOnset = photoDiodeOnset_vector{e};
        cleanLogs = cleanLogs_vector{e};
        participantPath = participantPaths{e};
        
        %if VERBOSE
            disp('Working in pcp ');
            participantPath
        %end
    
        % Now, we can compare the time stamps (no photo diode for response) to
        % the log
        % (also convert to ms):
        conversion_factor = 1000./signalSR;
        intervalPhotodiode = diff(photoDiodeOnset)*conversion_factor;
        intervalLogs = diff(cleanLogs.time(~ismember(cleanLogs.eventType,'Response')))*1000;

        figure(); 
        plot(intervalPhotodiode,'b')
        hold on
        % Convert to ms
        plot(intervalLogs,'r');
        legend({'Photodiode' 'Log'}, 'FontSize', font_size);
        title('Interval between triggers, Photodiode vs log files ', 'FontSize', font_size);
        ylabel('Delta t between triggers [ms]', 'FontSize', font_size);
        xlabel('Time [ms]', 'FontSize', font_size);

        % Comparing the inaccuracies in the PTB timestamps (only potential
        % jitters, not delays):

        PTBInaccuracy = intervalPhotodiode(1:length(intervalLogs))-(intervalLogs');
        PTBInaccuracy_all_exp = [PTBInaccuracy_all_exp; PTBInaccuracy];
        if show_only_plots_that_are_not_saved
            figure('visible', 'off');
        else
            figure();
        end
        histogram(PTBInaccuracy);
        xlabel('Jitter [ms]', 'FontSize', font_size);
        ylabel('Number of samples', 'FontSize', font_size);
        title(sprintf('PTB timestamping jitter [ms] \n (diff between intervals in log and photodiode)'), 'FontSize', font_size);     
        cd(fullfile(dataPath, participantPath));
        if save_plots
            saveas(gcf, sprintf('PTB_timestamp_vs_photodiode_jitter_%s.png', participantPath));
        end
        
        cd(dataPath);

        % Now we want to check the durations of stimuli presentations using the 
        % photodiode time stamps using the Check_diff_between_logged_and_planned_durations
        % function.
        % Reusing the structure of the log file, we replace the log files time stamps 
        % by the photodiode ones.
        % I get rid of the response first:
        cleanLogsPhotodiode = cleanLogs(~ismember(cleanLogs.eventType,'Response'),:);

        % I then replace the timestamps by the photodiode ones and convert to ms:
        % Since the logs are in s, we also want the photodiode time stamp in seconds    
        photoDiodeOnset_s = photoDiodeOnset/signalSR;
        cleanLogsPhotodiode.time = photoDiodeOnset_s(1:height(cleanLogsPhotodiode))';

        % Checking the trial and stimuli durations based on the photodiode. The
        % stats structure contains estimations of percentage of skipped frames:

        % NOTE: checkDurations is now merged into Check_diff_between_logged_and_planned_durations
        %statsDurationPhotodiode = checkDurations(cleanLogsPhotodiode,'Photodiode',1,frameDuration);
        cd(fullfile(dataPath, participantPath));
        [stim_dur_inacc_percent_1_frame, jitter_dur_inacc_percent_1_frame, stimDur, stimDurAccuracy, JitterDur, JitterDurAccuracy, StimDurAccuracy_center, StimDurAccuracy_right, StimDurAccuracy_left, StimDurAccuracy_face, StimDurAccuracy_object, StimDurAccuracy_letter, StimDurAccuracy_false, StimDurAccuracy_05, StimDurAccuracy_10, StimDurAccuracy_15, StimDurAccuracy_target, StimDurAccuracy_non_target, StimDurAccuracy_irrelevant] = ECoG_Check_diff_between_logged_and_planned_durations(cleanLogsPhotodiode, 'photodiode', frameDuration, participantPath);
        
         % Do histograms and stats for saving:
        ECoG_hists_and_stats_durations(stimDur, stimDurAccuracy, JitterDur, JitterDurAccuracy, StimDurAccuracy_center, StimDurAccuracy_right, StimDurAccuracy_left, StimDurAccuracy_face, StimDurAccuracy_object, StimDurAccuracy_letter, StimDurAccuracy_false, StimDurAccuracy_05, StimDurAccuracy_10, StimDurAccuracy_15, StimDurAccuracy_target, StimDurAccuracy_non_target, StimDurAccuracy_irrelevant, 'photodiode', participantPath);
        cd(dataPath);
        % Save vectors for plotting for all files:
        stim_dur_inacc_percent_1_frame_all_exp = [stim_dur_inacc_percent_1_frame_all_exp; stim_dur_inacc_percent_1_frame];
        jitter_dur_inacc_percent_1_frame_all_exp = [jitter_dur_inacc_percent_1_frame_all_exp; jitter_dur_inacc_percent_1_frame];
        stimDurAccuracy_all_exp = [stimDurAccuracy_all_exp; stimDurAccuracy];
        JitterDur_all_exp = [JitterDur_all_exp; JitterDur];
        stimDur_all_exp = [stimDur_all_exp; stimDur];
        stimDurAccuracy_all_exp = [stimDurAccuracy_all_exp; stimDurAccuracy];
        JitterDur_all_exp = [JitterDur_all_exp; JitterDur];
        JitterDurAccuracy_all_exp = [JitterDurAccuracy_all_exp; JitterDurAccuracy];
        StimDurAccuracy_center_all_exp = [StimDurAccuracy_center_all_exp; StimDurAccuracy_center];
        StimDurAccuracy_right_all_exp = [StimDurAccuracy_right_all_exp; StimDurAccuracy_right];
        StimDurAccuracy_left_all_exp = [StimDurAccuracy_left_all_exp; StimDurAccuracy_left];
        StimDurAccuracy_face_all_exp = [StimDurAccuracy_face_all_exp; StimDurAccuracy_face];
        StimDurAccuracy_object_all_exp = [StimDurAccuracy_object_all_exp; StimDurAccuracy_object];
        StimDurAccuracy_letter_all_exp = [StimDurAccuracy_letter_all_exp; StimDurAccuracy_letter];
        StimDurAccuracy_false_all_exp = [StimDurAccuracy_false_all_exp; StimDurAccuracy_false];
        StimDurAccuracy_05_all_exp = [StimDurAccuracy_05_all_exp; StimDurAccuracy_05];
        StimDurAccuracy_10_all_exp = [StimDurAccuracy_10_all_exp; StimDurAccuracy_10];
        StimDurAccuracy_15_all_exp = [StimDurAccuracy_15_all_exp; StimDurAccuracy_15];
        StimDurAccuracy_target_all_exp = [StimDurAccuracy_target_all_exp; StimDurAccuracy_target];
        StimDurAccuracy_non_target_all_exp = [StimDurAccuracy_non_target_all_exp; StimDurAccuracy_non_target];
        StimDurAccuracy_irrelevant_all_exp = [StimDurAccuracy_irrelevant_all_exp; StimDurAccuracy_irrelevant];
        
        % Now, do a scatter plot with the PTB inaccuracy vs the duration
        % inacurracy, to see if the inaccuracy in PTB seems to be related
        % to the inaccuracy of the sstim durations
        % First we need to pick out the photodiode and the logging that only
        % corresponds to the stimuli:
        eventType_column_ind = 14;
        time_column_ind = 13;
        % We first creates an array of bools where the indices correspond to the indicides
        % where the events are reponse or save (0) and not (1)
        cleanLogs_cells = table2cell(cleanLogs);
        NoSaveNoResp_ind_bools = ~ismember(cleanLogs_cells(:,eventType_column_ind),'Response') & ~ismember(cleanLogs_cells(:,eventType_column_ind),'Save');
        NoSaveNoResp_inds = find(NoSaveNoResp_ind_bools == 1);
        miniBlockNoSaveNoResp = cleanLogs_cells(NoSaveNoResp_inds,:);
        
        % Then find the stimulus indices
        idxStimulus = find(ismember(miniBlockNoSaveNoResp(:,eventType_column_ind),'Stimulus'));
        intervalLogs_stim_only = cell2mat(miniBlockNoSaveNoResp(idxStimulus+1,time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(idxStimulus,time_column_ind));
        
        % Then do the same for photodiode time stamps
        cleanLogsPhotodiode_cells = table2cell(cleanLogsPhotodiode);
        idxStimulus = find(ismember(cleanLogsPhotodiode_cells(:,eventType_column_ind),'Stimulus'));
        intervalPhotodiode_stim_only = cell2mat(cleanLogsPhotodiode_cells(idxStimulus+1,time_column_ind)) - cell2mat(cleanLogsPhotodiode_cells(idxStimulus,time_column_ind));
        PTBInaccuracy_stim_only = (intervalPhotodiode_stim_only(1:length(intervalLogs_stim_only))-(intervalLogs_stim_only))*1000;
        
        if show_only_plots_that_are_not_saved
            figure('visible', 'off');
        else
            figure();
              end
        scatter(PTBInaccuracy_stim_only, stimDurAccuracy);
        hold on;
        xlabel('PTB inaccurcay [ms]', 'FontSize', font_size);
        ylabel('Stimulus duration inaccuracy [ms]', 'FontSize', font_size);
        title(sprintf('Stimulus duration accuracy vs PTB timestamping jitter [ms] '), 'FontSize', font_size);     
        cd(fullfile(dataPath, participantPath));
        if save_plots
            saveas(gcf, sprintf('Duration_accuracy_vs_PTB_inaccuracy_%s.png', participantPath));
        end
        cd(dataPath);
            
    end % loop over files
    
    if several_experiments_in_the_recording
        
        % Plot the hists for all files:    
        cd(plot_save_path_all_exp);
        ECoG_hists_and_stats_durations(stimDur_all_exp, stimDurAccuracy_all_exp, JitterDur_all_exp, JitterDurAccuracy_all_exp, StimDurAccuracy_center_all_exp, StimDurAccuracy_right_all_exp, StimDurAccuracy_left_all_exp, StimDurAccuracy_face_all_exp, StimDurAccuracy_object_all_exp, StimDurAccuracy_letter_all_exp, StimDurAccuracy_false_all_exp, StimDurAccuracy_05_all_exp, StimDurAccuracy_10_all_exp, StimDurAccuracy_15_all_exp, StimDurAccuracy_target_all_exp, StimDurAccuracy_non_target_all_exp, StimDurAccuracy_irrelevant_all_exp, 'photodiode', 'All_experiments');
        cd(dataPath)

        % Plot PTB time stamps for all files
        if show_only_plots_that_are_not_saved
            figure('visible', 'off');
        else
            figure();
        end
        histogram(PTBInaccuracy_all_exp);
        xlabel('Jitter [ms]', 'FontSize', font_size);
        ylabel('Number of samples', 'FontSize', font_size);
        title(sprintf('PTB timestamping jitter [ms] \n (photodiode interval - log interval)'), 'FontSize', font_size);     
        if save_plots
            cd(plot_save_path_all_exp);
            saveas(gcf, sprintf('PTB_timestamp_vs_photodiode_jitter_%s.png','All_exp'));
            cd(dataPath)
        end
        
        % Plot inaccuracy in duration
        if show_only_plots_that_are_not_saved
            figure('visible', 'off');
        else
            figure();
        end
        h = histogram(stim_dur_inacc_percent_1_frame_all_exp);
        xlabel('Percent 1 frame inaccuracy for stimuli', 'FontSize', font_size);
        ylabel('# experiments', 'FontSize', font_size);
        title(sprintf('Percent of stim with an inacc. of 1 frame'), 'FontSize', font_size);     
        txt1 = sprintf('Avg # of missed/extra frames = %s %%', num2str(mean(stim_dur_inacc_percent_1_frame_all_exp),2));
        txt2 = sprintf('Std # of missed/extra frames = %s %%', num2str(std(stim_dur_inacc_percent_1_frame_all_exp),2));
 
        
        posx = min(stim_dur_inacc_percent_1_frame_all_exp) + 0.*(max(stim_dur_inacc_percent_1_frame_all_exp) - min(stim_dur_inacc_percent_1_frame_all_exp));
        posy = 0.8*max(h.Values);
        text( posx, posy,  txt1, 'FontSize', font_size);
        text( posx, posy - 0.1*posy,  txt2, 'FontSize', font_size);
        
        if save_plots
            cd(plot_save_path_all_exp);
            saveas(gcf, sprintf('Percent_inaccuracy_duration_stim_%s.png','All_exp'));
            cd(dataPath)
        end

        disp(txt1)
        disp(txt2)
        disp(posx)
        disp(posy)
        disp(max(h.Values))
      
        if show_only_plots_that_are_not_saved
            figure('visible', 'off');
        else
            figure();
        end
        h = histogram(jitter_dur_inacc_percent_1_frame_all_exp);
        xlabel('Percent 1 frame inaccuracy for jitter', 'FontSize', font_size);
        ylabel('# experiments', 'FontSize', font_size);
        title(sprintf('Percent of jitter with an inacc. of 1 frame'), 'FontSize', font_size);     
        txt1 = sprintf('Avg # of missed/extra frames = %s %%', num2str(mean(jitter_dur_inacc_percent_1_frame_all_exp),2));
        txt2 = sprintf('Std # of missed/extra frames = %s %%', num2str(std(jitter_dur_inacc_percent_1_frame_all_exp),2));
        posx = min(jitter_dur_inacc_percent_1_frame_all_exp) + 0.1*(max(jitter_dur_inacc_percent_1_frame_all_exp) - min(jitter_dur_inacc_percent_1_frame_all_exp));
        posy = 0.8*max(h.Values);
        text( posx, posy,  txt1, 'FontSize', font_size);
        text( posx, posy - 0.1*posy,  txt2, 'FontSize', font_size);
        if save_plots
            cd(plot_save_path_all_exp);
            saveas(gcf, sprintf('Percent_inaccuracy_duration_jitter_%s.png','All_exp'));
            cd(dataPath)
        end
        
        
        
        
    end % if several experiments
    

    
end % end if photodiode



%% Audio triggers:

if AUDIO
    
    disp('Now working on the audio trigger');
    
    % Fetching the triggers from it:

    Plotting = 'on'; % Whether you want to plot stuffs
    Volume = 0.5; % Volume set in the experiement 1 script
    if strcmp(SITE, 'MPI')
        signalSR = data_eeg.hdr.Fs; % Sampling rate  
    end
    
      % Loop over participants/experiments
    %for p=1:length(participantPaths)
        
       % AudioSig = AudioSig_all_exp{p};
        %participantPath = participantPaths{p};
        
        % Getting the audio triggers:
        audioTriggers = audioBitDecoder(AudioSig',nBits,bitDurationMs,Volume,1, start_sample_audio, max_intermediate_noise_zeros_audio, noise_threshold_high_pass_audio);

        if VERBOSE
            audioTriggers
        end


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
        cd(fullfile(dataPath, participantPaths{1}))
        if COMPARE_AUDIO_T0_TRIGGER_LOGS
            % Compare to the trigger log files
            audio_trig_log_list = dir('*TrigDur*.csv');
            audio_trig_logs = [];
            if VERBOSE
                disp('length(audio_trig_log_list):' );
                length(audio_trig_log_list)
                disp('size(audioTriggers)')
                size(audioTriggers)
            end
            for i = 1:length(audio_trig_log_list)
                log = readtable(audio_trig_log_list(i).name, 'Format', '%s%s');
                audio_trig_logs = [audio_trig_logs; log];
            end
            %cd(dataPath);

            audio_trig_logs_bit_codes = table2cell(audio_trig_logs(:,1));

            if VERBOSE
                disp('The size of audio_trig_logs_bit_codes is: ')
                size(audio_trig_logs_bit_codes,1)
                disp('The size of audioTriggers is: ')
                size(audioTriggers)
            end

            % Save the first mismatch:
            first_mismatch = false;
            first_mismatch_info = [];

            %for e=1:size(audio_trig_logs_bit_codes,1)
            for e=1:size(audioTriggers,1)

                % convert the bit codes in audioTriggers to the same format as in the
                % logs
                audioTriggers_record_full_code = int2str(audioTriggers(e,6));

                audioTriggers_record = audioTriggers_record_full_code(2:nBits+1);

                if ~strcmp(audio_trig_logs_bit_codes{e},audioTriggers_record)
                    disp('NOTE! Mismatch between audio trigger log files and audio triggers decoded from recording at position ')
                    disp('trigger nr:')    
                    e
                    disp('The log file says: ') 
                    audio_trig_logs_bit_codes(e) 
                    disp('But the audio trigger recording says: ')
                    audioTriggers_record
                    disp('And we are at sample position ')
                    audioTriggers(e,4)
                    disp('in the audio_triggers')

                    if ~first_mismatch
                        h = char(audio_trig_logs_bit_codes(e));

                        first_mismatch_info = sprintf('The first mismatch was at trigger nr %d and sample position %d. The log file says %s but the recording says %s.', e, audioTriggers(e,4), h, audioTriggers_record);
                        first_mismatch = true;
                    end 


                end % end if mismatch

            end % for all entries in audio_trig_bit_codes

            if first_mismatch
                disp(first_mismatch_info);
            end

        end % end if comparing to audio trigger logs

        % I then extract only the stimuli triggers:
        stimAudioTriggers = array2table(audioTriggers(audioTriggers(:,1)<=108,:),'VariableNames',{'Triggers' 'TimeStamp_ms' 'TriggerDuration_ms' 'TimeStamp_sample_nr' 'TriggerDuration_sample_nr'  'BitCode'});

        % I then compute the diff to get the
        % I can then compare the timestamps:
        stimLogs = cleanLogs(ismember(cleanLogs.eventType,'Stimulus'),:);
        diffAudioLog = diff(stimLogs.time)*1000;
        diffAudioSig = diff(stimAudioTriggers.TimeStamp_ms);

        if COMPARE_AUDIO_T0_TRIGGER_LOGS
            figure
            plot(diffAudioLog);
            hold on;
            plot(diffAudioSig);
            ylabel('Delta t between triggers [ms]', 'FontSize', font_size);
            xlabel('Trial', 'FontSize', font_size);
            title('Interval between triggers, Audio vs full log files ', 'FontSize', font_size)
            legend({'Logs' 'Audio'}, 'FontSize', font_size);
        else

            % If things are misaligned, you will want to remove entries from the
            % log. Please enter 
            happy = 0;
            % Plotting the intervals:;
            while ~happy
                figure
                plot(diffAudioLog)
                hold on
                plot(diffAudioSig)
                legend({'Logs' 'Audio'}, 'FontSize', font_size)
                title('Audio vs log files interval between stimuli triggers',  'FontSize', font_size)
                ylabel('Delta t between triggers [ms]', 'FontSize', font_size);
                xlabel('Trial', 'FontSize', font_size);
                happyInput = input('Are things aligned? y/n [y]:','s');
                if strcmp(happyInput,'y')
                    happy = 1;
                elseif strcmp(happyInput,'n')
                    idxMisaligned = input('What is the index of the first misaligned sample in the audio signal? ');
                    idxPlusMin = input('Is there a sample too much or too few in the audio signal? [p for too many or m for too few]','s');
                    if strcmp(idxPlusMin,'p')
                        stimAudioTriggers = [stimAudioTriggers(1:idxMisaligned-1,:); stimAudioTriggers(idxMisaligned+1:end,:)];
                        diffAudioSig = diff(stimAudioTriggers.TimeStamp_ms);
                    elseif strcmp(idxPlusMin,'m')
                        stimAudioTriggers = [stimAudioTriggers(1:idxMisaligned,:); stimAudioTriggers(idxMisaligned:end,:)];
                        diffAudioSig = diff(stimAudioTriggers.TimeStamp_ms);
                    end
                    close
                end
            end

        end % compare to trigger logs
        % Responses:
        % Checking the responses:
        RespAudioTriggers = array2table(audioTriggers(audioTriggers(:,1) == 110,:),'VariableNames',{'Triggers' 'TimeStamp_ms' 'TriggerDuration_ms' 'TimeStamp_sample_nr' 'TriggerDuration_sample_nr'  'BitCode'});

        % I then compute the diff to get the
        % I can then compare the timestamps:
        RespLogs = cleanLogs(ismember(cleanLogs.eventType,'Response'),:);

        % Plotting the diffs:
        figure
        plot(diff(RespLogs.time)*1000)
        hold on
        plot(diff(RespAudioTriggers.TimeStamp_ms))
        legend({'Logs' 'Audio'}, 'FontSize', font_size)
        title('Audio vs log files interval between reponse triggers',  'FontSize', font_size)
        ylabel('Delta t between triggers [ms]', 'FontSize', font_size);
        xlabel('Response #', 'FontSize', font_size);


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
            disp(stimAudioTriggers.TimeStamp_ms(a))
        else
            disp('Your log and audio are fully consistent: things work fine')
        end
        
    %end % for participants
end % if audio

%% Comparing photo and audio timeStamps:
% This enables the quantification of audio delays and jitters:
if AUDIO && PHOTODIODE
    
    % Now, since the triggers are recorded with the same device, we can compare the time stamps directly. 
    % First we need to select the stimulus photodiode stamps by taking every third photodiode time stamp 
    % (corresponding to stimuli, since the photo trigger also flashes for onset of jitters and ITIs):
    StimPhotodiodeOnset = photoDiodeOnset(1,1:3:end-2);

    % First plot them to make sure everything looks fine
    figure
    %plot(PhotodiodeSigBin) 
    
    scatter(StimPhotodiodeOnset*(1000/signalSR),ones(length(StimPhotodiodeOnset),1)+0.1)
    hold on;
    scatter(stimAudioTriggers.TimeStamp_ms,ones(height(stimAudioTriggers),1))
    legend({'Photodiode' 'Audio'}, 'FontSize', font_size);
    title(sprintf('Photodiode onsets vs audio trigger onsets.'), 'FontSize', font_size);
    xlabel('time [ms]', 'FontSize', font_size);
    ylim([0 3]);
    
    if VERBOSE
        disp('Nr of detected photodiode flashed for stimuli: ')
        size(StimPhotodiodeOnset,2)
        size(stimAudioTriggers.TimeStamp_sample_nr)
    end
    % I can now compare them:

    PhotoVsAudio = stimAudioTriggers.TimeStamp_ms - (StimPhotodiodeOnset)';
    
    audio_delay_hists(PhotoVsAudio, participantPaths{1});
   

end


%% Comparing the logged duration vs the expected duration:
% This is the script written by Katarina, checking the timestamps from the
% log file against the expected duration

%if LOG_DURATIONS
%    disp('Checking log durations');
%    ECoG_Check_diff_between_logged_and_planned_durations(fullLogs, 'Full logs', (1/signalSR)*1000, participantPaths{1});
%    %ECoG_Check_diff_between_logged_and_planned_durations(table2cell(fullLogs), 'full logs', frameDur, signalSR);
%end


%% Checking trials balance:
if CHECK_TRIAL_BALANCE 
    disp('Checking for trial balance');
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


% Input the different vectors and this function will plot and save your
% several files histograms
function audio_delay_hists(PhotoVsAudio, plot_suffix)
    global save_plots font_size 
    

    figure()
    h = histogram(PhotoVsAudio);
    xlabel('Delay [ms]', 'FontSize', font_size);
    ylabel('Counts', 'FontSize', font_size);
    title('Distribution of audio delays (photodiode as reference) [ms]', 'FontSize', font_size)
    hold on;
    
    
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
    
    meanDelayMessage = sprintf('Average: %s',num2str(meanAudioDelay, 3));
    stdDelayMessage = sprintf('Standard error: %s',num2str(stdAudioDelay,3));
    delaysPrctile = sprintf('95 percent of the audio jitters are found between %d and %dms around the mean delay',y);
    %disp(meanDelayMessage)
    %disp(stdDelayMessage)
    %disp(delaysPrctile)
    
    posx = 0.5*(min(PhotoVsAudio) + (max(PhotoVsAudio) - min(PhotoVsAudio)));
    posy = 0.8*max(h.Values);
    text( posx, posy,  meanDelayMessage, 'FontSize', font_size);
    text( posx, posy - 0.1*posy,  stdDelayMessage, 'FontSize', font_size);
    
    if save_plots
        saveas(gcf, sprintf('Audio_trigger_delay_stimuli_%s.png', plot_suffix));
    end    

end