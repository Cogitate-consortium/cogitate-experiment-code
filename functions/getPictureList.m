
% PICTURELIST get the list of the picture file names as appear in the
% pre-defined folders
% input:
% ------
% folder - the folder from which to get a list of picture file names.
%
% output:
% -------
% pictureList - a vector of the file names within a given folder.

function [ pictureList ] = getPictureList(folder)

global FILE_POSTFIX

try
    fileList = dir(fullfile(folder,FILE_POSTFIX));
catch
    fileList = dir(fullfile(folder,FILE_POSTFIX));
end

pictureList = cell(size(fileList,1),1);

%display(pictureList,'pictureList');

for i = 1 : size(fileList,1)
    if (strcmp(fileList(i).name,'desktop.ini'))
        fileList(i)=[];
    end
    if i <= size(fileList,1)
        pictureList{i,1} = fileList(i).name;
    end
end
if size(pictureList,1) > size(fileList,1)
    pictureList = pictureList(1:end-1,:);
end

%display(pictureList,'pictureList');

end % end function