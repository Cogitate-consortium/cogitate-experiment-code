% TEXTPROCESS enables to transform all the text in the entire
% experiment, if needed (usually for needed for non-english text)
% input:
% ------
% txt - the text to be processed
%
% output:
% -------
% txt - the text after processing

function [ txt ] = textProcess( txt )
    txt = double(txt);
    %txt = flip(txt); % for Hebrew text
end