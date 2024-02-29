function scannerListener()
%   Script information:
%       - Version:      8.0.
%       - Author:       Aya Khalaf (email:aya.khalaf@yale.edu)
%       - Date:         08/17/2020
global  LAB_ID  bitsi_scanner compKbDevice abortKey
   
switch LAB_ID
    case 'SC'
        bitsi_scanner.clearResponses();         %clear bitsi buffer
        triggerKey = 97;                        % scanTrigger      
    case 'SD'
        KbName('UnifyKeyNames');
        triggerKey=  KbName('5%');   %TR triggers are received as key presses of 5
        KbNamesCodes= 1:256;
        RestrictKeysForKbCheck(KbNamesCodes);
end


triggerCount=0;
noKey=1;
key=noKey;
untilTime=Inf;

while (key~=0)
    switch LAB_ID
        case 'SC'
            [Resp, ~]= bitsi_scanner.getResponse(untilTime,true);
            if Resp == triggerKey
                key = 5;
            else
                [~, ~, Resp] = KbCheck(compKbDevice);
                if Resp(abortKey)
                    key=0;
                else
                    key=noKey;
                end
            end
        case 'SD'
            [~, Resp, ~] =KbPressWait(compKbDevice,WaitSecs(0)+untilTime);
            if Resp(triggerKey)
                key = 5;
            elseif Resp(abortKey)
                key=0;
            else
                key=noKey;
            end
    end
    
    if key == 5
        triggerCount=triggerCount+1;
        if(triggerCount==4)
            break
        end
        untilTime=2;
    end
    
end
end


