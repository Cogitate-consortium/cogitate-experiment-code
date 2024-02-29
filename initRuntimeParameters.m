% In this script, the experiment runtime parameters are being set. This
% is where the recording modes, hardware and physical parameters are being
% set. Before running the experiment, this file should be checked to be
% sure that the parameters are set correctly.
% This script has no input nor ouput. Every parameters being set are
% global variables. This means that they can be called by the different
% functions. 
function initRuntimeParameters
% Recording modalities:
global ECoG MEEG fMRI Behavior EYE_TRACKER    
% Photodiode parameters:
global PHOTODIODE DIOD_DURATION DIOD_SIZE DIOD_ON_COLOUR DIOD_OFF_COLOUR 
% MEEG Parameters:
global EEG_MACHINE_HEX 
% ECoG parameters:
global BIT_DURATION RESP_TRIG_ONSET
% Eyetracker parameters:
global Eyetrackerdummymode DISTANCE_SCREEN_TRACKER HEAD_FIXED calibration_area TOBII_EYETRACKER
% Hardware parameters:
global SCREEN_SIZE_CM REF_RATE_OPTIMAL viewDistanceBottomTop VIEW_DISTANCE
% Identifier for the labs:
global LAB_ID   
% Debugging and code parameters:
global VERBOSE NO_PRACTICE DEBUG RESOLUTION_FORCE NO_FULLSCREEN NO_AUDIO WINDOW_RESOLUTION NO_ERROR VERBOSE_PLUS 
% Matrix generation
global MATRIX_GENERATION PREEXISTING_MATRICES

% Legend: false = 0 | true = 1  

%% Recording modalities
MEEG = 0; % Set to 1 if recording with MEEG
fMRI = 0; % Set to 1 if recording with fMRI
EYE_TRACKER = 0; % Must be set to 1 if recording with Eyetracker
ECoG = 1; % ; Set to 1 if recording with ECoG
Behavior = 0; %Set to 1 if recording with Behavior only


%% Hardware and physical parameters:
REF_RATE_OPTIMAL = 60; % in Hz. Screen refresh rate.
VIEW_DISTANCE = 60; % Default viewing distance (if no viewDist argument sent with the function call)
SCREEN_SIZE_CM = [34.5 19.5]; % screen [width, height] in centimeters, change it to fit your setting
viewDistanceBottomTop = [144 144]; % IN CM!! Distance between the participant head and the top and bottom of the screen. Only needed if HEAD_FIXED on.

%% Eyetracker parameters:
DISTANCE_SCREEN_TRACKER = 90; % Distance between the eyetracker lense and the computer screen. Only needed for REMOTE MODE!
Eyetrackerdummymode = 0; % Dummy mode of the eyetracker: MUST BE SET TO 0 TO RUN THE EXPERIMENT
HEAD_FIXED = 0; % Head fixed must be set to 0 if remote mode
TOBII_EYETRACKER = 0; 
calibration_area = [0.75, 0.70];

%% MEEG parameters:
EEG_MACHINE_HEX = 'DFF8';

%% Identifier of the lab:
LAB_ID = 'SF';

%% ECoG parameters:
BIT_DURATION = 0.020; % Duration of single bit in ECoG audio triggers
RESP_TRIG_ONSET=0.2; % Lower time threshold to send audio resp trigger (in seconds)

%% Photodiode parameters:
PHOTODIODE = 0; % Must be set to 1 for the photodiode to be presented
DIOD_ON_COLOUR = 255; % Color of the photodiode when turned on (255 white, 0 black)
DIOD_OFF_COLOUR = 1;  % Color of the photodiode when off (255 white, 0 black)
DIOD_SIZE = 100; % Size of the square where the photodiode is presented (in pixels)
DIOD_DURATION = 3; % Duration of the photodiode flash when turned on (in frames)

%% DEBUG parameters
DEBUG = 0; % 0 = no debug | 1 = regular debug | 2 = fast debug
VERBOSE = 0; % Yoav: Katarina, if you have the time please encase the displays in if VERBOSE disp('hello'); end
VERBOSE_PLUS = 0; % for debugging duration balance only
NO_PRACTICE = 1; % skip the practice run
RESOLUTION_FORCE = 0; % the program will complain if optimal refresh rate is not possible on this screen
NO_FULLSCREEN = 0; % enable windowed mode for dubugging
NO_AUDIO = 1; % Disable audio for dubugging BIT_DURATION: duration of the single bits in ms
NO_ERROR = 0; % Disable testing program error throws
% Q: Do I need to fill this out? Pixels? Yoav: only if you want the debug scree to be of a different size
WINDOW_RESOLUTION = [10 10 1200 800];

%% Matrix generation
MATRIX_GENERATION = 0;
PREEXISTING_MATRICES = 1;

end