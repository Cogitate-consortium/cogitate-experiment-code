
%ENDEEG
% The function sends end of experiment signals to the trigger buffer and
% saves it in the data folder.
function [] = endEEG()

    global LPT_CODE_END  LPT_OBJECT LPT_ADDRESS TRG_MINIBLOCK_ENDED refRate

    sendTrig(LPT_CODE_END,LPT_OBJECT,LPT_ADDRESS);
    WaitSecs(refRate);
    sendTrig(0,LPT_OBJECT,LPT_ADDRESS);
    saveTrigToHD();

end