% This function performs the calibration of the tobii eyetracker when
% called:
function tobiiCalibration
global tobii_eyetracker YesKey compKbDevice ScreenWidth ScreenHeight w abortKey ValidationKey text spaceBar gray
 
% First of all, we introduce the calibration and ask whether they want to
% perform it or if it should be skipped?
Reiterate = 1;
while Reiterate
    showMessage('Welcome to the calibration!\n Do you wish to perform a calibration or a validation?\n\n [Y] Calibrate, [V] Validation, [Esc] Dont calibrate'); % ask the experiment if he/she wants to proceed
    [~, CalibKey, ~] =KbWait(compKbDevice,3);
    if CalibKey(YesKey)
        CALIBRATE = 1;
        Reiterate = 0;
    elseif (CalibKey(abortKey))
        CALIBRATE = 0;
        Reiterate = 0;
    elseif (CalibKey(ValidationKey))
        CALIBRATE = tobiiValidation;
        Reiterate = 0;
    else
        Reiterate = 1;
    end
end

% To avoid crashes, I put a try and catch statement:
% If it does crash, we ask the experimenter what he wants to do:
while CALIBRATE
    try
        %% Set calibration parameters:
        % Setup the calibration
        calib = ScreenBasedCalibration(tobii_eyetracker);
        % Go into calibration mode
        calib.enter_calibration_mode()
               
        % Set the coordinate of the points presented during the calibration
        points_to_collect = [[0.2,0.2];[0.2,0.5];[0.2,0.8];[0.5,0.2];[0.5,0.5];[0.5,0.8];...
            [0.8,0.2];[0.8,0.5];[0.8,0.8]]; % In proportion of the screen
        % Shuffling the points to collect:
        points_to_collect = points_to_collect(randperm(length(points_to_collect)),:);
        % Converting the points to pixels: 
        points_to_present = [points_to_collect(:,1)*ScreenWidth,points_to_collect(:,2)*ScreenHeight];
        % Set the duration of the targets presentation
        CalibrationPointsDur = 0.5;
        % Setting the redo calibration flag to do at least one iteration:
        RedoCalib = 1;
        
        %% Calibration onset screen:
        
        showMessage('The calibration will now start. \n Stare at the dots that will appear. \n\nPress any key to continues'); % ask the experiment if he/she wants to proceed
        KbWait(compKbDevice,3);
        % Making background grey:
        Screen('FillRect', w, gray);
        % First present a dot in the middle for participant to fixate:
        drawCalibTargetTobii(0.5*ScreenWidth,0.5*ScreenHeight)
        % Drawing instructions on the screen
        DrawFormattedText(w, textProcess('\n\n\n Press any key to start'), 'center', 'center', text.Color);
        Screen('Flip', w,[],0);
        KbWait(compKbDevice,3); % Here we don't really tell the experimenter, but that will be in the SOP. And basically, they can press anything!

        %% Calibration loop
        % As long as RedoCalib is set to 1, we will redo the calibration
        while  RedoCalib
            % When collecting data a point should be presented on the screen in the
            % appropriate position.
            for i=1:size(points_to_present,1)
                % Setting the acceptation flag:
                takeCalib = 0;
                
                % This function draws the calibration targets on the screen
                showCalibTargetTobii(points_to_present(i,1),points_to_present(i,2));
             
                % Waiting for the experimenter to press the space bar to accept
                % the dot:
                while ~takeCalib
                    % Waiting for a key to be pressed
                    [~, RestartCalib_keyCode, ~] =KbWait(compKbDevice,3);
                    if RestartCalib_keyCode(spaceBar) % If the key is the space bar, then take the point as fixation:
                        collect_result = calib.collect_data(points_to_collect(i,:));
                        takeCalib= 1; % Set the point as taken to proceed
                    end
                end
                % If this point could not be collected as calibration point, redo
                % the collection:
                Reiterations = 0;
                while ~collect_result.Success && Reiterations < 5
                    WaitSecs(CalibrationPointsDur) % Wait again for gaze to stabilize if needed
                    collect_result = calib.collect_data(points_to_collect(i,:)); % Collect the data again
                    Reiterations = Reiterations + 1;
                end
            end
            
            %% Calibration summary computations:
            
            % Compute and apply the calibration model:
            calibration_result = calib.compute_and_apply();
            tobiiCalibrationStatus = calibration_result.Status.Success;
            
            % For the experimenter to know how it went, presenting a screen where
            % the dots are shown as well as the collected data:
            % First, I need to make the collected data point a more manageable
            % format:            
            if ~isempty(calibration_result.CalibrationPoints) % If the eyes were detected
                % Looping through collected samples, but first:
                for i = 1:length(calibration_result.CalibrationPoints)
                    % Getting the left eye sample:
                    CalibrationSampleLeft(i,:) = calibration_result.CalibrationPoints(1,i).LeftEye.PositionOnDisplayArea;
                    % Getting the right eye sample:
                    CalibrationSampleRight(i,:) = calibration_result.CalibrationPoints(1,i).RightEye.PositionOnDisplayArea;
                end
                % Since we used both eyes to calibrate, we take the average of
                % both:
                LeftRightCalibSamples = cat(3,CalibrationSampleLeft,CalibrationSampleRight);
                % Averaging both eyes:
                BinoCalibrationSamples = nanmean(LeftRightCalibSamples,3);
                
                % Converting the collected samples from proportion to pixels for plotting:
                collectedSamples = [BinoCalibrationSamples(:,1)*ScreenWidth,BinoCalibrationSamples(:,2)*ScreenHeight];
                
            else % Otherwise, need to create an empty matrix to avoid crashes
                BinoCalibrationSamples = [];
                collectedSamples = [];
            end
            
            %% Calibration summary presentation and experimenter input query
            % Now, we present them against another to evaluate calibration
            % quality
            showSummaryCalibTobii(points_to_present,collectedSamples);
            
            % Then, waiting for experimenter input:
            KbWait(compKbDevice,3); % Here we don't really tell the experimenter, but that will be in the SOP. And basically, they can touch anything!
            % Asking the experimenter whether he/she wants to rerun the calibration:
            if tobiiCalibrationStatus
                showMessage('The calibration was successful.\n Do you wish to proceed to the validation or would you \nlike to redo the calibration?\n\n [Y] Proceed, [R] Recalibrate'); % ask the experiment if he/she wants to proceed
            else
                showMessage('The calibration failed.\n Would you like to redo the calibration or do you wish to abort?\n\n [Y] Abort, [R] Recalibrate'); % ask the experiment if he/she wants to proceed
            end
            [~, RestartCalib_keyCode, ~] =KbWait(compKbDevice,3);
            if RestartCalib_keyCode(YesKey)
                RedoCalib = 0;
            else
                RedoCalib = 1;
            end
        end
        
        % Leaving the calibration mode:
        calib.leave_calibration_mode()
        
        %% Validation: 
        % Getting gaze point based on the computed calibration to assess
        % its quality:
        CALIBRATE = tobiiValidation;
        
    
    catch
        %% Error handling:
        % If there was an error, show a message to the experimenter so that he/she can redo the calibration or proceed:
        % If it crashed before the calibration mode was closed, try closing
        % the calibration mode to avoid crashes down the line:
        try
            calib.leave_calibration_mode()
        catch
        end
        
        showMessage('The calibration crashed.\n Would you like to redo the calibration or do you wish to abort?\n\n [Y] Abort, [R] Recalibrate'); % ask the experiment if he/she wants to proceed
        [~, RestartCalib_keyCode, ~] =KbWait(compKbDevice,3);
        if RestartCalib_keyCode(YesKey)
            CALIBRATE = 0;
        else
            CALIBRATE = 1;
        end
    end
end

end