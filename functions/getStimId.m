
%GETSTIMID
% input - the full stimuli number (with orientation)
% output - the stimulus ID without the orientation, just type and its serial number
    function [ stimid ] = getStimId(num)
        
        stimid = num - floor(mod(num,1000)/100) * 100;
        
    
end % end function
