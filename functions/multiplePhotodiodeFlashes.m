function multiplePhotodiodeFlashes(numberOfFlashes)
global DIOD_DURATION refRate

for i = 1:numberOfFlashes
    turnPhotoTrigger('on');
    WaitSecs(refRate*(DIOD_DURATION-0.5));
    turnPhotoTrigger('off');
    WaitSecs(refRate*(DIOD_DURATION-0.5));
end

end