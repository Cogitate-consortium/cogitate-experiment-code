% This function decodes the audio triggers bit codes.
% Author: Alex Lepauvre
% Date: 28/05/2020
% Input: 
% - audio: the audio signal to be analyzed
% - nBits: number of bits of the audio code (with the flanks!)
% - bitDuration: duration of a single bit
% - audioSR: sampling rate of the audio signal
% - Plotting: command to plot the data or not: 'on' = figs will be plotted
% output:
% - decTrigger: audio triggers in decimals, together with the time stamps
% of each trigger
% - decTrigger: [audio triggers in decimals; time stamps; duration; bit_code with padding]

function decTrigger = audioBitDecoder(audio,nBits,bitDuration,audioSR,volume,Plotting)

% To make things a bit easier to work with, I downsample the signal to be
% 1000Hz. 
audio1000Hz = resample(audio,1000,audioSR);

if nargin>3 && strcmp(Plotting,'on')
    figure()
    title('audio resampled to 1000 Hz');
    hold on;
    plot(audio1000Hz(:,1))
    hold on
end
% Then, I filter the signal: everything thats above or below a certain threshold is 1 or -1, the rest is 0:
audio1000HzPlusOnes = double(audio1000Hz(:,1)>4*10^6);
audio1000HzMinusOnes = double(audio1000Hz(:,1)<-4*10^6);

% I then combine the two by adding them up:
denoiseaudio1000Hz = audio1000HzPlusOnes-audio1000HzMinusOnes;
% I then compute the diff of the audio signal. The reason for that is that
% we only want the onsets of the bits to detect them. This will get
% relevant later on
diffdenoiseaudio1000Hz = diff(denoiseaudio1000Hz);

% The next step is to isolate each triggers. Between each triggers, we have
% a set amount of time but within a trigger, max length of 0 in a row is
% the total number of bits in the code (assuming a code of 0). So I set as
% a threshold the max number of time of an entire trigger. Anyhing that
% will have more zeros in a row is an intertriggers interval. 
MaxDuration = (bitDuration)*nBits;
MaxDurationInSample = MaxDuration/1;


% I detect the different potential onset:
% Wherever we have a minus 1, we have a bit starting. 
idxOnsets = find(diffdenoiseaudio1000Hz<=-1);

% I set a counter for the triggers
TriggersCounter = 1;
% I then loop through each teh potential onsets
for i=1:length(idxOnsets)
    % I set a DurationCounter that progresses until something else than a
    % zero is detected:
    DurationCounter = 1;
    % I then start a while loop that progresses until the next onset is
    % found:
    while idxOnsets(i)+DurationCounter<=length(diffdenoiseaudio1000Hz)&& ...
            diffdenoiseaudio1000Hz(idxOnsets(i)+DurationCounter,1) < 1
        DurationCounter = DurationCounter + 1;
    end
    
    % Now if the duration counter is superior to the threshold we set
    % above, that means that the current bit onset is the last one of a
    % bunch:
    if i+1<= length(idxOnsets) && DurationCounter >= MaxDurationInSample
        % So we log the index of the bit onset as the end of a trigger
        TriggerEnd(TriggersCounter) = idxOnsets(i);
        % If it is the end of a bit, then it means the next bit coming up
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

% Plotting the onset and offset of the detected triggers
if nargin>3 && strcmp(Plotting,'on')
    plot(TriggerStart,ones(length(TriggerStart),1),'k+')
    hold on
    plot(TriggerEnd,ones(length(TriggerEnd),1),'ro')
    xlabel('Timestamp')
    legend({'AudioSignal' 'TriggerSart' 'TriggerEnd'})
end

% Now that we have all the starts of the different triggers, we can start
% decoding each. I loop through each start
for i = 1:length(TriggerStart)-1

     % I first extract the bits in between the beginning and the end:
     CurrentTrigger = denoiseaudio1000Hz(TriggerStart(i):TriggerEnd(i)+1);
     
     % Since we know the exact duration of each bit (assuming nothing goes
     % wrong in the audio signal), we can segment the trigger in bits and
     % then in each bit detect the peak.
     bitsCounter = 1;
     % I go bit by bit:
     for ii = 0:bitDuration:length(CurrentTrigger)-bitDuration
         % If the current trigger is longer than what it should be, that
         % means that there were interferences, two triggers kind of
         % merged. In such a scenarion, the trigger needs to be splitted in
         % two:
         
         % If in the current bit, we have something that is equal to 1,
         % then we are at a 1 bit. So I take the sum of the things that are
         % superior to 0 (i.e. equal to one or above). If we have more
         % than two ones, then we are sure we are in a 1 bit. The reason
         % for the >=2 is because in case the sequencing is off, and a 0
         % bit has a few zeros in there, we don't want to count it:
         % The 2 could be replaced by 4 or 5 if needs to be less strict
         leakage = 2;
        bits(1,bitsCounter) = double(sum(CurrentTrigger(ii+2:ii+bitDuration+1,1)>0)>=leakage);
        bitsCounter = bitsCounter + 1;
     end
     % Now that we have all the bits, we are only interested at the nbit
     % code. So we have to discard the first bit because it is just marking
     % the onset of the trigger. After that, we count 8:
     BitsCode(i,:) = bits(1,2:2+nBits-1); 
     
     % As a final step, we can decode the 8bit code to a decimal trigger:
     decTrigger(i,1) = bin2dec(num2str(BitsCode(i,:)));
     
     % I also add the timestamp next to it:
     decTrigger(i,2) = TriggerStart(i);
     
     % I also add the duration of the trigger:
     decTrigger(i,3) = TriggerEnd(i) - TriggerStart(i);

    % And the bit code
    decTrigger(i,4) = str2num(strjoin(string(BitsCode(i,:)), ''));

end

end
