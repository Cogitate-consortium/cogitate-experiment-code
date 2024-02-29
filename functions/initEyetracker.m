% INITEYETRACKER
% This function initialize the eyetracker
function [] = initEyetracker(BlockNum)
global el edfFile w subjectNum Eyetrackerdummymode SCREEN_SIZE_CM LAB_ID HEAD_FIXED viewDistanceBottomTop  DISTANCE_SCREEN_TRACKER calibration_area fMRI MEEG

% Initializing eyelink
Eyelink('Initialize');

% Initializes Eyelink and Ethernet system. Opens tracker connection, reports any problems.
%window (above) is the window you set with the function Screen(\91OpenWindow\92)
el = EyelinkInitDefaults(w);

% Initialize eyelink defaults and control code structure. If window is set, pixel coordinates are sent to eyetracker
% Set calibration tones. It is important to switch them of like this. Otherwise loud beeps during calibration when subject is wearing headphones.
% Parameters are in frequency, volume, and duration.
% Set the second value in each line to 0 to turn off the sound.
%     calib_vol=0.00;
%     el.cal_target_beep=[600 calib_vol 0.05];
%     el.drift_correction_target_beep=[600 calib_vol 0.05];
%     el.calibration_failed_beep=[400 calib_vol 0.25];
%     el.calibration_success_beep=[800 calib_vol 0.25];
%     el.drift_correction_failed_beep=[400 calib_vol 0.25];
%     el.drift_correction_success_beep=[800 calib_vol 0.25];
%
%
%     % you must call this function to apply the changes from above

% Setting the calibration background color to the same as during our
% experiment.
%Eyelink('command','screen_pixel_coords = %ld %ld %ld %ld', 0, 0,ScreenResolution(1,1)-1 ,ScreenResolution(1,2)-1 );

el.backgroundcolour = [125 125 125];
EyelinkUpdateDefaults(el);



% Initialization of the connection with the Eyelink Gazetracker. Exit program if this fails.
%if ~EyelinkInit(dummymode, 1)
if ~EyelinkInit(Eyetrackerdummymode, 1)
    fprintf('Eyelink Init aborted.\n');
    cleanup;% cleanup function
    return;
end

% name and open file to record data to
%VERY IMPORTANT: THE NAME OF THE FILE SHOULD BE SHORT (5-6 CHARACTERS), OTHERWISE IT WILL GIVE AN ERROR AND IT WON\92T SAVE THE FILE!!!
edfFile = sprintf('%s%d%d.edf',LAB_ID,subjectNum,BlockNum);
Eyelink('Openfile', edfFile);

if strcmp(LAB_ID, 'SE') % Camera lens is 16 mm
  s = Eyelink('command','camera_lens_focal_length = 17');
  if(s ==0)
      disp('set camera lens successfully')
  else
      disp('camera lens was not set successfully')
  end
end

if strcmp(LAB_ID, 'SE')
    s = Eyelink('Command', 'screen_write_prescale = 4'); 
    disp(s);
end

%Reduce FOV, THIS IS FOR fMRI POTENTIALLY
s = Eyelink('command', ['calibration_area_proportion ', num2str(calibration_area)]); % Default values:   0.88 0.83
disp(s)
Eyelink('command', ['validation_area_proportion ', num2str(calibration_area)]); % Default values:   0.88 0.83

% Setting the eyetracker to binocular
if MEEG
Eyelink('command','binocular_enabled = YES')
else
Eyelink('command','binocular_enabled = NO')
end

% Setting calibration to 9 dots
if  fMRI
Eyelink('command','calibration_type = HV5')
elseif MEEG
Eyelink('command','calibration_type = HV9')
else
Eyelink('command','calibration_type = HV13')
end

% Setting calibration to manual
Eyelink('command','enable_automatic_calibration = NO')
% Setting the thresholds for saccade and other events
% (see: http://download.sr-support.com/dispdoc/cmds9.html)
Eyelink('command','select_parser_configuration = 0') % 0 for standard, 1 for psychophysics!
% Here are the different parsers if you want to set them 1 by 1. If you do,
% place these commands after the command above, otherwise they will be
% overwritten!:
% Eyelink('command', 'saccade_velocity_threshold = 22');
% Eyelink('command', 'saccade_acceleration_threshold = 3800');
% Eyelink('command', 'saccade_motion_threshold=0.0');
% Eyelink('command', 'saccade_pursuit_fixup=60');
% Eyelink('command', 'fixation_update_interval=0');
% Eyelink('command', 'fixation_update_accumulate=0');


% We have to retrieve a few information concerning the tracker. It is
% likely that some labs will be using old version of the tracker. And in
% some versions the parameters are a bit different, and some parameters
% cannot be set from the experiment PC.
% Retrieving the tracker version and the tracker software version
[v,vs] = Eyelink('GetTrackerVersion');
fprintf('Running the experiment on a ''%s'' tracker.\n',vs)
vsn = regexp(vs,'\d','match'); % This command won't work on Eyelink I
% So
if isempty(vsn)
    eyelinkI = 1;
else
    eyelinkI = 0;
end

% I am now specifying what event should be
% recorded. In the output file, left and right eye fixation, saccade and
% blinks will be recorded. Also, the messages, button press and inputs will
% be recorded as events.
Eyelink('command', 'file_event_filter = LEFT,RIGHT,FIXATION,SACCADE,BLINK,MESSAGE,BUTTON,INPUT');
% Setting the type of data to be recorded: Gaze for left and right
% eye, area of the pupils. The other are here in case we decide we would
% like to analyze them at a later point!
Eyelink('command', 'file_sample_data  = LEFT,RIGHT,GAZE,HREF,RAW,AREA,HTARGET,GAZERES,BUTTON,STATUS,INPUT');

% Adding link events and samples: in case we later decide to do online
% stuffs. These commands make it possible to call the different events and
% data through the link, to call them during the experiment and use them as
% we are running:
Eyelink('command', 'link_event_filter = LEFT,RIGHT,FIXATION,SACCADE,BLINK,MESSAGE,BUTTON,FIXUPDATE,INPUT');
Eyelink('command', 'link_sample_data  = LEFT,RIGHT,GAZE,GAZERES,AREA,HTARGET,STATUS,INPUT');


% Setting the pupil tracking mode and sampling rate. If  the tracker
% version is older than 3, this won't work

if strcmp(LAB_ID, 'SE')
    Eyelink('command', 'sample_rate = 500');
else 
    if v >= 3 
        Eyelink('command', 'sample_rate = 1000');
        if ~fMRI
            % Setting the tracking mod to centroid and not ellipses
            Eyelink('command', 'use_ellipse_fitter = no');
        end
    else
        % If the tracker is older, tell display warnings to inform the
        % experimenter
        warning('Fail to set sampling rate and centroid mode, due to your tracker version')
        warning('Make sure to set the sampling rate to 500Hz and the pupil tracking to centroid on the tracker display!')
    end
end

% Setting the Illumination to 75%:
% But if the tracker is too old, this won't be possible either. If the
% tracker version is 4.2 or above, then it will work
if strcmp(LAB_ID, 'SE')
   Eyelink('command', 'elcl_tt_power = 1');
else
if ~ eyelinkI && (str2double(vsn{1}) == 4 && str2double(vsn{1}) >= 2) || ...
        str2double(vsn{1}) > 4
    % 1 is 100% illumination, 2 is 75 and 3 50%
    Eyelink('command', 'elcl_tt_power = 2');
else
    % If the tracker is older, tell display warnings to inform the
    % experimenter
    warning('Fail to set illumination, due to your tracker version')
    warning('Make sure to set the illumination to 75% on the tracker display!')
end

% The eyetracker needs a few parameters about the setup to be able to
% compute the gaze and also the parser accurately.
% First, the screen size. It consists of four values, which are the
% distance between the center of the screen and the corners [<left>, <top>, <right>, <bottom>]:
SCREENPHYSICALCOORDINATES = [-SCREEN_SIZE_CM(1,1)/2, SCREEN_SIZE_CM(1,2)/2,...
    SCREEN_SIZE_CM(1,1)/2, -SCREEN_SIZE_CM(1,2)/2]*10; % NEEDS TO BE IN MM
Eyelink('command',' screen_phys_coords=%s',num2str(SCREENPHYSICALCOORDINATES))% : to set the screen size in mm
% Eyelink('command','screen_pixel_coords= %ld %ld %ld %ld',SCREENCOORDINATES)% : to set the resolution. This does not need to be sent, it is done in the background

% Setting physical distance between screen and participant or screen and
% tracker depending on whether we are in head fixed or remote mode:
if HEAD_FIXED
    Eyelink('command',' screen_distance=%s',num2str(viewDistanceBottomTop*10))
else 
    EyelinkDistanceTrackerScreen = sprintf('remote_camera_position = -10 17 80 60 -%s',num2str(DISTANCE_SCREEN_TRACKER*10));
    status = Eyelink('Command', EyelinkDistanceTrackerScreen);
    if status
        warning('Make sure the distance between the tracker and the experiment computer matches the ini (GO CHECK ON SLAB IF YOU DONT KNOW WHAT THAT MEANS)')
    end
    
end

%% Sending physical parameters to the edfs:
Eyelink('Message', sprintf('Calibration_area: %s', num2str(calibration_area)))
Eyelink('Message', sprintf('Screen_size_mm: %s', num2str(SCREENPHYSICALCOORDINATES)))
Eyelink('Message', sprintf('Screen_distance_mm: %s', num2str(viewDistanceBottomTop*10)))
Eyelink('Message', sprintf('Camera_position_mm: %s', num2str(DISTANCE_SCREEN_TRACKER*10)))


end
