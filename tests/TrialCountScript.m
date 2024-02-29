

% Checking trials number compliance to the pre registration:

ECoG = 0;

% Loading the data:
load('data/Exp1_ResultsS999_b1_999-04-Mar-2020.mat')

trial_mod = 1;
if ECoG
    trial_mod = 0.5;
end 

% In the mini block cell, you have several for each trials, but we only
% need one. The extra lines are making things more complicated and are
% useless in the present script, so I get rid of every lines we don't need

miniBlocksNew = miniBlocks(ismember([miniBlocks{:,29}],'Stimulus'),:);


%% Counting the number of stimuli of the different categories:

% Counting the overall number of stimuli in each categories
% Following Yoav stimuli ID coding, if the first digit of the stimulus ID
% is a 1 then the stimulus is a face
Categories.Faces.All = sum(floor([miniBlocksNew{:,27}]./1000) == 1);

display(Categories.Faces.All, 'All faces');


if Categories.Faces.All < 320*trial_mod
    
    display(Categories.Faces.All, 'The number of center faces might be too small');
    
end % if

% If it is a 2 then it is an object
Categories.Objects.All = sum(floor([miniBlocksNew{:,27}]./1000) == 2);
display(Categories.Objects.All, 'All objects (should be 360)');
% If it is a 3 then it is a letter
Categories.Letters.All = sum(floor([miniBlocksNew{:,27}]./1000) == 3);
display(Categories.Letters.All, 'All letter (should be 360)');
% And if it is a 4 it is a false font
Categories.Falses.All = sum(floor([miniBlocksNew{:,27}]./1000) == 4);
display(Categories.Falses.All, 'All falses (should be 360)');

% TODO: Do these need to be balanced?
% Counting the number of stimuli within categories in the different
% orientations
% Again following Yoavs code, if the second digit of the stimulus ID is a
% 1, then the stimulus is in the front view
Categories.Faces.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 1 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
display(Categories.Faces.Center, 'All center faces');

% If it is a 2 then it is in the left view
Categories.Faces.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 1 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
display(Categories.Faces.Left, 'All left faces');
% And if it is a 3 then it is in the right view (I have to check it though,
% I may have mixed up right and left, but that does not matter here)
Categories.Faces.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 1 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 3);
display(Categories.Faces.Right, 'All right faces');

Categories.Letters.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 2 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
display(Categories.Letters.Center, 'All center letters');
Categories.Letters.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 2 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
display(Categories.Letters.Left, 'All left letter');
Categories.Letters.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 2 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 3);
display(Categories.Letters.Right, 'All right letters');

Categories.Objects.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 3 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
display(Categories.Objects.Center, 'All center objects');
Categories.Objects.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 3 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
display(Categories.Objects.Left, 'All left objects');
Categories.Objects.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 3 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
display(Categories.Objects.Right, 'All right objects');

Categories.Falses.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 4 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
display(Categories.Falses.Center, 'All center falses');
Categories.Falses.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 4 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
display(Categories.Falses.Left, 'All left falses');
Categories.Falses.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 4 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 3);
display(Categories.Falses.Right, 'All right falses');

% Now I do the same but for the different task demands:
% For the task demands it is a bit more complicated. For the targets, you
% have to have the first number of the 27th column to be equal to the first
% number of the 22nd OR 23rd column & have the 2 last number of colum 27 to
% be equal to the 2 last numbers of column 22 or 23!

% I am therefore loop through each trial, otherwise I would have to write a
% ridiculously long one line statement:
% So first, I set counters.
TRFace = 0;
TargetFace = 0;
TIFace = 0;

TRObjects = 0;
TargetObjects = 0;
TIObjects = 0;

TRLetters = 0;
TargetLetters = 0;
TILetters = 0;

TRFalse = 0;
TargetFalse = 0;
TIFalse = 0;

% Making the counters for the full combination:
TargetFaceCenter = 0;
TargetFaceLeft = 0;
TargetFaceRight = 0;

TargetObjectCenter = 0;
TargetObjectLeft = 0;
TargetObjectRight = 0;

TargetLetterCenter = 0;
TargetLetterLeft = 0;
TargetLetterRight = 0;

TargetFalseCenter = 0;
TargetFalseLeft = 0;
TargetFalseRight = 0;


TRFaceCenter = 0;
TRFaceLeft = 0;
TRFaceRight = 0;

TRObjectCenter = 0;
TRObjectLeft = 0;
TRObjectRight = 0;

TRLetterCenter = 0;
TRLetterLeft = 0;
TRLetterRight = 0;

TRFalseCenter = 0;
TRFalseLeft = 0;
TRFalseRight = 0;


TIFaceCenter = 0;
TIFaceLeft = 0;
TIFaceRight = 0;

TIObjectCenter = 0;
TIObjectLeft = 0;
TIObjectRight = 0;

TILetterCenter = 0;
TILetterLeft = 0;
TILetterRight = 0;

TIFalseCenter = 0;
TIFalseLeft = 0;
TIFalseRight = 0;

% I then loop through each trials of the miniBlocks cell:
for i = 1:size(miniBlocksNew,1)
    % For each trial I get the ID of the stimulus and of the two targets
    stimID = num2str(miniBlocksNew{i,27});
    TargetsID1 = num2str(miniBlocksNew{i,22});
    TargetsID2 = num2str(miniBlocksNew{i,23});
    % If the stimulus ID has the first digit equal to the first digit of
    % one of the two targets & if the two last digits of the stimulus are
    % the same than one of the two targets, then we have a target stimulus
    if (stimID(1) == TargetsID1(1) && str2double(stimID(3:4)) == str2double(TargetsID1(3:4)))|| ...
            (stimID(1) == TargetsID2(1) && str2double(stimID(3:4)) == str2double(TargetsID2(3:4)))
        % If the first digit of the stimulus is a one, then we have a face,
        % so we have a face target
        if str2double(stimID(1)) == 1
            TargetFace = TargetFace+1 ;
            % The last thing to check is whether the combination of
            % orientation, category and task demands are okay, so this is
            % what is done here:
            if str2double(stimID(2)) == 1
                TargetFaceCenter = TargetFaceCenter+1;
            elseif str2double(stimID(2)) == 2
                TargetFaceLeft = TargetFaceLeft+1;
            elseif str2double(stimID(2)) == 3
                TargetFaceRight = TargetFaceRight+1;
            end                
            % If the first digit is a 2 then we have an object
        elseif str2double(stimID(1)) == 2
            TargetObjects = TargetObjects+1 ;
            if str2double(stimID(2)) == 1
                TargetObjectCenter = TargetObjectCenter+1;
            elseif str2double(stimID(2)) == 2
                TargetObjectLeft = TargetObjectLeft+1;
            elseif str2double(stimID(2)) == 3
                TargetObjectRight = TargetObjectRight+1;
            end
            % 3 for letters
        elseif str2double(stimID(1)) == 3
            TargetLetters = TargetLetters+1 ;
            if str2double(stimID(2)) == 1
                TargetLetterCenter = TargetLetterCenter+1;
            elseif str2double(stimID(2)) == 2
                TargetLetterLeft = TargetLetterLeft+1;
            elseif str2double(stimID(2)) == 3
                TargetLetterRight = TargetLetterRight+1;
            end
            % 4 for false fonts
        elseif str2double(stimID(1)) == 4
            TargetFalse = TargetFalse+1 ;
            if str2double(stimID(2)) == 1
                TargetFalseCenter = TargetFalseCenter+1;
            elseif str2double(stimID(2)) == 2
                TargetFalseLeft =TargetFalseLeft +1;
            elseif str2double(stimID(2)) == 3
                TargetFalseRight = TargetFalseRight+1;
            end
        end
        % If the first digit of the stimulus is the same as one of the
        % targets, but the two last digits are not the same as any of the
        % targets, then it is a task relevant non target
    elseif ((stimID(1) == TargetsID1(1)) && (str2double(stimID(3:4)) ~= str2double(TargetsID1(3:4))) )|| ...
            (stimID(1) == TargetsID2(1) && str2double(stimID(3:4)) ~= str2double(TargetsID2(3:4)))
        if str2double(stimID(1)) == 1
            TRFace = TRFace+1 ;
            if str2double(stimID(2)) == 1
                TRFaceCenter = TRFaceCenter+1;
            elseif str2double(stimID(2)) == 2
                TRFaceLeft = TRFaceLeft+1;
            elseif str2double(stimID(2)) == 3
                TRFaceRight = TRFaceRight+1;
            end
        elseif str2double(stimID(1)) == 2
            TRObjects = TRObjects+1 ;
            if str2double(stimID(2)) == 1
                TRObjectCenter = TRObjectCenter+1;
            elseif str2double(stimID(2)) == 2
                TRObjectLeft = TRObjectLeft+1;
            elseif str2double(stimID(2)) == 3
                TRObjectRight = TRObjectRight+1;
            end
        elseif str2double(stimID(1)) == 3
            TRLetters = TRLetters+1 ;
            if str2double(stimID(2)) == 1
                TRLetterCenter = TRLetterCenter+1;
            elseif str2double(stimID(2)) == 2
                TRLetterLeft = TRLetterLeft+1;
            elseif str2double(stimID(2)) == 3
                TRLetterRight = TRLetterRight+1;
            end
        elseif str2double(stimID(1)) == 4
            TRFalse = TRFalse+1 ;
            if str2double(stimID(2)) == 1
                TRFalseCenter = TRFalseCenter+1;
            elseif str2double(stimID(2)) == 2
                TRFalseLeft = TRFalseLeft+1;
            elseif str2double(stimID(2)) == 3
                TRFalseRight = TRFalseRight+1;
            end
        end
        % Here else would have been sufficient, but I am always a little
        % nervous about not specifying something. So if the first digit is
        % not the same as the first digit of one of the target, then it is
        % a task irrelevant stimulus
    elseif stimID(1) ~= TargetsID1(1) && stimID(1) ~= TargetsID2(1)
        if str2double(stimID(1)) == 1
            TIFace = TIFace+1 ;
            if str2double(stimID(2)) == 1
                TIFaceCenter = TIFaceCenter+1;
            elseif str2double(stimID(2)) == 2
                TIFaceLeft = TIFaceLeft+1;
            elseif str2double(stimID(2)) == 3
                TIFaceRight = TIFaceRight+1;
            end
        elseif str2double(stimID(1)) == 2
            TIObjects = TIObjects+1 ;
            if str2double(stimID(2)) == 1
                TIObjectCenter = TIObjectCenter+1;
            elseif str2double(stimID(2)) == 2
                TIObjectLeft = TIObjectLeft+1;
            elseif str2double(stimID(2)) == 3
                TIObjectRight = TIObjectRight+1;
            end
        elseif str2double(stimID(1)) == 3
            TILetters = TILetters+1 ;
            if str2double(stimID(2)) == 1
                TILetterCenter = TILetterCenter+1;
            elseif str2double(stimID(2)) == 2
                TILetterLeft = TILetterLeft+1;
            elseif str2double(stimID(2)) == 3
                TILetterRight = TILetterRight+1;
            end
        elseif str2double(stimID(1)) == 4
            TIFalse = TIFalse+1 ;
            if str2double(stimID(2)) == 1
                TIFalseCenter = TIFalseCenter+1;
            elseif str2double(stimID(2)) == 2
                TIFalseLeft = TIFalseLeft+1;
            elseif str2double(stimID(2)) == 3
                TIFalseRight = TIFalseRight+1;
            end
        end
    end
end

% Checking that the counters are correct

disp('---------------Overall categories and relevance---------------------');

disp('These should be 160 (80 for EoG)')
display(TRFace, 'TRFace'); % Task relevant non-target
display(TIFace, 'TIFace '); % Task irrelevant
display(TRObjects, 'TRObjects');
display(TIObjects, 'TIObjects');
display(TRLetters, 'TRLetters');
display(TILetters, 'TILetters');
display(TRFalse, 'TRFalse');
display(TIFalse, 'TIFalse');

disp('These should be 40 (20 for EoG)')
display(TargetFace, 'TargetFace'); % Target
display(TargetObjects, 'TargetObjects');
display(TargetLetters, 'TargetLetters');
display(TargetFalse, 'TargetFalse');

disp('---------------------------------------')

disp('---------------Categories, relevance and orientation ---------------------');

display(TRFaceCenter,'TRFaceCenter');
display(TRFaceLeft,'TRFaceLeft');
display(TRFaceRight,'TRFaceRight');

display(TRObjectCenter,'TRObjectCenter');
display(TRObjectLeft,'TRObjectLeft');
display(TRObjectRight,'TRObjectRight');

display(TRLetterCenter,'TRLetterCenter');
display(TRLetterLeft,'TRLetterLeft');
display(TRLetterRight,'RLetterRight');

display(TRFalseCenter,'TRFalseCenter');
display(TRFalseLeft,'TRFalseLeft');
display(TRFalseRight,'TRFalseRight');


display(TIFaceCenter,'TIFaceCenter');
display(TIFaceLeft,'TIFaceLeft');
display(TIFaceRight,'TIFaceRight');

display(TIObjectCenter,'TIObjectCenter');
display(TIObjectLeft,'TIObjectLeft');
display(TIObjectRight,'TIObjectRight');

display(TILetterCenter,'TILetterCenter');
display(TILetterLeft,'TILetterLeft');
display(TILetterRight,'TILetterRight');

display(TIFalseCenter,'TIFalseCenter');
display(TIFalseLeft,'TIFalseLeft');
display(TIFalseRight,'TIFalseRight');

disp('---------------------------------------')


return

%% Checking the timings:

% There are several things to look at for the timings:
% First, did the stimuli lasted for the duration they were supposed to.
% There are several ways to look at that:
% First, I make a histogram of the duration of all stimuli, there should be
% three bars: 1 at 0.5, 1 at 1 and 1 a 1.5
% Getting the duration of the stimuli:
% I first remove the responses and the save events, because they might come
% inbetween and we don't need them
miniBlockNoSaveNoResp = miniBlocks(~ismember([miniBlocks{:,29}],'Response') & ~ismember([miniBlocks{:,29}],'Save'),:);

% I then get the indices of the stimulus presentation begining
idxStimulus = find(ismember([miniBlockNoSaveNoResp{:,29}],'Stimulus'));

% To get the duration of the presentation, one need to subtract the
% timestamp of the begining of the stimulus presentation to the timestamp
% of the begining of the fixation. Since I removed the responses, the
% fixation always directly follows the stimulus, so I can do it as follows:

StimDur = [miniBlockNoSaveNoResp{idxStimulus+1,28}] - [miniBlockNoSaveNoResp{idxStimulus,28}];

% I then plot the histogram
figure
histogram(StimDur)

% But then, there is something else we want to make sure: do they last as
% long as what was planned?
% To get this information, I subtract the planned duration to the actual
% duration, and I make a scatter plot of that:
StimDurAccuracy = ([miniBlockNoSaveNoResp{idxStimulus+1,28}] - [miniBlockNoSaveNoResp{idxStimulus,28}]) - ...
    [miniBlockNoSaveNoResp{idxStimulus,24}];
% Ithen plot it:
figure
scatter(1:length(StimDurAccuracy),StimDurAccuracy)


% -------------------------------------------------------------------------
% The other things we want to check is whether the overall trial duration
% matches the planned 2 seconds. We follow the same procedure:
TrialDur = [miniBlockNoSaveNoResp{idxStimulus+2,28}] - [miniBlockNoSaveNoResp{idxStimulus,28}];

% I then plot the histogram
figure
histogram(StimDur)

% But then, there is something else we want to make sure: do they last as
% long as what was planned?
% To get this information, I subtract the planned duration to the actual
% duration, and I make a scatter plot of that:
TrialDurAccuracy = ([miniBlockNoSaveNoResp{idxStimulus+2,28}] - [miniBlockNoSaveNoResp{idxStimulus,28}]) - ...
    2;
% I then plot it:
figure
scatter(1:length(StimDurAccuracy),StimDurAccuracy)



% -------------------------------------------------------------------------
% The last thing we can check is the duration of the jitters. First I
% compute the observed jitter. It is a bit trickyer because inbetween
% blocks, there are the longer breaks. 
% BUT: in the original miniblock cell, at the end of the jitter at the end
% of a miniblock, saving occurs. So I take it, remove the responses and
% then I can do the same as before:
miniBlocNoResp = miniBlocks(~ismember([miniBlocks{:,29}],'Response'),:);

% I then get the indices of the stimulus presentation begining
idxStimulus = find(ismember([miniBlocNoResp{:,29}],'Stimulus'));
% Then, I compute the observed jitter: 
JitterDur = [miniBlocNoResp{idxStimulus(1:end-1)+3,28}] - [miniBlocNoResp{idxStimulus(1:end-1)+2,28}];

% I then make a histogram of the jitters:

figure
histogram(JitterDur)


% Again, we want to make sure the jitters are what they should be:
JitterDurAccuracy = ([miniBlocNoResp{idxStimulus(1:end-1)+3,28}] - [miniBlocNoResp{idxStimulus(1:end-1)+2,28}]) - ...
    [miniBlocNoResp{idxStimulus(1:end-1)+2,25}];

% Finally, making a scatter plot of the jitters:
figure
scatter(1:length(JitterDurAccuracy),JitterDurAccuracy)



