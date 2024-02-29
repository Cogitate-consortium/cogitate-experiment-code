% SENDTRIGAUDIO
% Send a bit code trigger through the adio port
function [] = sendTrigAudio(trig_code_binary)


    % These variables are for debugging
    global triggersAudio 
    global VERBOSE
    global pahandle
    global zerowave squarewave
    global triggsAudioCounter
    if VERBOSE disp('Into sendTrigAudio'); end
    if VERBOSE display(triggersAudio,'triggersAudio beginning of sendTrigAudio'); end


    if VERBOSE 
        display(trig_code_binary, 'trig_code_binary');
        %sdisplay(size(trig_code_binary,2), 'size(trig_code_binary)');
    end
    
    % For debugging:
    %audreptrigger = [squarewave; zerowave; zerowave; zerowave; squarewave; squarewave; squarewave; squarewave];
    %save SashaAudiotrigger.mat audreptrigger;
    
    
    % Adding a 1 in the beginning to communicate we're starting the code
    audio_trigger = squarewave;
    % Consruct the trigger wave:
    for bit_ind = 1:size(trig_code_binary,2)
        %if VERBOSE disp(bit_ind); end
        wave = squarewave;
        bit = trig_code_binary(bit_ind);
        if bit == '0'
            wave = zerowave;
        end % for
        if VERBOSE disp(size(audio_trigger)); end
        audio_trigger =  [audio_trigger; wave]; 
        
    end % end for
    
    % Adding a 1 at the end to indicate the end of the code
    audio_trigger = [audio_trigger; squarewave];
    
    % For debugging: Make sure this looks the same:
    %save MyAudiotrigger.mat audio_trigger;
    
    PsychPortAudio('FillBuffer', pahandle, audio_trigger');
    % %play sound in port
    AudioTimeStamp = PsychPortAudio('Start',pahandle, 1, 0);
    % Handling the timing like advised above triggers
    % Commeting this out now so that the program will continue while the
    % sound is being played
    %[actual_start_time, ~, ~, estStopTime] = PsychPortAudio('Stop', pahandle, 1, 1);
    %if VERBOSE display(actual_start_time, 'startTime'); end
    
     %triggsAudioCounter = triggsAudioCounter + 1;
     %triggersAudioTimes(triggsAudioCounter,2) = actual_start_time - triggsAudioStart;
    % Storing the triggers to the trigersAudio Matrix:
     triggersAudio{triggsAudioCounter,1} = cellstr(trig_code_binary);
     triggersAudio{triggsAudioCounter,2} = AudioTimeStamp;
   
     
     triggsAudioCounter = triggsAudioCounter+1;
     
     if VERBOSE 
        display(triggersAudio,'triggersAudio');
         disp('end of sendTrigAudio');
     end  
   
end
