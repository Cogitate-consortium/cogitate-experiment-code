% Written by Katarina Bendtz and Alex LePauvre, 2020.

function [stim_dur_inacc_percent_1_frame, jitter_dur_inacc_percent_1_frame, StimDur, StimDurAccuracy, JitterDur, JitterDurAccuracy, StimDurAccuracy_center, StimDurAccuracy_right, StimDurAccuracy_left, StimDurAccuracy_face, StimDurAccuracy_object, StimDurAccuracy_letter, StimDurAccuracy_false, StimDurAccuracy_05, StimDurAccuracy_10, StimDurAccuracy_15, StimDurAccuracy_target, StimDurAccuracy_non_target, StimDurAccuracy_irrelevant] = ECoG_Check_diff_between_logged_and_planned_durations(miniBlocks_table, time_stamp_type, frame_dur, plot_suffix)
global font_size save_plots signalSR show_only_plots_that_are_not_saved
% Part of the code needs the table format and another part the 
% cell format
MiniBlocks = table2cell(miniBlocks_table);

%% Checking the timings:

% Getting the duration of the stimuli:
% I first remove the responses and the save events, because they might
% come
% inbetween and we don't need them
eventType_column_ind = 14;
% We first creates an array of bools where the indices correspond to the indicides
% where the events are reponse or save (0) and not (1)
NoSaveNoResp_ind_bools = ~ismember([MiniBlocks(:,eventType_column_ind)],'Response') & ~ismember([MiniBlocks(:,eventType_column_ind)],'Save');
NoSaveNoResp_inds = find(NoSaveNoResp_ind_bools == 1);
miniBlockNoSaveNoResp = MiniBlocks(NoSaveNoResp_inds,:);

% I then get the indices of the stimulus presentation begining
idxStimulus = find(ismember([miniBlockNoSaveNoResp(:,eventType_column_ind)],'Stimulus'));

% To get the duration of the presentation, we need to subtract the
% timestamp of the begining of the stimulus presentation to the timestamp
% of the begining of the fixation. Since I removed the responses, the
% fixation always directly follows the stimulus, so I can do it as follows:
time_column_ind = 13;
% The cell2mat converts the cells to integers
StimDur = cell2mat(miniBlockNoSaveNoResp(idxStimulus+1,time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(idxStimulus,time_column_ind));

% Histogram plotted in ECoG_hists_and_stats_durations

% But then, there is something else we want to make sure: do they last as
% long as what was planned?
% To get this information, I subtract the planned duration to the actual
% duration, and I make a scatter plot of that:
planned_duration_column_ind = 9;
StimDurAccuracy = cell2mat(miniBlockNoSaveNoResp(idxStimulus+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(idxStimulus,time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(idxStimulus,planned_duration_column_ind));
% Convert back to ms
StimDurAccuracy = StimDurAccuracy*1000;

% We then plot it:
if show_only_plots_that_are_not_saved
    figure('visible', 'off');
else
    figure();
end
title_str = sprintf('Difference between stimulus durations [ms] \n as measured by %s, and planned durations', time_stamp_type );
title(title_str, 'FontSize', font_size);
xlabel('Trial nr', 'FontSize', font_size)
ylabel('Time difference (presented - planned) [ms]', 'FontSize', font_size)
hold on;
% Print maximum deviaiton
txt1 = sprintf('Max inaccuracy = %s ms', num2str(max(StimDurAccuracy),2));
txt2 = sprintf('Min inaccuracy = %s ms', num2str(min(StimDurAccuracy),2));
% print average deviation
txt3 = sprintf('Avg inaccuracy = %s ms', num2str(mean(StimDurAccuracy),2));

% Adjusting for the resolution due to the sampling frequency
sampling_res = 1000./signalSR;
ratio_above_one_frame = length(find(abs(StimDurAccuracy) > (frame_dur + sampling_res + 1) ==1))/double(length(StimDurAccuracy));

below_one_frame_inds = abs(StimDurAccuracy) < (frame_dur + sampling_res + 1)  & abs(StimDurAccuracy) > sampling_res + 1;
ratio_one_frame = length(find(below_one_frame_inds ==1))/double(length(StimDurAccuracy));
stim_dur_inacc_percent_1_frame = ratio_one_frame*100;
txt4 = sprintf('Inaccurary of 1 frame: %s %%', num2str(stim_dur_inacc_percent_1_frame,3));
txt5 = sprintf('Inaccuracy of > 1 frame: %s %%', num2str(ratio_above_one_frame*100,3));

posx1 = 0.5*length(StimDurAccuracy);
%posy1 = mean(StimDurAccuracy) + 0.5*(max(StimDurAccuracy) - mean(StimDurAccuracy));
posy1 = 0.5*(frame_dur);
step = 0.25;
text( posx1, posy1, txt1, 'FontSize', font_size);
text( posx1, posy1 - step*posy1, txt2, 'FontSize', font_size);
text( posx1, posy1 - 10*step*posy1, txt3, 'FontSize', font_size);
text( posx1, posy1 -11*step*posy1, txt4, 'FontSize', font_size);
text( posx1, posy1 - 12*step*posy1, txt5, 'FontSize', font_size);



yline(frame_dur,'r');
yline(-frame_dur,'r');
hold on;
scatter(1:length(StimDurAccuracy),StimDurAccuracy);
if save_plots
    saveas(gcf, sprintf('Duration_accuracy_stimuli_from_%s_%s.png', time_stamp_type, plot_suffix));
end



% Let's look at the jitters as well:
% First I
% compute the observed jitter. It is a bit trickier because inbetween
% blocks, there are the longer breaks.
% BUT: in the original miniblock cell, at the end of the jitter at the end
% of a miniblock, saving occurs. So I take it, remove the responses and
% then I can do the same as before:
% Then, I compute the observed jitter:
% Finding the change of miniBlock, because there we have a pause, so the
% timings will be off if we look at those:

idxminiBlkBegin = find(diff(miniBlocks_table.miniBlock) == 1) + 1;
% Now I get the intersection between that and the stimuli onsets:
[~, ~, firstStimIdx] = intersect(idxminiBlkBegin,idxStimulus);
lastStimIdx = firstStimIdx - 1;

% I now remove the last stimuli of a miniblock, because I can't compute
% the jitter of those:
jitterStimIdx = idxStimulus;
jitterStimIdx(lastStimIdx) = [];

JitterDur = miniBlocks_table.time(jitterStimIdx(1:end-1)+3) - miniBlocks_table.time(jitterStimIdx(1:end-1)+2);
format long


% Histogram of the jitters plotted in ECoG_hists_and_stats_durations



% Again, we want to make sure the jitters are what they should be:
JitterDurAccuracy = (miniBlocks_table.time(jitterStimIdx(1:end-1)+3) - miniBlocks_table.time(jitterStimIdx(1:end-1)+2) - ...
    miniBlocks_table.plndJitterDur(jitterStimIdx(1:end-1)))*1000;

%disp('miniBlocks_table.time(jitterStimIdx(1:end-1)+3):')
%miniBlocks_table.time(jitterStimIdx(1:end-1)+3)

%disp('miniBlocks_table.time(jitterStimIdx(1:end-1)+2):')
%miniBlocks_table.time(jitterStimIdx(1:end-1)+2)

%disp('miniBlocks_table.time(jitterStimIdx(1:end-1))*1000):')
%miniBlocks_table.plndJitterDur(jitterStimIdx(1:end-1))*1000

%disp('JitterDurAccuracy')
%JitterDurAccuracy




PercentSkippedFrameJitters = sum(JitterDurAccuracy>=frame_dur)/double(length(JitterDurAccuracy));

% Finally, making a scatter plot of the jitters:
if show_only_plots_that_are_not_saved
    figure('visible', 'off');
else
    figure();
end
scatter(1:length(JitterDurAccuracy),JitterDurAccuracy)
yline(frame_dur,'r');
yline(-frame_dur,'r');
xlabel('Trial nr', 'FontSize', font_size);
title_str = sprintf('Difference between jitter durations [ms] \n as measured by %s, and the planned durations', time_stamp_type );
title(title_str, 'FontSize', font_size);
ylabel('Time difference (presented - planned) [ms]', 'FontSize', font_size)
% Adjusting for the resolution due to the sampling frequency
sampling_res = 1000./signalSR;
ratio_above_one_frame = length(find(abs(JitterDurAccuracy) > (frame_dur + sampling_res + 1) ==1))/double(length(JitterDurAccuracy));

% Print maximum deviaiton
txt1 = sprintf('Max inaccuracy = %s ms', num2str(max(JitterDurAccuracy),2));
txt2 = sprintf('Min inaccuracy = %s ms', num2str(min(JitterDurAccuracy),2));
% print average deviation
txt3 = sprintf('Avg inaccuracy = %s ms', num2str(mean(JitterDurAccuracy),2));

below_one_frame_inds = abs(JitterDurAccuracy) < (frame_dur + sampling_res + 1)  & abs(JitterDurAccuracy) > sampling_res + 1;
ratio_one_frame = length(find(below_one_frame_inds ==1))/double(length(JitterDurAccuracy));
jitter_dur_inacc_percent_1_frame = ratio_one_frame*100;
txt4 = sprintf('Inaccurary of 1 frame: %s %%', num2str(jitter_dur_inacc_percent_1_frame ,3));
txt5 = sprintf('Inaccuracy of > 1 frame: %s %%', num2str(ratio_above_one_frame*100,3));

posx1 = 0.5*length(JitterDurAccuracy);
%posy1 = mean(JitterDurAccuracy) + 0.5*(max(JitterDurAccuracy) - mean(JitterDurAccuracy));
posy1 = 0.5*(frame_dur);
step = 0.2;
text( posx1, posy1, txt1, 'FontSize', font_size);
text( posx1, posy1 - step*posy1, txt2, 'FontSize', font_size);
text( posx1, posy1 - 10*step*posy1, txt3, 'FontSize', font_size);
text( posx1, posy1 -11*step*posy1, txt4, 'FontSize', font_size);
text( posx1, posy1 - 12*step*posy1, txt5, 'FontSize', font_size);

if save_plots
    saveas(gcf, sprintf('Duration_accuracy_jitters_from_%s_%s.png', time_stamp_type, plot_suffix));
end

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


% We now want to check whether the delay is systematic in any way wrt relevance, orientation, category or duration

% Coding of the stimuli
% 1st digit = stimulus type (1 = face; 2 = object; 3 = letter; 4 = false font)
% 2nd digit = stimulus orientation (1 = center; 2 = left; 3 = right)
% 3rd & 4th digits = stimulus id (1...20; for faces 1...10 is male, 11...20 is female)
% e.g., "1219" = 1 is face, 2 is left orientation and 19 is a female stimulus #19
% The decimal is for duration



% Make an accuracy plot comparing all different orientations, categories,
% relevances and durations

% Find indices for ech ori:
ind_left = [];
ind_right = [];
ind_center = [];

ind_face = [];
ind_object = [];
ind_letter = [];
ind_false = [];

ind_target = [];
ind_non_target = [];
ind_irrelevant = [];

ind_05 = [];
ind_10 = [];
ind_15 = [];


% This is the column where the stimulus codes are given
event_column_ind = 12;
targ1_column_ind = 7;
targ2_column_ind = 8;

%stim_codes = cell2mat(miniBlockNoSaveNoResp(idxStimulus ,event_column_ind));
%stim_codes
s = size(idxStimulus,1);

for i = 1:s
    
    ind = idxStimulus(i);
    stim_code =cell2mat(miniBlockNoSaveNoResp(ind ,event_column_ind));
    stim_code_str = int2str(stim_code);

    
    % Orientation
    stim_code_ori_str = stim_code_str(2);
 
    % Category
    stim_code_cat_str = stim_code_str(1);

    
    % Relevance
    % First get the target stim_code:
    stim_code_targ1 = cell2mat(miniBlockNoSaveNoResp(ind ,targ1_column_ind));
    stim_code_targ2 = cell2mat(miniBlockNoSaveNoResp(ind ,targ2_column_ind));

    
    stim_code_targ1_str = int2str(stim_code_targ1);
    stim_code_targ2_str = int2str(stim_code_targ2);
   
    
    stim_code_targ1_cat_str = stim_code_targ1_str(1);
    stim_code_targ2_cat_str = stim_code_targ2_str(1);
    
    % For comparison to targets, get rid of second digit denoting orientaiton before comparing to
    % target code
    stim_code_str_no_ori = strcat(stim_code_str(1), '0', stim_code_str(3:4));
    
    % Orientation inds
    if stim_code_ori_str == '1'
      
        ind_center = [ind_center ind];
    elseif stim_code_ori_str == '2'
      
        ind_left = [ind_left ind];
    elseif stim_code_ori_str == '3'
        
        ind_right = [ind_right ind];
    end
    
     % Category inds
     if stim_code_cat_str == '1'
         ind_face = [ind_face ind];
     elseif stim_code_cat_str == '2'
         ind_object = [ind_object ind];
     elseif stim_code_cat_str == '3'
         ind_letter = [ind_letter ind];
     else
         ind_false = [ind_false ind];
     end
    
    % Relevance inds 
    
    if ( strcmp(stim_code_str_no_ori,stim_code_targ1_str) || strcmp(stim_code_str_no_ori,stim_code_targ2_str) )
       
        ind_target = [ind_target ind];
    elseif ( (stim_code_cat_str == stim_code_targ1_cat_str) || (stim_code_cat_str == stim_code_targ2_cat_str) )
        ind_non_target = [ind_non_target ind];
    else
        ind_irrelevant = [ind_irrelevant ind];
    end
   
    % Duration inds
    planned_dur = cell2mat(miniBlockNoSaveNoResp(ind, planned_duration_column_ind));
 
    if planned_dur < 0.6
        ind_05 = [ind_05 ind];
    elseif planned_dur < 1.1
        ind_10 = [ind_10 ind];  
    else
        ind_15 = [ind_15 ind]; 
    end
          
end % for

% Sanity checks:
%difference = size(idxStimulus,1) - (size(ind_center,2) + size(ind_left,2) + size(ind_right,2));
%difference = size(idxStimulus,1) - (size(ind_05,2) + size(ind_10,2) + size(ind_15,2));
%difference = size(idxStimulus,1) - (size(ind_face,2) + size(ind_object,2) + size(ind_letter,2) + size(ind_false,2));
%difference = size(idxStimulus,1) - (size(ind_target,2) + size(ind_non_target,2) + size(ind_irrelevant,2));

    %%%%%%%%%% Duration accuracy

    % Orientation

    StimDurAccuracy_left = ( (cell2mat(miniBlockNoSaveNoResp(ind_left+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_left,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_left,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_right =( (cell2mat(miniBlockNoSaveNoResp(ind_right+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_right,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_right,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_center =( (cell2mat(miniBlockNoSaveNoResp(ind_center+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_center,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_center,planned_duration_column_ind)) ) * 1000;

    % Category

    StimDurAccuracy_face =( (cell2mat(miniBlockNoSaveNoResp(ind_face+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_face,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_face,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_object =( (cell2mat(miniBlockNoSaveNoResp(ind_object+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_object,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_object,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_letter =( (cell2mat(miniBlockNoSaveNoResp(ind_letter+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_letter,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_letter,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_false =( (cell2mat(miniBlockNoSaveNoResp(ind_false+1, time_column_ind)) - cell2mat(miniBlockNoSaveNoResp(ind_false,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_false,planned_duration_column_ind)) ) * 1000;

    % Relevance

    StimDurAccuracy_target =( (cell2mat(miniBlockNoSaveNoResp(ind_target+1, time_column_ind))- cell2mat(miniBlockNoSaveNoResp(ind_target,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_target,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_non_target =( (cell2mat(miniBlockNoSaveNoResp(ind_non_target+1, time_column_ind))- cell2mat(miniBlockNoSaveNoResp(ind_non_target,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_non_target,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_irrelevant =( (cell2mat(miniBlockNoSaveNoResp(ind_irrelevant+1, time_column_ind))- cell2mat(miniBlockNoSaveNoResp(ind_irrelevant,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_irrelevant,planned_duration_column_ind)) ) * 1000;

    % Duration

    StimDurAccuracy_05 =( (cell2mat(miniBlockNoSaveNoResp(ind_05+1, time_column_ind))- cell2mat(miniBlockNoSaveNoResp(ind_05,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_05,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_10 =( (cell2mat(miniBlockNoSaveNoResp(ind_10+1, time_column_ind))- cell2mat(miniBlockNoSaveNoResp(ind_10,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_10,planned_duration_column_ind)) ) * 1000;

    StimDurAccuracy_15 =( (cell2mat(miniBlockNoSaveNoResp(ind_15+1, time_column_ind))- cell2mat(miniBlockNoSaveNoResp(ind_15,time_column_ind))) - ...
        cell2mat(miniBlockNoSaveNoResp(ind_15,planned_duration_column_ind)) ) * 1000;

     
    
end






