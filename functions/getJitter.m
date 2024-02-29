% GETJITTER sample the jitter from the truncated exponential distribution with
% JITTER_RANGE_MEAN as mean, and minumum (JITTER_RANGE_MIN) and maximum
% (JITTER_RANGE_MEAN). But since we need the jitters to be multiple of the
% refresh rate, we will
%
% input:
% ------
% numOfTrials - the number of trials for which a vector of planned jitter
% times is needed.
%
% output:
% -------
% jitter - a planned jitter times vector.
function [ jitter ] = getJitter(numOfTrials)

    global JITTER_RANGE_MIN JITTER_RANGE_MAX JITTER_RANGE_MEAN refRate
    % So the mean jitter was set in the constant parameters. We don't want to
    % get the actuall jitter but the coefficient by which to multiply the
    % jitter to get to it, does that make sense?
    meanRefRateCoeff = round(JITTER_RANGE_MEAN/refRate);
    minRefRateCoeff = round(JITTER_RANGE_MIN/refRate);
    maxRefRateCoeff = round(JITTER_RANGE_MAX/refRate);
    


    pd = makedist('Exponential','mu',meanRefRateCoeff);

    t = truncate(pd,minRefRateCoeff,maxRefRateCoeff);
    jitter = nan(1,numOfTrials);
    for i = 1:size(jitter,2)
        % I round it, because decimal number would kill the purpose of the whole
        % thing
        jitter(1,i) = round(random(t))*refRate;
    end
end