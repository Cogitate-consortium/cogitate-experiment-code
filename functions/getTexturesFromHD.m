
% GETTEXTUREFROMHD reads all textures from HD. Wasteful and should be
% employed before the experiment is running
% input:
% ------
% folder - the folder from which to read the images
%
% output:
% -------
% textureArray - an array of texture pointers, corresponding to the
% pictures in the given folder.

function [ textureArray ] = getTexturesFromHD(folder)
    global w originalHeight originalWidth FILE_POSTFIX

    fileList = dir(fullfile(folder,FILE_POSTFIX));
    %disp('----------------Here comes the file list ------------');
   

    textureArray = [];

    for i = 1 : size(fileList,1)
        try
            [img, ~, alpha] = imread(fullfile(folder,fileList(i).name));
            %disp(i);
            %disp(fullfile(fileList(i).folder,fileList(i).name));
        catch
            disp(fullfile(folder,fileList(i).name));
            [img, ~, alpha] = imread(fullfile(folder,fileList(i).name));
        end
        %https://stackoverflow.com/questions/40381751/how-do-i-make-a-png-file-display-in-psychtoolbox-with-a-transparent-background
        [originalHeight, originalWidth, t3, ~] = size(img);
        if t3 == 1 % this means its a monochrome image
            img(:, :, 2) = alpha;
        else
            img(:, :, 4) = alpha;
        end
        textureArray = [textureArray, Screen('MakeTexture', w, img)];
    end
end