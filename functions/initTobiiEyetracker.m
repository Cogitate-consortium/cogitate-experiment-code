% This function initialize the connection with the tobii eyetracker:
function initTobiiEyetracker
global tobii_eyetracker_address tobii_eyetracker tobii
% Adding the path to the tobii SDK
addpath(genpath('C:\Users\ecogTask\Documents\eyetracking\eyetracking\TobiiPro.SDK.Matlab_1.7.1.4'))

% Create an instance of the eyetracker operation class
tobii = EyeTrackingOperations();
% Find all the connected tobii eyetrackers:
found_eyetrackers = tobii.find_all_eyetrackers();

% Access the meta of the eyetracker
my_eyetracker = found_eyetrackers(1);
% Display some infos in the command window:
disp(["Address: ", my_eyetracker.Address])
disp(["Model: ", my_eyetracker.Model])
disp(["Name (It's OK if this is empty): ", my_eyetracker.Name])
disp(["Serial number: ", my_eyetracker.SerialNumber])

% Get the address of the eyetracker
tobii_eyetracker_address = my_eyetracker.Address;
% Get the eyetracker object (required for calibration):
tobii_eyetracker = tobii.get_eyetracker(tobii_eyetracker_address);

end
