%% Exp 1 and 2 automatic exclusion test behavior
%This small script checks if the correct responses and incorrect responses pass the predefined cut off values
% From SLAB: Behavior: Low performance (Exp. 1, target detection: <90% hits, >10% FAs; Exp. 2: Exp. 2, AT condition: <85% hits, >15% FAs).

%% Exp1
exp_folder = 'C:\Users\csaba\OneDrive\Documents\GitHub\Experiment1Development';
fMRI = 1;
ECoG =0;
MEEG = 0;
LAB_ID = 'SF';
subjectNum = 999;
data_file = sprintf('%s%c%s%c%s%c%s',exp_folder,filesep,'data',filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),'_Beh_V1_SumDur','.xls']);
data = readtable(data_file,'ReadVariableNames', 1);


%Check if Hits and False Alarms have passed the criteria
if fMRI || ECoG
    
    if ((data.Hits)<(160*0.5)*0.9) & ((data.False_Alarm)>((1280*0.5)*0.1))
        %     warning('Hits and False Alarms limit has NOT been passed, need to exclude participant')
        questdlg(['Hits=',num2str(data.Hits),' and FA=',num2str(data.False_Alarm),', out of 80 targets and 640 non-targets/irrelevants, which means Hits and False Alarms limit has been passed, need to exclude participant'],'OK','OK','OK');
    elseif (data.Hits)<((160*0.5)*0.9)
        questdlg(['Hits=',num2str(data.Hits),', out of 80 targets, which means Hits are below 90%, need to exclude participant'],'OK','OK','OK');
        % warning('Hits are below 90%, need to exclude participant')
    elseif (data.False_Alarm)>((1280*0.5)*0.1)
        questdlg(['FA=',num2str(data.False_Alarm),', out of 640 non-targets/irrelevants, which means False Alarms are above 10%, need to exclude participant'],'OK','OK','OK');
        %     warning('False Alarms are above 10%, need to exclude participant')
    else
        questdlg('Participant`s behavioral scores are good','Message','OK','OK');
        %     disp('Participant`s behavioral scores are good')
    end
    
elseif MEEG %MEEG
    
    if ((data.Hits)<(160)*0.9 & ((data.False_Alarm)>(1280)*0.1))
        %     warning('Hits and False Alarms limit has NOT been passed, need to exclude participant')
        questdlg(['Hits=',num2str(data.Hits+data.Correct_Reject),' and FA=',num2str(data.False_Alarm+data.Misses),', out of 160 targets and 1280 non-targets/irrelevants, which means Hits and False Alarms limit has NOT been passed, need to exclude participant'],'OK','OK','OK');
    elseif (data.Hits)<(160)*0.9
        questdlg(['Hits=',num2str(data.Hits),', out of 160 targets, which means Hits are below 90%, need to exclude participant'],'OK','OK','OK');
        % warning('Hits are below 90%, need to exclude participant')
    elseif (data.False_Alarm)>(1280)*0.1
        questdlg(['FA=',num2str(data.False_Alarm),', out of 1280 non-targets/irrelevants, which means False Alarms are above 10%, need to exclude participant'],'OK','OK','OK');
        %     warning('False Alarms are above 10%, need to exclude participant')
    else
        questdlg('Participant`s behavioral scores are good','Message','OK','OK');
        %     disp('Participant`s behavioral scores are good')
    end
end

%% Exp2 %%%%%
LogPath = 'C:\Users\csaba\OneDrive\Documents\ECoG team TCWF\Experiment 2\NYU_Data_08_31_20\SF381\A\AnalyzerOutput'; 
cd(LogPath)


logFilesList = dir('*Cumulative.csv');
fullLogs = [];
for i = 1:length(logFilesList)
    Log = readtable(logFilesList(i).name);
    if isfloat(Log.world(1,1))
        Log.world = num2cell(Log.world);
    else
        Log.world = cell(Log.world);
    end
    fullLogs = [fullLogs;Log];
end


%% LOCALIZER
FP_loc=[];
TP_loc=[];

for i=1:height(fullLogs)
    if isfloat(fullLogs.world{i})
        fullLogs.world{(i)} =num2str(fullLogs.world{i});
    end
    if (contains(fullLogs.world(i),'L')) & (contains(fullLogs.responseEvaluation(i),'FalsePositive'))
        FP_loc(i) = 1;
    elseif (contains(fullLogs.world(i),'L')) & (contains(fullLogs.responseEvaluation(i),'TruePositive'))
        TP_loc(i) = 1;
    end
end

%localizer all

for i=1:height(fullLogs)
    if (contains(fullLogs.world(i),'L')) & ((contains(fullLogs.stimType(i),'Face')) || (contains(fullLogs.stimType(i),'Object')))
        localizer_all(i)=1;
    end
end



if ((sum(TP_loc)) < (sum(localizer_all)*.85)) & ((sum(FP_loc)) > (sum(localizer_all)*.15))
%     warning('Hits and False Alarms limit has NOT been passed, need to exclude participant') 
     questdlg('Hits and False Alarms limit has been passed, need to exclude participant','OK','OK','OK');

elseif (sum(TP_loc)) < (sum(localizer_all)*.85)
     questdlg('Hits are below 85%, need to exclude participant','OK','OK','OK');
%     warning('Hits are below 85%, need to exclude participant')
elseif(sum(FP_loc)) > (sum(localizer_all)*.15)
     questdlg('False Alarms are above 15%, need to exclude participant','OK','OK');
%     warning('False Alarms are above 15%, need to exclude participant')
else
    questdlg('Participant`s behavioral scores are good','Message','OK','OK');
%     disp('Participant`s behavioral scores are good')
end

%Select out TP for each type
for i=1:height(fullLogs)
    if (contains(fullLogs.world(i),'L')) & (contains(fullLogs.stimType(i),'Face')) & (contains(fullLogs.responseEvaluation(i),'TruePositive'))
        localizer_face(i)=1;
    elseif (contains(fullLogs.world(i),'L')) & (contains(fullLogs.stimType(i),'Object')) & (contains(fullLogs.responseEvaluation(i),'TruePositive'))
        localizer_object(i)=1;
    end

end

%Select out all for each type
for i=1:height(fullLogs)
    if (contains(fullLogs.world(i),'L')) & (contains(fullLogs.stimType(i),'Face')) 
        localizer_face_all(i)=1;
    elseif (contains(fullLogs.world(i),'L')) & (contains(fullLogs.stimType(i),'Object')) 
        localizer_object_all(i)=1;
    end

end

%Seen Objects and Faces in localizer
questdlg([num2str(sum(localizer_face)),' faces',' and ',num2str(sum(localizer_object)),' object',' were seen from ',num2str(sum(localizer_face_all)),' faces and ', num2str(sum(localizer_object_all)),' objects!'],'Message','OK','OK');




%% Game levels (How many faces and/or objects have been seen-Hits)
for i=1:height(fullLogs)
    if ~contains(fullLogs.world(i),'L') & ((contains(fullLogs.stimType(i),'Face')) || (contains(fullLogs.stimType(i),'Face')))
        game_all(i)=1;
    end
end

for i = 1:height(fullLogs)
    if (contains(fullLogs.stimType(i),'Face')) & (contains(fullLogs.responseEvaluation(i),'TruePositive')) & ~contains(fullLogs.world(i),'L')
        face(i)=1;
    else
        face(i)=0;
    end
    
    if  (contains(fullLogs.stimType(i),'Object')) & (contains(fullLogs.responseEvaluation(i),'TruePositive')) & ~contains(fullLogs.world(i),'L')
        object(i)=1;
    else
        object(i)=0;
    end
    
end

num_face=sum(face);
num_object=sum(object);

for i = 1:height(fullLogs)
    if (contains(fullLogs.isProbed(i),'True')) & (contains(fullLogs.stimType(i),'Face')) & ~contains(fullLogs.world(i),'L')
        face_total(i)=1;
    else
        face_total(i)=0;
    end
    if (contains(fullLogs.isProbed(i),'True')) & (contains(fullLogs.stimType(i),'Object')) & ~contains(fullLogs.world(i),'L')
        object_total(i)=1;
    else
        object_total(i)=0;
    end
end

num_face_total=sum(face_total);
num_object_total=sum(object_total);

%Seen and unseen message
questdlg([num2str(num_face),' faces',' and ',num2str(num_object),' object',' were seen from ',num2str(num_face_total),' faces and ', num2str(num_object_total),' objects!'],'Message','OK','OK');

