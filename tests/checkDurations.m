function stats = checkDurations(miniBlocks,type,msec,frameDuration)

% First, I clean the miniBlocks and get rid of everything that is not
% fixation, jitter and stimuli
miniBlocks = miniBlocks(ismember(miniBlocks.eventType,'Stimulus') |...
    ismember(miniBlocks.eventType,'Fixation') | ...
    ismember(miniBlocks.eventType,'Jitter'),:);

if msec == 0
    miniBlocks.time = miniBlocks.time*1000;
end

% There are several things to look at for the timings:
% 1. Stimuli:
    % First, did the stimuli lasted for the duration they were supposed to.
    % There are several ways to look at that:
    % First, I make a histogram of the duration of all stimuli, there should be
    % three bars: 1 at 0.5, 1 at 1 and 1 a 1.5
    % Getting the duration of the stimuli:
    % I first remove the responses and the save events, because they might come
    % inbetween and we don't need them
    % I then get the indices of the stimulus presentation begining
    idxStimulus = find(ismember(miniBlocks.eventType,'Stimulus'));
    
    % To get the duration of the presentation, one need to subtract the
    % timestamp of the begining of the stimulus presentation to the timestamp
    % of the begining of the fixation. Since I removed the responses, the
    % fixation always directly follows the stimulus, so I can do it as follows:
    
    StimDur = [miniBlocks.time(idxStimulus+1)] - [miniBlocks.time(idxStimulus)];
    
    % I then plot the histogram
    figure
    histogram(StimDur)
    ylabel('Number of trials')
    xlabel('Measured stimulus duration')
    title(sprintf('Histogram of the measured stimuli durations %s', type))
    
    % But then, there is something else we want to make sure: do they last as
    % long as what was planned?
    % To get this information, I subtract the planned duration to the actual
    % duration, and I make a scatter plot of that:
    StimDurAccuracy = (miniBlocks.time(idxStimulus+1) - miniBlocks.time(idxStimulus)) - ...
        miniBlocks.plndStimulusDur(idxStimulus) *1000;
    
    % Count the number of missed or skipped frames
    stats.PercentSkippedFrameStimuli = sum(StimDurAccuracy>=frameDuration)/length(StimDurAccuracy);
    
    % Ithen plot it:
    figure
    scatter(1:length(StimDurAccuracy),StimDurAccuracy)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Stimulus duration error (msec)')
    title(sprintf('Stimuli durations inaccuracies (%s)', type))
    
    % -------------------------------------------------------------------------
    % 2. Trials:
    % The other things we want to check is whether the overall trial duration
    % matches the planned 2 seconds. We follow the same procedure:
    TrialDur =  miniBlocks.time(idxStimulus+2) - miniBlocks.time(idxStimulus);
    
    % I then plot the histogram
    figure
    histogram(TrialDur)
    ylabel('Number of trials')
    xlabel('Measured trial duration')
    title(sprintf('Histogram of the measured trial durations (%s)', type))
    % But then, there is something else we want to make sure: do they last as
    % long as what was planned?
    % To get this information, I subtract the planned duration to the actual
    % duration, and I make a scatter plot of that:
    TrialDurAccuracy =(miniBlocks.time(idxStimulus+2) - miniBlocks.time(idxStimulus)) - ...
        2000;
    
    stats.PercentSkippedFrameTrial = sum(TrialDurAccuracy>=frameDuration)/length(TrialDurAccuracy);
    % I then plot it:
    figure
    scatter(1:length(TrialDurAccuracy),TrialDurAccuracy)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Trial duration error (msec)')
    title(sprintf('Trial durations inaccuracies (%s)',type))
    
    
    % -------------------------------------------------------------------------
    % Jitters: 
    % The last thing we can check is the duration of the jitters. First I
    % compute the observed jitter. It is a bit trickyer because inbetween
    % blocks, there are the longer breaks.
    % BUT: in the original miniblock cell, at the end of the jitter at the end
    % of a miniblock, saving occurs. So I take it, remove the responses and
    % then I can do the same as before:
    % Then, I compute the observed jitter:
    % Finding the change of miniBlock, because there we have a pause, so the
	% timings will be off if we look at those:
    idxminiBlkBegin = find(diff(miniBlocks.miniBlock) == 1) + 1;
    % Now I get the intersection between that and the stimuli onsets:
    [~, ~, firstStimIdx] = intersect(idxminiBlkBegin,idxStimulus);
    lastStimIdx = firstStimIdx - 1;
    
    % I now remove the last stimuli of a miniblock, because I can't compute
    % the jitter of those:
    jitterStimIdx = idxStimulus;
    jitterStimIdx(lastStimIdx) = [];
    
    JitterDur = miniBlocks.time(jitterStimIdx(1:end-1)+3) - miniBlocks.time(jitterStimIdx(1:end-1)+2);
    
    % I then make a histogram of the jitters:
    
    figure
    histogram(JitterDur)
    ylabel('Number of trials')
    xlabel('Measured Jitter duration')
    title(sprintf('Histogram of the measured jitters durations (%s)', type))
    
    % Again, we want to make sure the jitters are what they should be:
    JitterDurAccuracy = (miniBlocks.time(jitterStimIdx(1:end-1)+3) - miniBlocks.time(jitterStimIdx(1:end-1)+2)) - ...
        miniBlocks.plndJitterDur(jitterStimIdx(1:end-1))*1000;
    stats.PercentSkippedFrameJitters = sum(JitterDurAccuracy>=frameDuration)/length(JitterDurAccuracy);
    % Finally, making a scatter plot of the jitters:
    figure
    scatter(1:length(JitterDurAccuracy),JitterDurAccuracy)
    yline(frameDuration,'r')
    yline(-frameDuration,'r')
    xlabel('Trial')
    ylabel('Jitter duration error (msec)')
    title(sprintf('Jitter durations inaccuracies (%s)', type))
    
end