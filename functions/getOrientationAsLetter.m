function orientation = getOrientationAsLetter(num)

    orientationAsNr = (mod(num,1000) - mod(num,100))/100;
    if orientationAsNr == 1
        orientation = 'C';
    elseif orientationAsNr == 2
        orientation = 'L';
    elseif orientationAsNr == 3
        orientation = 'R';
    else
        disp('WARNING NO ORIENTATION ASSIGNED WHEN ASKING FOR IT')
    end % if
    
end % end function getOrientationAsLetter

