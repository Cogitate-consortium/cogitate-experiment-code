% drawCalibTargetTobii
% Draws and show the tobii caliration and validation targets
% Inputs: 
% - cx: x coordinates of the point to present in pixels
% - cy: y coordinates of the point to present in pixels

function [ ] = showCalibTargetTobii(cx,cy)

global w gray

% Making background grey:
Screen('FillRect', w, gray);

% Drawing the fixation target:
drawCalibTargetTobii(cx,cy)

% Flippling the screen:
Screen('Flip', w,[],0);

end