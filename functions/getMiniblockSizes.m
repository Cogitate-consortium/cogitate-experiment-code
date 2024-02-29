function sizesVec = getMiniblockSizes(num)
    global MIN_NUM_OF_TRIALS_PER_MINI_BLOCK MAX_NUM_OF_TRIALS_PER_MINI_BLOCK NUM_OF_TARGETS_PER_MINIBLOCK NUM_OF_STIM_TYPE_PER_MINIBLOCK NUM_OF_CATEGORIES
    sizesVec = datasample(NUM_OF_TARGETS_PER_MINIBLOCK,num) + NUM_OF_STIM_TYPE_PER_MINIBLOCK * NUM_OF_CATEGORIES;
    if sum(sizesVec) ~= ((MIN_NUM_OF_TRIALS_PER_MINI_BLOCK + MAX_NUM_OF_TRIALS_PER_MINI_BLOCK) / 2) * num % 36 average number of stim per miniblock
        sizesVec = getMiniblockSizes(num);
    end
end