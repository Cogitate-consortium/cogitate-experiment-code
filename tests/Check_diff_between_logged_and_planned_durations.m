% Checking trials number compliance to the pre registration:


function Check_diff_between_logged_and_planned_durations(MiniBlocks)
% close all

% % Getting the name of the file:
% filesList = dir(fullfile(dataPath,'*RawDur*.mat'));
% 
% % Loading the data
% miniBlocks = [];
% for i = 1:length(filesList)
%     load(filesList(i).name)
%     miniBlocks = [miniBlocks; MiniBlocks];
% end
% 
% clear MiniBlocks
% MiniBlocks = miniBlocks;


%% Checking the timings:

% There are several things to look at for the timings:
% First, did the stimuli lasted for the duration they were supposed to.
% There are several ways to look at that:
% First, I make a histogram of the duration of all stimuli, there should be
% three bars: 1 at 0.5, 1 at 1 and 1 a 1.5
% Getting the duration of the stimuli:
% I first remove the responses and the save events, because they might come
% inbetween and we don't need them
eventType_column_ind = 13;
% Creates an array of bools where the indices correspond to the indicides
% where the events are reponse or save (0) and not (1)
NoSaveNoResp_ind_bools = ~ismember([MiniBlocks(:,eventType_column_ind)],'Response') & ~ismember([MiniBlocks(:,eventType_column_ind)],'Save');
NoSaveNoResp_inds = find(NoSaveNoResp_ind_bools == 1);
miniBlockNoSaveNoResp = MiniBlocks(NoSaveNoResp_inds,:);


% I then get the indices of the stimulus presentation begining
idxStimulus = find(ismember([miniBlockNoSaveNoResp(:,eventType_column_ind)],'Stimulus'));



% To get the duration of the presentation, one need to subtract the
% timestamp of the begining of the stimulus presentation to the timestamp
% of the begining of the fixation. Since I removed the responses, the
% fixation always directly follows the stimulus, so I can do it as follows:
time_column_ind = 12;
% The cell2mat converts the cells to integers
StimDur = [cell2mat(miniBlockNoSaveNoResp(idxStimulus+1,time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(idxStimulus,time_column_ind))];


% I then plot the histogram

figure
title('Logged durations [s]');
xlabel('Durations [s]');
ylabel('Frequency');
hold on;
histogram(StimDur, 500)
% TODO: Why sometimes negative?
% But then, there is something else we want to make sure: do they last as
% long as what was planned?
% To get this information, I subtract the planned duration to the actual
% duration, and I make a scatter plot of that:
planned_duration_column_ind = 8;
StimDurAccuracy = ([cell2mat(miniBlockNoSaveNoResp(idxStimulus+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(idxStimulus,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(idxStimulus,planned_duration_column_ind))];
% Ithen plot it:
figure
title('Difference between planned and logged stimulus durations [s] for all stimuli')
xlabel('Stim presentation order')
ylabel('Time difference (planned - logged) [s]')
hold on;
% Print maximum deviaiton
txt1 = sprintf('%s %f', 'Maximum diff = ', max(StimDurAccuracy));
txt2 = sprintf('%s %f', 'Minumum diff = ', min(StimDurAccuracy));
% print average deviation
txt3 = sprintf('%s %f', 'Average diff = ', mean(StimDurAccuracy));


posx1 = 0.75*length(StimDurAccuracy);
posy1 = mean(StimDurAccuracy) + 0.5*(max(StimDurAccuracy) - mean(StimDurAccuracy));

text( posx1, posy1, txt1);
text( posx1, posy1 - 0.05*posy1, txt2);
text( posx1, posy1 - 0.1*posy1, txt3);
hold on;
scatter(1:length(StimDurAccuracy),StimDurAccuracy)


% TODO print the percentage that are above 16 ms


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
event_column_ind = 11;
targ1_column_ind = 6;
targ2_column_ind = 7;

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
diff = size(idxStimulus,1) - (size(ind_center,2) + size(ind_left,2) + size(ind_right,2));


diff = size(idxStimulus,1) - (size(ind_05,2) + size(ind_10,2) + size(ind_15,2));



diff = size(idxStimulus,1) - (size(ind_face,2) + size(ind_object,2) + size(ind_letter,2) + size(ind_false,2));



diff = size(idxStimulus,1) - (size(ind_target,2) + size(ind_non_target,2) + size(ind_irrelevant,2));





%%%%%%%%%% Duration accuracy

% Orientation

StimDurAccuracy_left = ([cell2mat(miniBlockNoSaveNoResp(ind_left+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_left,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_left,planned_duration_column_ind))];

StimDurAccuracy_right = ([cell2mat(miniBlockNoSaveNoResp(ind_right+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_right,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_right,planned_duration_column_ind))];

StimDurAccuracy_center = ([cell2mat(miniBlockNoSaveNoResp(ind_center+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_center,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_center,planned_duration_column_ind))];

% Category

StimDurAccuracy_face = ([cell2mat(miniBlockNoSaveNoResp(ind_face+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_face,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_face,planned_duration_column_ind))];

StimDurAccuracy_object = ([cell2mat(miniBlockNoSaveNoResp(ind_object+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_object,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_object,planned_duration_column_ind))];

StimDurAccuracy_letter = ([cell2mat(miniBlockNoSaveNoResp(ind_letter+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_letter,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_letter,planned_duration_column_ind))];

StimDurAccuracy_false = ([cell2mat(miniBlockNoSaveNoResp(ind_false+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_false,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_false,planned_duration_column_ind))];

% Relevance

StimDurAccuracy_target = ([cell2mat(miniBlockNoSaveNoResp(ind_target+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_target,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_target,planned_duration_column_ind))];

StimDurAccuracy_non_target = ([cell2mat(miniBlockNoSaveNoResp(ind_non_target+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_non_target,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_non_target,planned_duration_column_ind))];

StimDurAccuracy_irrelevant = ([cell2mat(miniBlockNoSaveNoResp(ind_irrelevant+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_irrelevant,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_irrelevant,planned_duration_column_ind))];

% Duration

StimDurAccuracy_05 = ([cell2mat(miniBlockNoSaveNoResp(ind_05+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_05,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_05,planned_duration_column_ind))];

StimDurAccuracy_10 = ([cell2mat(miniBlockNoSaveNoResp(ind_10+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_10,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_10,planned_duration_column_ind))];

StimDurAccuracy_15 = ([cell2mat(miniBlockNoSaveNoResp(ind_15+1, time_column_ind))] - [cell2mat(miniBlockNoSaveNoResp(ind_15,time_column_ind))]) - ...
    [cell2mat(miniBlockNoSaveNoResp(ind_15,planned_duration_column_ind))];


%%%%%%%%% plotting

color1 = '#000000';
color2 = '#ACDBE8';
color3 = '#FFFAA7';
color4 = '#FFD3A7';


% Orientation
figure
title('Difference between planned and logged stimulus durations [s] for different stimuli orientations');
xlabel('Difference [s]');
ylabel('Frequency');
hold on;
histogram(StimDurAccuracy_center,20,  'FaceColor', color1)
hold on
histogram(StimDurAccuracy_right,20,  'FaceColor', color2)
hold on
histogram(StimDurAccuracy_left,20, 'FaceColor', color3)

% Add info about mean and standard deviation:
legend_left = sprintf('%s %f %s %f %s', 'Left (avg = ', mean(StimDurAccuracy_left), ', std = ', std(StimDurAccuracy_left), ')' );
legend_right = sprintf('%s %f %s %f %s', 'Right (avg = ', mean(StimDurAccuracy_right), ', std = ', std(StimDurAccuracy_right), ')' );
legend_center = sprintf('%s %f %s %f %s', 'Center (avg = ', mean(StimDurAccuracy_center), ', std = ', std(StimDurAccuracy_center), ')' );

legend(legend_left, legend_right, legend_center);

% Category

figure
title('Difference between planned and logged stimulus durations [s] for different stimuli cetegories');
xlabel('Difference [s]');
ylabel('Frequency');
hold on;
histogram(StimDurAccuracy_face,20,  'FaceColor', color1)
hold on
histogram(StimDurAccuracy_object,20,  'FaceColor', color2)
hold on
histogram(StimDurAccuracy_letter,20, 'FaceColor', color3)
hold on
histogram(StimDurAccuracy_false,20, 'FaceColor', color4)

% Add info about mean and standard deviation:
legend_face = sprintf('%s %f %s %f %s', 'Face (avg = ', mean(StimDurAccuracy_face), ', std = ', std(StimDurAccuracy_face), ')' );
legend_object = sprintf('%s %f %s %f %s', 'Object (avg = ', mean(StimDurAccuracy_object), ', std = ', std(StimDurAccuracy_object), ')' );
legend_letter = sprintf('%s %f %s %f %s', 'Letter (avg = ', mean(StimDurAccuracy_letter), ', std = ', std(StimDurAccuracy_letter), ')' );
legend_false = sprintf('%s %f %s %f %s', 'False (avg = ', mean(StimDurAccuracy_false), ', std = ', std(StimDurAccuracy_false), ')' );

legend(legend_face, legend_object, legend_letter, legend_false);

% Relevance

figure
title('Difference between planned and logged stimulus durations [s] for different stimuli relevances');
xlabel('Difference [s]');
ylabel('Frequency');
hold on;
histogram(StimDurAccuracy_target,20,  'FaceColor', color1)
hold on
histogram(StimDurAccuracy_non_target,20,  'FaceColor', color2)
hold on
histogram(StimDurAccuracy_irrelevant,20, 'FaceColor', color3)

% Add info about mean and standard deviation:
legend_target = sprintf('%s %f %s %f %s', 'target (avg = ', mean(StimDurAccuracy_target), ', std = ', std(StimDurAccuracy_target), ')' );
legend_irrelevant = sprintf('%s %f %s %f %s', 'irrelevant (avg = ', mean(StimDurAccuracy_irrelevant), ', std = ', std(StimDurAccuracy_irrelevant), ')' );
legend_non_target = sprintf('%s %f %s %f %s', 'non-target (avg = ', mean(StimDurAccuracy_non_target), ', std = ', std(StimDurAccuracy_non_target), ')' );


legend( legend_target, legend_irrelevant, legend_non_target);

% Duration

figure
title('Difference between planned and logged stimulus durations [s] for different stimuli durations');
xlabel('Difference in [s]');
ylabel('Frequency');
hold on;
histogram(StimDurAccuracy_05,20,  'FaceColor', color1)
hold on
histogram(StimDurAccuracy_10,20, 'FaceColor', color3)
hold on
histogram(StimDurAccuracy_15,20,  'FaceColor', color2)


% Add info about mean and standard deviation:
legend_05 = sprintf('%s %f %s %f %s', '0.5 s (avg = ', mean(StimDurAccuracy_05), ', std = ', std(StimDurAccuracy_05), ')' );
legend_10 = sprintf('%s %f %s %f %s', '1.0 s (avg = ', mean(StimDurAccuracy_10), ', std = ', std(StimDurAccuracy_10), ')' );
legend_15 = sprintf('%s %f %s %f %s', '1.5 s (avg = ', mean(StimDurAccuracy_15), ', std = ', std(StimDurAccuracy_15), ')' );

legend(legend_05, legend_10, legend_15);

%%%%%%%%%%%% Check significance with anova

% Orientation

sc = size(StimDurAccuracy_center,1);
sl = size(StimDurAccuracy_left,1);
sr = size(StimDurAccuracy_right,1);

groups_ori = {};
for i = 1:sc
    groups_ori{i} = 'center';
end

for i = (1 + sc):(sc + sl)
    groups_ori{i} = 'left';
end

for i = (1+ sc + sl):(sc + sl + sr)
    groups_ori{i} = 'right';
end

data_vector_ori = [StimDurAccuracy_center' StimDurAccuracy_right' StimDurAccuracy_left'];

anova_test = anova1(data_vector_ori, groups_ori );


%anova_test

% Category

sf = size(StimDurAccuracy_face,1);
so = size(StimDurAccuracy_object,1);
sl = size(StimDurAccuracy_letter,1);
sfalse = size(StimDurAccuracy_false,1);


groups_cat = {};
for i = 1:sf
    groups_cat{i} = 'face';
end

for i = (1 + sf):(sf + so)
    groups_cat{i} = 'object';
end

for i = (1+ sf + so):(sf + so + sl)
    groups_cat{i} = 'letter';
end

for i = (1+ sf + so + sl):(sf + so+ sl + sfalse)
    groups_cat{i} = 'false';
end

data_vector_cat = [StimDurAccuracy_face' StimDurAccuracy_object' StimDurAccuracy_letter' StimDurAccuracy_false'];
anova_test = anova1(data_vector_cat, groups_cat );




% Relevance

st = size(StimDurAccuracy_target,1);
sn = size(StimDurAccuracy_non_target,1);
si = size(StimDurAccuracy_irrelevant,1);

groups_rel = {};
for i = 1:st
    groups_rel{i} = 'target';
end

for i = (1 + st):(st + sn)
    groups_rel{i} = 'non_target';
end

for i = (1+ st + sn):(st + sn + si)
    groups_rel{i} = 'irrelevant';
end

data_vector_rel = [StimDurAccuracy_target' StimDurAccuracy_non_target' StimDurAccuracy_irrelevant'];
anova_test = anova1(data_vector_rel, groups_rel );


% Duration

s05 = size(StimDurAccuracy_05,1);
s10 = size(StimDurAccuracy_10,1);
s15 = size(StimDurAccuracy_15,1);

groups_dur = {};
for i = 1:s05
    groups_dur{i} = '0.5';
end

for i = (1 + s05):(s05 + s10)
    groups_dur{i} = '1.0';
end

for i = (1+ s05 + s10):(s05 + s10 + s15)
    groups_dur{i} = '1.5';
end

data_vector_dur = [StimDurAccuracy_05' StimDurAccuracy_10' StimDurAccuracy_15'];
anova_test = anova1(data_vector_dur, groups_dur);

     
    
end






