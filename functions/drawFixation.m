%DRAWFIXATION
%Draws fixation on the screen buffer without "flipping"

function [ ] = drawFixation()

global  ppd w center DIAMOUT_FIXATION DIAMIN_FIXATION

colorOval = [0 0 0]; % color of the two circles [R G B]
colorCross = [255 255 255]; % color of the Cross [R G B]

%Change size based on dva
cx = center(1,1);
cy = center(2,1);


Screen('FillOval', w, colorOval, [cx-DIAMOUT_FIXATION/2*ppd, cy-DIAMOUT_FIXATION/2*ppd, cx+DIAMOUT_FIXATION/2*ppd, cy+DIAMOUT_FIXATION/2 * ppd], DIAMOUT_FIXATION*ppd);
Screen('DrawLine', w, colorCross, cx-DIAMOUT_FIXATION/2*ppd, cy,cx+DIAMOUT_FIXATION/2*ppd, cy, DIAMIN_FIXATION*ppd);
Screen('DrawLine', w, colorCross, cx, cy-DIAMOUT_FIXATION/2*ppd,cx, cy+DIAMOUT_FIXATION/2 * ppd, DIAMIN_FIXATION*ppd);
Screen('FillOval', w, colorOval, [cx-DIAMIN_FIXATION/0.75*ppd, cy-DIAMIN_FIXATION/0.75*ppd, cx+DIAMIN_FIXATION/0.75*ppd, cy+DIAMIN_FIXATION/0.75*ppd], DIAMIN_FIXATION*ppd);
% 
% global w FIXATION_FONT_SIZE FIXATION screenScaler fontSize FIXATION_COLOR
% 
% Screen('TextSize', w, round(FIXATION_FONT_SIZE*screenScaler));
% 
% DrawFormattedText(w, textProcess(FIXATION), 'center', 'center', FIXATION_COLOR);
% 
% Screen('TextSize', w, round(fontSize*screenScaler));

end