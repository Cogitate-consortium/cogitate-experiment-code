% This function saves the tobii Eyetracker data. Inspired by the following
% github function: https://github.com/mekline/Tobii-PsychToolBox/blob/master/TobiiCalibrationPsychtoolbox/SaveGazeData.m

function saveTobiiGazeData(RestartFlag)
global viewDistance SCREEN_SIZE_CM ScreenWidth ScreenHeight tobii_eyetracker DATA_FOLDER LAB_ID subjectNum EYETRACKER_FILE_NAMING Block_ctr excelFormat tobii_TimeCell SAVING_MESSAGE

% Starting by showing a message on the screen to let the participant know
% that they have to wait for a bit:
showMessage(SAVING_MESSAGE)

% Getting the gaze data from the tobii tracker
GazeData = tobii_eyetracker.get_gaze_data();

tobii_TimeTable = cell2table(tobii_TimeCell(2:end,:),'VariableNames',tobii_TimeCell(1,:));
% preparing the cell to store the data
gazeCell = {'subjectID', 'DistanceToScreen', 'ScreenHeightCm', 'ScreenWidthCm', 'ScreenHeightPix', 'ScreenWidthPix',...
    'device_time_stamp','system_time_stamp','L_x','L_y','R_x','R_y','L_xPix','L_yPix','R_xPix','R_yPix',...
    'L_xCm','L_yCm','R_xCm','R_yCm','Triggers'}; %Can't preallocate rows because each point may have a different n of samples
SUBJECT = [LAB_ID,num2str(subjectNum)];
% Looping through each raw of the gaze data
for i=1:length(GazeData)
    thisPoint = GazeData(i);
    %Check if data was collected for each eye
    if length(thisPoint.LeftEye.GazePoint.OnDisplayArea)==2
        Lx = thisPoint.LeftEye.GazePoint.OnDisplayArea(1);
        Ly = thisPoint.LeftEye.GazePoint.OnDisplayArea(2);
        [LxCm, LyCm, LxPix, LyPix] = PropToCmAndPix(Lx,Ly);
    else
        Lx   = NaN;
        Ly   = NaN;
        LxCm = NaN;
        LyCm = NaN;
        LxPix= NaN;
        LyPix= NaN;
    end
    
    if length(thisPoint.RightEye.GazePoint.OnDisplayArea)==2
        Rx = thisPoint.RightEye.GazePoint.OnDisplayArea(1);
        Ry = thisPoint.RightEye.GazePoint.OnDisplayArea(2);
        [RxCm, RyCm, RxPix, RyPix] = PropToCmAndPix(Rx,Ry);
    else
        Rx = NaN;
        Ry = NaN;
        RxCm = NaN;
        RyCm = NaN;
        RxPix= NaN;
        RyPix= NaN;
    end
    
    % In order to also add the triggers into this table, I first fill the
    % trigger column with NaN. The reason for that is that the triggers
    % timestamp will not (actually almost never ever) match the data
    % timestamps. While the data timestamps are contrained by the sampling
    % rate of the tracker, the triggers are not. So they will be added
    % separately
    Trigger = {NaN};
    
    %Make a full row for each sample
    gazeCell(end+1,:) = {SUBJECT,...
        viewDistance,...
        SCREEN_SIZE_CM(1,2),...
        SCREEN_SIZE_CM(1,1),...
        ScreenHeight,...
        ScreenWidth,...
        thisPoint.DeviceTimeStamp,...
        thisPoint.SystemTimeStamp,...
        Lx,...
        Ly,...
        Rx,...
        Ry,...
        LxPix,...
        LyPix,...
        RxPix,...
        RyPix,...
        LxCm,...
        LyCm,...
        RxCm,...
        RyCm,...
        Trigger};  
end

% Turning the gaze cell to a table:
gazeTable = cell2table(gazeCell(2:end,:));
gazeTable.Properties.VariableNames = gazeCell(1,:);

% Now I just need to add the triggers:
% To do so, I need to first reformat the triggers table, to match the
% format of the gaze table:
SubjVec    = (repmat(cellstr(SUBJECT),height(tobii_TimeTable),1));
viewDisVec = repmat(viewDistance,height(tobii_TimeTable),1);
ScreenWidthCmVec = repmat(SCREEN_SIZE_CM(1,2),height(tobii_TimeTable),1);
ScreenHeightCmVec = repmat(SCREEN_SIZE_CM(1,1),height(tobii_TimeTable),1);
ScreenWidthPixVec = repmat(ScreenHeight,height(tobii_TimeTable),1);
ScreenHeightPixVec = repmat(ScreenWidth,height(tobii_TimeTable),1);
DeviceTimeVec = nan(height(tobii_TimeTable),1);
SystTimeVec = tobii_TimeTable.system_time_stamp;
LxVec = nan(height(tobii_TimeTable),1);
LyVec = nan(height(tobii_TimeTable),1);
RxVec = nan(height(tobii_TimeTable),1);
RyVec = nan(height(tobii_TimeTable),1);
LxPixVec = nan(height(tobii_TimeTable),1);
LyPixVec = nan(height(tobii_TimeTable),1);
RxPixVec = nan(height(tobii_TimeTable),1);
RyPixVec = nan(height(tobii_TimeTable),1);
LxCmVec = nan(height(tobii_TimeTable),1);
LyCmVec = nan(height(tobii_TimeTable),1);
RxCmVec = nan(height(tobii_TimeTable),1);
RyCmVec = nan(height(tobii_TimeTable),1);
TriggerVec = tobii_TimeTable.point_description;

TriggersTable = table(SubjVec,viewDisVec,ScreenWidthCmVec,ScreenHeightCmVec,...
    ScreenWidthPixVec,ScreenHeightPixVec,DeviceTimeVec,SystTimeVec,LxVec,LyVec,...
    RxVec,RyVec,LxPixVec,LyPixVec,RxPixVec,RyPixVec,LxCmVec,LyCmVec,RxCmVec,RyCmVec,...
    TriggerVec,'VariableNames',gazeCell(1,:));

% Then, I append the triggers to the gaze table:
gazeTable = [gazeTable;TriggersTable];

% And finally,sorting the table by timestamps:
gazeTable = sortrows(gazeTable,'system_time_stamp');

%And save the file!
% If this is a restarting case, we need to save it as such
if RestartFlag
    % Generating the file name to store the edf data:
    fileName = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(Block_ctr-1),'_RESTARTED',excelFormat]);
else
    fileName = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),EYETRACKER_FILE_NAMING,num2str(Block_ctr-1),excelFormat]);
end
writetable(gazeTable, fileName);
end


