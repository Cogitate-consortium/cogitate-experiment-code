function [textureArray]=load_animal_imag(folder)
%% This function is only written for ECoG mode to motivate patients further with cute animal pictures
global w FILE_POSTFIX
textureArray = [];
fileList = dir(fullfile(folder,FILE_POSTFIX));

for i = 1 : size(fileList,1)
    [img, ~, alpha] = imread(fullfile(folder,fileList(i).name));
    textureArray = [textureArray, Screen('MakeTexture', w, img)];
end
end