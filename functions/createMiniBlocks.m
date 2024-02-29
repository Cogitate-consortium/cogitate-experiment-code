% CREATEBLOCKS the function that creates the needed block with all basic
% properties: trial list, mini block type, mini block size, trial times, jitter times and mini block number.
% It return a cell array ready to be used as the data structure for the experiment.
% This function is wasteful and uses the HD, and should run only before the experiment runs!
%
% output:
% -------
% data - a data structure used by the runBlock function (see the initConstantsParameters function for its structure).
function [ data, TriggerMatrix ] = createMiniBlocks( )


    global MINI_BLK_NUM_COL MINI_BLOCK_SIZE_COL SUBJECT_START_TIME SUBJECT_NUMBER_COL DATA_TABLE_SIZE TRIAL1_JITTER_TIME_COL TRIAL1_NAME_COL MAX_NUM_OF_TRIALS_PER_MINI_BLOCK TARGET1_COL TARGET2_COL TRIAL1_TIME_COL MINIBLOCK_TYPE_COL subjectNum subStartTime %subject charicatristics
    global VERBOSE 
    global ECoG
    global DATA_FOLDER LAB_ID TEMPORARY_FOLDER
    global MATRIX_GENERATION
     if VERBOSE
        disp('');
        disp('WELCOME TO createMiniBlocks');
        disp('');
    end
    % All trials creation and verification happens here
    [ miniBlocks, miniBlockSize, targetsType, targets, times, jitter ] = createTrials();
    
    % For each trial, a trigger vector is created, containing the different
    % successive triggers to be sent:
    
    % Putting the trials in place within the experiment data structure
    data = cell(size(miniBlocks,1),DATA_TABLE_SIZE); % 40 rows x 430 columns
    data(:,TRIAL1_NAME_COL:(TRIAL1_NAME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK-1)) = num2cell(miniBlocks);
    data(:,MINIBLOCK_TYPE_COL) = num2cell(targetsType);
    data(:,MINI_BLOCK_SIZE_COL) = num2cell(miniBlockSize);
    data(:,TARGET1_COL:TARGET2_COL) = num2cell(targets);
    data(:,TRIAL1_TIME_COL:(TRIAL1_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK-1)) = num2cell(times);
    data(:,TRIAL1_JITTER_TIME_COL:(TRIAL1_JITTER_TIME_COL + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK-1)) = num2cell(jitter);
    for tr = 1 : size(data,1)
        data{tr,SUBJECT_NUMBER_COL} = subjectNum;
        data{tr,SUBJECT_START_TIME} = subStartTime;
    end
    
     % The miniblocks (rows) are shuffled.
     %data = ShuffleRows(data);
     
     % debugging:
     % save all structures to be able to compare
     % save("data.mat", "data");
     % Make sure that the miniblocks are organized as fo fo lf lf, lf lf fo fo or fo fo lf lf etc
     
     % Gather all fo mbs. Pick out the rows which correspond to fo mbs.
     
     fo_mbs = cell(size(miniBlocks,1)/2,DATA_TABLE_SIZE);
     lf_mbs = cell(size(miniBlocks,1)/2,DATA_TABLE_SIZE);
     
     % Go throuh all rows and pick out the fos and the lfs
     mb_type_col = data(:,MINIBLOCK_TYPE_COL);
     
     fo_mbs_ctr = 1;
     lf_mbs_ctr = 1;
     for i= 1:size(data,1) % iterate over rows = miniblocks
        
         mb_type = mb_type_col{i};
         if mb_type == 1
            fo_mbs(fo_mbs_ctr,:) = data(i,:); 
            fo_mbs_ctr = fo_mbs_ctr + 1;
         else
            lf_mbs(lf_mbs_ctr,:) = data(i,:); 
            lf_mbs_ctr = lf_mbs_ctr + 1;
         end
     end
     
     %save("fo_mbs.mat", "fo_mbs");
     %save("lf_mbs.mat", "lf_mbs");
     
     % Shuffle.
     fo_mbs = ShuffleRows(fo_mbs);
     lf_mbs = ShuffleRows(lf_mbs);
     
     % Randomize which to start with, fo or lf.
     r = [1 2];
     r = Shuffle(r);
     first_mb_type = r(1);
     
     % pick 2 from this category
     if first_mb_type == 1
         mbs_A = fo_mbs;
         mbs_B = lf_mbs;
     else 
         mbs_A = lf_mbs;
         mbs_B = fo_mbs;
     end
     
     data_new = cell(size(miniBlocks,1),DATA_TABLE_SIZE);
     
     % Place the first two
     
     data_new(1,:) = mbs_A(1,:);
     data_new(2,:) = mbs_A(2,:);
     
     ind_data_new = 3;
     ind_A = 3;
     ind_B = 1;
     
     A_loop = false;
     B_loop = true;
     
     % pick 4 from every other category until end of mbs: AA BB BB AA
     % AA BB BB AA
    
     % Only fill until last two, they need to be AA
     while ind_data_new < size(miniBlocks,1) - 1
         
         if A_loop
             
             % Fill with 4 A mbs
             for j=1:4
             data_new(ind_data_new,:) = mbs_A(ind_A, :);
             ind_data_new = ind_data_new+1;
             ind_A = ind_A+1;
             end 
             
             A_loop = false;
             B_loop = true;
             
         elseif B_loop % B loop
             
             % Fill with 4 B mbs
             for j=1:4
             data_new(ind_data_new,:) = mbs_B(ind_B, :);
             ind_data_new = ind_data_new+1;
             ind_B = ind_B+1;
             end 
             
             B_loop = false;
             A_loop = true;
             
         end % end
         
     end % for
     
     % Place the last two
     if ECoG %Uneven number of blocks AA BB / BB AA / AA BB / BB AA / AA BB 
         data_new(ind_data_new,:) = mbs_B(ind_B,:);
         ind_B = ind_B+1;
         ind_data_new = ind_data_new+1;
         data_new(ind_data_new,:) = mbs_B(ind_B,:);   
     else 
         data_new(ind_data_new,:) = mbs_A(ind_A,:);
         ind_A = ind_A+1;
         ind_data_new = ind_data_new+1;
         data_new(ind_data_new,:) = mbs_A(ind_A,:);
     end
     
     %save("data_new.mat", "data_new");
     
     data = data_new;
     
     data(:,MINI_BLK_NUM_COL) = num2cell(transpose(1:1:size(miniBlocks,1))); % add miniblock numbers
     
     [TriggerMatrix] = createTriggersVector(data);
     
     if ~MATRIX_GENERATION
         % Preparing the saving of the trigger matrix, in case we need to fetch
         % it when restarting
         TriggerFile  = sprintf('%s%c%s%c%s%c%s%c%s.mat',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,TEMPORARY_FOLDER,filesep,'TriggerMatrix');
         % Making the dir:
         if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],TEMPORARY_FOLDER),'dir')
             mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)],TEMPORARY_FOLDER));
         end
         
         save(TriggerFile,'TriggerMatrix')
     else
         % Preparing the saving of the trigger matrix, in case we need to fetch
         % it when restarting
         TriggerFile  = fullfile(pwd,'TrialMatrices',LAB_ID,[LAB_ID,num2str(subjectNum),'_TriggerMatrix']);
         MiniblockFile = fullfile(pwd,'TrialMatrices',LAB_ID,[LAB_ID,num2str(subjectNum),'_TrialMatrix']);
         % Making the dir:
         if ~exist(fullfile(pwd,'TrialMatrices',LAB_ID),'dir')
             mkdir(fullfile(pwd,'TrialMatrices',LAB_ID));
         end
         save(MiniblockFile,'data')
         save(TriggerFile,'TriggerMatrix')
     end
end
