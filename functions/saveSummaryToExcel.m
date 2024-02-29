
% SAVESUMMARYTOEXCEL this functions creates and saves the summary to excel (mat file, if excel fails) according
% to specifications. For every stimuli, orientation and type, % accuracy,
% hits, misses, FA, CR, and RT.
% input:
% ------
% miniBlocks - the cell array to analyzed
% miniBlockNumber - the block number to be written in the filename
% output:
% -------
% An excel file containning a summary of the data in blockIn

function [ ] = saveSummaryToExcel( miniBlocks, miniBlockNumber )

    global EVENT_TYPE_COL CATEGORY_COL ORIENTATION_COL EVENT_COL DATA_FOLDER subjectNum %subject number
    global MALE_FOLDER FEMALE_FOLDER CHARS_C_FOL FALSES_C_FOL OBJECTS_C_FOL FACES_C_FOL  ACCURATE_COL HT_COL MS_COL CRS_COL FAS_COL RT_COL
    global NUM_OF_STIMULI_EACH NUM_OF_ORIENTATIONS NUM_OF_CATEGORIES excelFormatSummary LAB_ID BEHAV_FILE_SUMMARY_NAMING
    
    if ~exist(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]),'dir')
        mkdir(fullfile(pwd,DATA_FOLDER,[LAB_ID,num2str(subjectNum)]));
    end
    % We cannot save the date
    %prf1 = sprintf('%d-%s',subjectNum, date);
    %fileName  = sprintf('%s%c%s%c%d%c%s_Summary_S%d_b%d_%s',pwd,filesep,DATA_FOLDER,filesep,subjectNum,filesep,EXPERIMENT_NAME,subjectNum, miniBlockNumber);
    fileName  = sprintf('%s%c%s%c%s%c%s',pwd,filesep,DATA_FOLDER,filesep,[LAB_ID,num2str(subjectNum)],filesep,[LAB_ID,num2str(subjectNum),BEHAV_FILE_SUMMARY_NAMING]);

    % 4 types, 3 orientations, 20 stimuli, 6 information types + 1 ctr for RT
    % accuracy, hits, misses, correct reject, false accept, RT
    headerInfo = {'Accuracy','Hits','Misses','Correct_Reject','False_Alarm','RT'};
    summaryMatStim = zeros(NUM_OF_CATEGORIES,NUM_OF_STIMULI_EACH,7); % 20 in 4 categories stimuli + ctr
    summaryMatOrient = zeros(NUM_OF_ORIENTATIONS,7); % 3 orientations + ctr
    summaryMatCat = zeros(NUM_OF_CATEGORIES,7); % 4 categories + ctr
    summaryTotal = zeros(1,7); % 6 info types + ctr

    for i = 1 : size(miniBlocks,1)
        if strcmp(miniBlocks{i,EVENT_TYPE_COL},'Stimulus')
            summaryMatCat(miniBlocks{i,CATEGORY_COL},1) = miniBlocks{i,ACCURATE_COL} + summaryMatCat(miniBlocks{i,CATEGORY_COL},1);
            summaryMatCat(miniBlocks{i,CATEGORY_COL},2) = miniBlocks{i,HT_COL} + summaryMatCat(miniBlocks{i,CATEGORY_COL},2);
            summaryMatCat(miniBlocks{i,CATEGORY_COL},3) = miniBlocks{i,MS_COL} + summaryMatCat(miniBlocks{i,CATEGORY_COL},3);
            summaryMatCat(miniBlocks{i,CATEGORY_COL},4) = miniBlocks{i,CRS_COL} + summaryMatCat(miniBlocks{i,CATEGORY_COL},4);
            summaryMatCat(miniBlocks{i,CATEGORY_COL},5) = miniBlocks{i,FAS_COL} + summaryMatCat(miniBlocks{i,CATEGORY_COL},5);
            if ~isempty(miniBlocks{i,RT_COL})
                summaryMatCat(miniBlocks{i,CATEGORY_COL},6) = miniBlocks{i,RT_COL} + summaryMatCat(miniBlocks{i,CATEGORY_COL},6);
                summaryMatCat(miniBlocks{i,CATEGORY_COL},7) = 1 + summaryMatCat(miniBlocks{i,CATEGORY_COL},7);
            end

            summaryMatOrient(miniBlocks{i,ORIENTATION_COL},1) = miniBlocks{i,ACCURATE_COL} + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},1);
            summaryMatOrient(miniBlocks{i,ORIENTATION_COL},2) = miniBlocks{i,HT_COL} + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},2);
            summaryMatOrient(miniBlocks{i,ORIENTATION_COL},3) = miniBlocks{i,MS_COL} + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},3);
            summaryMatOrient(miniBlocks{i,ORIENTATION_COL},4) = miniBlocks{i,CRS_COL} + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},4);
            summaryMatOrient(miniBlocks{i,ORIENTATION_COL},5) = miniBlocks{i,FAS_COL} + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},5);
            if ~isempty(miniBlocks{i,RT_COL})
                summaryMatOrient(miniBlocks{i,ORIENTATION_COL},6) = miniBlocks{i,RT_COL} + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},6);
                summaryMatOrient(miniBlocks{i,ORIENTATION_COL},7) = 1 + summaryMatOrient(miniBlocks{i,ORIENTATION_COL},7);
            end

            summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),1) = miniBlocks{i,ACCURATE_COL} + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),1);
            summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),2) = miniBlocks{i,HT_COL} + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),2);
            summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),3) = miniBlocks{i,MS_COL} + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),3);
            summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),4) = miniBlocks{i,CRS_COL} + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),4);
            summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),5) = miniBlocks{i,FAS_COL} + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),5);
            if ~isempty(miniBlocks{i,RT_COL})
                summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),6) = miniBlocks{i,RT_COL} + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),6);
                summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),7) = 1 + summaryMatStim(miniBlocks{i,CATEGORY_COL},mod(miniBlocks{i,EVENT_COL},100),7);
            end

            summaryTotal(1) = summaryTotal(1) + miniBlocks{i,ACCURATE_COL};
            summaryTotal(2) = summaryTotal(2) + miniBlocks{i,HT_COL};
            summaryTotal(3) = summaryTotal(3) + miniBlocks{i,MS_COL};
            summaryTotal(4) = summaryTotal(4) + miniBlocks{i,CRS_COL};
            summaryTotal(5) = summaryTotal(5) + miniBlocks{i,FAS_COL};
            if ~isempty(miniBlocks{i,RT_COL})
                summaryTotal(6) = summaryTotal(6) + miniBlocks{i,RT_COL};
                summaryTotal(7) = summaryTotal(7) + 1;
            end
        end
    end

    %calculate mean RT with the cntr
    for i = 1 : NUM_OF_CATEGORIES
        if summaryMatCat(i,7) ~= 0
            summaryMatCat(i,6) = summaryMatCat(i,6)/summaryMatCat(i,7);
        end
    end

    for i = 1 : NUM_OF_ORIENTATIONS
        if summaryMatOrient(i,7) ~= 0
            summaryMatOrient(i,6) = summaryMatOrient(i,6)/summaryMatOrient(i,7);
        end
    end

    for i = 1 : NUM_OF_STIMULI_EACH
        for j = 1 : NUM_OF_CATEGORIES
            if summaryMatStim(j,i,7) ~= 0
                summaryMatStim(j,i,6) = summaryMatStim(j,i,6)/summaryMatStim(j,i,7);
            end
        end
    end

    if summaryTotal(7) ~= 0
        summaryTotal(6) = summaryTotal(6)/summaryTotal(7);
    end

    summaryMatStim = summaryMatStim(:,:,1:end-1); % 20 in 4 categories stimuli - ctr
    summaryMatOrient = summaryMatOrient(:,1:end-1); % 3 orientations - ctr
    summaryMatCat = summaryMatCat(:,1:end-1); % 4 categories - ctr
    summaryTotal = summaryTotal(:,1:end-1); % 6 info types - ctr

    TsummaryMatCat = array2table(summaryMatCat,'VariableNames',headerInfo);
    TsummaryMatCat = [{'Face';'Object';'Letter';'False_Font'},TsummaryMatCat];%,'Before','Accuracy');
    TsummaryMatCat.Properties.VariableNames{1} = 'Category';
    TsummaryMatCat.Accuracy = TsummaryMatCat.Accuracy ./ (TsummaryMatCat.Hits + TsummaryMatCat.Misses + TsummaryMatCat.Correct_Reject + TsummaryMatCat.False_Alarm);
    TsummaryMatOrient = array2table(summaryMatOrient,'VariableNames',headerInfo);
    TsummaryMatOrient = ([{'Center';'Left';'Right'},TsummaryMatOrient]);
    TsummaryMatOrient.Properties.VariableNames{1} = 'Orientation';
    TsummaryMatOrient.Accuracy = TsummaryMatOrient.Accuracy ./ (TsummaryMatOrient.Hits + TsummaryMatOrient.Misses + TsummaryMatOrient.Correct_Reject + TsummaryMatOrient.False_Alarm);
    tmp = squeeze(summaryMatStim(1,:,:));
    TsummaryMatStim1 = array2table(tmp,'VariableNames',headerInfo);
    TsummaryMatStim1 = ([array2table(transpose(1:1:20)),TsummaryMatStim1]);
    TsummaryMatStim1.Properties.VariableNames{1} = 'Filename';
    TsummaryMatStim1.Accuracy = TsummaryMatStim1.Accuracy ./ (TsummaryMatStim1.Hits + TsummaryMatStim1.Misses + TsummaryMatStim1.Correct_Reject + TsummaryMatStim1.False_Alarm);
    TsummaryMatStim1.Filename = transpose([transpose(getPictureList(fullfile(FACES_C_FOL,MALE_FOLDER))), transpose(getPictureList(fullfile(FACES_C_FOL,FEMALE_FOLDER)))]);
    tmp = squeeze(summaryMatStim(2,:,:));
    TsummaryMatStim2 = array2table(tmp,'VariableNames',headerInfo);
    TsummaryMatStim2 = ([array2table(transpose(1:1:20)),TsummaryMatStim2]);
    TsummaryMatStim2.Properties.VariableNames{1} = 'Filename';
    TsummaryMatStim2.Accuracy = TsummaryMatStim2.Accuracy ./ (TsummaryMatStim2.Hits + TsummaryMatStim2.Misses + TsummaryMatStim2.Correct_Reject + TsummaryMatStim2.False_Alarm);
    TsummaryMatStim2.Filename = (getPictureList(OBJECTS_C_FOL));
    tmp = squeeze(summaryMatStim(3,:,:));
    TsummaryMatStim3 = array2table(tmp,'VariableNames',headerInfo);
    TsummaryMatStim3 = ([array2table(transpose(1:1:20)),TsummaryMatStim3]);
    TsummaryMatStim3.Properties.VariableNames{1} = 'Filename';
    TsummaryMatStim3.Accuracy = TsummaryMatStim3.Accuracy ./ (TsummaryMatStim3.Hits + TsummaryMatStim3.Misses + TsummaryMatStim3.Correct_Reject + TsummaryMatStim3.False_Alarm);
    TsummaryMatStim3.Filename = (getPictureList(CHARS_C_FOL));
    tmp = squeeze(summaryMatStim(4,:,:));
    TsummaryMatStim4 = array2table(tmp,'VariableNames',headerInfo);
    TsummaryMatStim4 = ([array2table(transpose(1:1:20)),TsummaryMatStim4]);
    TsummaryMatStim4.Properties.VariableNames{1} = 'Filename';
    TsummaryMatStim4.Accuracy = TsummaryMatStim4.Accuracy ./ (TsummaryMatStim4.Hits + TsummaryMatStim4.Misses + TsummaryMatStim4.Correct_Reject + TsummaryMatStim4.False_Alarm);
    TsummaryMatStim4.Filename = (getPictureList(FALSES_C_FOL));
    TsummaryTotal = array2table((summaryTotal),'VariableNames',headerInfo);
    TsummaryTotal.Accuracy = TsummaryTotal.Accuracy ./ (TsummaryTotal.Hits + TsummaryTotal.Misses + TsummaryTotal.Correct_Reject + TsummaryTotal.False_Alarm);

    try
        sheet = 1;
        writetable(TsummaryTotal,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
        sheet = 2;
        writetable(TsummaryMatCat,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
        sheet = 3;
        writetable(TsummaryMatOrient,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
        sheet = 4;
        writetable(TsummaryMatStim1,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
        sheet = 5;
        writetable(TsummaryMatStim2,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
        sheet = 6;
        writetable(TsummaryMatStim3,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
        sheet = 7;
        writetable(TsummaryMatStim4,[fileName, excelFormatSummary],'sheet',sheet,'Range','A1')
     
    catch ME% if excel malfunctions write csv as mat: 
        warning(ME.message)
        warning('The saveSummaryToExcel function crashed!')
        saveBlockToHD({TsummaryTotal TsummaryMatCat TsummaryMatOrient TsummaryMatStim1 TsummaryMatStim2 TsummaryMatStim3 TsummaryMatStim4},1,'Summary');
    end
end
