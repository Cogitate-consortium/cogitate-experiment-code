
% MEEG trigger function trigger hardware intilization
function [ Object,LPT_address ] = init_LPT()
    global EEG_MACHINE_HEX
    % config_io;
    % RB_address = hex2dec('D010');
    % LPT_address = hex2dec('0378');
    LPT_address = hex2dec(EEG_MACHINE_HEX); %Machine specific address

    Object=io64;
    status=io64(Object);
    if status
        disp ('fail')
        Object = [];
    end
end