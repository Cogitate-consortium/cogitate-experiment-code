%SENDTRIG
% MEEG trigger function : manages sending triggers and documents them in internal program array "triggers".
function [] = sendTrig(trigCode,LPT_OBJECT,LPT_ADDRESS)

    global DEBUG triggsStart triggers triggsCounter
    
    if (isempty(LPT_OBJECT))
        if ~DEBUG
            error('LPT not initiated, trigger will not be sent!');
        else
            warning ('LPT not initiated, trigger will not be sent!');
        end
        triggsCounter = triggsCounter + 1;
        % Mark  the time stamp
        triggers{triggsCounter,2} = GetSecs - triggsStart;
        % Add the trigger code
        triggers{triggsCounter,1} = trigCode;
        % Mark the trigger as failed
        triggers{triggsCounter,3} = cellstr('TRIGGER_FAILED');
        return;
    end
    try
        % The first thing to do is to query the status of the port, because
        % if we have anything else than a 0, we have a problem:
        LTP_State = io64( LPT_OBJECT, LPT_ADDRESS );
        % If the port is not on 0
        if LTP_State ~= 0 && trigCode ~=0
            if DEBUG
                error('Port occupied! State: %d',LTP_State)
            elseif ~DEBUG
                % Send a warning
                warning('Port occupied! State: %d',LTP_State)
            end
            % Actualize the counter
            triggsCounter = triggsCounter + 1;
            % Mark  the time stamp
            triggers{triggsCounter,2} = GetSecs - triggsStart;
            % Add the trigger code
            triggers{triggsCounter,1} = trigCode;
            % Mark the trigger as failed
            triggers{triggsCounter,3} = cellstr('TRIGGER_FAILED');
            % But if it is on 0
        else
            % Send the trigger
            io64(LPT_OBJECT,LPT_ADDRESS,trigCode);
            % Actualize counter
            triggsCounter = triggsCounter + 1;
            % Store time stamp
            triggers{triggsCounter,2} = GetSecs - triggsStart;
            % Add the trigger
            triggers{triggsCounter,1} = trigCode;
            % Mark it as sent
            triggers{triggsCounter,3} = cellstr('TRIGGER_SENT');
        end
        % If there is any other issue, note the trigger as failed too
    catch %e 
        warning ('Trigger not sent correctly!');
        if DEBUG
            triggsCounter = triggsCounter + 1;
            triggers{triggsCounter,2} = GetSecs - triggsStart;
            triggers{triggsCounter,1} = trigCode;
            % Mark the trigger as failed
            triggers{triggsCounter,3} = cellstr('TRIGGER_FAILED');
        end
    end  
    
end