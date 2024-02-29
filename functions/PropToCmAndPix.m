% This function converts the proportion of screen into degrees of visusl
% angle
function [xCm, yCm, xPix, yPix] = PropToCmAndPix(x,y)
global SCREEN_SIZE_CM ScreenWidth ScreenHeight

% Getting the size of a single pixel in cm:
singlePixInCM = SCREEN_SIZE_CM./[ScreenWidth ScreenHeight];

xCm = x * SCREEN_SIZE_CM(1,1);
yCm = y * SCREEN_SIZE_CM(1,2);

xPix = x * ScreenWidth;
yPix = y * ScreenHeight;

end