% showSummaryCalibTobii
% Draws all the presented tobii calibration/validation targets on screen
% and overlays the collected data points to evaluate how good it went:
% Input:
% - targetCoord: matrix with one column for x and for y coordinates of the
% presented fixation targets in pixels
% - calibPoints: matrix with one column for x and for y coordinates of the
% collected fixation samples in pixels

function [ ] = showSummaryCalibTobii(targetCoord,calibPoints)

global  ppd w DIAMOUT_FIXATION DIAMIN_FIXATION text gray

% Making background grey:
Screen('FillRect', w, gray);

% Redrawing all the calibration targets
for i = 1:length(targetCoord)
    drawCalibTargetTobii(targetCoord(i,1),targetCoord(i,2))
end

% Setting the color of the cross to white (RGB):
colorCross = [255 255 255]; 

% Drawing all the collected data points:
for i =  1:length(calibPoints)
    Screen('DrawLine', w, colorCross, calibPoints(i,1)-DIAMOUT_FIXATION/2*ppd, calibPoints(i,2),calibPoints(i,1)+DIAMOUT_FIXATION/2*ppd, calibPoints(i,2), DIAMIN_FIXATION*ppd);
    Screen('DrawLine', w, colorCross, calibPoints(i,1), calibPoints(i,2)-DIAMOUT_FIXATION/2*ppd,calibPoints(i,1), calibPoints(i,2)+DIAMOUT_FIXATION/2 * ppd, DIAMIN_FIXATION*ppd);
end

DrawFormattedText(w, textProcess('\n\n\n Press any key to continue'), 'center', 'center', text.Color);

drawPhotodiodBlock('off')

Screen('Flip', w,[],1); % Flipping the screen.
end