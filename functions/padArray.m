
%PADARRAY this function pads the array given with NO_TRIAL until it is
%of size num. Wasteful, should be used before experiment is running.
% input:
% ------
% arr - the array to be padded.
% num - the desired size of the array.
%
% output:
% -------
% arr - the array after padding.

function [ arr ] = padArray(arr,num)

    global NO_TRIAL
    while size(arr,2) < num
        arr = [arr NO_TRIAL];
    end
end
