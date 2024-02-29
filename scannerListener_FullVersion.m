function scannerListener(subNum)
%   Script information:
%       - Version:      8.0.
%       - Author:       Aya Khalaf (email:aya.khalaf@yale.edu)
%       - Date:         06/25/2020
global  LAB_ID TriggerSubfolder TriggerFileNaming WholeFile
global  bitsi_scanner key RunID TotalRuns RunIDCol EventTypeCol InterruptCounter AbortCounter subjectNum InterruptedRestarted
subjectNum = subNum;
LAB_ID = 'SD';
TriggerFileNaming = '_TI_V1_DurR';
DataFolder = 'data';
TemporaryFolder = 'temporary';
BehavFileNamingWhole = '_Beh_V1_RawDurWHOLE';

ParticipantFolder = sprintf('%s%c%s%c%s%c%s%s',pwd,filesep,DataFolder,filesep,[LAB_ID,num2str(subjectNum)]);
TriggerSubfolder = sprintf('%s%c%s%c%s%c%s%s',ParticipantFolder,filesep,'TI');
WholeFile = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DataFolder,filesep,[LAB_ID,num2str(subjectNum)],filesep,TemporaryFolder,filesep,[LAB_ID,num2str(subjectNum),BehavFileNamingWhole]);
ExistFlag=exist(ParticipantFolder,'dir');

TotalRuns=8;
MiniBlockCol=4;
EventTypeCol=14;
RunIDCol=3;
InterruptCounter=0;
AbortCounter=0;
InterruptedRestarted=0;
key=1;
if ExistFlag
    
    warning ('This participant number was already attributed!')
    proceedInput = questdlg({'This participant number was already attributed!', 'Are you sure you want to proceed?'},'RestartPrompt','yes','no','no');
    if strcmp(proceedInput,'no')
        error('Program aborted by user')
    end
    miniBlocksInfo=load(WholeFile);
    OutputTable=miniBlocksInfo.miniBlocks;
    miniBlockIndices=find(cell2mat(OutputTable(2:end,MiniBlockCol)))+1;   %+1 to account for the table header
    miniBlockNum=cell2mat(OutputTable(miniBlockIndices(end),MiniBlockCol));
    if(~strcmp(OutputTable{miniBlockIndices(end),EventTypeCol},'Save'))
    miniBlockNum=1+4*floor((miniBlockNum-1)/4);
    else
    miniBlockNum=miniBlockNum+1;     
    end
    RunID=floor(miniBlockNum/4)+1;
    AbortCounter=sum((ismember(OutputTable(find(cell2mat(OutputTable(2:end,RunIDCol))==RunID)+1,EventTypeCol),'Abortion')));
    clear OutputTable
else
    RunID=1;
end


switch LAB_ID
    case 'SC'
        % init bitsi
        addpath(genpath('Functions'));
        delete(instrfind)                       %clear old instances
        bitsi_scanner = Bitsi_Scanner('com3');  %init bitsis receiving scanTrigger
    case 'SD'
end


while RunID<=TotalRuns
    disp(strcat(strcat('Starting run'," "),num2str(RunID)))
    Listener()
    if(key==0)
        break
    end
end

end

function Listener()
%RunID: 1,2,..
% devices=PsychHID('Devices' [, deviceClass]);
%http://psychtoolbox.org/docs/PsychHardware
%[keyboardIndices, productNames, allInfos] = GetKeyboardIndices([productName][, serialNumber][, locationID])
% KbDevice1=devices(1);
% KbWait(KbDevice1,3);
% KbDevice
global  LAB_ID compKbDevice TriggerSubfolder TriggerFileNaming WholeFile
global  bitsi_scanner key RunID TotalRuns RunIDCol EventTypeCol InterruptCounter AbortCounter subjectNum

% %Wait in a while loop until the server side is on
% while (~exist('ServerFlag.mat','file'))
% end
% WaitSecs(1)
% fclose('all');
% delete('ServerFlag.mat');


switch LAB_ID
    case 'SC'
        bitsi_scanner.clearResponses();         %clear bitsi buffer
        triggerKey = 97;                        % scanTrigger
        KbName('UnifyKeyNames');
        abortKey=  KbName('ESCAPE'); % ESC aborts TR logging process
%         RestrictKeysForKbCheck([abortKey]);
        deviceNumber=-1;
    case 'SD'
        KbName('UnifyKeyNames');
        triggerKey=  KbName('5%');   %TR triggers are received as key presses of 5
        abortKey=  KbName('ESCAPE'); % ESC aborts TR logging process
%         RestrictKeysForKbCheck([triggerKey,abortKey]);
        deviceNumber=-1;
end

% Create a client interface
tcpipStart = tcpip('localhost', 30000, 'NetworkRole', 'client');
%Wait in a while loop until the server side is on
ServerFlag = false;
while ~ServerFlag
   try
       %Try opening the client interface
      fopen(tcpipStart);
      ServerFlag = true;
   catch
   end
end
%Create a client interface and open it
tcpipEnd = tcpip('localhost', 20000, 'NetworkRole', 'client'); 
% Can't be created at the beginning of the code since this will result
% in 2 folders for each subject: one created by runExp1.m and one
% created by scannerListener.m
if(~exist(TriggerSubfolder,'file'))
    mkdir(TriggerSubfolder);
end

triggerCount=0;
noKey=1;
key=noKey;
untilTime=Inf;
NormalModeFlag=0;

while (key~=0)
    switch LAB_ID
        case 'SC'
            [Resp, Resp_Time]= bitsi_scanner.getResponse(untilTime,true);
            if Resp == triggerKey
                key = 5;
            else
                [~, Resp_Time, Resp] = KbCheck(compKbDevice);
                if Resp(abortKey)
                    key=0;
                else
                    key=noKey;
                    Resp_Time=[];
                end
            end
        case 'SD'
            [Resp_Time, Resp, deltaSecs] =KbPressWait(deviceNumber,WaitSecs(0)+untilTime);
            if Resp(triggerKey)
                key = 5;
            elseif Resp(abortKey)
                key=0;
            else
                key=noKey;
                Resp_Time=[];
            end
    end
    
    if key == 5
        triggerCount=triggerCount+1;
        TriggerInfo(triggerCount,:)=[key,Resp_Time];
        if(triggerCount==4)
            Sending_Time=GetSecs;
            fwrite(tcpipStart,key,'double')
            fclose(tcpipStart);
            disp(strcat(strcat('Run'," "),num2str(RunID)))
        end
        untilTime=2;
    end
    
    if(exist('TriggerInfo','var'))
%         %Break if no TR triggers are received within 10 seconds
%         if((GetSecs-TriggerInfo(triggerCount,2))>10)
%             RunID=RunID+1;
%             NormalModeFlag=1;
%             break
           if(triggerCount>4)
           try
              fopen(tcpipEnd);
              fwrite(tcpipEnd,0,'double')
              RunID=RunID+1;
              NormalModeFlag=1;
              fclose(tcpipEnd);
              break
           catch
           end
           end
            %Break if runExp1.m was interrupted and do not increment
            %run number since we will restart the same run
        if (exist('RestartFlag.mat','file'))
            miniBlocksInfo=load(WholeFile);
            OutputTable=miniBlocksInfo.miniBlocks;
            InterruptCounter=sum((ismember(OutputTable(find(cell2mat(OutputTable(2:end,RunIDCol))==RunID)+1,EventTypeCol),'Interruption')));
            clear OutputTable
            delete('RestartFlag.mat');
            break
        end
    end 
end
if(NormalModeFlag)
    disp(strcat(strcat('End of run'," "),num2str(RunID-1)))
else
    disp(strcat(strcat('End of run'," "),num2str(RunID)))
end

load('Receiving_Time');
if(exist('Sending_Time','var'))
    TriggerInfo(4,3)=Receiving_Time-Sending_Time;
else
    TriggerInfo(4,3)=0;
end

TriggerInformation= array2table((TriggerInfo),'VariableNames',{'Trigger','Time','Delay'});
delete('Receiving_Time');
clear TriggerInfo
% When aborting runExp1.m,scannerListerner.m will be aborted by
% pressing ESC key
if(key==0) %ESC
    files=dir(TriggerSubfolder);
    if(length(files)>2)
        for j=3:length(files)
            AbortedFiles(j-2)=~isempty(strfind(files(j).name,[num2str(RunID) '_ABORTED']));
        end
    else
        AbortedFiles=0;
    end
    fileName= sprintf('%s%c%s.csv',TriggerSubfolder,filesep,[LAB_ID,num2str(subjectNum),TriggerFileNaming,num2str(RunID),'_ABORTED_',num2str(sum(AbortedFiles)+1)]);
else
    % If ESC key was not pressed, it means the code normally progresses through runs
    if(AbortCounter && NormalModeFlag)
        fileName= sprintf('%s%c%s.csv',TriggerSubfolder,filesep,[LAB_ID,num2str(subjectNum),TriggerFileNaming,num2str(RunID-1),'_RESTARTED']);
        AbortCounter=0;
    elseif(InterruptCounter)
        if(~NormalModeFlag)
            fileName= sprintf('%s%c%s.csv',TriggerSubfolder,filesep,[LAB_ID,num2str(subjectNum),TriggerFileNaming,num2str(RunID),'_INTERRUPTED_',num2str(InterruptCounter)]);
        else
            fileName= sprintf('%s%c%s.csv',TriggerSubfolder,filesep,[LAB_ID,num2str(subjectNum),TriggerFileNaming,num2str(RunID-1),'_RESTARTED']);
            InterruptCounter=0;
        end
    elseif(~AbortCounter && ~InterruptCounter)
        fileName= sprintf('%s%c%s.csv',TriggerSubfolder,filesep,[LAB_ID,num2str(subjectNum),TriggerFileNaming,num2str(RunID-1)]);
    end
end

writetable(TriggerInformation,fileName)
end
