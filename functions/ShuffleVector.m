function outVec = ShuffleVector(inVec)
    inVec = transpose(inVec);
    outVec = transpose(inVec(randperm(size(inVec,1)),:));            
end