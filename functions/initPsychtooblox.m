

function screenError = initPsychtooblox()
    
    disp('WELCOME to initPsychtooblox')
% INITPSYCHTOOLBOX Initilizes Psychtoolbox and opens graphics window
% output:
% -------
% * Opens a psychtoolbox window *
% screenError - TRUE or FALSE answer if the function did not succeed in
% opening a 100Hz window.

    global DEBUG ScreenHeight stimSizeHeight ScreenWidth refRate screenScaler fontType fontSize fontColor text gray w REF_RATE_OPTIMAL center TRUE FALSE
    global WINDOW_RESOLUTION debugFactor NO_FULLSCREEN  VIEWING_DISTANCE MAX_VISUAL_ANGEL STIM_DURATION TRIAL_DURATION MRI_BASELINE_PERIOD ppd cx cy
    global NO_AUDIO VOLUME SAMPLE_RATE NR_CHANNELS pahandle% These are required for the sound display

    PsychDefaultSetup(2);
    
    % Set sync tests:
    try
        if DEBUG Screen('Preference', 'SkipSyncTests', 1); else Screen('Preference', 'SkipSyncTests', 0); end
    catch
        if DEBUG Screen('Preference', 'SkipSyncTests', 1); else Screen('Preference', 'SkipSyncTests', 0); end
    end
   
    % Setting the verbosity to high:
    Screen('Preference', 'VisualDebugLevel', 4);

    screens         =   Screen('Screens');
    screenNumber    =   max(screens);
    %screenNumber   =   1;
    %gray           =   round(GrayIndex(screenNumber)); % 128
    gray            =   [125,125,125];

    % Finding the screen size, current resolution and refresh rate:
    try
        if NO_FULLSCREEN [w, wRect]  =  Screen('OpenWindow',screenNumber, gray, WINDOW_RESOLUTION); else [w, wRect]  =  Screen('OpenWindow',screenNumber, gray); end
    catch
        if NO_FULLSCREEN [w, wRect]  =  Screen('OpenWindow',screenNumber, gray, WINDOW_RESOLUTION); else [w, wRect]  =  Screen('OpenWindow',screenNumber, gray); end
    end
    ScreenWidth     =  wRect(3);
    ScreenHeight    =  wRect(4);
    center          =  [ScreenWidth/2; ScreenHeight/2];
    hz = Screen('NominalFrameRate', w);
    disp(hz);
    refRate = hz.^(-1);
    if DEBUG == 2 refRate = refRate / debugFactor ; end
    sca;
    
    %trying to set the refresh rate to optimal
    if ~DEBUG
        try
            SetResolution(screenNumber,ScreenWidth,ScreenHeight,(REF_RATE_OPTIMAL));
            screenError = FALSE;
        catch
            screenError = TRUE;
        end
    else
        screenError = TRUE;
    end
    
    % We can now actualize the stimuli duration:
    % It needs to be a multiple of the frame rate:
    DurationCoefficients = round((STIM_DURATION )/refRate);
    STIM_DURATION = DurationCoefficients*refRate;
    
    % We can now actualize the trial duration:
    % It needs to be a multiple of the frame rate:
    DurationCoefficients_trial = round((TRIAL_DURATION )/refRate);
    TRIAL_DURATION = DurationCoefficients_trial*refRate;
    
    % We can now actualize the MRI baseline period:
    % It needs to be a multiple of the frame rate:
    DurationCoefficients_baseline = round((MRI_BASELINE_PERIOD)/refRate);
    MRI_BASELINE_PERIOD = DurationCoefficients_baseline*refRate;
    
    screenScaler = ScreenWidth/1920; % allows scaling so that with smaller screens, objects will be of smaller sizes (1 = Full HD)

    % Opening the window for the experiment:
    try
        if NO_FULLSCREEN [w, wRect]  =  Screen('OpenWindow',screenNumber, gray, WINDOW_RESOLUTION); else [w, wRect]  =  Screen('OpenWindow',screenNumber, gray); end
    catch
        if NO_FULLSCREEN [w, wRect]  =  Screen('OpenWindow',screenNumber, gray, WINDOW_RESOLUTION); else [w, wRect]  =  Screen('OpenWindow',screenNumber, gray); end
    end

    if ~NO_FULLSCREEN
        HideCursor;
    end

    % Stimuli size by visual angle, based on viewing distance and physical
    % size of screen (MAKE SURE YOU INPUT THESE IN)
    stimSizeHeight = getVisualAngel(VIEWING_DISTANCE,MAX_VISUAL_ANGEL(1)); % in px

    % Setting the text renderer:
    Screen('Preference', 'TextRenderer', 1);
    %Screen('Preference','TextEncodingLocale','UTF-8');

    % this enables us to use the alpha transparency
    Screen('BlendFunction', w, 'GL_SRC_ALPHA', 'GL_ONE_MINUS_SRC_ALPHA', [gray 128]);

    % Setting priority to max, such that the experiment has full priority
    % on the CPU:
    priorityLevel=MaxPriority(w);
    Priority(priorityLevel);

    %% Text params

    Screen('TextFont',w, fontType);
    Screen('TextStyle', w, 0);
    Screen('TextSize', w, round(fontSize*screenScaler));
    text.Color = fontColor; %black
    %% Fixation
    ppd = getVisualAngel(VIEWING_DISTANCE,1); % pixel per degree

    %% PRELIMINTY PREPATATION
    % check for Opengl compatibility, abort otherwise
    AssertOpenGL;

    rng('default');
    % Reseed the random-number generator for each expt.
    rng('Shuffle');

    % Do dummy calls to GetSecs, WaitSecs, KbCheck
    KbCheck;
    WaitSecs(0.1);
    GetSecs;
    
    % Listening for keyboard inputs:
    if ~NO_FULLSCREEN
        ListenChar(2);
    end
    
 
    %% Initiate system for sound signal
    if ~NO_AUDIO
        InitializePsychSound(1);
        
        % Open Psych-Audio port, with the follow arguements
        % (1) [] = default sound device
        % (2) 1 = sound playback only
        % (3) 1 = default level of latency
        % (4) Requested frequency in samples per second
        % (5) 2 = stereo putput
        pahandle = PsychPortAudio('Open', [], 1, 3, SAMPLE_RATE, NR_CHANNELS);
        
        % Set the volume to half for this demo
        PsychPortAudio('Volume', pahandle, VOLUME);
    end
    

end
