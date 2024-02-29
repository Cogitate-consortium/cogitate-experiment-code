% Checking trials number compliance to the pre registration:


% Loading the data:
load('Exp1_ResultsS999_b1_999-04-Mar-2020.mat')


% In the mini block cell, you have several for each trials, but we only
% need one. The extra lines are making things more complicated and are
% useless in the present script, so I get rid of every lines we don't need
miniBlocksNew = miniBlocks(ismember([miniBlocks{:,29}],'Stimulus'),:);

%% Counting the number of stimuli of the different categories:

% Counting the overall number of stimuli in each categories
% Following Yoav stimuli ID coding, if the first digit of the stimulus ID
% is a 1 then the stimulus is a face
Categories.Faces.All = sum(floor([miniBlocksNew{:,27}]./1000) == 1);
% If it is a 2 then it is an object
Categories.Objects.All = sum(floor([miniBlocksNew{:,27}]./1000) == 2);
% If it is a 3 then it is a letter
Categories.Letters.All = sum(floor([miniBlocksNew{:,27}]./1000) == 3);
% And if it is a 4 it is a false font
Categories.Falses.All = sum(floor([miniBlocksNew{:,27}]./1000) == 4);


% Counting the number of stimuli within categories in the different
% orientations
% Again following Yoavs code, if the second digit of the stimulus ID is a
% 1, then the stimulus is in the front view
Categories.Faces.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 1 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
% If it is a 2 then it is in the left view
Categories.Faces.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 1 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
% And if it is a 3 then it is in the right view 
Categories.Faces.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 1 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 3);

% I then do it for the letters
Categories.Letters.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 2 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
Categories.Letters.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 2 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
Categories.Letters.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 2 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 3);

% For the objects
Categories.Objects.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 3 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
Categories.Objects.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 3 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
Categories.Objects.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 3 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);

% For the falses:
Categories.Falses.Center = sum(floor([miniBlocksNew{:,27}]./1000) == 4 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 1);
Categories.Falses.Left = sum(floor([miniBlocksNew{:,27}]./1000) == 4 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 2);
Categories.Falses.Right = sum(floor([miniBlocksNew{:,27}]./1000) == 4 & floor(mod([miniBlocksNew{:,27}],1000)/100) == 3);

% Now I do the same but for the different task demands:
% For the task demands it is a bit more complicated. For the targets, you
% have to have the first number of the 27th column to be equal to the first
% number of the 22nd OR 23rd column & have the 2 last number of colum 27 to
% be equal to the 2 last numbers of column 22 or 23!

% I am therefore loop through each trial, otherwise I would have to write a
% ridiculously long one line statement: (I know these are way too many
% counters, there must be a smarter way to do that, sorry about that)
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

% Making the counters for the task relevance, category and orientation combination:
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

% And for the task demands category durations combination:
TargetFaceShort = 0;
TargetFaceInter = 0;
TargetFaceLong = 0;

TargetObjectShort = 0;
TargetObjectInter = 0;
TargetObjectLong = 0;

TargetLetterShort = 0;
TargetLetterInter = 0;
TargetLetterLong = 0;

TargetFalseShort = 0;
TargetFalseInter = 0;
TargetFalseLong = 0;


TRFaceShort = 0;
TRFaceInter = 0;
TRFaceLong = 0;

TRObjectShort = 0;
TRObjectInter = 0;
TRObjectLong = 0;

TRLetterShort = 0;
TRLetterInter = 0;
TRLetterLong = 0;

TRFalseShort = 0;
TRFalseInter = 0;
TRFalseLong = 0;


TIFaceShort = 0;
TIFaceInter = 0;
TIFaceLong = 0;

TIObjectShort = 0;
TIObjectInter = 0;
TIObjectLong = 0;

TILetterShort = 0;
TILetterInter = 0;
TILetterLong = 0;

TIFalseShort = 0;
TIFalseInter = 0;
TIFalseLong = 0;


% For the whole combination:
TargetFaceCenterShort = 0;
TargetFaceLeftShort = 0;
TargetFaceRightShort = 0;

TargetFaceCenterInter = 0;
TargetFaceLeftInter = 0;
TargetFaceRightInter = 0;

TargetFaceCenterLong = 0;
TargetFaceLeftLong = 0;
TargetFaceRightLong = 0;

TargetObjectCenterShort = 0;
TargetObjectLeftShort = 0;
TargetObjectRightShort = 0;

TargetObjectCenterInter = 0;
TargetObjectLeftInter = 0;
TargetObjectRightInter = 0;

TargetObjectCenterLong = 0;
TargetObjectLeftLong = 0;
TargetObjectRightLong = 0;


TargetLetterCenterShort = 0;
TargetLetterLeftShort = 0;
TargetLetterRightShort = 0;

TargetLetterCenterInter = 0;
TargetLetterLeftInter = 0;
TargetLetterRightInter = 0;

TargetLetterCenterLong = 0;
TargetLetterLeftLong = 0;
TargetLetterRightLong = 0;


TargetFalseCenterShort = 0;
TargetFalseLeftShort = 0;
TargetFalseRightShort = 0;

TargetFalseCenterInter = 0;
TargetFalseLeftInter = 0;
TargetFalseRightInter = 0;

TargetFalseCenterLong = 0;
TargetFalseLeftLong = 0;
TargetFalseRightLong = 0;

% Now for the task relevant:
TRFaceCenterShort = 0;
TRFaceLeftShort = 0;
TRFaceRightShort = 0;

TRFaceCenterInter = 0;
TRFaceLeftInter = 0;
TRFaceRightInter = 0;

TRFaceCenterLong = 0;
TRFaceLeftLong = 0;
TRFaceRightLong = 0;

TRObjectCenterShort = 0;
TRObjectLeftShort = 0;
TRObjectRightShort = 0;

TRObjectCenterInter = 0;
TRObjectLeftInter = 0;
TRObjectRightInter = 0;

TRObjectCenterLong = 0;
TRObjectLeftLong = 0;
TRObjectRightLong = 0;


TRLetterCenterShort = 0;
TRLetterLeftShort = 0;
TRLetterRightShort = 0;

TRLetterCenterInter = 0;
TRLetterLeftInter = 0;
TRLetterRightInter = 0;

TRLetterCenterLong = 0;
TRLetterLeftLong = 0;
TRLetterRightLong = 0;


TRFalseCenterShort = 0;
TRFalseLeftShort = 0;
TRFalseRightShort = 0;

TRFalseCenterInter = 0;
TRFalseLeftInter = 0;
TRFalseRightInter = 0;

TRFalseCenterLong = 0;
TRFalseLeftLong = 0;
TRFalseRightLong = 0;

% Now for the task irrelevant
TIFaceCenterShort = 0;
TIFaceLeftShort = 0;
TIFaceRightShort = 0;

TIFaceCenterInter = 0;
TIFaceLeftInter = 0;
TIFaceRightInter = 0;

TIFaceCenterLong = 0;
TIFaceLeftLong = 0;
TIFaceRightLong = 0;

TIObjectCenterShort = 0;
TIObjectLeftShort = 0;
TIObjectRightShort = 0;

TIObjectCenterInter = 0;
TIObjectLeftInter = 0;
TIObjectRightInter = 0;

TIObjectCenterLong = 0;
TIObjectLeftLong = 0;
TIObjectRightLong = 0;


TILetterCenterShort = 0;
TILetterLeftShort = 0;
TILetterRightShort = 0;

TILetterCenterInter = 0;
TILetterLeftInter = 0;
TILetterRightInter = 0;

TILetterCenterLong = 0;
TILetterLeftLong = 0;
TILetterRightLong = 0;


TIFalseCenterShort = 0;
TIFalseLeftShort = 0;
TIFalseRightShort = 0;

TIFalseCenterInter = 0;
TIFalseLeftInter = 0;
TIFalseRightInter = 0;

TIFalseCenterLong = 0;
TIFalseLeftLong = 0;
TIFalseRightLong = 0;

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
            % So for the faces presented in the center orientation
            if str2double(stimID(2)) == 1
                TargetFaceCenter = TargetFaceCenter+1;
                % For a short duration
                if miniBlocksNew{i,24} == 0.5
                    TargetFaceCenterShort = TargetFaceCenterShort+1;
                    % For an intermediate duration
                elseif  miniBlocksNew{i,24} == 1
                    TargetFaceCenterInter = TargetFaceCenterInter+1;
                    % For a long duration
                elseif miniBlocksNew{i,24} == 1.5
                    TargetFaceCenterLong = TargetFaceCenterLong+1;
                end
                % Faces in the left orientation
            elseif str2double(stimID(2)) == 2
                TargetFaceLeft = TargetFaceLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetFaceLeftShort = TargetFaceLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetFaceLeftInter = TargetFaceLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetFaceLeftLong = TargetFaceLeftLong+1;
                end
                % In the right orientation
            elseif str2double(stimID(2)) == 3
                TargetFaceRight = TargetFaceRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetFaceRightShort = TargetFaceRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetFaceRightInter = TargetFaceRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetFaceRightLong = TargetFaceRightLong+1;
                end
            end      
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TargetFaceShort = TargetFaceShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TargetFaceInter = TargetFaceInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TargetFaceLong = TargetFaceLong + 1;
            end
            % If the first digit is a 2 then we have an object
        elseif str2double(stimID(1)) == 2
            TargetObjects = TargetObjects+1 ;
            if str2double(stimID(2)) == 1
                TargetObjectCenter = TargetObjectCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetObjectCenterShort = TargetObjectCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetObjectCenterInter = TargetObjectCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetObjectCenterLong = TargetObjectCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TargetObjectLeft = TargetObjectLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetObjectLeftShort = TargetObjectLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetObjectLeftInter = TargetObjectLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetObjectLeftLong = TargetObjectLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TargetObjectRight = TargetObjectRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetObjectRightShort = TargetObjectRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetObjectRightInter = TargetObjectRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetObjectRightLong = TargetObjectRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TargetObjectShort = TargetObjectShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TargetObjectInter = TargetObjectInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TargetObjectLong = TargetObjectLong + 1;
            end
            % 3 for letters
        elseif str2double(stimID(1)) == 3
            TargetLetters = TargetLetters+1 ;
            if str2double(stimID(2)) == 1
                TargetLetterCenter = TargetLetterCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetLetterCenterShort = TargetLetterCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetLetterCenterInter = TargetLetterCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetLetterCenterLong = TargetLetterCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TargetLetterLeft = TargetLetterLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetLetterLeftShort = TargetLetterLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetLetterLeftInter = TargetLetterLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetLetterLeftLong = TargetLetterLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TargetLetterRight = TargetLetterRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetLetterRightShort = TargetLetterRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetLetterRightInter = TargetLetterRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetLetterRightLong = TargetLetterRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TargetLetterShort = TargetLetterShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TargetLetterInter = TargetLetterInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TargetLetterLong = TargetLetterLong + 1;
            end
            % 4 for false fonts
        elseif str2double(stimID(1)) == 4
            TargetFalse = TargetFalse+1 ;
            if str2double(stimID(2)) == 1
                TargetFalseCenter = TargetFalseCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetFalseCenterShort = TargetFalseCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetFalseCenterInter = TargetFalseCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetFalseCenterLong = TargetFalseCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TargetFalseLeft =TargetFalseLeft +1;
                if miniBlocksNew{i,24} == 0.5
                    TargetFalseLeftShort = TargetFalseLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetFalseLeftInter = TargetFalseLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetFalseLeftLong = TargetFalseLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TargetFalseRight = TargetFalseRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TargetFalseRightShort = TargetFalseRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TargetFalseRightInter = TargetFalseRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TargetFalseRightLong = TargetFalseRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TargetFalseShort = TargetFalseShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TargetFalseInter = TargetFalseInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TargetFalseLong = TargetFalseLong + 1;
            end
        end
        % If the first digit of the stimulus is the same as one of the
        % targets, but the two last digits are not the same as any of the
        % targets, then it is a task relevant non target
    elseif ((stimID(1) == TargetsID1(1)) && (str2double(stimID(3:4)) ~= str2double(TargetsID1(3:4))) )|| ...
            (stimID(1) == TargetsID2(1) && str2double(stimID(3:4)) ~= str2double(TargetsID2(3:4)))
        % First for the faces
        if str2double(stimID(1)) == 1
            TRFace = TRFace+1 ;
            % In the center orientation
            if str2double(stimID(2)) == 1
                TRFaceCenter = TRFaceCenter+1;
                % Presented for 0.5
                if miniBlocksNew{i,24} == 0.5
                    TRFaceCenterShort = TRFaceCenterShort+1;
                    % 1
                elseif  miniBlocksNew{i,24} == 1
                    TRFaceCenterInter = TRFaceCenterInter+1;
                    % Or 1.5 second
                elseif miniBlocksNew{i,24} == 1.5
                    TRFaceCenterLong = TRFaceCenterLong+1;
                end
                % In the left orientation
            elseif str2double(stimID(2)) == 2
                TRFaceLeft = TRFaceLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TRFaceLeftShort = TRFaceLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRFaceLeftInter = TRFaceLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRFaceLeftLong = TRFaceLeftLong+1;
                end
                % In the right orientation
            elseif str2double(stimID(2)) == 3
                TRFaceRight = TRFaceRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TRFaceRightShort = TRFaceRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRFaceRightInter = TRFaceRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRFaceRightLong = TRFaceRightLong+1;
                end
            end
            % I also do the same but for the durations alone:
            if miniBlocksNew{i,24} == 0.5
                TRFaceShort = TRFaceShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TRFaceInter = TRFaceInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TRFaceLong = TRFaceLong + 1;
            end
            
            % Then for the objects
        elseif str2double(stimID(1)) == 2
            TRObjects = TRObjects+1 ;
            % Center
            if str2double(stimID(2)) == 1
                TRObjectCenter = TRObjectCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TRObjectCenterShort = TRObjectCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRObjectCenterInter = TRObjectCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRObjectCenterLong = TRObjectCenterLong+1;
                end
                % Left
            elseif str2double(stimID(2)) == 2
                TRObjectLeft = TRObjectLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TRObjectLeftShort = TRObjectLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRObjectLeftInter = TRObjectLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRObjectLeftLong = TRObjectLeftLong+1;
                end
                % Right
            elseif str2double(stimID(2)) == 3
                TRObjectRight = TRObjectRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TRObjectRightShort = TRObjectRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRObjectRightInter = TRObjectRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRObjectRightLong = TRObjectRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TRObjectShort = TRObjectShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TRObjectInter = TRObjectInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TRObjectLong = TRObjectLong + 1;
            end
            
            % For the letters
        elseif str2double(stimID(1)) == 3
            TRLetters = TRLetters+1 ;
            % In the center orientation
            if str2double(stimID(2)) == 1
                TRLetterCenter = TRLetterCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TRLetterCenterShort = TRLetterCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRLetterCenterInter = TRLetterCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRLetterCenterLong = TRLetterCenterLong+1;
                end
                % Left 
            elseif str2double(stimID(2)) == 2
                TRLetterLeft = TRLetterLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TRLetterLeftShort = TRLetterLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRLetterLeftInter = TRLetterLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRLetterLeftLong = TRLetterLeftLong+1;
                end
                % Right
            elseif str2double(stimID(2)) == 3
                TRLetterRight = TRLetterRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TRLetterRightShort = TRLetterRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRLetterRightInter = TRLetterRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRLetterRightLong = TRLetterRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TRLetterShort = TRLetterShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TRLetterInter = TRLetterInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TRLetterLong = TRLetterLong + 1;
            end
            % And finally for the false fonts:
        elseif str2double(stimID(1)) == 4
            TRFalse = TRFalse+1 ;
            if str2double(stimID(2)) == 1
                TRFalseCenter = TRFalseCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TRFalseCenterShort = TRFalseCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRFalseCenterInter = TRFalseCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRFalseCenterLong = TRFalseCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TRFalseLeft = TRFalseLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TRFalseLeftShort = TRFalseLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRFalseLeftInter = TRFalseLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRFalseLeftLong = TRFalseLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TRFalseRight = TRFalseRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TRFalseRightShort = TRFalseRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TRFalseRightInter = TRFalseRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TRFalseRightLong = TRFalseRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TRFalseShort = TRFalseShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TRFalseInter = TRFalseInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TRFalseLong = TRFalseLong + 1;
            end
        end
        % Here else would have been sufficient, but I am always a little
        % nervous about not specifying something. So if the first digit is
        % not the same as the first digit of one of the target, then it is
        % a task irrelevant stimulus
    elseif stimID(1) ~= TargetsID1(1) && stimID(1) ~= TargetsID2(1)
        if str2double(stimID(1)) == 1
            TIFace = TIFace+1 ;
            % Faces
            if str2double(stimID(2)) == 1
                TIFaceCenter = TIFaceCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TIFaceCenterShort = TIFaceCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIFaceCenterInter = TIFaceCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIFaceCenterLong = TIFaceCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TIFaceLeft = TIFaceLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TIFaceLeftShort = TIFaceLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIFaceLeftInter = TIFaceLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIFaceLeftLong = TIFaceLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TIFaceRight = TIFaceRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TIFaceRightShort = TIFaceRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIFaceRightInter = TIFaceRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIFaceRightLong = TIFaceRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TIFaceShort = TIFaceShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TIFaceInter = TIFaceInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TIFaceLong = TIFaceLong + 1;
            end
           % Objects 
        elseif str2double(stimID(1)) == 2
            TIObjects = TIObjects+1 ;
            if str2double(stimID(2)) == 1
                TIObjectCenter = TIObjectCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TIObjectCenterShort = TIObjectCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIObjectCenterInter = TIObjectCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIObjectCenterLong = TIObjectCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TIObjectLeft = TIObjectLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TIObjectLeftShort = TIObjectLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIObjectLeftInter = TIObjectLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIObjectLeftLong = TIObjectLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TIObjectRight = TIObjectRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TIObjectRightShort = TIObjectRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIObjectRightInter = TIObjectRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIObjectRightLong = TIObjectRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TIObjectShort = TIObjectShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TIObjectInter = TIObjectInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TIObjectLong = TIObjectLong + 1;
            end
            % Letters
        elseif str2double(stimID(1)) == 3
            TILetters = TILetters+1 ;
            if str2double(stimID(2)) == 1
                TILetterCenter = TILetterCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TILetterCenterShort = TILetterCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TILetterCenterInter = TILetterCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TILetterCenterLong = TILetterCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TILetterLeft = TILetterLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TILetterLeftShort = TILetterLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TILetterLeftInter = TILetterLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TILetterLeftLong = TILetterLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TILetterRight = TILetterRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TILetterRightShort = TILetterRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TILetterRightInter = TILetterRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TILetterRightLong = TILetterRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TILetterShort = TILetterShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TILetterInter = TILetterInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TILetterLong = TILetterLong + 1;
            end
            % Falses
        elseif str2double(stimID(1)) == 4
            TIFalse = TIFalse+1 ;
            if str2double(stimID(2)) == 1
                TIFalseCenter = TIFalseCenter+1;
                if miniBlocksNew{i,24} == 0.5
                    TIFalseCenterShort = TIFalseCenterShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIFalseCenterInter = TIFalseCenterInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIFalseCenterLong = TIFalseCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TIFalseLeft = TIFalseLeft+1;
                if miniBlocksNew{i,24} == 0.5
                    TIFalseLeftShort = TIFalseLeftShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIFalseLeftInter = TIFalseLeftInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIFalseLeftLong = TIFalseLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TIFalseRight = TIFalseRight+1;
                if miniBlocksNew{i,24} == 0.5
                    TIFalseRightShort = TIFalseRightShort+1;
                elseif  miniBlocksNew{i,24} == 1
                    TIFalseRightInter = TIFalseRightInter+1;
                elseif miniBlocksNew{i,24} == 1.5
                    TIFalseRightLong = TIFalseRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocksNew{i,24} == 0.5
                TIFalseShort = TIFalseShort + 1;
            elseif miniBlocksNew{i,24} == 1                
                TIFalseInter = TIFalseInter + 1;
            elseif miniBlocksNew{i,24} == 1.5
                TIFalseLong = TIFalseLong + 1;
            end
        end
    end
end



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


