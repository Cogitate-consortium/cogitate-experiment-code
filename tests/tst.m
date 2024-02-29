clear; 
%load([pwd '\Seattle_ResultsS1_b1_1-06-Aug-2019.mat']);
load([pwd '\Seattle_ResultsS999_b1_999-04-Feb-2020.mat']);
t = cell2table(miniBlocks(2:end,:));
t.Properties.VariableNames = miniBlocks(1,:);
for i=2:size(t,1)
    t.eventType{i} = sprintf('%s',t.eventType{i});
end

%%
% do we have rows 1 to 40? 
rwIdx = unique(cell2mat(t.run))';
chck(1) = isequal(rwIdx,1:10);   

% is every run made of 4 different miniblocks?
chck(2) = true;     
for r=1:length(rwIdx)
    chck(2) = chck(2) && length(unique(cell2mat(t.miniBlock(cell2mat(t.run)==rwIdx(r)))))==4;
end

[mblkCnt, mblkIdx] = hist(cell2mat(t.miniBlock),40);
% do we have miniblocks 1 to 40? 
chck(3) = isequal(mblkIdx,1:40);   
% is minimal amount of miniblock rows between 34*4 and 34*5? (34 = 32 trials + 2 targets)
chck(4) = (min(mblkCnt)>=34*2) && (min(mblkCnt)<=34*5); 
% is maximal amount of miniblock rows between 38*4 and 38*5? (38 = 32 trials + 6 targets)
chck(5) = (max(mblkCnt)>=38*2) && (max(mblkCnt)<=38*5); 


mbks = unique(cell2mat(t.miniBlock));
mbtyp = zeros(length(mbks),1);
chck(6:8) = true; 
for mb=1:length(mbks)
    miniblockLoc = cell2mat(t.miniBlock)==mbks(mb);
    if strcmp(unique(t.miniBlockType(miniblockLoc)),'face & object')
        mbtyp(mb) = 1;
    elseif strcmp(unique(t.miniBlockType(miniblockLoc)),'letter & false')
        mbtyp(mb) = 2;
    end
    targ1(mb) = unique(cell2mat(t.targ1(miniblockLoc)));
    targ2(mb) = unique(cell2mat(t.targ2(miniblockLoc)));
    tartyps(mb,:) = sort([floor(targ1(mb)/1000) floor(targ2(mb)/1000)]);
    stimulusLoc = strcmp(t.eventType,'Stimulus');
    stimz = cell2mat(t.event(intersect(find(stimulusLoc),find(miniblockLoc))));
    
    % every miniblock must contain the same number of female and male faces
    facestim = stimz(floor(stimz/1000)==1);
    chck(6) = chck(6) && mean(mod(facestim,100)<=10)==.5;
    if mean(mod(facestim,100)<=10)~=.5
        %mean(mod(facestim,100)<=10)
        %facestim
    end
    % amount of directed and non-directed stimuli should be the same (within miniblock? or entire experiment?)
    stimdirection = mod(floor(stimz/100),10);
    tabulate(stimdirection)
    chck(7) = chck(7) && mean(stimdirection==1)==.5;
    if mean(stimdirection==1)~=.5
        mean(stimdirection==1)
        stimdirection
    end    
    % amount of rightward and leftward directed stimuli should be the same (within miniblock? or entire experiment?)
    stimdirection(stimdirection==1) = [];
    chck(8) = chck(8) && mean(stimdirection==2);
end

% targets can't repeat in different miniblocks (stimuli can act as target only once)
chck(9) = length(unique([targ1 targ2]))==2*length(mbks);

% target types conform to miniblock types
chck(10) = unique(tartyps(mbtyp==1,1))==1 && unique(tartyps(mbtyp==1,2))==2;
chck(10) = chck(10) && unique(tartyps(mbtyp==2,1))==3 && unique(tartyps(mbtyp==2,2))==4;

% miniblock types are distributed equally
chck(11) = mean(mbtyp==1)==.5;

% responses are either 1 or 2
chck(12) = all(ismember(unique(cell2mat(t.event(strcmp(t.eventType,'Response')))),[1 2]));