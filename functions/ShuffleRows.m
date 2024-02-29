function [ShuffledMatrix] = ShuffleRows(orderedMatrix)

    % input a matrix argument
    % create a random permutation of the rows
    ShuffledMatrix = orderedMatrix(randperm(size(orderedMatrix,1)),:);

end