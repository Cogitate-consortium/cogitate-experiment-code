% This function attributes the times based on the stimuli ID:
function tm = getTime(numArr)
global STIM_DURATION
    tm = nan(1,size(numArr,2));
    for i = 1: size(numArr,2)
        switch round(mod(numArr(i),1)*10)
            case round(0.1000*10)
                tm(i) = STIM_DURATION(1);
            case round(0.2000*10)
                tm(i) = STIM_DURATION(2);
            case round(0.3000*10)
                tm(i) = STIM_DURATION(3);
        end
    end
end