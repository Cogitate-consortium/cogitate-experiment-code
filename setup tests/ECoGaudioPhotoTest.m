% This script presents triggers in a loop (audio and photodiode triggers)

function ECoGaudioPhotoTest

% Clear the workspace and the screen
sca;
close all;
%clearvars;

test_response = 1;
test_triggers = 1;

% Adding path to the functions
addpath ..\functions

% Time to test triggers:
trigger_test_time = 0.5; % minutes
debug = 0;
%photo_diode_size = 500;
photo_diode_size = 100;



compKbDevice = -1;

%% Response params
% This will have to be replaced by the keys of the response box
% TODO: Fix this
KbName('UnifyKeyNames');
%OneKey      =  KbName('1');
%TwoKey      =  KbName('2');
%ThreeKey    =  KbName('3');
%FourKey     =  KbName('4');

OneKey = KbName('1!');
TwoKey = KbName('2@');

ThreeKey    =  KbName('3#');
FourKey     =  KbName('4$');
SixKey      =  KbName('6^');
EightKey      =  KbName('8*');


abortKey    =  KbName('ESCAPE'); % ESC aborts experiment

%% Setting up PTB
% Here we call some default settings for setting up Psychtoolbox
PsychDefaultSetup(2);

% Get the screen numbers. This gives us a number for each of the screens
% attached to our computer.
screens = Screen('Screens');

% To draw we select the maximum of these numbers. So in a situation where we
% have two screens attached to our monitor we will draw to the external
% screen.
screenNumber = max(screens);

% Define black and white (white will be 1 and black 0). This is because
% in general luminace values are defined between 0 and 1 with 255 steps in
% between. All values in Psychtoolbox are defined between 0 and 1
white = WhiteIndex(screenNumber);
black = BlackIndex(screenNumber);

% Do a simply calculation to calculate the luminance value for grey. This
% will be half the luminace values for white
gray            =   [125,125,125];
% Open an on screen window using PsychImaging and color it grey.
Screen('Preference', 'SkipSyncTests', 1);
Screen('Preference', 'TextRenderer', 1);
if debug
    WINDOW_RESOLUTION = [100 100 600 900];
    [w, wRect] =  Screen('OpenWindow',screenNumber, gray,WINDOW_RESOLUTION);
else
    % Opening a gray window fullscreen:
    [w, wRect]  =  Screen('OpenWindow',screenNumber, gray);
end

% Getting the dimensions of the window:
ScreenWidth     =  wRect(3);
ScreenHeight    =  wRect(4);
center          =  [ScreenWidth/2; ScreenHeight/2];

%% Trigger parameters:
%DIOD_SIZE = 100; % Size of the photodiode trigger in pixels
DIOD_SIZE = photo_diode_size;
DIOD_ON_COLOUR = 255; % Color of the photodiode when on
DIOD_OFF_COLOUR= 0;
corner = [ScreenWidth; ScreenHeight]; % Coordinates of the corner of the screen
xDiode = transpose(corner) - [DIOD_SIZE/2 DIOD_SIZE/2]; % Getting the x coordinates of the photodiode
yDiode = transpose(corner); % Getting the Y coordinates of the photodiode

%% Text parameters:
% Creating the text messages to be presented:
IntroText = 'Welcome to the audio and photo test.\n You will start by testing the response box.\n Press any key to pursue';
ResponseTestIntroText = 'Press the response keys or press escape to pursue to the trigger test. \nIf nothing happens when pressing the response keys, try flipping the box';
KeyPressText = 'This key is working';
TriggerTestIntroTest = 'You will now enter the trigger test, press any key to pursue. \n Once you are done, press escape to terminate this script';
% Converting them to doubles:
IntroText = double(IntroText);
ResponseTestIntroText = double(ResponseTestIntroText);
KeyPressText = double(KeyPressText);
TriggerTestIntroTest = double(TriggerTestIntroTest);

% Setting the parameters for the visual aspect of the text:
fontType = 'David'; % Font of the text
fontSize = 50; % general text size
fontColor = 0; % black;
screenScaler = ScreenWidth/1920; % Setting screen scaler to adapt text size to different screen sizes
Screen('TextFont',w, fontType); % Setting the font type
Screen('TextStyle', w, 0); % Text style
Screen('TextSize', w, round(fontSize*screenScaler)); % Text size
text.Color = fontColor; %black

if test_response

    %% Response loop:
    % Present a small instruction text
    Screen('FillRect', w, gray);
    DrawFormattedText(w, IntroText, 'center', 'center', text.Color);
    Screen('Flip', w);
    % Wait for any key press
    KbWait(compKbDevice,3);
    KbReleaseWait(compKbDevice);

    Abort = 0;
    try
        % Setting flag to avoid repeating the same action many times:
        introTextShown = 0;

        % Now we start looping until the escape key is pressed
        while ~Abort

            % First, we show a text telling the experimenter what to do:
            if ~introTextShown
                Screen('FillRect', w, gray);
                DrawFormattedText(w, ResponseTestIntroText, 'center', 'center', text.Color);
                Screen('Flip', w);
                introTextShown = 1;
            end

            % Then at each loop iteration, check whether there was an input:
            [KeyIsDown, ~, Resp1] = KbCheck(compKbDevice);

            % If the key was indeed pressed:
            if KeyIsDown
                % If it was one of the activated key, something appears on the
                % screen:
                if Resp1(OneKey) || Resp1(TwoKey) || Resp1(ThreeKey) || Resp1(FourKey) || Resp1(SixKey) || Resp1(EightKey)
                    % Make the screen gray:
                    Screen('FillRect', w, gray);
                    % Draw the text on top
                    if Resp1(ThreeKey)
                        KeyPressText = 'BLUE button pressed!';
                    elseif Resp1(FourKey)
                        KeyPressText = 'PINK button pressed!';
                    elseif Resp1(SixKey)
                        KeyPressText = 'YELLOW button pressed (right arc)';
                    elseif Resp1(EightKey)
                        KeyPressText = 'WHITE button pressed (right arc)';
                    elseif Resp1(OneKey)
                        KeyPressText = 'WHITE button pressed (left arc)';
                    elseif Resp1(TwoKey)
                        KeyPressText = 'YELLOW button pressed (left arc)';
                    end

                        
                    DrawFormattedText(w, KeyPressText, 'center', 'center', text.Color);
                    % Flip the screen:
                    Screen('Flip', w);
                    % Make a pause of 1 seconds, to have time to look at it
                    WaitSecs(2);
                    % Turn the screen back to gray
                    Screen('FillRect', w, gray);
                    % Flip the screen:
                    Screen('Flip', w);
                elseif Resp1(abortKey) % If the abort key is pressed, abort this loop
                    Abort = 1;
                    % Need to wait for the key to be released, to avoid in the
                    % next loop that the key is still pressed!
                    KbReleaseWait(compKbDevice);
                end
            end
        end
    catch ME% If something went wrong, close everything and abort:
        warning(ME.message)
        % Clear the screen.
        sca;
        return
    end
    
end % if test response


%% Triggers loop:
% In this loop, photodiode triggers are being sent until the
% experimenter aborts:
Abort = 0;

% Present a small instruction text
Screen('FillRect', w, gray);
DrawFormattedText(w, TriggerTestIntroTest, 'center', 'center', text.Color);
Screen('Flip', w);
% Wait for any key press
KbWait(compKbDevice,3);
KbReleaseWait(compKbDevice);
elapsedTimeTotal = 0;

if test_triggers

    try
        while (elapsedTimeTotal < trigger_test_time*60) && ~Abort % Until the experiment hits escape:
            elapsedTime = 0; % Initiate time counter
            ResponseFlag = 0; % Set flag for response
            PhotodiodeFlag = 0; % Set flag for photodiode

            Screen('FillRect', w, DIOD_ON_COLOUR, [xDiode yDiode]);
            [~, loopOnset] = Screen('Flip', w,[],1);

            % Looping for 4000msec
            while elapsedTime < 4

                % If 250msec have passed, return the photodiode to black
                if elapsedTime > 3.25 && ~PhotodiodeFlag
                    Screen('FillRect', w, DIOD_OFF_COLOUR, [xDiode yDiode]);
                    Screen('Flip', w,[],1);
                    PhotodiodeFlag = 1;
                end

                % Checking for patient response:
                if ~ResponseFlag
                    [KeyIsDown, ~, Resp1] = KbCheck(compKbDevice);
                    % If the participant pressed a key
                    if KeyIsDown && ResponseFlag ~=1
                        if Resp1(abortKey)
                            Abort = 1;
                        end
                    end
                end
                % Actualize clock:
                elapsedTime = GetSecs - loopOnset;

            end

            elapsedTimeTotal = elapsedTimeTotal + (GetSecs - loopOnset);

        end
        sca;
    catch ME
        warning(ME.message)
        % Clear the screen.
        sca;
        return
    end
    
end % if test triggers

end % end function







