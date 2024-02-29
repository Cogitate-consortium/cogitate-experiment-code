
function TrialBalanceControl(miniBlocks)
%% Counting the number of stimuli of the different categories:
global MEEG Behavior ECoG fMRI
miniBlocks = miniBlocks(ismember(miniBlocks.eventType,'Stimulus'),:);
% Counting the overall number of stimuli in each categories
% Following Yoav stimuli ID coding, if the first digit of the stimulus ID
% is a 1 then the stimulus is a face
Categories.Faces.All = sum(floor(miniBlocks.event/1000) == 1);
% If it is a 2 then it is an object
Categories.Objects.All = sum(floor([miniBlocks.event]./1000) == 2);
% If it is a 3 then it is a letter
Categories.Letters.All = sum(floor([miniBlocks.event]./1000) == 3);
% And if it is a 4 it is a false font
Categories.Falses.All = sum(floor([miniBlocks.event]./1000) == 4);


% Counting the number of stimuli within categories in the different
% orientations
% Again following Yoavs code, if the second digit of the stimulus ID is a
% 1, then the stimulus is in the front view
Categories.Faces.Center = sum(floor([miniBlocks.event]./1000) == 1 & floor(mod([miniBlocks.event],1000)/100) == 1);
% If it is a 2 then it is in the left view
Categories.Faces.Left = sum(floor([miniBlocks.event]./1000) == 1 & floor(mod([miniBlocks.event],1000)/100) == 2);
% And if it is a 3 then it is in the right view
Categories.Faces.Right = sum(floor([miniBlocks.event]./1000) == 1 & floor(mod([miniBlocks.event],1000)/100) == 3);

% I then do it for the letters
Categories.Letters.Center = sum(floor([miniBlocks.event]./1000) == 2 & floor(mod([miniBlocks.event],1000)/100) == 1);
Categories.Letters.Left = sum(floor([miniBlocks.event]./1000) == 2 & floor(mod([miniBlocks.event],1000)/100) == 2);
Categories.Letters.Right = sum(floor([miniBlocks.event]./1000) == 2 & floor(mod([miniBlocks.event],1000)/100) == 3);

% For the objects
Categories.Objects.Center = sum(floor([miniBlocks.event]./1000) == 3 & floor(mod([miniBlocks.event],1000)/100) == 1);
Categories.Objects.Left = sum(floor([miniBlocks.event]./1000) == 3 & floor(mod([miniBlocks.event],1000)/100) == 2);
Categories.Objects.Right = sum(floor([miniBlocks.event]./1000) == 3 & floor(mod([miniBlocks.event],1000)/100) == 2);

% For the falses:
Categories.Falses.Center = sum(floor([miniBlocks.event]./1000) == 4 & floor(mod([miniBlocks.event],1000)/100) == 1);
Categories.Falses.Left = sum(floor([miniBlocks.event]./1000) == 4 & floor(mod([miniBlocks.event],1000)/100) == 2);
Categories.Falses.Right = sum(floor([miniBlocks.event]./1000) == 4 & floor(mod([miniBlocks.event],1000)/100) == 3);

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
for i = 1:size(miniBlocks,1)
    % For each trial I get the ID of the stimulus and of the two targets
    stimID = num2str(miniBlocks.event(i));
    TargetsID1 = num2str(miniBlocks.targ1(i));
    TargetsID2 = num2str(miniBlocks.targ2(i));
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
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetFaceCenterShort = TargetFaceCenterShort+1;
                    % For an intermediate duration
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetFaceCenterInter = TargetFaceCenterInter+1;
                    % For a long duration
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetFaceCenterLong = TargetFaceCenterLong+1;
                end
                % Faces in the left orientation
            elseif str2double(stimID(2)) == 2
                TargetFaceLeft = TargetFaceLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetFaceLeftShort = TargetFaceLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetFaceLeftInter = TargetFaceLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetFaceLeftLong = TargetFaceLeftLong+1;
                end
                % In the right orientation
            elseif str2double(stimID(2)) == 3
                TargetFaceRight = TargetFaceRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetFaceRightShort = TargetFaceRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetFaceRightInter = TargetFaceRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetFaceRightLong = TargetFaceRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TargetFaceShort = TargetFaceShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TargetFaceInter = TargetFaceInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TargetFaceLong = TargetFaceLong + 1;
            end
            % If the first digit is a 2 then we have an object
        elseif str2double(stimID(1)) == 2
            TargetObjects = TargetObjects+1 ;
            if str2double(stimID(2)) == 1
                TargetObjectCenter = TargetObjectCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetObjectCenterShort = TargetObjectCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetObjectCenterInter = TargetObjectCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetObjectCenterLong = TargetObjectCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TargetObjectLeft = TargetObjectLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetObjectLeftShort = TargetObjectLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetObjectLeftInter = TargetObjectLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetObjectLeftLong = TargetObjectLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TargetObjectRight = TargetObjectRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetObjectRightShort = TargetObjectRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetObjectRightInter = TargetObjectRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetObjectRightLong = TargetObjectRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TargetObjectShort = TargetObjectShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TargetObjectInter = TargetObjectInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TargetObjectLong = TargetObjectLong + 1;
            end
            % 3 for letters
        elseif str2double(stimID(1)) == 3
            TargetLetters = TargetLetters+1 ;
            if str2double(stimID(2)) == 1
                TargetLetterCenter = TargetLetterCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetLetterCenterShort = TargetLetterCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetLetterCenterInter = TargetLetterCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetLetterCenterLong = TargetLetterCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TargetLetterLeft = TargetLetterLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetLetterLeftShort = TargetLetterLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetLetterLeftInter = TargetLetterLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetLetterLeftLong = TargetLetterLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TargetLetterRight = TargetLetterRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetLetterRightShort = TargetLetterRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetLetterRightInter = TargetLetterRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetLetterRightLong = TargetLetterRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TargetLetterShort = TargetLetterShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TargetLetterInter = TargetLetterInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TargetLetterLong = TargetLetterLong + 1;
            end
            % 4 for false fonts
        elseif str2double(stimID(1)) == 4
            TargetFalse = TargetFalse+1 ;
            if str2double(stimID(2)) == 1
                TargetFalseCenter = TargetFalseCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetFalseCenterShort = TargetFalseCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetFalseCenterInter = TargetFalseCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetFalseCenterLong = TargetFalseCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TargetFalseLeft =TargetFalseLeft +1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetFalseLeftShort = TargetFalseLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetFalseLeftInter = TargetFalseLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetFalseLeftLong = TargetFalseLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TargetFalseRight = TargetFalseRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TargetFalseRightShort = TargetFalseRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TargetFalseRightInter = TargetFalseRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TargetFalseRightLong = TargetFalseRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TargetFalseShort = TargetFalseShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TargetFalseInter = TargetFalseInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
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
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRFaceCenterShort = TRFaceCenterShort+1;
                    % 1
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRFaceCenterInter = TRFaceCenterInter+1;
                    % Or 1.5 second
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRFaceCenterLong = TRFaceCenterLong+1;
                end
                % In the left orientation
            elseif str2double(stimID(2)) == 2
                TRFaceLeft = TRFaceLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRFaceLeftShort = TRFaceLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRFaceLeftInter = TRFaceLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRFaceLeftLong = TRFaceLeftLong+1;
                end
                % In the right orientation
            elseif str2double(stimID(2)) == 3
                TRFaceRight = TRFaceRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRFaceRightShort = TRFaceRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRFaceRightInter = TRFaceRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRFaceRightLong = TRFaceRightLong+1;
                end
            end
            % I also do the same but for the durations alone:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TRFaceShort = TRFaceShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TRFaceInter = TRFaceInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TRFaceLong = TRFaceLong + 1;
            end
            
            % Then for the objects
        elseif str2double(stimID(1)) == 2
            TRObjects = TRObjects+1 ;
            % Center
            if str2double(stimID(2)) == 1
                TRObjectCenter = TRObjectCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRObjectCenterShort = TRObjectCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRObjectCenterInter = TRObjectCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRObjectCenterLong = TRObjectCenterLong+1;
                end
                % Left
            elseif str2double(stimID(2)) == 2
                TRObjectLeft = TRObjectLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRObjectLeftShort = TRObjectLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRObjectLeftInter = TRObjectLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRObjectLeftLong = TRObjectLeftLong+1;
                end
                % Right
            elseif str2double(stimID(2)) == 3
                TRObjectRight = TRObjectRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRObjectRightShort = TRObjectRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRObjectRightInter = TRObjectRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRObjectRightLong = TRObjectRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TRObjectShort = TRObjectShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TRObjectInter = TRObjectInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TRObjectLong = TRObjectLong + 1;
            end
            
            % For the letters
        elseif str2double(stimID(1)) == 3
            TRLetters = TRLetters+1 ;
            % In the center orientation
            if str2double(stimID(2)) == 1
                TRLetterCenter = TRLetterCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRLetterCenterShort = TRLetterCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRLetterCenterInter = TRLetterCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRLetterCenterLong = TRLetterCenterLong+1;
                end
                % Left
            elseif str2double(stimID(2)) == 2
                TRLetterLeft = TRLetterLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRLetterLeftShort = TRLetterLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRLetterLeftInter = TRLetterLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRLetterLeftLong = TRLetterLeftLong+1;
                end
                % Right
            elseif str2double(stimID(2)) == 3
                TRLetterRight = TRLetterRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRLetterRightShort = TRLetterRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRLetterRightInter = TRLetterRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRLetterRightLong = TRLetterRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TRLetterShort = TRLetterShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TRLetterInter = TRLetterInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TRLetterLong = TRLetterLong + 1;
            end
            % And finally for the false fonts:
        elseif str2double(stimID(1)) == 4
            TRFalse = TRFalse+1 ;
            if str2double(stimID(2)) == 1
                TRFalseCenter = TRFalseCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRFalseCenterShort = TRFalseCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRFalseCenterInter = TRFalseCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRFalseCenterLong = TRFalseCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TRFalseLeft = TRFalseLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRFalseLeftShort = TRFalseLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRFalseLeftInter = TRFalseLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRFalseLeftLong = TRFalseLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TRFalseRight = TRFalseRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TRFalseRightShort = TRFalseRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TRFalseRightInter = TRFalseRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TRFalseRightLong = TRFalseRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TRFalseShort = TRFalseShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TRFalseInter = TRFalseInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
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
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIFaceCenterShort = TIFaceCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIFaceCenterInter = TIFaceCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIFaceCenterLong = TIFaceCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TIFaceLeft = TIFaceLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIFaceLeftShort = TIFaceLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIFaceLeftInter = TIFaceLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIFaceLeftLong = TIFaceLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TIFaceRight = TIFaceRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIFaceRightShort = TIFaceRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIFaceRightInter = TIFaceRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIFaceRightLong = TIFaceRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TIFaceShort = TIFaceShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TIFaceInter = TIFaceInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TIFaceLong = TIFaceLong + 1;
            end
            % Objects
        elseif str2double(stimID(1)) == 2
            TIObjects = TIObjects+1 ;
            if str2double(stimID(2)) == 1
                TIObjectCenter = TIObjectCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIObjectCenterShort = TIObjectCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIObjectCenterInter = TIObjectCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIObjectCenterLong = TIObjectCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TIObjectLeft = TIObjectLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIObjectLeftShort = TIObjectLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIObjectLeftInter = TIObjectLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIObjectLeftLong = TIObjectLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TIObjectRight = TIObjectRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIObjectRightShort = TIObjectRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIObjectRightInter = TIObjectRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIObjectRightLong = TIObjectRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TIObjectShort = TIObjectShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TIObjectInter = TIObjectInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TIObjectLong = TIObjectLong + 1;
            end
            % Letters
        elseif str2double(stimID(1)) == 3
            TILetters = TILetters+1 ;
            if str2double(stimID(2)) == 1
                TILetterCenter = TILetterCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TILetterCenterShort = TILetterCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TILetterCenterInter = TILetterCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TILetterCenterLong = TILetterCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TILetterLeft = TILetterLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TILetterLeftShort = TILetterLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TILetterLeftInter = TILetterLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TILetterLeftLong = TILetterLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TILetterRight = TILetterRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TILetterRightShort = TILetterRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TILetterRightInter = TILetterRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TILetterRightLong = TILetterRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TILetterShort = TILetterShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TILetterInter = TILetterInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TILetterLong = TILetterLong + 1;
            end
            % Falses
        elseif str2double(stimID(1)) == 4
            TIFalse = TIFalse+1 ;
            if str2double(stimID(2)) == 1
                TIFalseCenter = TIFalseCenter+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIFalseCenterShort = TIFalseCenterShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIFalseCenterInter = TIFalseCenterInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIFalseCenterLong = TIFalseCenterLong+1;
                end
            elseif str2double(stimID(2)) == 2
                TIFalseLeft = TIFalseLeft+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIFalseLeftShort = TIFalseLeftShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIFalseLeftInter = TIFalseLeftInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIFalseLeftLong = TIFalseLeftLong+1;
                end
            elseif str2double(stimID(2)) == 3
                TIFalseRight = TIFalseRight+1;
                if miniBlocks.plndStimulusDur(i) == 0.5
                    TIFalseRightShort = TIFalseRightShort+1;
                elseif  miniBlocks.plndStimulusDur(i) == 1
                    TIFalseRightInter = TIFalseRightInter+1;
                elseif miniBlocks.plndStimulusDur(i) == 1.5
                    TIFalseRightLong = TIFalseRightLong+1;
                end
            end
            % I also do the same but for the durations:
            if miniBlocks.plndStimulusDur(i) == 0.5
                TIFalseShort = TIFalseShort + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1
                TIFalseInter = TIFalseInter + 1;
            elseif miniBlocks.plndStimulusDur(i) == 1.5
                TIFalseLong = TIFalseLong + 1;
            end
        end
    end
end

%% Checking if trial counts match the preregistration
% All combination of potential variables
if MEEG || Behavior
    %TARGETS
    if (TargetFaceCenterShort ~= 7 &&  TargetFaceLeftShort ~= 3  && TargetFaceRightShort  ~= 3) && (TargetFaceCenterShort ~= 6 &&  TargetFaceLeftShort ~= 4  && TargetFaceRightShort  ~= 4)
        display(TargetFaceCenterShort, 'TargetFaceCenterShort')
        display(TargetFaceLeftShort, 'TargetFaceLeftShort')
        display(TargetFaceRightShort, 'TargetFaceRightShort')
        warning('Target Face short is out of balance')
    elseif (TargetFaceCenterInter ~= 7 &&  TargetFaceLeftInter ~= 3  && TargetFaceRightInter  ~= 3) && (TargetFaceCenterInter ~= 6 &&  TargetFaceLeftInter ~= 4  && TargetFaceRightInter  ~= 4)
        display(TargetFaceCenterInter, 'TargetFaceCenterInter')
        display(TargetFaceLeftInter, 'TargetFaceLeftInter')
        display(TargetFaceRightInter, 'TargetFaceRightInter')
        warning('Target Face Inter is out of balance')
    elseif (TargetFaceCenterLong ~= 7 &&  TargetFaceLeftLong ~= 3  && TargetFaceRightLong  ~= 3) && (TargetFaceCenterLong ~= 6 &&  TargetFaceLeftLong ~= 4  && TargetFaceRightLong  ~= 4)
        display(TargetFaceCenterLong, 'TargetFaceCenterLong')
        display(TargetFaceLeftLong, 'TargetFaceLeftLong')
        display(TargetFaceRightLong, 'TargetFaceRightLong')
        warning('Target Face Long is out of balance')
        %%%Objects
    elseif (TargetObjectCenterShort ~= 7 &&  TargetObjectLeftShort ~= 3  && TargetObjectRightShort  ~= 3) && (TargetObjectCenterShort ~= 6 &&  TargetObjectLeftShort ~= 4  && TargetObjectRightShort  ~= 4)
        display(TargetObjectCenterShort, 'TargetObjectCenterShort')
        display(TargetObjectLeftShort, 'TargetObjectLeftShort')
        display(TargetObjectRightShort, 'TargetObjectRightShort')
        warning('Target Object short is out of balance')
    elseif (TargetObjectCenterInter ~= 7 &&  TargetObjectLeftInter ~= 3  && TargetObjectRightInter  ~= 3) && (TargetObjectCenterInter ~= 6 &&  TargetObjectLeftInter ~= 4  && TargetObjectRightInter  ~= 4)
        display(TargetObjectCenterInter, 'TargetObjectCenterInter')
        display(TargetObjectLeftInter, 'TargetObjectLeftInter')
        display(TargetObjectRightInter, 'TargetObjectRightInter')
        warning('Target Object Inter is out of balance')
    elseif (TargetObjectCenterLong ~= 7 &&  TargetObjectLeftLong ~= 3  && TargetObjectRightLong  ~= 3) && (TargetObjectCenterLong ~= 6 &&  TargetObjectLeftLong ~= 4  && TargetObjectRightLong  ~= 4)
        display(TargetObjectCenterLong, 'TargetObjectCenterLong')
        display(TargetObjectLeftLong, 'TargetObjectLeftLong')
        display(TargetObjectRightLong, 'TargetObjectRightLong')
        warning('Target Object Long is out of balance')
        %%%Letter
    elseif (TargetLetterCenterShort ~= 7 &&  TargetLetterLeftShort ~= 3  && TargetLetterRightShort  ~= 3) && (TargetLetterCenterShort ~= 6 &&  TargetLetterLeftShort ~= 4  && TargetLetterRightShort  ~= 4)
        display(TargetLetterCenterShort, 'TargetLetterCenterShort')
        display(TargetLetterLeftShort, 'TargetLetterLeftShort')
        display(TargetLetterRightShort, 'TargetLetterRightShort')
        warning('Target Letter short is out of balance')
    elseif (TargetLetterCenterInter ~= 7 &&  TargetLetterLeftInter ~= 3  && TargetLetterRightInter  ~= 3) && (TargetLetterCenterInter ~= 6 &&  TargetLetterLeftInter ~= 4  && TargetLetterRightInter  ~= 4)
        display(TargetLetterCenterInter, 'TargetLetterCenterInter')
        display(TargetLetterLeftInter, 'TargetLetterLeftInter')
        display(TargetLetterRightInter, 'TargetLetterRightInter')
        warning('Target Letter Inter is out of balance')
    elseif (TargetLetterCenterLong ~= 7 &&  TargetLetterLeftLong ~= 3  && TargetLetterRightLong  ~= 3) && (TargetLetterCenterLong ~= 6 &&  TargetLetterLeftLong ~= 4  && TargetLetterRightLong  ~= 4)
        display(TargetLetterCenterLong, 'TargetLetterCenterLong')
        display(TargetLetterLeftLong, 'TargetLetterLeftLong')
        display(TargetLetterRightLong, 'TargetLetterRightLong')
        warning('Target Letter Long is out of balance')
        %False
    elseif (TargetFalseCenterShort ~= 7 &&  TargetFalseLeftShort ~= 3  && TargetFalseRightShort  ~= 3) && (TargetFalseCenterShort ~= 6 &&  TargetFalseLeftShort ~= 4  && TargetFalseRightShort  ~= 4)
        display(TargetFalseCenterShort, 'TargetFalseCenterShort')
        display(TargetFalseLeftShort, 'TargetFalseLeftShort')
        display(TargetFalseRightShort, 'TargetFalseRightShort')
        warning('Target False short is out of balance')
    elseif (TargetFalseCenterInter ~= 7 &&  TargetFalseLeftInter ~= 3  && TargetFalseRightInter  ~= 3) && (TargetFalseCenterInter ~= 6 &&  TargetFalseLeftInter ~= 4  && TargetFalseRightInter  ~= 4)
        display(TargetFalseCenterInter, 'TargetFalseCenterInter')
        display(TargetFalseLeftInter, 'TargetFalseLeftInter')
        display(TargetFalseRightInter, 'TargetFalseRightInter')
        warning('Target False Inter is out of balance')
    elseif (TargetFalseCenterLong ~= 7 &&  TargetFalseLeftLong ~= 3  && TargetFalseRightLong  ~= 3) && (TargetFalseCenterLong ~= 6 &&  TargetFalseLeftLong ~= 4  && TargetFalseRightLong  ~= 4)
        display(TargetFalseCenterLong, 'TargetFalseCenterLong')
        display(TargetFalseLeftLong, 'TargetFalseLeftLong')
        display(TargetFalseRightLong, 'TargetFalseRightLong')
        warning('Target False Long is out of balance')
    else
        disp('All targets are balanced')
    end
elseif ECoG
    if (TargetFaceCenterShort ~= 4 &&  TargetFaceLeftShort ~= 1  && TargetFaceRightShort  ~= 1) && (TargetFaceCenterShort ~= 3 &&  TargetFaceLeftShort ~= 2  && TargetFaceRightShort  ~= 2)
        display(TargetFaceCenterShort, 'TargetFaceCenterShort')
        display(TargetFaceLeftShort, 'TargetFaceLeftShort')
        display(TargetFaceRightShort, 'TargetFaceRightShort')
        warning('Target Face short is out of balance')
    elseif (TargetFaceCenterInter ~= 4 &&  TargetFaceLeftInter ~= 1  && TargetFaceRightInter  ~= 1) && (TargetFaceCenterInter ~= 3 &&  TargetFaceLeftInter ~= 2  && TargetFaceRightInter  ~= 2)
        display(TargetFaceCenterInter, 'TargetFaceCenterInter')
        display(TargetFaceLeftInter, 'TargetFaceLeftInter')
        display(TargetFaceRightInter, 'TargetFaceRightInter')
        warning('Target Face Inter is out of balance')
    elseif (TargetFaceCenterLong ~= 4 &&  TargetFaceLeftLong ~= 1  && TargetFaceRightLong  ~= 1) && (TargetFaceCenterLong ~= 3 &&  TargetFaceLeftLong ~= 2  && TargetFaceRightLong  ~= 2)
        display(TargetFaceCenterLong, 'TargetFaceCenterLong')
        display(TargetFaceLeftLong, 'TargetFaceLeftLong')
        display(TargetFaceRightLong, 'TargetFaceRightLong')
        warning('Target Face Long is out of balance')
        %%%Object
    elseif (TargetObjectCenterShort ~= 4 &&  TargetObjectLeftShort ~= 1  && TargetObjectRightShort  ~= 1) && (TargetObjectCenterShort ~= 3 &&  TargetObjectLeftShort ~= 2  && TargetObjectRightShort  ~= 2)
        display(TargetObjectCenterShort, 'TargetObjectCenterShort')
        display(TargetObjectLeftShort, 'TargetObjectLeftShort')
        display(TargetObjectRightShort, 'TargetObjectRightShort')
        warning('Target Object short is out of balance')
    elseif (TargetObjectCenterInter ~= 4 &&  TargetObjectLeftInter ~= 1  && TargetObjectRightInter  ~= 1) && (TargetObjectCenterInter ~= 3 &&  TargetObjectLeftInter ~= 2  && TargetObjectRightInter  ~= 2)
        display(TargetObjectCenterInter, 'TargetObjectCenterInter')
        display(TargetObjectLeftInter, 'TargetObjectLeftInter')
        display(TargetObjectRightInter, 'TargetObjectRightInter')
        warning('Target Object Inter is out of balance')
    elseif (TargetObjectCenterLong ~= 4 &&  TargetObjectLeftLong ~= 1  && TargetObjectRightLong  ~= 1) && (TargetObjectCenterLong ~= 3 &&  TargetObjectLeftLong ~= 2  && TargetObjectRightLong  ~= 2)
        display(TargetObjectCenterLong, 'TargetObjectCenterLong')
        display(TargetObjectLeftLong, 'TargetObjectLeftLong')
        display(TargetObjectRightLong, 'TargetObjectRightLong')
        warning('Target Object Long is out of balance')
        %%%Letter
    elseif (TargetLetterCenterShort ~= 4 &&  TargetLetterLeftShort ~= 1  && TargetLetterRightShort  ~= 1) && (TargetLetterCenterShort ~= 3 &&  TargetLetterLeftShort ~= 2  && TargetLetterRightShort  ~= 2)
        display(TargetLetterCenterShort, 'TargetLetterCenterShort')
        display(TargetLetterLeftShort, 'TargetLetterLeftShort')
        display(TargetLetterRightShort, 'TargetLetterRightShort')
        warning('Target Letter short is out of balance')
    elseif (TargetLetterCenterInter ~= 4 &&  TargetLetterLeftInter ~= 1  && TargetLetterRightInter  ~= 1) && (TargetLetterCenterInter ~= 3 &&  TargetLetterLeftInter ~= 2  && TargetLetterRightInter  ~= 2)
        display(TargetLetterCenterInter, 'TargetLetterCenterInter')
        display(TargetLetterLeftInter, 'TargetLetterLeftInter')
        display(TargetLetterRightInter, 'TargetLetterRightInter')
        warning('Target Letter Inter is out of balance')
    elseif (TargetLetterCenterLong ~= 4 &&  TargetLetterLeftLong ~= 1  && TargetLetterRightLong  ~= 1) && (TargetLetterCenterLong ~= 3 &&  TargetLetterLeftLong ~= 2  && TargetLetterRightLong  ~= 2)
        display(TargetLetterCenterLong, 'TargetLetterCenterLong')
        display(TargetLetterLeftLong, 'TargetLetterLeftLong')
        display(TargetLetterRightLong, 'TargetLetterRightLong')
        warning('Target Letter Long is out of balance')
        %False
    elseif (TargetFalseCenterShort ~= 4 &&  TargetFalseLeftShort ~= 1  && TargetFalseRightShort  ~= 1) && (TargetFalseCenterShort ~= 3 &&  TargetFalseLeftShort ~= 2  && TargetFalseRightShort  ~= 2)
        display(TargetFalseCenterShort, 'TargetFalseCenterShort')
        display(TargetFalseLeftShort, 'TargetFalseLeftShort')
        display(TargetFalseRightShort, 'TargetFalseRightShort')
        warning('Target False short is out of balance')
    elseif (TargetFalseCenterInter ~= 4 &&  TargetFalseLeftInter ~= 1  && TargetFalseRightInter  ~= 1) && (TargetFalseCenterInter ~= 3 &&  TargetFalseLeftInter ~= 2  && TargetFalseRightInter  ~= 2)
        display(TargetFalseCenterInter, 'TargetFalseCenterInter')
        display(TargetFalseLeftInter, 'TargetFalseLeftInter')
        display(TargetFalseRightInter, 'TargetFalseRightInter')
        warning('Target False Inter is out of balance')
    elseif (TargetFalseCenterLong ~= 4 &&  TargetFalseLeftLong ~= 1  && TargetFalseRightLong  ~= 1) && (TargetFalseCenterLong ~= 3 &&  TargetFalseLeftLong ~= 2  && TargetFalseRightLong  ~= 2)
        display(TargetFalseCenterLong, 'TargetFalseCenterLong')
        display(TargetFalseLeftLong, 'TargetFalseLeftLong')
        display(TargetFalseRightLong, 'TargetFalseRightLong')
        warning('Target False Long is out of balance')
    else
        disp('All targets are balanced')
    end
    %fMRI
elseif fMRI
    if (TargetFaceCenterShort ~= 4 && TargetFaceLeftShort ~= 2  && TargetFaceRightShort  ~= 2)
        display(TargetFaceCenterShort, 'TargetFaceCenterShort')
        display(TargetFaceLeftShort, 'TargetFaceLeftShort')
        display(TargetFaceRightShort, 'TargetFaceRightShort')
        warning('Target Face short is out of balance')
    elseif (TargetFaceCenterInter ~= 4 &&  TargetFaceLeftInter ~= 2  && TargetFaceRightInter  ~= 2)
        display(TargetFaceCenterInter, 'TargetFaceCenterInter')
        display(TargetFaceLeftInter, 'TargetFaceLeftInter')
        display(TargetFaceRightInter, 'TargetFaceRightInter')
        warning('Target Face Inter is out of balance')
    elseif (TargetFaceCenterLong ~= 4  &&  TargetFaceLeftLong ~= 2  && TargetFaceRightLong  ~= 2)
        display(TargetFaceCenterLong, 'TargetFaceCenterLong')
        display(TargetFaceLeftLong, 'TargetFaceLeftLong')
        display(TargetFaceRightLong, 'TargetFaceRightLong')
        warning('Target Face Long is out of balance')
        %%%Object
    elseif (TargetObjectCenterShort ~= 4  &&  TargetObjectLeftShort ~= 2  && TargetObjectRightShort  ~= 2)
        display(TargetObjectCenterShort, 'TargetObjectCenterShort')
        display(TargetObjectLeftShort, 'TargetObjectLeftShort')
        display(TargetObjectRightShort, 'TargetObjectRightShort')
        warning('Target Object short is out of balance')
    elseif (TargetObjectCenterInter ~= 4 &&  TargetObjectLeftInter ~= 2  && TargetObjectRightInter  ~= 2)
        display(TargetObjectCenterInter, 'TargetObjectCenterInter')
        display(TargetObjectLeftInter, 'TargetObjectLeftInter')
        display(TargetObjectRightInter, 'TargetObjectRightInter')
        warning('Target Object Inter is out of balance')
    elseif (TargetObjectCenterLong ~= 4 &&  TargetObjectLeftLong ~= 2  && TargetObjectRightLong  ~= 2)
        display(TargetObjectCenterLong, 'TargetObjectCenterLong')
        display(TargetObjectLeftLong, 'TargetObjectLeftLong')
        display(TargetObjectRightLong, 'TargetObjectRightLong')
        warning('Target Object Long is out of balance')
        %%%Letter
    elseif (TargetLetterCenterShort ~= 4 &&  TargetLetterLeftShort ~= 2  && TargetLetterRightShort  ~= 2)
        display(TargetLetterCenterShort, 'TargetLetterCenterShort')
        display(TargetLetterLeftShort, 'TargetLetterLeftShort')
        display(TargetLetterRightShort, 'TargetLetterRightShort')
        warning('Target Letter short is out of balance')
    elseif (TargetLetterCenterInter ~= 4  &&  TargetLetterLeftInter ~= 2  && TargetLetterRightInter  ~= 2)
        display(TargetLetterCenterInter, 'TargetLetterCenterInter')
        display(TargetLetterLeftInter, 'TargetLetterLeftInter')
        display(TargetLetterRightInter, 'TargetLetterRightInter')
        warning('Target Letter Inter is out of balance')
    elseif (TargetLetterCenterLong ~= 4  &&  TargetLetterLeftLong ~= 2  && TargetLetterRightLong  ~= 2)
        display(TargetLetterCenterLong, 'TargetLetterCenterLong')
        display(TargetLetterLeftLong, 'TargetLetterLeftLong')
        display(TargetLetterRightLong, 'TargetLetterRightLong')
        warning('Target Letter Long is out of balance')
        %False
    elseif (TargetFalseCenterShort ~= 4 &&  TargetFalseLeftShort ~= 2  && TargetFalseRightShort  ~= 2)
        display(TargetFalseCenterShort, 'TargetFalseCenterShort')
        display(TargetFalseLeftShort, 'TargetFalseLeftShort')
        display(TargetFalseRightShort, 'TargetFalseRightShort')
        warning('Target False short is out of balance')
    elseif (TargetFalseCenterInter ~= 4  &&  TargetFalseLeftInter ~= 2  && TargetFalseRightInter  ~= 2)
        display(TargetFalseCenterInter, 'TargetFalseCenterInter')
        display(TargetFalseLeftInter, 'TargetFalseLeftInter')
        display(TargetFalseRightInter, 'TargetFalseRightInter')
        warning('Target False Inter is out of balance')
    elseif (TargetFalseCenterLong ~= 4 &&  TargetFalseLeftLong ~= 2  && TargetFalseRightLong  ~= 2)
        display(TargetFalseCenterLong, 'TargetFalseCenterLong')
        display(TargetFalseLeftLong, 'TargetFalseLeftLong')
        display(TargetFalseRightLong, 'TargetFalseRightLong')
        warning('Target False Long is out of balance')
    else
        disp('All targets are balanced')
    end
end
% Now for the task relevant:

if MEEG || Behavior
    %TRS
    if (TRFaceCenterShort ~= 27 &&  TRFaceLeftShort ~= 13  && TRFaceRightShort  ~= 13) && (TRFaceCenterShort ~= 26 &&  TRFaceLeftShort ~= 14  && TRFaceRightShort  ~= 14)
        display(TRFaceCenterShort, 'TRFaceCenterShort')
        display(TRFaceLeftShort, 'TRFaceLeftShort')
        display(TRFaceRightShort, 'TRFaceRightShort')
        warning('TR Face short is out of balance')
    elseif (TRFaceCenterInter ~= 27 &&  TRFaceLeftInter ~= 13  && TRFaceRightInter  ~= 13) && (TRFaceCenterInter ~= 26 &&  TRFaceLeftInter ~= 14  && TRFaceRightInter  ~= 14)
        display(TRFaceCenterInter, 'TRFaceCenterInter')
        display(TRFaceLeftInter, 'TRFaceLeftInter')
        display(TRFaceRightInter, 'TRFaceRightInter')
        warning('TR Face Inter is out of balance')
    elseif (TRFaceCenterLong ~= 27 &&  TRFaceLeftLong ~= 13  && TRFaceRightLong  ~= 13) && (TRFaceCenterLong ~= 26 &&  TRFaceLeftLong ~= 14  && TRFaceRightLong  ~= 14)
        display(TRFaceCenterLong, 'TRFaceCenterLong')
        display(TRFaceLeftLong, 'TRFaceLeftLong')
        display(TRFaceRightLong, 'TRFaceRightLong')
        warning('TR Face Long is out of balance')
        %%%Object
    elseif (TRObjectCenterShort ~= 27 &&  TRObjectLeftShort ~= 13  && TRObjectRightShort  ~= 13) && (TRObjectCenterShort ~= 26 &&  TRObjectLeftShort ~= 14  && TRObjectRightShort  ~= 14)
        display(TRObjectCenterShort, 'TRObjectCenterShort')
        display(TRObjectLeftShort, 'TRObjectLeftShort')
        display(TRObjectRightShort, 'TRObjectRightShort')
        warning('TR Object short is out of balance')
    elseif (TRObjectCenterInter ~= 27 &&  TRObjectLeftInter ~= 13  && TRObjectRightInter  ~= 13) && (TRObjectCenterInter ~= 26 &&  TRObjectLeftInter ~= 14  && TRObjectRightInter  ~= 14)
        display(TRObjectCenterInter, 'TRObjectCenterInter')
        display(TRObjectLeftInter, 'TRObjectLeftInter')
        display(TRObjectRightInter, 'TRObjectRightInter')
        warning('TR Object Inter is out of balance')
    elseif (TRObjectCenterLong ~= 27 &&  TRObjectLeftLong ~= 13  && TRObjectRightLong  ~= 13) && (TRObjectCenterLong ~= 26 &&  TRObjectLeftLong ~= 14  && TRObjectRightLong  ~= 14)
        display(TRObjectCenterLong, 'TRObjectCenterLong')
        display(TRObjectLeftLong, 'TRObjectLeftLong')
        display(TRObjectRightLong, 'TRObjectRightLong')
        warning('TR Object Long is out of balance')
        %%%Letter
    elseif (TRLetterCenterShort ~= 27 &&  TRLetterLeftShort ~= 13  && TRLetterRightShort  ~= 13) && (TRLetterCenterShort ~= 26 &&  TRLetterLeftShort ~= 14  && TRLetterRightShort  ~= 14)
        display(TRLetterCenterShort, 'TRLetterCenterShort')
        display(TRLetterLeftShort, 'TRLetterLeftShort')
        display(TRLetterRightShort, 'TRLetterRightShort')
        warning('TR Letter short is out of balance')
    elseif (TRLetterCenterInter ~= 27 &&  TRLetterLeftInter ~= 13  && TRLetterRightInter  ~= 13) && (TRLetterCenterInter ~= 26 &&  TRLetterLeftInter ~= 14  && TRLetterRightInter  ~= 14)
        display(TRLetterCenterInter, 'TRLetterCenterInter')
        display(TRLetterLeftInter, 'TRLetterLeftInter')
        display(TRLetterRightInter, 'TRLetterRightInter')
        warning('TR Letter Inter is out of balance')
    elseif (TRLetterCenterLong ~= 27 &&  TRLetterLeftLong ~= 13  && TRLetterRightLong  ~= 13) && (TRLetterCenterLong ~= 26 &&  TRLetterLeftLong ~= 14  && TRLetterRightLong  ~= 14)
        display(TRLetterCenterLong, 'TRLetterCenterLong')
        display(TRLetterLeftLong, 'TRLetterLeftLong')
        display(TRLetterRightLong, 'TRLetterRightLong')
        warning('TR Letter Long is out of balance')
        %False
    elseif (TRFalseCenterShort ~= 27 &&  TRFalseLeftShort ~= 13  && TRFalseRightShort  ~= 13) && (TRFalseCenterShort ~= 26 &&  TRFalseLeftShort ~= 14  && TRFalseRightShort  ~= 14)
        display(TRFalseCenterShort, 'TRFalseCenterShort')
        display(TRFalseLeftShort, 'TRFalseLeftShort')
        display(TRFalseRightShort, 'TRFalseRightShort')
        warning('TR False short is out of balance')
    elseif (TRFalseCenterInter ~= 27 &&  TRFalseLeftInter ~= 13  && TRFalseRightInter  ~= 13) && (TRFalseCenterInter ~= 26 &&  TRFalseLeftInter ~= 14  && TRFalseRightInter  ~= 14)
        display(TRFalseCenterInter, 'TRFalseCenterInter')
        display(TRFalseLeftInter, 'TRFalseLeftInter')
        display(TRFalseRightInter, 'TRFalseRightInter')
        warning('TR False Inter is out of balance')
    elseif (TRFalseCenterLong ~= 27 &&  TRFalseLeftLong ~= 13  && TRFalseRightLong  ~= 13) && (TRFalseCenterLong ~= 26 &&  TRFalseLeftLong ~= 14  && TRFalseRightLong  ~= 14)
        display(TRFalseCenterLong, 'TRFalseCenterLong')
        display(TRFalseLeftLong, 'TRFalseLeftLong')
        display(TRFalseRightLong, 'TRFalseRightLong')
        warning('TR False Long is out of balance')
    else
        disp('All TRs are balanced')
    end
elseif ECoG
    if (TRFaceCenterShort ~= 14 &&  TRFaceLeftShort ~= 6 && TRFaceRightShort  ~= 6) && (TRFaceCenterShort ~= 13 &&  TRFaceLeftShort ~= 7  && TRFaceRightShort  ~= 7 )
        display(TRFaceCenterShort, 'TRFaceCenterShort')
        display(TRFaceLeftShort, 'TRFaceLeftShort')
        display(TRFaceRightShort, 'TRFaceRightShort')
        warning('TR Face short is out of balance')
    elseif (TRFaceCenterInter ~= 14 &&  TRFaceLeftInter ~= 6 && TRFaceRightInter  ~= 6) && (TRFaceCenterInter ~= 13 &&  TRFaceLeftInter ~= 7  && TRFaceRightInter  ~= 7 )
        display(TRFaceCenterInter, 'TRFaceCenterInter')
        display(TRFaceLeftInter, 'TRFaceLeftInter')
        display(TRFaceRightInter, 'TRFaceRightInter')
        warning('TR Face Inter is out of balance')
    elseif (TRFaceCenterLong ~= 14 &&  TRFaceLeftLong ~= 6 && TRFaceRightLong  ~= 6) && (TRFaceCenterLong ~= 13 &&  TRFaceLeftLong ~= 7  && TRFaceRightLong  ~= 7 )
        display(TRFaceCenterLong, 'TRFaceCenterLong')
        display(TRFaceLeftLong, 'TRFaceLeftLong')
        display(TRFaceRightLong, 'TRFaceRightLong')
        warning('TR Face Long is out of balance')
        %%%Object
    elseif (TRObjectCenterShort ~= 14 &&  TRObjectLeftShort ~= 6 && TRObjectRightShort  ~= 6) && (TRObjectCenterShort ~= 13 &&  TRObjectLeftShort ~= 7  && TRObjectRightShort  ~= 7 )
        display(TRObjectCenterShort, 'TRObjectCenterShort')
        display(TRObjectLeftShort, 'TRObjectLeftShort')
        display(TRObjectRightShort, 'TRObjectRightShort')
        warning('TR Object short is out of balance')
    elseif (TRObjectCenterInter ~= 14 &&  TRObjectLeftInter ~= 6 && TRObjectRightInter  ~= 6) && (TRObjectCenterInter ~= 13 &&  TRObjectLeftInter ~= 7  && TRObjectRightInter  ~= 7 )
        display(TRObjectCenterInter, 'TRObjectCenterInter')
        display(TRObjectLeftInter, 'TRObjectLeftInter')
        display(TRObjectRightInter, 'TRObjectRightInter')
        warning('TR Object Inter is out of balance')
    elseif (TRObjectCenterLong ~= 14 &&  TRObjectLeftLong ~= 6 && TRObjectRightLong  ~= 6) && (TRObjectCenterLong ~= 13 &&  TRObjectLeftLong ~= 7  && TRObjectRightLong  ~= 7 )
        display(TRObjectCenterLong, 'TRObjectCenterLong')
        display(TRObjectLeftLong, 'TRObjectLeftLong')
        display(TRObjectRightLong, 'TRObjectRightLong')
        warning('TR Object Long is out of balance')
        %%%Letter
    elseif (TRLetterCenterShort ~= 14 &&  TRLetterLeftShort ~= 6 && TRLetterRightShort  ~= 6) && (TRLetterCenterShort ~= 13 &&  TRLetterLeftShort ~= 7  && TRLetterRightShort  ~= 7 )
        display(TRLetterCenterShort, 'TRLetterCenterShort')
        display(TRLetterLeftShort, 'TRLetterLeftShort')
        display(TRLetterRightShort, 'TRLetterRightShort')
        warning('TR Letter short is out of balance')
    elseif (TRLetterCenterInter ~= 14 &&  TRLetterLeftInter ~= 6 && TRLetterRightInter  ~= 6) && (TRLetterCenterInter ~= 13 &&  TRLetterLeftInter ~= 7  && TRLetterRightInter  ~= 7 )
        display(TRLetterCenterInter, 'TRLetterCenterInter')
        display(TRLetterLeftInter, 'TRLetterLeftInter')
        display(TRLetterRightInter, 'TRLetterRightInter')
        warning('TR Letter Inter is out of balance')
    elseif (TRLetterCenterLong ~= 14 &&  TRLetterLeftLong ~= 6 && TRLetterRightLong  ~= 6) && (TRLetterCenterLong ~= 13 &&  TRLetterLeftLong ~= 7  && TRLetterRightLong  ~= 7 )
        display(TRLetterCenterLong, 'TRLetterCenterLong')
        display(TRLetterLeftLong, 'TRLetterLeftLong')
        display(TRLetterRightLong, 'TRLetterRightLong')
        warning('TR Letter Long is out of balance')
        %False
    elseif (TRFalseCenterShort ~= 14 &&  TRFalseLeftShort ~= 6 && TRFalseRightShort  ~= 6) && (TRFalseCenterShort ~= 13 &&  TRFalseLeftShort ~= 7  && TRFalseRightShort  ~= 7 )
        display(TRFalseCenterShort, 'TRFalseCenterShort')
        display(TRFalseLeftShort, 'TRFalseLeftShort')
        display(TRFalseRightShort, 'TRFalseRightShort')
        warning('TR False short is out of balance')
    elseif (TRFalseCenterInter ~= 14 &&  TRFalseLeftInter ~= 6 && TRFalseRightInter  ~= 6) && (TRFalseCenterInter ~= 13 &&  TRFalseLeftInter ~= 7  && TRFalseRightInter  ~= 7 )
        display(TRFalseCenterInter, 'TRFalseCenterInter')
        display(TRFalseLeftInter, 'TRFalseLeftInter')
        display(TRFalseRightInter, 'TRFalseRightInter')
        warning('TR False Inter is out of balance')
    elseif (TRFalseCenterLong ~= 14 &&  TRFalseLeftLong ~= 6 && TRFalseRightLong  ~= 6) && (TRFalseCenterLong ~= 13 &&  TRFalseLeftLong ~= 7  && TRFalseRightLong  ~= 7 )
        display(TRFalseCenterLong, 'TRFalseCenterLong')
        display(TRFalseLeftLong, 'TRFalseLeftLong')
        display(TRFalseRightLong, 'TRFalseRightLong')
        warning('TR False Long is out of balance')
    else
        disp('All TRs are balanced')
    end
elseif fMRI
    if (TRFaceCenterShort ~= 11 &&  TRFaceLeftShort ~= 5  && TRFaceRightShort  ~= 5) && (TRFaceCenterShort ~= 10 &&  TRFaceLeftShort ~= 6  && TRFaceRightShort  ~= 6)
        display(TRFaceCenterShort, 'TRFaceCenterShort')
        display(TRFaceLeftShort, 'TRFaceLeftShort')
        display(TRFaceRightShort, 'TRFaceRightShort')
        warning('TR Face short is out of balance')
    elseif (TRFaceCenterInter ~= 11 &&  TRFaceLeftInter ~= 5  && TRFaceRightInter  ~= 5) && (TRFaceCenterInter ~= 10 &&  TRFaceLeftInter ~= 6  && TRFaceRightInter  ~= 6)
        display(TRFaceCenterInter, 'TRFaceCenterInter')
        display(TRFaceLeftInter, 'TRFaceLeftInter')
        display(TRFaceRightInter, 'TRFaceRightInter')
        warning('TR Face Inter is out of balance')
    elseif (TRFaceCenterLong ~= 11 &&  TRFaceLeftLong ~= 5  && TRFaceRightLong  ~= 5) && (TRFaceCenterLong ~= 10 &&  TRFaceLeftLong ~= 6  && TRFaceRightLong  ~= 6)
        display(TRFaceCenterLong, 'TRFaceCenterLong')
        display(TRFaceLeftLong, 'TRFaceLeftLong')
        display(TRFaceRightLong, 'TRFaceRightLong')
        warning('TR Face Long is out of balance')
        %%%Object
    elseif (TRObjectCenterShort ~= 11 &&  TRObjectLeftShort ~= 5  && TRObjectRightShort  ~= 5) && (TRObjectCenterShort ~= 10 &&  TRObjectLeftShort ~= 6  && TRObjectRightShort  ~= 6)
        display(TRObjectCenterShort, 'TRObjectCenterShort')
        display(TRObjectLeftShort, 'TRObjectLeftShort')
        display(TRObjectRightShort, 'TRObjectRightShort')
        warning('TR Object short is out of balance')
    elseif (TRObjectCenterInter ~= 11 &&  TRObjectLeftInter ~= 5  && TRObjectRightInter  ~= 5) && (TRObjectCenterInter ~= 10 &&  TRObjectLeftInter ~= 6  && TRObjectRightInter  ~= 6)
        display(TRObjectCenterInter, 'TRObjectCenterInter')
        display(TRObjectLeftInter, 'TRObjectLeftInter')
        display(TRObjectRightInter, 'TRObjectRightInter')
        warning('TR Object Inter is out of balance')
    elseif (TRObjectCenterLong ~= 11 &&  TRObjectLeftLong ~= 5  && TRObjectRightLong  ~= 5) && (TRObjectCenterLong ~= 10 &&  TRObjectLeftLong ~= 6  && TRObjectRightLong  ~= 6)
        display(TRObjectCenterLong, 'TRObjectCenterLong')
        display(TRObjectLeftLong, 'TRObjectLeftLong')
        display(TRObjectRightLong, 'TRObjectRightLong')
        warning('TR Object Long is out of balance')
        %%%Letter
    elseif (TRLetterCenterShort ~= 11 &&  TRLetterLeftShort ~= 5  && TRLetterRightShort  ~= 5) && (TRLetterCenterShort ~= 10 &&  TRLetterLeftShort ~= 6  && TRLetterRightShort  ~= 6)
        display(TRLetterCenterShort, 'TRLetterCenterShort')
        display(TRLetterLeftShort, 'TRLetterLeftShort')
        display(TRLetterRightShort, 'TRLetterRightShort')
        warning('TR Letter short is out of balance')
    elseif (TRLetterCenterInter ~= 11 &&  TRLetterLeftInter ~= 5  && TRLetterRightInter  ~= 5) && (TRLetterCenterInter ~= 10 &&  TRLetterLeftInter ~= 6  && TRLetterRightInter  ~= 6)
        display(TRLetterCenterInter, 'TRLetterCenterInter')
        display(TRLetterLeftInter, 'TRLetterLeftInter')
        display(TRLetterRightInter, 'TRLetterRightInter')
        warning('TR Letter Inter is out of balance')
    elseif (TRLetterCenterLong ~= 11 &&  TRLetterLeftLong ~= 5  && TRLetterRightLong  ~= 5) && (TRLetterCenterLong ~= 10 &&  TRLetterLeftLong ~= 6  && TRLetterRightLong  ~= 6)
        display(TRLetterCenterLong, 'TRLetterCenterLong')
        display(TRLetterLeftLong, 'TRLetterLeftLong')
        display(TRLetterRightLong, 'TRLetterRightLong')
        warning('TR Letter Long is out of balance')
        %False
    elseif (TRFalseCenterShort ~= 11 &&  TRFalseLeftShort ~= 5  && TRFalseRightShort  ~= 5) && (TRFalseCenterShort ~= 10 &&  TRFalseLeftShort ~= 6  && TRFalseRightShort  ~= 6)
        display(TRFalseCenterShort, 'TRFalseCenterShort')
        display(TRFalseLeftShort, 'TRFalseLeftShort')
        display(TRFalseRightShort, 'TRFalseRightShort')
        warning('TR False short is out of balance')
    elseif (TRFalseCenterInter ~= 11 &&  TRFalseLeftInter ~= 5  && TRFalseRightInter  ~= 5) && (TRFalseCenterInter ~= 10 &&  TRFalseLeftInter ~= 6  && TRFalseRightInter  ~= 6)
        display(TRFalseCenterInter, 'TRFalseCenterInter')
        display(TRFalseLeftInter, 'TRFalseLeftInter')
        display(TRFalseRightInter, 'TRFalseRightInter')
        warning('TR False Inter is out of balance')
    elseif (TRFalseCenterLong ~= 11 &&  TRFalseLeftLong ~= 5  && TRFalseRightLong  ~= 5) && (TRFalseCenterLong ~= 10 &&  TRFalseLeftLong ~= 6  && TRFalseRightLong  ~= 6)
        display(TRFalseCenterLong, 'TRFalseCenterLong')
        display(TRFalseLeftLong, 'TRFalseLeftLong')
        display(TRFalseRightLong, 'TRFalseRightLong')
        warning('TR False Long is out of balance')
    else
        disp('All TRs are balanced')
    end
end


% Now for the task irrelevant

if MEEG || Behavior
    %TIS
    if (TIFaceCenterShort ~= 27 &&  TIFaceLeftShort ~= 13  && TIFaceRightShort  ~= 13) && (TIFaceCenterShort ~= 26 &&  TIFaceLeftShort ~= 14  && TIFaceRightShort  ~= 14)
        display(TIFaceCenterShort, 'TIFaceCenterShort')
        display(TIFaceLeftShort, 'TIFaceLeftShort')
        display(TIFaceRightShort, 'TIFaceRightShort')
        warning('TI Face short is out of balance')
    elseif (TIFaceCenterInter ~= 27 &&  TIFaceLeftInter ~= 13  && TIFaceRightInter  ~= 13) && (TIFaceCenterInter ~= 26 &&  TIFaceLeftInter ~= 14  && TIFaceRightInter  ~= 14)
        display(TIFaceCenterInter, 'TIFaceCenterInter')
        display(TIFaceLeftInter, 'TIFaceLeftInter')
        display(TIFaceRightInter, 'TIFaceRightInter')
        warning('TI Face Inter is out of balance')
    elseif (TIFaceCenterLong ~= 27 &&  TIFaceLeftLong ~= 13  && TIFaceRightLong  ~= 13) && (TIFaceCenterLong ~= 26 &&  TIFaceLeftLong ~= 14  && TIFaceRightLong  ~= 14)
        display(TIFaceCenterLong, 'TIFaceCenterLong')
        display(TIFaceLeftLong, 'TIFaceLeftLong')
        display(TIFaceRightLong, 'TIFaceRightLong')
        warning('TI Face Long is out of balance')
        %%%Object
    elseif (TIObjectCenterShort ~= 27 &&  TIObjectLeftShort ~= 13  && TIObjectRightShort  ~= 13) && (TIObjectCenterShort ~= 26 &&  TIObjectLeftShort ~= 14  && TIObjectRightShort  ~= 14)
        display(TIObjectCenterShort, 'TIObjectCenterShort')
        display(TIObjectLeftShort, 'TIObjectLeftShort')
        display(TIObjectRightShort, 'TIObjectRightShort')
        warning('TI Object short is out of balance')
    elseif (TIObjectCenterInter ~= 27 &&  TIObjectLeftInter ~= 13  && TIObjectRightInter  ~= 13) && (TIObjectCenterInter ~= 26 &&  TIObjectLeftInter ~= 14  && TIObjectRightInter  ~= 14)
        display(TIObjectCenterInter, 'TIObjectCenterInter')
        display(TIObjectLeftInter, 'TIObjectLeftInter')
        display(TIObjectRightInter, 'TIObjectRightInter')
        warning('TI Object Inter is out of balance')
    elseif (TIObjectCenterLong ~= 27 &&  TIObjectLeftLong ~= 13  && TIObjectRightLong  ~= 13) && (TIObjectCenterLong ~= 26 &&  TIObjectLeftLong ~= 14  && TIObjectRightLong  ~= 14)
        display(TIObjectCenterLong, 'TIObjectCenterLong')
        display(TIObjectLeftLong, 'TIObjectLeftLong')
        display(TIObjectRightLong, 'TIObjectRightLong')
        warning('TI Object Long is out of balance')
        %%%Letter
    elseif (TILetterCenterShort ~= 27 &&  TILetterLeftShort ~= 13  && TILetterRightShort  ~= 13) && (TILetterCenterShort ~= 26 &&  TILetterLeftShort ~= 14  && TILetterRightShort  ~= 14)
        display(TILetterCenterShort, 'TILetterCenterShort')
        display(TILetterLeftShort, 'TILetterLeftShort')
        display(TILetterRightShort, 'TILetterRightShort')
        warning('TI Letter short is out of balance')
    elseif (TILetterCenterInter ~= 27 &&  TILetterLeftInter ~= 13  && TILetterRightInter  ~= 13) && (TILetterCenterInter ~= 26 &&  TILetterLeftInter ~= 14  && TILetterRightInter  ~= 14)
        display(TILetterCenterInter, 'TILetterCenterInter')
        display(TILetterLeftInter, 'TILetterLeftInter')
        display(TILetterRightInter, 'TILetterRightInter')
        warning('TI Letter Inter is out of balance')
    elseif (TILetterCenterLong ~= 27 &&  TILetterLeftLong ~= 13  && TILetterRightLong  ~= 13) && (TILetterCenterLong ~= 26 &&  TILetterLeftLong ~= 14  && TILetterRightLong  ~= 14)
        display(TILetterCenterLong, 'TILetterCenterLong')
        display(TILetterLeftLong, 'TILetterLeftLong')
        display(TILetterRightLong, 'TILetterRightLong')
        warning('TI Letter Long is out of balance')
        %False
    elseif (TIFalseCenterShort ~= 27 &&  TIFalseLeftShort ~= 13  && TIFalseRightShort  ~= 13) && (TIFalseCenterShort ~= 26 &&  TIFalseLeftShort ~= 14  && TIFalseRightShort  ~= 14)
        display(TIFalseCenterShort, 'TIFalseCenterShort')
        display(TIFalseLeftShort, 'TIFalseLeftShort')
        display(TIFalseRightShort, 'TIFalseRightShort')
        warning('TI False short is out of balance')
    elseif (TIFalseCenterInter ~= 27 &&  TIFalseLeftInter ~= 13  && TIFalseRightInter  ~= 13) && (TIFalseCenterInter ~= 26 &&  TIFalseLeftInter ~= 14  && TIFalseRightInter  ~= 14)
        display(TIFalseCenterInter, 'TIFalseCenterInter')
        display(TIFalseLeftInter, 'TIFalseLeftInter')
        display(TIFalseRightInter, 'TIFalseRightInter')
        warning('TI False Inter is out of balance')
    elseif (TIFalseCenterLong ~= 27 &&  TIFalseLeftLong ~= 13  && TIFalseRightLong  ~= 13) && (TIFalseCenterLong ~= 26 &&  TIFalseLeftLong ~= 14  && TIFalseRightLong  ~= 14)
        display(TIFalseCenterLong, 'TIFalseCenterLong')
        display(TIFalseLeftLong, 'TIFalseLeftLong')
        display(TIFalseRightLong, 'TIFalseRightLong')
        warning('TI False Long is out of balance')
    else
        disp('All TIs are balanced')
    end
elseif ECoG
    if (TIFaceCenterShort ~= 14 &&  TIFaceLeftShort ~= 6 && TIFaceRightShort  ~= 6) && (TIFaceCenterShort ~= 13 &&  TIFaceLeftShort ~= 7  && TIFaceRightShort  ~= 7 )
        display(TIFaceCenterShort, 'TIFaceCenterShort')
        display(TIFaceLeftShort, 'TIFaceLeftShort')
        display(TIFaceRightShort, 'TIFaceRightShort')
        warning('TI Face short is out of balance')
    elseif (TIFaceCenterInter ~= 14 &&  TIFaceLeftInter ~= 6 && TIFaceRightInter  ~= 6) && (TIFaceCenterInter ~= 13 &&  TIFaceLeftInter ~= 7  && TIFaceRightInter  ~= 7 )
        display(TIFaceCenterInter, 'TIFaceCenterInter')
        display(TIFaceLeftInter, 'TIFaceLeftInter')
        display(TIFaceRightInter, 'TIFaceRightInter')
        warning('TI Face Inter is out of balance')
    elseif (TIFaceCenterLong ~= 14 &&  TIFaceLeftLong ~= 6 && TIFaceRightLong  ~= 6) && (TIFaceCenterLong ~= 13 &&  TIFaceLeftLong ~= 7  && TIFaceRightLong  ~= 7 )
        display(TIFaceCenterLong, 'TIFaceCenterLong')
        display(TIFaceLeftLong, 'TIFaceLeftLong')
        display(TIFaceRightLong, 'TIFaceRightLong')
        warning('TI Face Long is out of balance')
        %%%Object
    elseif (TIObjectCenterShort ~= 14 &&  TIObjectLeftShort ~= 6 && TIObjectRightShort  ~= 6) && (TIObjectCenterShort ~= 13 &&  TIObjectLeftShort ~= 7  && TIObjectRightShort  ~= 7 )
        display(TIObjectCenterShort, 'TIObjectCenterShort')
        display(TIObjectLeftShort, 'TIObjectLeftShort')
        display(TIObjectRightShort, 'TIObjectRightShort')
        warning('TI Object short is out of balance')
    elseif (TIObjectCenterInter ~= 14 &&  TIObjectLeftInter ~= 6 && TIObjectRightInter  ~= 6) && (TIObjectCenterInter ~= 13 &&  TIObjectLeftInter ~= 7  && TIObjectRightInter  ~= 7 )
        display(TIObjectCenterInter, 'TIObjectCenterInter')
        display(TIObjectLeftInter, 'TIObjectLeftInter')
        display(TIObjectRightInter, 'TIObjectRightInter')
        warning('TI Object Inter is out of balance')
    elseif (TIObjectCenterLong ~= 14 &&  TIObjectLeftLong ~= 6 && TIObjectRightLong  ~= 6) && (TIObjectCenterLong ~= 13 &&  TIObjectLeftLong ~= 7  && TIObjectRightLong  ~= 7 )
        display(TIObjectCenterLong, 'TIObjectCenterLong')
        display(TIObjectLeftLong, 'TIObjectLeftLong')
        display(TIObjectRightLong, 'TIObjectRightLong')
        warning('TI Object Long is out of balance')
        %%%Letter
    elseif (TILetterCenterShort ~= 14 &&  TILetterLeftShort ~= 6 && TILetterRightShort  ~= 6) && (TILetterCenterShort ~= 13 &&  TILetterLeftShort ~= 7  && TILetterRightShort  ~= 7 )
        display(TILetterCenterShort, 'TILetterCenterShort')
        display(TILetterLeftShort, 'TILetterLeftShort')
        display(TILetterRightShort, 'TILetterRightShort')
        warning('TI Letter short is out of balance')
    elseif (TILetterCenterInter ~= 14 &&  TILetterLeftInter ~= 6 && TILetterRightInter  ~= 6) && (TILetterCenterInter ~= 13 &&  TILetterLeftInter ~= 7  && TILetterRightInter  ~= 7 )
        display(TILetterCenterInter, 'TILetterCenterInter')
        display(TILetterLeftInter, 'TILetterLeftInter')
        display(TILetterRightInter, 'TILetterRightInter')
        warning('TI Letter Inter is out of balance')
    elseif (TILetterCenterLong ~= 14 &&  TILetterLeftLong ~= 6 && TILetterRightLong  ~= 6) && (TILetterCenterLong ~= 13 &&  TILetterLeftLong ~= 7  && TILetterRightLong  ~= 7 )
        display(TILetterCenterLong, 'TILetterCenterLong')
        display(TILetterLeftLong, 'TILetterLeftLong')
        display(TILetterRightLong, 'TILetterRightLong')
        warning('TI Letter Long is out of balance')
        %False
    elseif (TIFalseCenterShort ~= 14 &&  TIFalseLeftShort ~= 6 && TIFalseRightShort  ~= 6) && (TIFalseCenterShort ~= 13 &&  TIFalseLeftShort ~= 7  && TIFalseRightShort  ~= 7 )
        display(TIFalseCenterShort, 'TIFalseCenterShort')
        display(TIFalseLeftShort, 'TIFalseLeftShort')
        display(TIFalseRightShort, 'TIFalseRightShort')
        warning('TI False short is out of balance')
    elseif (TIFalseCenterInter ~= 14 &&  TIFalseLeftInter ~= 6 && TIFalseRightInter  ~= 6) && (TIFalseCenterInter ~= 13 &&  TIFalseLeftInter ~= 7  && TIFalseRightInter  ~= 7 )
        display(TIFalseCenterInter, 'TIFalseCenterInter')
        display(TIFalseLeftInter, 'TIFalseLeftInter')
        display(TIFalseRightInter, 'TIFalseRightInter')
        warning('TI False Inter is out of balance')
    elseif (TIFalseCenterLong ~= 14 &&  TIFalseLeftLong ~= 6 && TIFalseRightLong  ~= 6) && (TIFalseCenterLong ~= 13 &&  TIFalseLeftLong ~= 7  && TIFalseRightLong  ~= 7 )
        display(TIFalseCenterLong, 'TIFalseCenterLong')
        display(TIFalseLeftLong, 'TIFalseLeftLong')
        display(TIFalseRightLong, 'TIFalseRightLong')
        warning('TI False Long is out of balance')
    else
        disp('All TIs are balanced')
    end
elseif fMRI
    if (TIFaceCenterShort ~= 11 &&  TIFaceLeftShort ~= 5  && TIFaceRightShort  ~= 5) && (TIFaceCenterShort ~= 10 &&  TIFaceLeftShort ~= 6  && TIFaceRightShort  ~= 6)
        display(TIFaceCenterShort, 'TIFaceCenterShort')
        display(TIFaceLeftShort, 'TIFaceLeftShort')
        display(TIFaceRightShort, 'TIFaceRightShort')
        warning('TI Face short is out of balance')
    elseif (TIFaceCenterInter ~= 11 &&  TIFaceLeftInter ~= 5  && TIFaceRightInter  ~= 5) && (TIFaceCenterInter ~= 10 &&  TIFaceLeftInter ~= 6  && TIFaceRightInter  ~= 6)
        display(TIFaceCenterInter, 'TIFaceCenterInter')
        display(TIFaceLeftInter, 'TIFaceLeftInter')
        display(TIFaceRightInter, 'TIFaceRightInter')
        warning('TI Face Inter is out of balance')
    elseif (TIFaceCenterLong ~= 11 &&  TIFaceLeftLong ~= 5  && TIFaceRightLong  ~= 5) && (TIFaceCenterLong ~= 10 &&  TIFaceLeftLong ~= 6  && TIFaceRightLong  ~= 6)
        display(TIFaceCenterLong, 'TIFaceCenterLong')
        display(TIFaceLeftLong, 'TIFaceLeftLong')
        display(TIFaceRightLong, 'TIFaceRightLong')
        warning('TI Face Long is out of balance')
        %%%Object
    elseif (TIObjectCenterShort ~= 11 &&  TIObjectLeftShort ~= 5  && TIObjectRightShort  ~= 5) && (TIObjectCenterShort ~= 10 &&  TIObjectLeftShort ~= 6  && TIObjectRightShort  ~= 6)
        display(TIObjectCenterShort, 'TIObjectCenterShort')
        display(TIObjectLeftShort, 'TIObjectLeftShort')
        display(TIObjectRightShort, 'TIObjectRightShort')
        warning('TI Object short is out of balance')
    elseif (TIObjectCenterInter ~= 11 &&  TIObjectLeftInter ~= 5  && TIObjectRightInter  ~= 5) && (TIObjectCenterInter ~= 10 &&  TIObjectLeftInter ~= 6  && TIObjectRightInter  ~= 6)
        display(TIObjectCenterInter, 'TIObjectCenterInter')
        display(TIObjectLeftInter, 'TIObjectLeftInter')
        display(TIObjectRightInter, 'TIObjectRightInter')
        warning('TI Object Inter is out of balance')
    elseif (TIObjectCenterLong ~= 11 &&  TIObjectLeftLong ~= 5  && TIObjectRightLong  ~= 5) && (TIObjectCenterLong ~= 10 &&  TIObjectLeftLong ~= 6  && TIObjectRightLong  ~= 6)
        display(TIObjectCenterLong, 'TIObjectCenterLong')
        display(TIObjectLeftLong, 'TIObjectLeftLong')
        display(TIObjectRightLong, 'TIObjectRightLong')
        warning('TI Object Long is out of balance')
        %%%Letter
    elseif (TILetterCenterShort ~= 11 &&  TILetterLeftShort ~= 5  && TILetterRightShort  ~= 5) && (TILetterCenterShort ~= 10 &&  TILetterLeftShort ~= 6  && TILetterRightShort  ~= 6)
        display(TILetterCenterShort, 'TILetterCenterShort')
        display(TILetterLeftShort, 'TILetterLeftShort')
        display(TILetterRightShort, 'TILetterRightShort')
        warning('TI Letter short is out of balance')
    elseif (TILetterCenterInter ~= 11 &&  TILetterLeftInter ~= 5  && TILetterRightInter  ~= 5) && (TILetterCenterInter ~= 10 &&  TILetterLeftInter ~= 6  && TILetterRightInter  ~= 6)
        display(TILetterCenterInter, 'TILetterCenterInter')
        display(TILetterLeftInter, 'TILetterLeftInter')
        display(TILetterRightInter, 'TILetterRightInter')
        warning('TI Letter Inter is out of balance')
    elseif (TILetterCenterLong ~= 11 &&  TILetterLeftLong ~= 5  && TILetterRightLong  ~= 5) && (TILetterCenterLong ~= 10 &&  TILetterLeftLong ~= 6  && TILetterRightLong  ~= 6)
        display(TILetterCenterLong, 'TILetterCenterLong')
        display(TILetterLeftLong, 'TILetterLeftLong')
        display(TILetterRightLong, 'TILetterRightLong')
        warning('TI Letter Long is out of balance')
        %False
    elseif (TIFalseCenterShort ~= 11 &&  TIFalseLeftShort ~= 5  && TIFalseRightShort  ~= 5) && (TIFalseCenterShort ~= 10 &&  TIFalseLeftShort ~= 6  && TIFalseRightShort  ~= 6)
        display(TIFalseCenterShort, 'TIFalseCenterShort')
        display(TIFalseLeftShort, 'TIFalseLeftShort')
        display(TIFalseRightShort, 'TIFalseRightShort')
        warning('TI False short is out of balance')
    elseif (TIFalseCenterInter ~= 11 &&  TIFalseLeftInter ~= 5  && TIFalseRightInter  ~= 5) && (TIFalseCenterInter ~= 10 &&  TIFalseLeftInter ~= 6  && TIFalseRightInter  ~= 6)
        display(TIFalseCenterInter, 'TIFalseCenterInter')
        display(TIFalseLeftInter, 'TIFalseLeftInter')
        display(TIFalseRightInter, 'TIFalseRightInter')
        warning('TI False Inter is out of balance')
    elseif (TIFalseCenterLong ~= 11 &&  TIFalseLeftLong ~= 5  && TIFalseRightLong  ~= 5) && (TIFalseCenterLong ~= 10 &&  TIFalseLeftLong ~= 6  && TIFalseRightLong  ~= 6)
        display(TIFalseCenterLong, 'TIFalseCenterLong')
        display(TIFalseLeftLong, 'TIFalseLeftLong')
        display(TIFalseRightLong, 'TIFalseRightLong')
        warning('TI False Long is out of balance')
    else
        disp('All TIs are balanced')
    end
end
