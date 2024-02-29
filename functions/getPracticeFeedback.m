function [ ] = getPracticeFeedback()
global compKbDevice RestartKey YesKey
global RUN_PRACTICE MaxPracticeHits MaxPracticeHits_fMRI PracticeHits PracticeFalseAlarms TotalScore
global PRACTICE_FEEDBACK_MESSAGES RESTART_PRACTICE_MESSAGE_ECoG RESTART_PRACTICE_MESSAGE
global ECoG fMRI

    while RUN_PRACTICE
                RestartPracticeFlag=1;
                runPractice();
                if (RUN_PRACTICE == 1)
                    if(~ECoG)
                     if (length(PRACTICE_FEEDBACK_MESSAGES)==1)
%                     restart_practice_message=sprintf(strcat('Your score is ', ' %s\n\n%s\n\n\n\n%s'),strcat(num2str(round(TotalScore)),'%'),PRACTICE_FEEDBACK_MESSAGES{1},RESTART_PRACTICE_MESSAGE);
                      splitted_message=strsplit(PRACTICE_FEEDBACK_MESSAGES{1},{'x'}); 
                      if (isempty(strfind(PRACTICE_FEEDBACK_MESSAGES{1},'missed')))
                      restart_practice_message=sprintf(strcat('Your score is ', ' %s\n\n%s %s %s \n\n\n\n%s'),strcat(num2str(round(TotalScore)),'%'),splitted_message{1},num2str(PracticeFalseAlarms),splitted_message{2},RESTART_PRACTICE_MESSAGE);
                      else
                          if(fMRI)
                      restart_practice_message=sprintf(strcat('Your score is ', ' %s\n\n%s %s %s \n\n\n\n%s'),strcat(num2str(round(TotalScore)),'%'),splitted_message{1},num2str(MaxPracticeHits_fMRI-PracticeHits),splitted_message{2},RESTART_PRACTICE_MESSAGE);   
                          else
                      restart_practice_message=sprintf(strcat('Your score is ', ' %s\n\n%s %s %s \n\n\n\n%s'),strcat(num2str(round(TotalScore)),'%'),splitted_message{1},num2str(MaxPracticeHits-PracticeHits),splitted_message{2},RESTART_PRACTICE_MESSAGE);  
                          end
                      end

                     else
                     splitted_message1=strsplit(PRACTICE_FEEDBACK_MESSAGES{1},{'x'});
                     splitted_message2=strsplit(PRACTICE_FEEDBACK_MESSAGES{2},{'x'});
                     if(fMRI)
                     restart_practice_message=sprintf(strcat('Your score is ', ' %s\n\n%s %s %s \n\n%s %s %s\n\n\n\n%s'),strcat(num2str(round(TotalScore)),'%'),splitted_message1{1},num2str(MaxPracticeHits_fMRI-PracticeHits),splitted_message1{2},splitted_message2{1},num2str(PracticeFalseAlarms),splitted_message2{2},RESTART_PRACTICE_MESSAGE);    
                     else
                     restart_practice_message=sprintf(strcat('Your score is ', ' %s\n\n%s %s %s \n\n%s %s %s\n\n\n\n%s'),strcat(num2str(round(TotalScore)),'%'),splitted_message1{1},num2str(MaxPracticeHits-PracticeHits),splitted_message1{2},splitted_message2{1},num2str(PracticeFalseAlarms),splitted_message2{2},RESTART_PRACTICE_MESSAGE);    
                     end
                     end
                    else
                     restart_practice_message=RESTART_PRACTICE_MESSAGE_ECoG;
                    end
                   showMessage(restart_practice_message) 
                 % Wait for answer
                [~, RestartPracticeResp, ~] =KbWait(compKbDevice,3);
                while RestartPracticeFlag
                if RestartPracticeResp(RestartKey)
                    RestartPracticeFlag=0;
                    RUN_PRACTICE = 1;
                elseif RestartPracticeResp(YesKey)
                    RestartPracticeFlag=0;
                    RUN_PRACTICE = 0;
                else
                   [~, RestartPracticeResp, ~] =KbWait(compKbDevice,3);
                   RestartPracticeFlag=1;
                end
                end
                end
    end
end