%DRAWFRAME
% draws a frame around the stimuli
function [] = drawFrame()

    global FRAME_COLOR
    global stimSizeHeight  VIEWING_DISTANCE MAX_VISUAL_ANGEL
    global w center gray FRAME_WIDTH

    frameSizeLength = getVisualAngel(VIEWING_DISTANCE,MAX_VISUAL_ANGEL(2)); % in px;

    x = transpose(center) - [frameSizeLength/2 stimSizeHeight/2];
    y = transpose(center) + [frameSizeLength/2 stimSizeHeight/2];

    Screen('FillRect', w, FRAME_COLOR, [x y] + [-FRAME_WIDTH -FRAME_WIDTH FRAME_WIDTH FRAME_WIDTH]);
    Screen('FillRect', w, gray, [x y]);

end
