% This function decodes the audio triggers bit codes.
% Author: Alex Lepauvre
% Date: 28/05/2020
% Input:
% - audio: the audio signal to be analyzed
% - nBits: number of bits of the audio code (with the flanks!) Q:No, it is
% 7.
% - bitDurationMs: duration of a single bit in ms
% - signalSR: sampling rate of the audio signal
% - Plotting: command to plot the data or not: 'on' = figs will be plotted
% output:
% - decTrigger: decoded triggers [audio triggers in decimals; time stamps in ms; duration in ms; time stamp in sample_nr; duration in sample_nr; bit_code with padding]




function decTrigger = audioBitDecoder_Harvard(audio,nBits,bitDurationMs, volume, show_plots, start_sample_audio, max_intermediate_noise_zeros_audio, noise_threshold_high_pass_audio)
    global signalSR VERBOSE font_size

if VERBOSE
    disp('Inside audioBitDecoder');
end

%noise_threshold_low_pass = 3000;

% Get rid of initial noise
audio(1:start_sample_audio) = 0;

% Then, I threshold the signal: everything thats above or below a certain threshold is 1 or -1, the rest is 0:

% If you also want to use low pass filter you can use these lines:
%audioPlusOnes = double( audio(:,1)> noise_threshold_high_pass & audio(:,1) < noise_threshold_low_pass);
%audioMinusOnes = double( audio(:,1)<- noise_threshold_high_pass & audio(:,1) > - noise_threshold_low_pass);

audioPlusOnes = double( audio(:,1)> noise_threshold_high_pass_audio);
audioMinusOnes = double( audio(:,1)<- noise_threshold_high_pass_audio);

% I then combine the two by adding them up again reconstructing the signal like it was before but without noise:
denoiseaudio = audioPlusOnes-audioMinusOnes;
if show_plots
    figure()
    title('Sanity check: Denoised audio', 'FontSize', font_size);
    hold on;
    plot(denoiseaudio)
end

% Now in case things were plugged upside down, I invert the signal:
% indexFirstBit = find(denoiseaudio == 1 | denoiseaudio == -1,1,'first');
% if denoiseaudio(indexFirstBit) == -1
%     denoiseaudio = - denoiseaudio;
% end
% I then compute the diff of the audio signal. The reason for that is that
% we only want the onsets of the positives and the negatives.

diffdenoiseaudio = diff(denoiseaudio);

% Cleaning of the denoise vector.
% There can be occasional zeros in between the ones and minus ones. These
% make it harder to decode the triggers, so we will just get rid of them, i.e. replacing
% them with the preceding sample:
%denoiseaudio

sliding_window_size = max_intermediate_noise_zeros_audio + 1;

for i=1:length(denoiseaudio) - 3
    
    %i
    
    samples = denoiseaudio(i:i+3);
    %samples
    
    
    % Are you at a boundary and there are up to two zeros in between?
    if ~isempty(find(samples ==1)) && ~isempty(find(samples == -1)) && ~isempty(find(samples == 0))
        the_zeros_inds = find(samples == 0);
        if VERBOSE
            disp('found a zero and this is samples')
            samples
            the_zeros_inds
        end
        for j=1:length(the_zeros_inds)
            % assign them +1:
            ind = the_zeros_inds(j);
            samples(ind) = 1;
            %disp('inside assigning ones')
        end % for zeros
        
        
        % Assign denoideaudio:
        denoiseaudio(i:i+3) = samples;
      
        
    end % if zeros between ones found
      
end % for all samples in denoiseaudio

%denoiseaudio

if show_plots
    figure()
    title('Sanity check: Denoised audio after preprocessing', 'FontSize', font_size)
    hold on;
    plot(denoiseaudio)
end



% The next step is to isolate each trigger. Between each trigger, we have
% a set amount of time
% Q: Really?
% but within a trigger, max length of 0 in a row is
% the total number of bits in the code (assuming a code of 0). So I set as
% a threshold the max number of time of an entire trigger.

% Anything that will have more zeros in a row is an intertrigger interval.
% nBits are the number of information carrying bits = 7.
MaxDuration = (bitDurationMs)*nBits;
% How many samples per bit
n_samples_per_ms = signalSR/1000.;
ms_per_sample = 1000./signalSR;
MaxDuration_n_samples = MaxDuration*n_samples_per_ms;
n_samples_per_bit = bitDurationMs*n_samples_per_ms;

% I detect the different potential onsets (first 1 in the 9 bit trigger):
% Wherever we have a minus 1, we have had a bit starting. (if we would have
% required > 1 we would

%%idxOnsets = find(diffdenoiseaudio<=-1);
idxOnsets = find(diffdenoiseaudio>=1);

if show_plots
    figure()
    title('Sanity check: diffdenoiseaudio', 'FontSize', font_size)
    hold on
    plot(diffdenoiseaudio)
end


% I set a counter for the triggers
TriggersCounter = 1;
TriggerStart = [];
TriggerEnd = [];


%% Detecting onset and offset:
% I then loop through each potential onset, and try to find the last one in
% the trigger by checking which one has a a consecutive set of zeros
% after it that is exceeding the maxduration of a trigger.

for i=1:length(idxOnsets)
    
    DurationCounter = 1;
    
    % The first condition is just to not run out of bounds of the vector
    % The diff vector is 1 000 -2-2-2 for a bit = 1. If a zero bit comes
    % the diff will be 1 (0--1). If a bit = 1 comes the diff will be 2 (1
    % --1). So if we find a 1, we are inside a trigger. Only consecutive
    % zeros will mean we are between triggers.
    %% while idxOnsets(i)+DurationCounter<=length(diffdenoiseaudio) && diffdenoiseaudio(idxOnsets(i)+DurationCounter,1) < 1
    while idxOnsets(i)+DurationCounter<=length(diffdenoiseaudio) && diffdenoiseaudio(idxOnsets(i)+DurationCounter,1) == 0
        
        %disp('inside and diff is')
        %diffdenoiseaudio(idxOnsets(i)+DurationCounter,1)
        %disp('denoiseaudio')
        %denoiseaudio(idxOnsets(i)+DurationCounter,1)
        DurationCounter = DurationCounter + 1;
    end
    %disp('outside diff')
    %diffdenoiseaudio(idxOnsets(i)+DurationCounter,1)
    %disp('outside denoise')
    %denoiseaudio(idxOnsets(i)+DurationCounter,1)
    %DurationCounter
    
    % Now if the duration counter is exceeding the threshold we set
    % above, that means that the current bit onset is the last one of a
    % trigger:
    if i+1<= length(idxOnsets) && DurationCounter >= MaxDuration_n_samples
        
        %disp('inside triggerEnd etc')
        
        % So we log the index of the bit onset as the end of a trigger
        TriggerEnd(TriggersCounter) = idxOnsets(i);
        % If it is the end of a trigger, then it means the next onset will be the onset of a trigger and that hi is
        % will be a new one.
        TriggerStart(TriggersCounter) = idxOnsets(i+1);
        TriggersCounter = TriggersCounter + 1;
    end
end

% But of course, we still need to count the first onset trigger, so I
% append it:
TriggerStart = [idxOnsets(1),TriggerStart];
% And same for the last one:
TriggerEnd = [TriggerEnd,idxOnsets(end)];
%TriggerStart

% Plotting the onset and offset of the detected triggers
if show_plots
    if nargin>3 && show_plots
        figure()
        title('Raw audio signal with detcted onsets and offsets marked', 'FontSize', font_size);
        hold on;
        plot(audio(:,1))
        hold on;
    end
end % if show plots

if show_plots
    if nargin>3 && show_plots
        plot(TriggerStart,ones(length(TriggerStart),1),'k+')
        hold on;
        plot(TriggerEnd,ones(length(TriggerEnd),1),'ro')
        xlabel('Sample nr', 'FontSize', font_size)
        legend({'AudioSignal' 'TriggerSart' 'TriggerEnd'}, 'FontSize', font_size)
    end
end % if show plots



%% Decoding the triggers:
% Now that we have all the starts of the different triggers, we can start
% decoding each. I loop through each start
bit_codes = [];
decTrigger_ctr = 1;


for i = 1:length(TriggerStart)-1
    

    
    % If this triggerStart and triggerEnd has caught two triggers, then we
    % have to split them. This is to mark if we are in the region between
    % the triggers of zeros.
    between_triggers = false;
    
    % I first extract the bits in between the beginning and the end:
    % Adding a sample at the end just to make sure
    CurrentTrigger = denoiseaudio(TriggerStart(i):TriggerEnd(i)+1);
    
    if VERBOSE
        i
        disp('out of')
        (length(TriggerStart)-1)
        disp('Trigger Start: ')
        TriggerStart(i)
        CurrentTrigger
    end
    
    % There are sometimes more than one trigger
    trigger_ctr = 1;
    bits_counter = 1;
    % Loop through the code unitl we have gotten the first 9 bits:
    % ii will be the beginning of a bit
    % We have to loop through each sample bc they are not always the
    % same lengths, and then things get screwed up.
    % We will this save all bits in this structure (will will be a vector
    % if we have one trigger and a matrix if we find two! (one row for
    % each trigger).
    bits = [];
    j = 1;
    bits_counter = 0;
    % There could be a 0 in the beginning of the trigger
    j = 1;
    while CurrentTrigger(j) == 0
        j = j + 1;
    end
    
    
    
    % Loop through the current trigger (between trigger start and trigger
    % end)
    % In each loop, we will decode at least one bit (one bit if it is a 1,
    % and several if there are several zero bits in a row.
    % So, we only need to go through this loop if we have enough samples left
    % to be able to get at least one bit, so only until
    % length(CurrentTrigger) -1.
    while j < length(CurrentTrigger) && trigger_ctr <= 2
        
        sample = CurrentTrigger(j);
        if VERBOSE
            sample
        end
        
        % We are now working with the denoise audio signal
        if sample == 1
            
            if VERBOSE
                disp('found sample = 1')
            end
            
            if between_triggers == true
                if VERBOSE
                    disp('inside between triggers')
                end
                % This is not a real time stamp, only the sample nr
                trigger_start_2nd_trigger = TriggerStart(i) + j - 1;
                between_triggers = false;
            end % between triggers
            
            
   
            %trigger_ctr
            %bits_counter
            bits_counter = bits_counter + 1;
            bits(bits_counter) = 1;
            
            % increment until the bit is over
            while sample == 1 % sometimes there is a zero between the ones and the minus ones
                j = j + 1;
                sample = CurrentTrigger(j);
                if VERBOSE
                    disp('incrementing ones')
                    sample
                end
            end      % incrementing
            while sample == -1
                j = j + 1;
                sample = CurrentTrigger(j);
            end % incrementing
            
            
        elseif sample == 0
            
             if VERBOSE
                bits_counter
             end
            % increment until the bit is over

            sample_ctr = 1;
            while sample == 0 %&& sample_ctr <= n_samples_per_bit + uncertainty_bit_dur
                j = j + 1;
                sample = CurrentTrigger(j);
                sample_ctr = sample_ctr + 1;
                if VERBOSE
                    disp('incrementing zeros')
                end
            end   % incrementing
            % Try to estimate how many zeros there were in a row:
            n_zeros_est = uint16(sample_ctr/double(n_samples_per_bit));
            if VERBOSE
                n_zeros_est 
                sample_ctr
                double(n_samples_per_bit)
            end
            
            if n_zeros_est > nBits -1
                n_zeros_est = nBits -1;
            end
            ctr = 0;
            while ctr < n_zeros_est
                bits_counter = bits_counter + 1;
                bits(bits_counter) = 0;
                ctr = ctr + 1;
            end
             % It can be that this is too strict. If we have too many, it doesn't matter. 
            % but if we have too few, we won't count all..
            %while sample == 0 && sample_ctr <= n_samples_per_bit + uncertainty_bit_dur
            %   j = j + 1;
            %   sample = CurrentTrigger(j);
            %   sample_ctr = sample_ctr + 1;
            %   if VERBOSE
            %       disp('incrementing zeros')
            %   end
            %end   % incrementing
            
            if VERBOSE
                disp('after incrementation')
            end 
            
        end % zero or one sample
        
        % If we collected the first trigger already
        if bits_counter == nBits + 2 % 2 for padding
            
             if VERBOSE
                 disp('Into saving the trigger')
             end 
            % Decode the trigger (only the 7 bits in the middle)
            
            bit_code = bits(2:2+nBits-1);
            dec_code = bin2dec(num2str(bit_code));
            
            % As a final step, we can decode the 8bit code to a decimal trigger:
            decTrigger(decTrigger_ctr,1) = dec_code;
            % save the trigger
            if trigger_ctr == 1
                % Convert nr of samples to ms
                t_start = TriggerStart(i);
                t_end = (TriggerStart(i) + j - 1);
                decTrigger(decTrigger_ctr,2) = t_start*ms_per_sample;
                % j - 1 is how many samples which have passed
                % Duration
                decTrigger(decTrigger_ctr,3) = (t_end - t_start)*ms_per_sample; % ms
                decTrigger(decTrigger_ctr,4) = t_start; % sample nr
                decTrigger(decTrigger_ctr,5) = t_end - t_start;
                decTrigger(decTrigger_ctr,6) = str2num(strjoin(string(bits), ''));
                decTrigger_ctr = decTrigger_ctr + 1;
                bits_counter = 0;
            else
                
                t_start =  trigger_start_2nd_trigger;
                t_end = TriggerEnd(i);
                decTrigger(decTrigger_ctr,2) = t_start*ms_per_sample;
                decTrigger(decTrigger_ctr,3) = (t_end - t_start)*ms_per_sample;
                decTrigger(decTrigger_ctr,4) = t_start; % sample nr
                decTrigger(decTrigger_ctr,5) = t_end - t_start;
                decTrigger(decTrigger_ctr,6) = str2num(strjoin(string(bits), ''));
                decTrigger_ctr = decTrigger_ctr + 1;
                
                
            end % end trigger_ctrs
            
            % There could be a zero or two at the end. We don't want
            % to go into the loop again if we are finished. Or a couple
            % between the current and next trigger.
            while CurrentTrigger(j) == 0 && j < length(CurrentTrigger)
                %disp('inside incrementing zeros at the end of a trigger')
                j = j + 1;
                %j
            end
            
            
            between_triggers = true;
            
            trigger_ctr = trigger_ctr + 1;
            
            
        end % end if a whole 9 bit trigger has been found
        
        
    end % end while j
    
end  % end for trigger starts i
end % function
