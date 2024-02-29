% This script can be used to run the experiment one over and over again to
% generate the trial matrices ahead of time:
function createTrialMatrices(nMatrices)
for i = 1:nMatrices
   runExp1(i+100,60)    
end
end


