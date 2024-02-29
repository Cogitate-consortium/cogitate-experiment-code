function [ ] = Instructions()
global LAB_ID INSTRUCTIONS1 INSTRUCTIONS2 INSTRUCTIONS3 INSTRUCTIONS4 INSTRUCTIONS5 INSTRUCTIONS6
global fMRI ECoG compKbDevice bitsi_buttonbox
global RightKey LeftKey

Instructions1Restart=0;
Instructions2Restart=0;

if ECoG
    InstructionsPaths = [INSTRUCTIONS1;INSTRUCTIONS2;INSTRUCTIONS3;INSTRUCTIONS4;INSTRUCTIONS5;INSTRUCTIONS6];
else
    InstructionsPaths = [INSTRUCTIONS1;INSTRUCTIONS2;INSTRUCTIONS3];
end

% Setting the slide number to 1 to initiate the while loop
InstructionSlideNum = 1;
% displays all instructionn screens
while InstructionSlideNum<= size(InstructionsPaths,1) % Looping until we went through all slides:
    % Showing instruction slide
    showInstructions(InstructionsPaths(InstructionSlideNum,:));
    if fMRI
        switch LAB_ID
            case 'SC'
                % fMRI at SC lab:
                response = 0;
                bitsi_buttonbox.clearResponses()
                % If we are at the first slide, we can't go back one so:
                if InstructionSlideNum == 1
                    while ~(response == 97) % As long at the participants answer something else than go to next slide, don't proceed
                        [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
                    end
                else % If we are at slide 2 and beyond, participant can press to proceed or go back
                    while ~(response == 97 || response == 98)
                        [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
                    end
                end
                if response == 97 % If participant want to proceed, then the instruction slide is incremented by 1
                    InstructionSlideNum = InstructionSlideNum + 1;
                else % If participants pressed go back, then the slide number is reduced by one
                    InstructionSlideNum = InstructionSlideNum - 1;
                end
            case 'SD' % For the fMRI lab SD, they are using another response box:
                
                CorrectKey = 0; % Setting a flag for whether the response was correct, i.e. go back or forth
                while ~CorrectKey % As long as the participant press a key they shouldn't, keep looping
                    [~, InstructionsResp, ~] =KbWait(compKbDevice,3); % Waiting for input
                    if InstructionsResp(RightKey) % If participants pressed the right key (i.e. proceed)
                        InstructionSlideNum = InstructionSlideNum + 1; % Then incrememnt in the slide number
                        CorrectKey = 1;  % Setting the CorrectKey to 1 to break the loop
                    elseif InstructionsResp(LeftKey) % If participants pressed the left key
                        if InstructionSlideNum == 1 % If we are at slide 1, this is an incorrect key
                            CorrectKey = 0;
                        else % But if we are beyond, then it is correct and it goes back one slide
                            InstructionSlideNum = InstructionSlideNum - 1;
                            CorrectKey = 1; 
                        end
                    else % If the participants pressed anything else, just ask again
                        CorrectKey = 0;
                    end
                end
        end
    else % For ECoG and MEEG, they also use a different response box:
        
        CorrectKey = 0; % Setting the CorrectKey to 0 to initiate the loop
        while ~CorrectKey % As long as a non-accepted key is pressed, keep on asking
            [~, InstructionsResp, ~] =KbWait(compKbDevice,3); 
            if InstructionsResp(RightKey) % If the participant press the right key, increment by 1 the slide number
                InstructionSlideNum = InstructionSlideNum + 1;
                CorrectKey = 1; 
            elseif InstructionsResp(LeftKey) % Else if the participant pressed the left key:
                if InstructionSlideNum == 1 % If we are at slide one, that doesn't work
                    CorrectKey = 0; 
                else % Otherwise, just go back one slide
                    InstructionSlideNum = InstructionSlideNum - 1;
                    CorrectKey = 1; 
                end
            else
                CorrectKey = 0;
            end
        end
    end
end
% while 1
%     if(~Instructions2Restart)
%         showInstructions(INSTRUCTIONS1);
%         Instructions1Restart=0;
%         if fMRI
%             switch LAB_ID
%                 case 'SC'
%                     response = 0;
%                     bitsi_buttonbox.clearResponses()
%                     while ~(response == 97)
%                         [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
%                     end
%                 case 'SD'
%                     [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%                     while (~InstructionsResp(RightKey))
%                         [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%                     end
%
%             end
%         else
%             [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%             while (~InstructionsResp(RightKey))
%                 [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%             end
%         end
%     end
%     showInstructions(INSTRUCTIONS2);
%     Instructions2Restart=0;
%     if fMRI
%         switch LAB_ID
%             case 'SC'
%                 response = 0;
%                 bitsi_buttonbox.clearResponses()
%                 while ~(response == 97 || response == 98)
%                     [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
%                 end
%                 if response == 97
%                     Instructions1Restart=0;
%                 else
%                     Instructions1Restart=1;
%                 end
%             case 'SD'
%
%                 [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%                 if InstructionsResp(RightKey)
%                     Instructions1Restart=0;
%                 elseif InstructionsResp(LeftKey)
%                     Instructions1Restart=1;
%                 end
%         end
%     else
%         [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%         if InstructionsResp(RightKey)
%             Instructions1Restart=0;
%         elseif InstructionsResp(LeftKey)
%             Instructions1Restart=1;
%         end
%     end
%     if(~Instructions1Restart)
%         showInstructions(INSTRUCTIONS3);
%         Instructions2Restart=0;
%         if fMRI
%             switch LAB_ID
%                 case 'SC'
%                     response = 0;
%                     bitsi_buttonbox.clearResponses()
%                     while ~(response == 97 || response == 98)
%                         [response, ~] = bitsi_buttonbox.getResponse(Inf,true);
%                     end
%                     if response == 97
%                         break;
%                     else
%                         Instructions2Restart=1;
%                     end
%                 case 'SD'
%                     [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%                     if InstructionsResp(RightKey)
%                         break;
%                     elseif InstructionsResp(LeftKey)
%                         Instructions2Restart=1;
%                     end
%             end
%         else
%             [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%             if InstructionsResp(RightKey)
%                 break;
%             elseif InstructionsResp(LeftKey)
%                 Instructions2Restart=1;
%             end
%         end
%     end
%
%     if ECoG
%         if(~Instructions1Restart)
%             showInstructions(INSTRUCTIONS4);
%             Instructions2Restart=0;
%             [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%             if InstructionsResp(RightKey)
%                 break;
%             elseif InstructionsResp(LeftKey)
%                 Instructions2Restart=1;
%             end
%         end
%         if(~Instructions1Restart)
%             showInstructions(INSTRUCTIONS5);
%             Instructions2Restart=0;
%             [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%             if InstructionsResp(RightKey)
%                 break;
%             elseif InstructionsResp(LeftKey)
%                 Instructions2Restart=1;
%             end
%         end
%         if(~Instructions1Restart)
%             showInstructions(INSTRUCTIONS6);
%             Instructions2Restart=0;
%             [~, InstructionsResp, ~] =KbWait(compKbDevice,3);
%             if InstructionsResp(RightKey)
%                 break;
%             elseif InstructionsResp(LeftKey)
%                 Instructions2Restart=1;
%             end
%         end
%     end
end
