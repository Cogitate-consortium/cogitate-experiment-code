
function message_time=displayProgressReward(message, texturePtr, BlockNum, totalBlockNum)

%% This function is created to show a message and a progress bar for patients to motivate them between miniblocks

global   w   ScreenWidth ScreenHeight DIOD_OFF_COLOUR   text PHOTODIODE ECoG
if size(message,1)>1
    message2 = message{2};
    % Make it the wanted style
    Screen('TextStyle', w, 2);
    DrawFormattedText(w, textProcess(message2), 'center', round(ScreenHeight*(5/6)), text.Color);
    message1 = message{1};
    % Make it the wanted style
    Screen('TextStyle', w, 0);
    DrawFormattedText(w, textProcess(message1), 'center', round(ScreenHeight*(1/6)), text.Color);
else
    % Drawing the text
    DrawFormattedText(w, textProcess(message), 'center', ScreenHeight*(1/6), text.Color);
end
% Drawing the empty frame
% Setting the reference coordinates
progressBarXref = ScreenWidth*0.5;
if ECoG
    progressBarYRef = ScreenHeight*0.7;
else
    progressBarYRef = ScreenHeight*0.8;
end
% Setting the coordinates of the corners of the progress bar
YLeftProgressBar = progressBarYRef - ScreenHeight*0.025;
XLeftProgressBar = progressBarXref - ScreenWidth*0.25;
YRightProgressBar = progressBarYRef + ScreenHeight*0.025;
XRightProgressBar = progressBarXref + ScreenWidth*0.25;

% Transposing the coordinates to the right format
EmptyProgressbar = [XLeftProgressBar YLeftProgressBar XRightProgressBar YRightProgressBar ];
% Drawing the empty frame
Screen('FrameRect', w, DIOD_OFF_COLOUR, EmptyProgressbar);

% Now doing the same for the filled progress bar. Only the right
% coordinates change, because it is the same as the frame just not filled
% all the way:
FilledProgressbar = [XLeftProgressBar YLeftProgressBar   ...
    XLeftProgressBar+(XRightProgressBar-XLeftProgressBar)*(BlockNum/totalBlockNum) ...
    YRightProgressBar];
Screen('FillRect', w, DIOD_OFF_COLOUR, FilledProgressbar);

% Then, setting the image position:
if ECoG
    ImageRelativeSize = 0.35; % The image is 35% of the screen size
    PicXRef = ScreenWidth*0.5; % Setting the reference position: middle of the screen in the x axis
    PicyRef = ScreenHeight*0.45; % Setting the reference position: 65% of the screen in the y axis
else
    ImageRelativeSize = 0.20; % The image is 35% of the screen size
    PicXRef = ScreenWidth*0.5; % Setting the reference position: middle of the screen in the x axis
    PicyRef = ScreenHeight*0.65; % Setting the reference position: 65% of the screen in the y axis
end
% PicXRef = ScreenWidth*0.5; % Setting the reference position: middle of the screen in the x axis
% PicyRef = ScreenHeight*0.55; % Setting the reference position: 65% of the screen in the y axis

% Setting the picture coordinate
XLeftPic = PicXRef - (ImageRelativeSize/2)*ScreenWidth; 
XRightPic = PicXRef + (ImageRelativeSize/2)*ScreenWidth;
YLeftPic = PicyRef - (ImageRelativeSize/2)*ScreenHeight;
YRightPic = PicyRef + (ImageRelativeSize/2)*ScreenHeight;

% Drawing the image
if(~isempty(texturePtr))
Screen('DrawTexture',w, texturePtr,[],[XLeftPic YLeftPic XRightPic YRightPic]);
end
if PHOTODIODE
    drawPhotodiodBlock('off')
end

% And finally doing the flip
[~, message_time] = Screen('Flip', w) ;

end