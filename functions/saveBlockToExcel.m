
% SAVEBLOCKTOEXCLE saves a matrix to excel
% input:
% ------
% miniBlocks - the cell array to be saved
% miniBlockNumber - the block number to be written in the filename
%
% output:
% -------
% An excel file containning the data of blockIn
function [ ] = saveBlockToExcel( miniBlocks, miniBlockNumber, tr )

global EXP_COL EVENT_TYPE_COL OUTPUT_TABLE OUTPUT_TABLE_HEADER DATA_FOLDER EXPERIMENT_NAME subjectNum excelFormat LAB_ID ECoG BEHAV_FILE_NAMING%subject number
global fMRI MEEG Behavior
% Creating the directory:
if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]))
    mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]));
end
try % Turn the miniBlocks cell to a table:
    miniBlocks = miniBlocks(2:size(miniBlocks,1),:);
    miniBlocks = cell2table(miniBlocks,'VariableNames',OUTPUT_TABLE_HEADER);
    miniBlocks = miniBlocks(:,EXP_COL:EVENT_TYPE_COL);
catch
end
try % Try chopping the data and saving them
    %% Chopping the whole log in blocks for the different recording techniques:
    % If ECoG or fMRI, save the data per blocks
    if ECoG||fMRI
        % Getting the block number:
        BlockNumber = floor((miniBlockNumber-1)/4)+1;
        % Getting the file name:
        fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),excelFormat]);
        % Getting the index of the beginning of the block we are at:
        idxBeginSave = find([miniBlocks.block{:,1}]==BlockNumber,1,'first');
        % Getting the index in the miniblock table of the very trial we
        % are at (same block, same miniblock, same trial)
        idxEndSave = find([miniBlocks.block{:,1}]==BlockNumber & [miniBlocks.miniBlock{:,1}] == miniBlockNumber &...
            [miniBlocks.trial{:,1}] == tr+1,1,'last');
        % Getting the relevant data
        MiniBlocks = miniBlocks(idxBeginSave:idxEndSave,:);
        %% Restarting contingency:
        % If the experiment was aborted, the part of the whole log
        % that was aborted gets saved as ABORTED, and whatever
        % happens after the abortion in the same block gets saved
        % separately as: RESTARTED
        % I know it looks like a weird thing to do, because we are
        % in essence resaving things everytime we come there. But
        % this way, we make the code quite concise, we don't need
        % to add bunch of if and switch statements. And it doesn't
        % matter that we resave things because they don't change,
        % we just overwrite files by themselves:
        
        if ECoG
            if sum((ismember(MiniBlocks.eventType,'Abortion'))) ~= 0
                % If we aborted the experiment, then we need to change the name
                % of the file of the block that was s   tarted, by deleting it and
                % resaving it under another name:
                if isfile(fileName)
                    delete(fileName)
                end
                % Counting how many abortions we had in there:
                exp_abort_counter=sum((ismember(MiniBlocks.eventType,'Abortion')));
                
                % If the the file was aborted, then we change the name:
                abortedfileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_ABORTED']);
                fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_RESTARTED',excelFormat]);
                % Getting the index of where things are aborted
                idxAbortion = find(ismember(MiniBlocks.eventType,'Abortion'));
                % Getting the index of where things start:
                idxBeginSave = 1;
                idxEndSave = height(MiniBlocks);
                % Creating the aborted miniblock as being from the beginning to
                % where things gets aborted. If there were more
                % than one abortion within the same block, then we
                % save from abortion to abortion:
                if length(idxAbortion) > 1
                    AbortedMiniBlocks = MiniBlocks(idxAbortion(end-1)+1:idxAbortion(end), :);
                else
                    AbortedMiniBlocks = MiniBlocks(idxBeginSave:idxAbortion, :);
                end
                
                % The miniblock is from the abortion to the end:
                MiniBlocks = MiniBlocks(idxAbortion(end)+1:idxEndSave, :);
            end
        end
        
        % For fMRI, we save new log files both in case of abortion and
        % interruption
        if fMRI
            % If the experiment was aborted:
            if sum((ismember(MiniBlocks.eventType,'Abortion'))) ~= 0 ||...
                    sum((ismember(MiniBlocks.eventType,'Interruption'))) ~= 0
                % If we aborted the experiment, then we need to change the name
                % of the file of the block that was started, by deleting it and
                % resaving it under another name:
                if isfile(fileName)
                    delete(fileName)
                end
                % If the the file was aborted, then we change the name:
                fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_RESTARTED',excelFormat]);
                if(strcmp(MiniBlocks.eventType(end),'Abortion'))
                    % Counting how many abortions we had in there:
                    exp_abort_counter=sum((ismember(MiniBlocks.eventType,'Abortion')));
                    exp_Interrupt_counter=0;
                elseif(strcmp(MiniBlocks.eventType(end),'Interruption'))
                    % Counting how many interruption we had in there:
                    exp_Interrupt_counter=sum((ismember(MiniBlocks.eventType,'Interruption')));
                    exp_abort_counter=0;
                else
                    exp_Interrupt_counter=0;
                    exp_abort_counter=0;
                end
                % Getting the index of where things are aborted
                idxAbortion = find(ismember(MiniBlocks.eventType,'Abortion')|ismember(MiniBlocks.eventType,'Interruption'));
                % Getting the index of where things start:
                idxBeginSave = 1;
                idxEndSave = height(MiniBlocks);
                % Creating the aborted miniblock as being from the beginning to
                % where things gets aborted. If there were more
                % than one abortion within the same block, then we
                % save from abortion to abortion:
                if length(idxAbortion) > 1
                    AbortedMiniBlocks = MiniBlocks(idxAbortion(end-1)+1:idxAbortion(end), :);
                    if ismember(MiniBlocks{idxAbortion(end),EVENT_TYPE_COL},'Abortion')
                        % If the the file was aborted, then we change the name:
                        abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_ABORTED']);
                    elseif ismember(MiniBlocks{idxAbortion(end),EVENT_TYPE_COL},'Interruption')
                        % If the the file was interrupted, then we change the name:
                        abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_INTERRUPTED']);
                    end
                else
                    AbortedMiniBlocks = MiniBlocks(idxBeginSave:idxAbortion, :);
                    if ismember(MiniBlocks{idxAbortion,EVENT_TYPE_COL},'Abortion')
                        % If the the file was aborted, then we change the name:
                        abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_ABORTED']);
                    elseif ismember(MiniBlocks{idxAbortion,EVENT_TYPE_COL},'Interruption')
                        % If the the file was interrupted, then we change the name:
                        abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_INTERRUPTED']);
                    end
                end
                
                % The miniblock is from the abortion to the end:
                MiniBlocks = MiniBlocks(idxAbortion(end)+1:idxEndSave, :);
            end
        end
    elseif MEEG % FOr MEEG, save data per two blocks:
        BlockNumber = floor((miniBlockNumber-1)/4)+1;
        % Getting the index of the beginning of the previous block (or of the block we are at depending on whether we are at the beginnig:
        if mod(BlockNumber,2) == 0
            idxBeginSave = find([miniBlocks.block{:,1}]==BlockNumber-1,1,'first');
        else
            idxBeginSave = find([miniBlocks.block{:,1}]==BlockNumber,1,'first');
        end
        % Since we group the blocks in twos, the block number becomes
        % the floor of the block number divided by two
        DoubleBlockNumber = floor((BlockNumber-1)/2)+1;
        % Creating the file name:
        fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(DoubleBlockNumber),excelFormat]);
        % Getting the index in the miniblock table of the very trial we
        % are at (same block, same miniblock, same trial)
        idxEndSave = find([miniBlocks.block{:,1}]==BlockNumber & [miniBlocks.miniBlock{:,1}] == miniBlockNumber &...
            [miniBlocks.trial{:,1}] == tr+1,1,'last');
        % Getting the relevant data:
        MiniBlocks = miniBlocks(idxBeginSave:idxEndSave,:);
        %% Restarting contingency:
        % If the experiment was aborted, the part of the whole log
        % that was aborted gets saved as ABORTED, and whatever
        % happens after the abortion in the same block gets saved
        % separately as: RESTARTED
        % I know it looks like a weird thing to do, because we are
        % in essence resaving things everytime we come there. But
        % this way, we make the code quite concise, we don't need
        % to add bunch of if and switch statements. And it doesn't
        % matter that we resave things because they don't change,
        % we just overwrite files by themselves:
        if sum(ismember(MiniBlocks.eventType,'Abortion'))~=0
            % If we aborted the experiment, then we need to change the name
            % of the file of the block that was started, by deleting it and
            % resaving it under another name:
            if isfile(fileName)
                delete(fileName)
            end
            %Counting how many abortion there were:
            exp_abort_counter=sum((ismember(MiniBlocks.eventType,'Abortion')));
            
            % If the the file was aborted, then we change the name:
            abortedfileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(DoubleBlockNumber),'_ABORTED']);
            fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(DoubleBlockNumber),'_RESTARTED',excelFormat]);
            % Getting the index of where things are aborted
            idxAbortion = find(ismember(MiniBlocks.eventType,'Abortion'));
            % Getting the index of where things start:
            idxBeginSave = 1;
            idxEndSave = height(MiniBlocks);
            % Creating the aborted miniblock as being from the beginning to
            % where things gets aborted. If there were more
            % than one abortion within the same block, then we
            % save from abortion to abortion:
            if length(idxAbortion) > 1
                AbortedMiniBlocks = MiniBlocks(idxAbortion(end-1)+1:idxAbortion(end), :);
            else
                AbortedMiniBlocks = MiniBlocks(idxBeginSave:idxAbortion, :);
            end
            % The miniblock is from the abortion to the end:
            MiniBlocks = MiniBlocks(idxAbortion(end)+1:idxEndSave, :);
        end
        
    elseif Behavior
        % Getting the file name:
        fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,excelFormat]);
        MiniBlocks = miniBlocks;
    end
    
    writetable(MiniBlocks,fileName);
    if exist('AbortedMiniBlocks','var')
        if ~fMRI
            writetable(AbortedMiniBlocks,[abortedfileName '_' num2str(exp_abort_counter) excelFormat])
        else % For fMRI, depending on whether
            if exist('exp_Interrupt_counter','var') && exp_Interrupt_counter > 0
                writetable(AbortedMiniBlocks,[abortedfileName '_' num2str(exp_Interrupt_counter) excelFormat]);
            elseif exist('exp_abort_counter','var') && exp_abort_counter > 0
                writetable(AbortedMiniBlocks,[abortedfileName '_' num2str(exp_abort_counter) excelFormat]);
            end
        end
    end
    
catch ME% If the part above fails, we call the saveBlockToHD to make sure our data are saved. This function has a safety: if things fail, the whole log will be saved as a mat file: 
    warning(ME.message)
    warning('The saveBlockToExcel function crashed!')
    saveBlockToHD(OUTPUT_TABLE,1,'Results',tr);
end
end