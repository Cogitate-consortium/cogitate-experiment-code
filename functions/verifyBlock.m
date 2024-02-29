
% VERFIFYBLOCK
% verifies the block is up to the constraints: no two stimuli are the same
% one after the other, regardless of orientation
% input - a miniblock
% output - true or false
function [ answer ] = verifyBlock(miniBlock)
global TRUE FALSE VERBOSE
if VERBOSE
    disp('WELCOME TO verifyBlock')
end


answer = TRUE;

for i = 2: size(miniBlock,2)
    if floor(getStimId(miniBlock(i))) == floor(getStimId(miniBlock(i - 1)))
        answer = FALSE;
        return;
    end
end