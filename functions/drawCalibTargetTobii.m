% drawCalibTargetTobii
% Draws tobii caliration and validation targets without flipping the
% screen.
% Inputs: 
% - cx: x coordinates of the point to present in pixels
% - cy: y coordinates of the point to present in pixels

function [ ] = drawCalibTargetTobii(cx,cy)

global  ppd w DIAMOUT_FIXATION DIAMIN_FIXATION

colorOval = [0 0 0]; % color of the two circles [R G B]
colorCross = [255 255 255]; % color of the Cross [R G B]

Screen('FillOval', w, colorOval, [cx-DIAMOUT_FIXATION/2*ppd, cy-DIAMOUT_FIXATION/2*ppd, cx+DIAMOUT_FIXATION/2*ppd, cy+DIAMOUT_FIXATION/2 * ppd], DIAMOUT_FIXATION*ppd);
Screen('DrawLine', w, colorCross, cx-DIAMOUT_FIXATION/2*ppd, cy,cx+DIAMOUT_FIXATION/2*ppd, cy, DIAMIN_FIXATION*ppd);
Screen('DrawLine', w, colorCross, cx, cy-DIAMOUT_FIXATION/2*ppd,cx, cy+DIAMOUT_FIXATION/2 * ppd, DIAMIN_FIXATION*ppd);
Screen('FillOval', w, colorOval, [cx-DIAMIN_FIXATION/2*ppd, cy-DIAMIN_FIXATION/2*ppd, cx+DIAMIN_FIXATION/2*ppd, cy+DIAMIN_FIXATION/2*ppd], DIAMIN_FIXATION*ppd);

% Drawing the photodiode too:
drawPhotodiodBlock('off')

end