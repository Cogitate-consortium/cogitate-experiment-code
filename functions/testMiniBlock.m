% TESTMINIBLOCK a debugging function
function [ bool ] = testMiniBlock(stim)
    
    disp('WELCOME TO testMiniBlock')
    
    global DURATION_VEC FACE OBJECT LETTER FALSE_FONT CENTER RIGHT LEFT NO_ERROR
    
    bool = true;
    
    faces = stim(getType(stim) >= FACE & getType(stim) < OBJECT);
    objects = stim(getType(stim) >= OBJECT & getType(stim) < LETTER);
    chars = stim(getType(stim) >= LETTER & getType(stim) < FALSE_FONT);
    falses = stim(getType(stim) >= FALSE_FONT & getType(stim) < 10000);
    
    ori = faces-mod(faces,100)-round(faces/1000)*1000;
    ori_cen = size(ori(ori==CENTER),1);
    ori_left = size(ori(ori==LEFT),1);
    ori_right = size(ori(ori==RIGHT),1);
    if ori_cen ~= ori_left+ori_right
        warning('\n%s %d %d %d\n','faces Target orientations unbalanced!',ori_cen,ori_left,ori_right);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','faces Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end
    
    ori = objects-mod(objects,100)-round(objects/1000)*1000;
    ori_cen = size(ori(ori==CENTER),1);
    ori_left = size(ori(ori==LEFT),1);
    ori_right = size(ori(ori==RIGHT),1);
    if ori_cen ~= ori_left+ori_right
        warning('\n%s %d %d %d\n','objects Target orientations unbalanced!',ori_cen,ori_left,ori_right);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','objects Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end
    
    ori = chars-mod(chars,100)-round(chars/1000)*1000;
    ori_cen = size(ori(ori==CENTER),1);
    ori_left = size(ori(ori==LEFT),1);
    ori_right = size(ori(ori==RIGHT),1);
    if ori_cen ~= ori_left+ori_right
        warning('\n%s %d %d %d\n','chars Target orientations unbalanced!',ori_cen,ori_left,ori_right);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','chars Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end
    
    ori = falses-mod(falses,100)-round(falses/1000)*1000;
    ori_cen = size(ori(ori==CENTER),1);
    ori_left = size(ori(ori==LEFT),1);
    ori_right = size(ori(ori==RIGHT),1);
    if ori_cen ~= ori_left+ori_right
        warning('\n%s %d %d %d\n','falses Target orientations unbalanced!',ori_cen,ori_left,ori_right);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','falses Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end
     
    dur = round((faces-floor(faces))*10);
    dur_05 = size(dur(dur==DURATION_VEC(1)*10),1);
    dur_1 = size(dur(dur==DURATION_VEC(2)*10),1);
    dur_15 = size(dur(dur==DURATION_VEC(3)*10),1);
    if ~((dur_05 == dur_1 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1 && dur_05 == dur_15+1) || ...
       (dur_05 == dur_1+2 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1+1 && dur_05 == dur_15+1))    
        warning('\n%s %d %d %d\n','faces Target duration unbalanced!',dur_05,dur_1,dur_15);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','faces Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end
    
    dur = round((objects-floor(objects))*10);
    dur_05 = size(dur(dur==DURATION_VEC(1)*10),1);
    dur_1 = size(dur(dur==DURATION_VEC(2)*10),1);
    dur_15 = size(dur(dur==DURATION_VEC(3)*10),1);
    if ~((dur_05 == dur_1 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1 && dur_05 == dur_15+1) || ...
       (dur_05 == dur_1+2 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1+1 && dur_05 == dur_15+1))         warning('\n%s %d %d %d\n','faces Target duration unbalanced!',dur_05,dur_1,dur_15);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','faces Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end 
    
    dur = round((chars-floor(chars))*10);
    dur_05 = size(dur(dur==DURATION_VEC(1)*10),1);
    dur_1 = size(dur(dur==DURATION_VEC(2)*10),1);
    dur_15 = size(dur(dur==DURATION_VEC(3)*10),1);
    if ~((dur_05 == dur_1 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1 && dur_05 == dur_15+1) || ...
       (dur_05 == dur_1+2 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1+1 && dur_05 == dur_15+1))           warning('\n%s %d %d %d\n','faces Target duration unbalanced!',dur_05,dur_1,dur_15);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','faces Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end 
    
    dur = round((falses-floor(falses))*10);
    dur_05 = size(dur(dur==DURATION_VEC(1)*10),1);
    dur_1 = size(dur(dur==DURATION_VEC(2)*10),1);
    dur_15 = size(dur(dur==DURATION_VEC(3)*10),1);
    if ~((dur_05 == dur_1 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1 && dur_05 == dur_15+1) || ...
       (dur_05 == dur_1+2 && dur_05 == dur_15+2) || ...
       (dur_05 == dur_1+1 && dur_05 == dur_15+1))           warning('\n%s %d %d %d\n','faces Target duration unbalanced!',dur_05,dur_1,dur_15);
        bool = false;
        if ~NO_ERROR error('\n%s %d %d %d\n','faces Target orientations unbalanced!',ori_cen,ori_left,ori_right); end 
    end 
end