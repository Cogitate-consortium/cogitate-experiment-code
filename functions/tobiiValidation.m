% This function is a validation for the Tobii Eyetracker:
% Dots are being shown at specific locations on the screen. The participant
% should look at the dots on the screen. When the experimenter knows that
% the participant is fixating the dot, the experimenter should press the
% space bar. Once they do so, the gaze coordinates from the 20 samples
% before key press are averaged. Once all the dots have been collected, the
% fixation map is shown. The dots should be as close as possible from the
% dots that were presented, which indicates a good calibration.

function RedoCalib = tobiiValidation

global tobii_eyetracker compKbDevice ScreenWidth ScreenHeight w YesKey text spaceBar gray
global DATA_FOLDER LAB_ID subjectNum TOBII_VALIDATION_LOG_FILE_NAMING excelFormat

try
    
    %% Preparing validation:
    % Letting the participant know what will happen
    showMessage('The validation will now start. \n Stare at the dots that will appear. \n\nPress any key to continue'); % ask the experiment if he/she wants to proceed
    KbWait(compKbDevice,3);
    
    % Set the coordinate of the points presented during the calibration
    points_to_collect = [[0.2,0.2];[0.2,0.5];[0.2,0.8];[0.5,0.2];[0.5,0.5];[0.5,0.8];...
        [0.8,0.2];[0.8,0.5];[0.8,0.8]]; % In proportion of the screen
    % Shuffling the points to collect:
    points_to_collect = points_to_collect(randperm(length(points_to_collect)),:);
    % Last step: converting that to pixels for presentation:
    points_to_present = [points_to_collect(:,1)*ScreenWidth,points_to_collect(:,2)*ScreenHeight];
    % Set the duration of the targets presentation
    GazeStabilisation = 0.1;
    ValidationPointsDur = 1.5;
    
    % Setting the redo calibration flag to do at least one iteration:
    % Making background grey:
    Screen('FillRect', w, gray);
    drawCalibTargetTobii(0.5*ScreenWidth,0.5*ScreenHeight)
    DrawFormattedText(w, textProcess('\n\n\n Press any key to start'), 'center', 'center', text.Color);
    Screen('Flip', w,[],0);
    KbWait(compKbDevice,3); % Here we don't really tell the experimenter, but that will be in the SOP. And basically, they can touch anything!
    
    
    %% Looping through targets presented:
    % Clearing the gaze data
    tobii_eyetracker.stop_gaze_data()
    
    % When collecting data a point should be presented on the screen in the
    % appropriate position.
    for i=1:size(points_to_present,1)
        % Getting gaze data:
        GazeData = tobii_eyetracker.get_gaze_data();
        takeCalib = 0; % Set the flag
        % This function draws the calibration targets on the screen
        showCalibTargetTobii(points_to_present(i,1),points_to_present(i,2));
        
        % Waiting for the experimenter to press the space bar to accept
        % the dot:
        while ~takeCalib
            % Waiting for a key to be pressed
            [~, RestartCalib_keyCode, ~] =KbWait(compKbDevice,3);
            if RestartCalib_keyCode(spaceBar) % If the key is the space bar, then take the point as fixation:
                % Getting the gaze data:
                GazeData = tobii_eyetracker.get_gaze_data();
                takeCalib= 1;
            end
        end
        
        % -----------------------------------------------------------------
        % Computation of collected data point: 
        % We then go from 20 samples before to the last sample (roughly
        % 200msec) to take the average of the gaze over this period. But if
        % they went too fast, there won't be 20 points to go back to. So we
        % make sure there is:
        if length(GazeData)>20
            goBack = 20;
        else
            goBack = length(GazeData);
        end
        % Setting a counter of how many data points there are:
        ValidationSamplesCounter = 1;
        
        % Then we go back as many samples back we can up to 20:
        for ii=(length(GazeData)-goBack):length(GazeData)
            thisPoint = GazeData(ii);
            Lx(ValidationSamplesCounter,1) = thisPoint.LeftEye.GazePoint.OnDisplayArea(1);
            Ly(ValidationSamplesCounter,1) = thisPoint.LeftEye.GazePoint.OnDisplayArea(2);
            Rx(ValidationSamplesCounter,1) = thisPoint.RightEye.GazePoint.OnDisplayArea(1);
            Ry(ValidationSamplesCounter,1) = thisPoint.RightEye.GazePoint.OnDisplayArea(2);
            ValidationSamplesCounter = ValidationSamplesCounter + 1;
        end
        
        % Averaging collected gaze:
        meanValidX(i,1) = nanmean([nanmean(Lx) nanmean(Rx)]);
        meanValidY(i,1) = nanmean([nanmean(Ly) nanmean(Ry)]);
        
        % Clearing the gaze data:
        tobii_eyetracker.stop_gaze_data()
        clear GazeData
    end
    
    %% Validation summary presentation:
    collectedSamples = double([meanValidX(:,1)*ScreenWidth,meanValidY(:,1)*ScreenHeight]); % Need to convert to double
    % Now, we present them against another to evaluate calibration
    % quality
    showSummaryCalibTobii(points_to_present,collectedSamples);
    % Logging the validation points for evaluating quality down the line:
    validation_log = array2table([points_to_present,collectedSamples], 'VariableNames',{'x_presented','y_presented','x_measured','y_measured'});
    % Saving:
    % Generate file name
    validation_log_file_name = fullfile(pwd,DATA_FOLDER,strcat(LAB_ID,num2str(subjectNum)), strcat(LAB_ID, num2str(subjectNum),TOBII_VALIDATION_LOG_FILE_NAMING));
    % Because their might be several validations throughout the experiment, numbering the files to avoid overwriting. 
    % Using a while loop to loop until the file name doesn't exists
    ctr = 1;
    exist = 1;
    while exist == 1
        if isfile(strcat(validation_log_file_name, '_', num2str(ctr), excelFormat))
            ctr = ctr + 1;
        else
            writetable(validation_log, strcat(validation_log_file_name, '_', num2str(ctr), excelFormat))
            exist = 0;
        end
    end
            
    KbWait(compKbDevice,3); % Here we don't really tell the experimenter, but that will be in the SOP. And basically, they can touch anything!
    
    % Then, asking the experimenter whether he/she wants to redo the
    % calibration or not:
    showMessage('Do you wish to proceed or would you like to redo the calibration?\n\n [Y] Proceed, [R] Recalibrate'); % ask the experiment if he/she wants to proceed
    [~, RestartCalib_keyCode, ~] =KbWait(compKbDevice,3);
    if RestartCalib_keyCode(YesKey)
        RedoCalib = 0;
    else
        RedoCalib = 1;
    end
    
catch
    showMessage('The validation crashed. \n\nDo you wish to proceed or would you like to redo the calibration?\n\n [Y] Proceed, [R] Recalibrate'); % ask the experiment if he/she wants to proceed
    [~, RestartCalib_keyCode, ~] =KbWait(compKbDevice,3);
    if RestartCalib_keyCode(YesKey)
        RedoCalib = 0;
    else
        RedoCalib = 1;
    end
end
end
