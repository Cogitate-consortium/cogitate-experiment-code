
% GETVISUALANGEL
% Adapted from Amir Tal, April 2019 amirostal@gmail.com
% Find stimuli display size given visual angle and viewing distance
% input:
% ------
% viewingDistance - participant's viewing distance in centimeters
% maxVisualAngel - the desired visual angel in degrees
%
% output:
% -------
% max_px - the size in pixels corresponding to the visual angle
function [max_px] = getVisualAngel(viewingDistance,maxVisualAngel)
    global SCREEN_SIZE_CM ScreenWidth ScreenHeight

    max_cm = sizeByVisualAngle(maxVisualAngel,viewingDistance);
    singlePixInCM = SCREEN_SIZE_CM./[ScreenWidth ScreenHeight];
    max_px = round(max_cm / singlePixInCM(1)); % calculate pixel equivalent of required size in cm

    % calculates size of display that satisfies given visual angle and viewing distance
    function size = sizeByVisualAngle(ang, distance)
        size = tan(deg2rad(ang)/2)*(2*distance);
    end
end
