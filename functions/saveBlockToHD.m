
% SAVEBLOCKTOHD saves a matrix as mat
% input:
% ------
% miniBlocks - the cell array to be saved
% miniBlockNumber - the block number to be written in the filename
% name - a file name addition
% output:
% -------
% A mat file containning the data of blockIn
function [ ] = saveBlockToHD( miniBlocks , miniBlockNumber, name,tr)

global OUTPUT_COLS EXP_COL EVENT_TYPE_COL DATA_FOLDER EXPERIMENT_NAME subjectNum LAB_ID ECoG BLK_COL MINIBLK_COL TRAIL_COL BEHAV_FILE_SUMMARY_NAMING%subject number
global BEHAV_FILE_NAMING MEEG Behavior fMRI TEMPORARY_FOLDER BEHAV_FILE_NAMING_WHOLE EVENT_TYPE_COL exp_Interrupt_counter

% Creating the directories if they don't already exist:
if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]),'dir')
    mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]));
end
if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],TEMPORARY_FOLDER),'dir')
    mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],TEMPORARY_FOLDER));
end
switch name
    case 'Results'
        try % Try to chop the whole log into pieces to save it separately for each block:
            % I first create the name of the file for the entire session
            fileNameWhole  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING_WHOLE]);
            
            %% Chopping of the whole log into blocks for the different techniques:
            
            % For the ECoG and fMRI, we save the data in separate folders for
            % each blocks
            if ECoG||fMRI
                % Getting the block number:
                BlockNumber = floor((miniBlockNumber-1)/4)+1;
                % Generating the name of the file of this specific block:
                fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber)]);
                % Getting the index of the beginning of the block we are at:
                idxBeginSave = find([miniBlocks{2:end,BLK_COL}]==BlockNumber,1,'first')+1;
                % Getting the index in the miniblock table of the very trial we
                % are at (same block, same miniblock, same trial)
                idxEndSave = find([miniBlocks{2:end,BLK_COL}]==BlockNumber & [miniBlocks{2:end,MINIBLK_COL}] == miniBlockNumber &...
                    [miniBlocks{2:end,TRAIL_COL}] == tr+1,1,'last')+1;
                % Getting the relevant data: from block onset to the last
                % trial recorded so far:
                MiniBlocks = [miniBlocks(1,:); miniBlocks(idxBeginSave:idxEndSave,:)];
                
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
                % we just overwrite files by themselves. Using appending
                % functions would have been more long and cumbersome:
                if ECoG % For the ECoG, only make a new log file if you abort:
                    if sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion'))) ~= 0
                        
                        % If the experiment gets aborted in the middle of
                        % a block, the file of this block needs to be changed
                        % to append _ABORTED. To do so, the file saved so
                        % far for this block gets deleted and resaved under
                        % another name:
                        if isfile (fileName)
                            delete(fileName)
                        end
                        
                        % If the the file was aborted, then we change the name:
                        abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_ABORTED']);
                        
                        % Creating the file name for the restarting ahead
                        % of time:
                        fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_RESTARTED']);
                        
                        % Counting how many abortions we had in there:
                        exp_abort_counter=sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion')));
                        
                        % Getting the index of where things are aborted
                        idxAbortion = find(ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion'));
                        
                        % Getting the index of where things start:
                        idxBeginSave = 1;
                        idxEndSave = size(MiniBlocks,1);
                        
                        % Creating the aborted miniblock as being from the beginning to
                        % where things gets aborted. If there were more
                        % than one abortion within the same block, then we
                        % save from abortion to abortion:
                        if length(idxAbortion) > 1
                            AbortedMiniBlocks = MiniBlocks(idxAbortion(end-1)+1:idxAbortion(end), :);
                        else 
                            AbortedMiniBlocks = MiniBlocks(idxBeginSave:idxAbortion, :);
                        end
                        
                        % The MiniBlocks structure (in that case it is what happens
                        % after the interruption) is the obtained. In the case
                        % where the experiment was interrupted when reaching it,
                        % this yields an empty matrix, because the matrix hasn't
                        % been created yet.
                        MiniBlocks = MiniBlocks(idxAbortion(end)+1:idxEndSave, :);
                        
                    end
                end
                
                if fMRI % For fMRI, a new log should be created both if the experiment is aborted and interrupted:
                    % If abortion, create a log file called ..._ABORTED and
                    % one called _RESTARTED
                    if sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion'))) ~= 0 || ...
                            sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Interruption'))) ~= 0
                        % If we aborted the experiment, then we need to change the name
                        % of the file of the block that was started, by deleting it and
                        % resaving it under another name:
                        if isfile (fileName)
                            delete(fileName)
                        end
                        % If the experiment was aborted, rename the file:
                        fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_RESTARTED']);
                        
                        if(strcmp(MiniBlocks(end,EVENT_TYPE_COL),'Abortion'))
                            % Counting how many abortions we had in there:
                            exp_abort_counter=sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion')));
                            exp_Interrupt_counter=0;
                        elseif(strcmp(MiniBlocks(end,EVENT_TYPE_COL),'Interruption'))
                            % Counting how many interruptions we had in there:
                            exp_Interrupt_counter=sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Interruption')));
                            exp_abort_counter=0;
                        else
                            exp_Interrupt_counter=0;
                            exp_abort_counter=0;
                        end
                        
                        % Getting the index of where things are aborted or
                        % interrupted
                        idxAbortion = find(ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion') | ismember(MiniBlocks(:,EVENT_TYPE_COL),'Interruption'));
                        
                        % Getting the index of where things start:
                        idxBeginSave = 1;
                        idxEndSave = size(MiniBlocks,1);
                        
                        % Creating the aborted miniblock as being from the beginning to
                        % where things gets aborted. If there were more
                        % than one abortion within the same block, then we
                        % save from abortion to abortion:
                        if length(idxAbortion) > 1
                            AbortedMiniBlocks = MiniBlocks(idxAbortion(end-1)+1:idxAbortion(end), :);
                            if ismember(MiniBlocks{idxAbortion(end),EVENT_TYPE_COL},'Abortion')
                                % If the the file was aborted, then we change the name:
                                abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_ABORTED']);
                            elseif ismember(MiniBlocks{idxAbortion(end),EVENT_TYPE_COL},'Interruption')
                                % If the the file was interrupted, then we change the name:
                                abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_INTERRUPTED']);
                            end
                        else 
                            AbortedMiniBlocks = MiniBlocks(idxBeginSave:idxAbortion, :);
                            if ismember(MiniBlocks{idxAbortion,EVENT_TYPE_COL},'Abortion')
                                % If the the file was aborted, then we change the name:
                                abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_ABORTED']);
                            elseif ismember(MiniBlocks{idxAbortion,EVENT_TYPE_COL},'Interruption')
                                % If the the file was interrupted, then we change the name:
                                abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(BlockNumber),'_INTERRUPTED']);
                            end
                        end
                        % The MiniBlocks structure (in that case it is what happens
                        % after the interruption) is the obtained. In the case
                        % where the experiment was interrupted when reaching it,
                        % this yields an empty matrix, because the matrix hasn't
                        % been created yet.
                        MiniBlocks = MiniBlocks(idxAbortion(end)+1:idxEndSave, :);
                    end
                end
                
                % MEEG savings: save the miniBlocks per chunk of two blocks:
            elseif MEEG
                % Getting block number
                BlockNumber = floor((miniBlockNumber-1)/4)+1;
                
                % Getting the index of the beginning of the previous block (or of the block we are at depending on whether we are at the beginnig:
                if mod(BlockNumber,2) == 0
                    idxBeginSave = find([miniBlocks{2:end,BLK_COL}]==BlockNumber-1,1,'first')+1;
                else
                    idxBeginSave = find([miniBlocks{2:end,BLK_COL}]==BlockNumber,1,'first')+1;
                end
                
                % Since we group the blocks in twos, the block number becomes
                % the floor of the block number divided by two
                DoubleBlockNumber = floor((BlockNumber-1)/2)+1;
                
                % Generating the name of the file:
                fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(DoubleBlockNumber)]);
                
                % Getting the index in the miniblock table of the very trial we
                % are at (same block, same miniblock, same trial)
                idxEndSave = find([miniBlocks{2:end,BLK_COL}]==BlockNumber & [miniBlocks{2:end,MINIBLK_COL}] == miniBlockNumber &...
                    [miniBlocks{2:end,TRAIL_COL}] == tr+1,1,'last')+1;
                
                % Getting the relevant data for the file to save:
                MiniBlocks = [miniBlocks(1,:); miniBlocks(idxBeginSave:idxEndSave,:)];
                %% Restarting contingency:
                % If the experiment was aborted, the part of the whole log
                % that was aborted gets saved as ABORTED, and whatever
                % happens after the abortion in the same block gets saved
                % separately as: RESTARTED
                if sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion'))) ~= 0
                    % If we aborted the experiment, then we need to clean up
                    % the log files:
                    if isfile(fileName)
                        delete(fileName)
                    end
                    
                    % Counting how many abortions we had in there:
                    exp_abort_counter=sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion')));
                    
                    % If the the file was aborted, then we change the name:
                    abortedfileName  = sprintf('%s%c%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(DoubleBlockNumber),'_ABORTED']);
                    fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING,num2str(DoubleBlockNumber),'_RESTARTED']);
                    
                    % Getting the index of where things are aborted
                    idxAbortion = find(ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion'));
                    
                    % Getting the index of where things start and end
                    idxBeginSave = 1;
                    idxEndSave = size(MiniBlocks,1);
                    
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
                % For the behavioral, data are saved all together, no need
                % to worry about consistency with the neural data files:
            elseif Behavior
                fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING]);
                MiniBlocks = miniBlocks;
            end
            
            %% Saving the data:
            % This is where the data actually get saved. We first save the
            % chopped MiniBlocks, then the whole log table as a backup:
            
            try
                
                % If the table has the right format, save it from column EXP to
                % eventType
                if size(miniBlocks,2) == OUTPUT_COLS
                    % Sainve the MiniBlocks, which is a fraction of the
                    % miniBlocks
                    MiniBlocks = MiniBlocks(:,EXP_COL:EVENT_TYPE_COL);
                    save(fileName,'MiniBlocks');
                else
                    save(fileName,'MiniBlocks');
                end
                % in case of fMRI each aborted or interrupted session
                % will be saved as separate file with separate index in
                % the end
                if fMRI
                    % If the experiment was aborted, saving it as the ABORTED data:
                    if exist('AbortedMiniBlocks','var')
                        if size(AbortedMiniBlocks,2) == OUTPUT_COLS
                            % Saive the MiniBlocks, which is a fraction of the
                            % miniBlocks
                            AbortedMiniBlocks = AbortedMiniBlocks(:,EXP_COL:EVENT_TYPE_COL);
                            if exist('exp_Interrupt_counter','var') && exp_Interrupt_counter > 0 
                                save([abortedfileName '_' num2str(exp_Interrupt_counter) '.mat'],'AbortedMiniBlocks');
                            elseif exist('exp_abort_counter','var') &&  exp_abort_counter > 0
                                save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                            end
                        else
                            save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                        end
                    end
                elseif MEEG||ECoG
                    if exist('AbortedMiniBlocks','var')
                        if size(AbortedMiniBlocks,2) == OUTPUT_COLS
                            % Sainve the MiniBlocks, which is a fraction of the
                            % miniBlocks
                            AbortedMiniBlocks = AbortedMiniBlocks(:,EXP_COL:EVENT_TYPE_COL);
                            save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                        else
                            save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                        end
                    end
                end
                % We also save the whole miniBlock in the temporary file, this
                % is important to keep it if we interrupt for example
                save(fileNameWhole,'miniBlocks');
                
            catch % If things crash, retry the saving:
                if size(MiniBlocks,2) == OUTPUT_COLS
                    MiniBlocks = MiniBlocks(:,EXP_COL:EVENT_TYPE_COL);
                    save(fileName,'MiniBlocks');
                else
                    save(fileName,'MiniBlocks');
                end
                % If the experiment was aborted, saving it as the ABORTED data:
                % in case of fMRI each aborted or interrupted session
                % will be saved as separate file with separate index in
                % the end
                if fMRI 
                    exp_abort_counter=sum((ismember(MiniBlocks(:,EVENT_TYPE_COL),'Abortion')));
                    if exist('AbortedMiniBlocks','var')
                        if size(AbortedMiniBlocks,2) == OUTPUT_COLS
                            % Sainve the MiniBlocks, which is a fraction of the
                            % miniBlocks
                            AbortedMiniBlocks = AbortedMiniBlocks(:,EXP_COL:EVENT_TYPE_COL);
                            if exist('exp_Interrupt_counter','var') && exp_Interrupt_counter > 0 
                                save([abortedfileName '_' num2str(exp_Interrupt_counter) '.mat'],'AbortedMiniBlocks');
                            elseif  exist('exp_abort_counter','var') && exp_abort_counter > 0
                                save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                            end
                        end
                    end
                elseif MEEG||ECoG
                    if exist('AbortedMiniBlocks','var')
                        if size(AbortedMiniBlocks,2) == OUTPUT_COLS
                            % Sainve the MiniBlocks, which is a fraction of the
                            % miniBlocks
                            AbortedMiniBlocks = AbortedMiniBlocks(:,EXP_COL:EVENT_TYPE_COL);
                            save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                        else
                            save([abortedfileName '_' num2str(exp_abort_counter) '.mat'],'AbortedMiniBlocks');
                        end
                    end
                end
                % We also save the whole miniBlock in the temporary file, this
                % is important to keep it if we interrupt for example
                save(fileNameWhole,'miniBlocks');
            end
            
        catch ME% If the statements above fail, always save the whole miniBlock:
            warning(ME.message)
            warning('The saveBlockToHD function crashed, only the whole log saved!')
            fileNameWhole  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_NAMING_WHOLE]);
            save(fileNameWhole,'miniBlocks'); % Here, we have no contingencies if this statement fails. This is by DESIGN!!!! If that crashes, that means that the data are not saved, so there is no point in pursuing!!!
        end
        

%% Summary:
% Here, the summary gets saved. The summary should only get written
% in xls, but if there are issues with excel, the backup solution
% is to save it as .mat file:
    case 'Summary'
    fileName  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_SUMMARY_NAMING]);
    save(fileName,'miniBlocks');
end
end

