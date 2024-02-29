% CREATETRIALS This is the main function creating mini-blocks. It
% returns the matrix of the trials in each mini-block, the type of
% targets to appear, the names of the targets themselves, the stimuli
% times, and the jitter times.
% This function is wasteful and uses the HD, and should run only before the experiment runs!!
%
% output:
% -------
% miniBlocks - a matrix of the names of each stimuli (and targets) by order of presentation
% miniBlockSize - a vector of the sizes of each miniblock (varies between 34-38)
% targetType - a vector of the type of targets ("face/object", "letter/false font")
% targets - a matrix of the 2 targets of each mini-block
% times - a matrix of planned display times of each stimuli
% jitter - a matrix of planned jitter times of each stimuli
function [ miniBlocks, miniBlockSize, targetType, targets, times, jitter ] = createTrials()
  
 
    global NO_ERROR RIGHT LEFT DURATION_VEC RECURSE_TIME TEXTURES_L_objects TEXTURES_L_chars TEXTURES_L_falses TEXTURES_L_faces NUM_OF_BLOCKS CHAR_FALSE_MINIBLOCK  FACE_OBJECT_MINIBLOCK FACE OBJECT FALSE LETTER NUM_OF_STIM_TYPE_PER_MINIBLOCK FALSE_FONT NUM_OF_MINI_BLOCKS_PER_BLOCK MAX_NUM_OF_TRIALS_PER_MINI_BLOCK CENTER;
    global NUM_OF_STIMULI_EACH NUM_OF_CATEGORIES NUMBER_OF_NON_TARGET_SETS_PER_CAT 
    global NUM_TARGETS_RIGHT_PER_CAT NUM_TARGETS_LEFT_PER_CAT NUM_TARGETS_CENTER_PER_CAT NUM_TARGETS_PER_CAT  
    global NUM_TARGETS_CENTER_DUR_05_PER_CAT NUM_TARGETS_CENTER_DUR_1_PER_CAT NUM_TARGETS_CENTER_DUR_15_PER_CAT
    global NUM_TARGETS_RIGHT_DUR_05_PER_CAT NUM_TARGETS_RIGHT_DUR_1_PER_CAT NUM_TARGETS_RIGHT_DUR_15_PER_CAT
    global NUM_TARGETS_LEFT_DUR_05_PER_CAT NUM_TARGETS_LEFT_DUR_1_PER_CAT NUM_TARGETS_LEFT_DUR_15_PER_CAT
    global NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE
    global NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE
    global NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE 
    global NON_TARGETS_FACES_PER_MB NON_TARGETS_OBJECTS_PER_MB NON_TARGETS_CHARS_PER_MB NON_TARGETS_FALSES_PER_MB
    global FACES_TARGET_ECoG OBJECTS_TARGET_ECoG CHARS_TARGET_ECoG FALSES_TARGET_ECoG
    global FACES_TARGET_fMRI OBJECTS_TARGET_fMRI CHARS_TARGET_fMRI FALSES_TARGET_fMRI
    global ECoG fMRI
    global VERBOSE VERBOSE_PLUS
    % NOTE
    global version_duration
    
    if VERBOSE  
        disp('')
        disp('WELCOME TO createTrials')
        disp('')
    end
    
    starttime = GetSecs;
    RECURSE_TIME = 1;
    DURATION_VEC = [0.1, 0.2, 0.3];    % Corresponding to 0.5, 1 or 3 s   

    times = [];
    targets = [];
    jitter = [];
    miniBlocks = [];
    targetType = [];
    target_test = [];
    miniBlocks_test = [];
    stimuli_no_target_test_face_object = [];
    stimuli_no_target_test_char_false = [];
    
    % All the global variables (capital letters)
    % are initialized in the initConstants function
    
    % What follows are the number of stimuli of each of the specific
    % properties that the experiment should include. The properties
    % are:
    % 1) relevance (target, non-target relevant, irrelevant)
    % 2) category (face, object, letter, false-font)
    % 3) position (center, left, right)
    % 4) duration (0.5, 1, 1.5) s
    % These variables work as "backwards counters" giving how many
    % we have left of these categories to add to our trial lists. 
    % Everytime a stimulus is assigned the properties of one of these counters,
    % we decrease the corresponding counter by 1. This happens in the
    % assignDuration and assignOrientation fucntions.
    
    %% TARGETS
    
    % These are the number of different stimuli throughout the whole experiment, regardsless
    % of orientation
    
    face_num_targets = NUM_TARGETS_PER_CAT; %40 * trial_mod, where trial_mod = 0.5 for ECoG
    object_num_targets = NUM_TARGETS_PER_CAT;
    char_num_targets = NUM_TARGETS_PER_CAT;
    false_num_targets = NUM_TARGETS_PER_CAT;
    
    % This map of counters keep track of relevance and orientation and are used
    % and changed in assignOrientation.
    % The counters are indexed as 'xyz', 
    % where x = relevance ('t', 'n', 'i') for target, non-target and irrelevant
    % y = type ('f', 'o', 'c', 's') for face, object, character and 
    % false-font 
    % z = orientation ('C', 'L', 'R') for center, left and right  (capital
    % for discriminating from the number 1.)
    
    counters_no_duration_info = containers.Map;
    
    % Faces
    counters_no_duration_info('tfC') = NUM_TARGETS_CENTER_PER_CAT; % 20*trial_mod
    counters_no_duration_info('tfL') = NUM_TARGETS_LEFT_PER_CAT;
    counters_no_duration_info('tfR') = NUM_TARGETS_RIGHT_PER_CAT;
    
    % Objects
    counters_no_duration_info('toC') = NUM_TARGETS_CENTER_PER_CAT; % 10*trial_mod
    counters_no_duration_info('toL') = NUM_TARGETS_LEFT_PER_CAT;
    counters_no_duration_info('toR') = NUM_TARGETS_RIGHT_PER_CAT;
    
    % Characters
    
    counters_no_duration_info('tcC') = NUM_TARGETS_CENTER_PER_CAT; % 10*trial_mod
    counters_no_duration_info('tcL') = NUM_TARGETS_LEFT_PER_CAT;
    counters_no_duration_info('tcR') = NUM_TARGETS_RIGHT_PER_CAT;
    
    % False-fonts
    
    counters_no_duration_info('tsC') = NUM_TARGETS_CENTER_PER_CAT;
    counters_no_duration_info('tsL') = NUM_TARGETS_LEFT_PER_CAT;
    counters_no_duration_info('tsR') = NUM_TARGETS_RIGHT_PER_CAT;
    
    % This map of counters keep track of relevance and orientation AND DURATION and are used
    % and changed in assignDuration
    % The counters are indexed as 'xyzj', 
    % where x = relevance ('t', 'n', 'i') for target, non-target and irrelevant
    % y = type ('f', 'o', 'c', 's') for face, object, character and
    % false-font 
    % z = orientation ('C', 'L', 'R') for center, left and right (capital
    % for lower-case L not to look like a number one.)
    % j = duration ('05', '10', '15') for 0.5 s, 1.0 s and 1.5 s
    
    counters_duration_info = containers.Map;
   
    % Faces
    counters_duration_info('tfC05') = NUM_TARGETS_CENTER_DUR_05_PER_CAT;
    counters_duration_info('tfC10') = NUM_TARGETS_CENTER_DUR_1_PER_CAT;
    counters_duration_info('tfC15') = NUM_TARGETS_CENTER_DUR_15_PER_CAT;
    
    counters_duration_info('tfL05') = NUM_TARGETS_LEFT_DUR_05_PER_CAT;
    counters_duration_info('tfL10') = NUM_TARGETS_LEFT_DUR_1_PER_CAT;
    counters_duration_info('tfL15') = NUM_TARGETS_LEFT_DUR_15_PER_CAT;
    
    counters_duration_info('tfR05') = NUM_TARGETS_RIGHT_DUR_05_PER_CAT;
    counters_duration_info('tfR10') = NUM_TARGETS_RIGHT_DUR_1_PER_CAT;
    counters_duration_info('tfR15') = NUM_TARGETS_RIGHT_DUR_15_PER_CAT;
    
    % Objects
    
    counters_duration_info('toC05') = NUM_TARGETS_CENTER_DUR_05_PER_CAT;
    counters_duration_info('toC10') = NUM_TARGETS_CENTER_DUR_1_PER_CAT;
    counters_duration_info('toC15') = NUM_TARGETS_CENTER_DUR_15_PER_CAT;
    
    counters_duration_info('toL05') = NUM_TARGETS_LEFT_DUR_05_PER_CAT;
    counters_duration_info('toL10') = NUM_TARGETS_LEFT_DUR_1_PER_CAT;
    counters_duration_info('toL15') = NUM_TARGETS_LEFT_DUR_15_PER_CAT;
    
    %display(counters_no_duration_info('toL'),'counters_no_duration_info(toL)');
    %display(counters_duration_info('toL05'),'counters_duration_info(toL05)');
    %display(counters_duration_info('toL10'),'counters_duration_info(toL10)');
    %display(counters_duration_info('toL15'),'counters_duration_info(toL15)');
    

    
    counters_duration_info('toR05') = NUM_TARGETS_RIGHT_DUR_05_PER_CAT;
    counters_duration_info('toR10') = NUM_TARGETS_RIGHT_DUR_1_PER_CAT;
    counters_duration_info('toR15') = NUM_TARGETS_RIGHT_DUR_15_PER_CAT;
    
    % Characters
    
    counters_duration_info('tcC05') = NUM_TARGETS_CENTER_DUR_05_PER_CAT;
    counters_duration_info('tcC10') = NUM_TARGETS_CENTER_DUR_1_PER_CAT;
    counters_duration_info('tcC15') = NUM_TARGETS_CENTER_DUR_15_PER_CAT;
    
    counters_duration_info('tcL05') = NUM_TARGETS_LEFT_DUR_05_PER_CAT;
    counters_duration_info('tcL10') = NUM_TARGETS_LEFT_DUR_1_PER_CAT;
    counters_duration_info('tcL15') = NUM_TARGETS_LEFT_DUR_15_PER_CAT;
    
    counters_duration_info('tcR05') = NUM_TARGETS_RIGHT_DUR_05_PER_CAT;
    counters_duration_info('tcR10') = NUM_TARGETS_RIGHT_DUR_1_PER_CAT;
    counters_duration_info('tcR15') = NUM_TARGETS_RIGHT_DUR_15_PER_CAT;
    
    % False-fonts
    
    counters_duration_info('tsC05') = NUM_TARGETS_CENTER_DUR_05_PER_CAT;
    counters_duration_info('tsC10') = NUM_TARGETS_CENTER_DUR_1_PER_CAT;
    counters_duration_info('tsC15') = NUM_TARGETS_CENTER_DUR_15_PER_CAT;
    
    counters_duration_info('tsL05') = NUM_TARGETS_LEFT_DUR_05_PER_CAT;
    counters_duration_info('tsL10') = NUM_TARGETS_LEFT_DUR_1_PER_CAT;
    counters_duration_info('tsL15') = NUM_TARGETS_LEFT_DUR_15_PER_CAT;
    
    counters_duration_info('tsR05') = NUM_TARGETS_RIGHT_DUR_05_PER_CAT;
    counters_duration_info('tsR10') = NUM_TARGETS_RIGHT_DUR_1_PER_CAT;
    counters_duration_info('tsR15') = NUM_TARGETS_RIGHT_DUR_15_PER_CAT;
    
    
    % A vector of the sizes of each miniblock (varies between 34-38)
    miniBlockSize = transpose([getMiniblockSizes(NUM_OF_BLOCKS*NUM_OF_MINI_BLOCKS_PER_BLOCK/2) getMiniblockSizes(NUM_OF_BLOCKS*NUM_OF_MINI_BLOCKS_PER_BLOCK/2)]);    

    % Create vectors according to the number of stimuli in each category
    % These are the nr of unique stimuli that exist (e.g. different faces corresponding to different humans)
    NUMBER_OF_FACES = size(TEXTURES_L_faces,2); % TEXTURES_L_faces are the nr of stimuli in the stimuli folders
    NUMBER_OF_FALSES = size(TEXTURES_L_falses,2);
    NUMBER_OF_CHARS = size(TEXTURES_L_chars,2);
    NUMBER_OF_OBJECTS = size(TEXTURES_L_objects,2);
    if VERBOSE
        display(NUMBER_OF_FACES, 'NUMBER_OF_FACES');
        display(NUMBER_OF_FALSES, 'NUMBER_OF_FALSES');
        display(NUMBER_OF_CHARS, 'NUMBER_OF_CHARS');
        display(NUMBER_OF_OBJECTS, 'NUMBER_OF_OBJECTS');
    end 
    if NUM_OF_STIMULI_EACH > NUMBER_OF_FACES || ...
       NUM_OF_STIMULI_EACH > NUMBER_OF_FALSES || ...
       NUM_OF_STIMULI_EACH > NUMBER_OF_CHARS || ...
       NUM_OF_STIMULI_EACH > NUMBER_OF_OBJECTS
        if ~NO_ERROR error('Not enough stimuli in at least one of the stimuli folders!'); end
    end
    
    % These are the IDs of the different face stimuli there are (different
    % humans) so there are 20 different ones, 10 female, 10 male.
    
    faces_target = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + FACE; % FACE = 1000 so that all faces will start with 10
    objects_target = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + OBJECT;
    
    chars_target = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + LETTER;
    falses_target = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + FALSE_FONT;  
    
    if VERBOSE
        disp('These arrays should contain 20 unique IDs. They should be 20 long.');
        display(faces_target, 'faces_target');
        display(size(faces_target,2), 'size of faces_target');
        display(objects_target, 'objects_target');
        display(size(objects_target,2), 'size of objects_target');
        display(falses_target, 'falses_target');
        display(size(falses_target,2), 'size of falses_target');
        display(chars_target, 'chars_target');
        display(size(chars_target,2), 'size of chars_target');
    end
    
    if ECoG
        faces_target = ShuffleVector(FACES_TARGET_ECoG) + FACE;
        objects_target = ShuffleVector(OBJECTS_TARGET_ECoG) + OBJECT;
        chars_target = ShuffleVector(CHARS_TARGET_ECoG) + LETTER;
        falses_target = ShuffleVector(FALSES_TARGET_ECoG) + FALSE_FONT;
    end % end if
    if fMRI
        faces_target = ShuffleVector(FACES_TARGET_fMRI) + FACE;
        objects_target = ShuffleVector(OBJECTS_TARGET_fMRI) + OBJECT;
        chars_target = ShuffleVector(CHARS_TARGET_fMRI) + LETTER;
        falses_target = ShuffleVector(FALSES_TARGET_fMRI) + FALSE_FONT;
    end % end if
    
    if VERBOSE
        disp('These arrays should contain 10 unique IDs. They should be 10 long for ECoG');
        display(faces_target, 'faces_target');
        display(size(faces_target,2), 'size of faces_target');
        display(objects_target, 'objects_target');
        display(size(objects_target,2), 'size of objects_target');
        display(falses_target, 'falses_target');
        display(size(falses_target,2), 'size of falses_target');
        display(chars_target, 'chars_target');
        display(size(chars_target,2), 'size of chars_target');
    end
    
    %% FACE OBJECT Miniblocks %%
    
    % The following counters are for all the FACE-OBJECT miniblocks only
    % For explanation of the indexing, pls see just above the assignment of the
    % target counters (above).    
   
    % Non-targets
    
    % Faces
    counters_no_duration_info('nfC') = NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('nfC05') = NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nfC10') = NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nfC15') = NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('nfL') = NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('nfL05') = NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nfL10') = NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nfL15') = NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('nfR') = NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('nfR05') = NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nfR10') = NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nfR15') = NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    % Objects
    counters_no_duration_info('noC') = NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('noC05') = NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noC10') = NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noC15') = NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('noL') = NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noL05') = NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noL10') = NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noL15') = NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('noR') = NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('noR05') = NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noR10') = NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('noR15') = NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    
    % Irrelevants
    
    % Characters
    counters_no_duration_info('icC') = NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('icC05') = NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('icC10') = NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('icC15') = NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('icL') = NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('icL05') = NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('icL10') = NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('icL15') = NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('icR') = NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('icR05') = NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('icR10') = NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('icR15') = NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    % False-fonts
    counters_no_duration_info('isC') = NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('isC05') = NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('isC10') = NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('isC15') = NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('isL') = NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('isL05') = NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('isL10') = NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('isL15') = NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    if NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE > (NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE + NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE + NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE)
        display('ERROR');
        return
    end
    
    counters_no_duration_info('isR') = NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('isR05') = NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('isR10') = NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('isR15') = NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    if VERBOSE_PLUS
      
        display(version_duration, '----- All targets for face and object mbs with verison duration  : ---------');
       
        i05f = counters_duration_info('tfC05') + counters_duration_info('tfL05') + counters_duration_info('tfR05'); 
        i10f = counters_duration_info('tfC10') + counters_duration_info('tfL10') + counters_duration_info('tfR10'); 
        i15f = counters_duration_info('tfC15') + counters_duration_info('tfL15') + counters_duration_info('tfR15'); 
        i = [i05f, i10f, i15f];
        display(i,'Face 0.5 1.0 1.5 s: ');
        
        i05o = counters_duration_info('toC05') + counters_duration_info('toL05') + counters_duration_info('toR05'); 
        i10o = counters_duration_info('toC10') + counters_duration_info('toL10') + counters_duration_info('toR10'); 
        i15o = counters_duration_info('toC15') + counters_duration_info('toL15') + counters_duration_info('toR15'); 
        i = [i05o, i10o, i15o];
        display(i,'Object 0.5 1.0 1.5 s: ');
        
         display(version_duration, '----- All non-targets for version_duration : ---------');
        
        i05f = counters_duration_info('nfC05') + counters_duration_info('nfL05') + counters_duration_info('nfR05'); 
        i10f = counters_duration_info('nfC10') + counters_duration_info('nfL10') + counters_duration_info('nfR10'); 
        i15f = counters_duration_info('nfC15') + counters_duration_info('nfL15') + counters_duration_info('nfR15'); 
        i = [i05f, i10f, i15f];
        display(i,'Face 0.5 1.0 1.5 s: ');
        
        i05o = counters_duration_info('noC05') + counters_duration_info('noL05') + counters_duration_info('noR05'); 
        i10o = counters_duration_info('noC10') + counters_duration_info('noL10') + counters_duration_info('noR10'); 
        i15o = counters_duration_info('noC15') + counters_duration_info('noL15') + counters_duration_info('noR15'); 
        i = [i05o, i10o, i15o];
        display(i,'Object 0.5 1.0 1.5 s: ');
    
        
         display(version_duration, '----- All irrelevants for version_duration : ---------');
        
        i05l = counters_duration_info('icC05') + counters_duration_info('icL05') + counters_duration_info('icR05'); 
        i10l = counters_duration_info('icC10') + counters_duration_info('icL10') + counters_duration_info('icR10'); 
        i15l = counters_duration_info('icC15') + counters_duration_info('icL15') + counters_duration_info('icR15'); 
        i = [i05l, i10l, i15l];
        display(i,'Letters 0.5 1.0 1.5 s : ');
        
        i05s = counters_duration_info('isC05') + counters_duration_info('isL05') + counters_duration_info('isR05'); 
        i10s = counters_duration_info('isC10') + counters_duration_info('isL10') + counters_duration_info('isR10'); 
        i15s = counters_duration_info('isC15') + counters_duration_info('isL15') + counters_duration_info('isR15'); 
        i = [i05s, i10s, i15s];
        display(i,'False-font 0.5 1.0 1.5 s: ');      
        
    end % VERBOSE PLUS
    
    
    % These are vectors of the IDs of all non non-relevant faces or objects (irrelevant chars or false-fonts)
    % that are to be used for all the Face object miniblocks
    % shown for all the face-object miniblocks 
    % NUM_OF_STIMULI_EACH is the number of unique stimuli (20)
    % randperm(x) is a vector with a random permutation of numbers 1 - x
    % repmat(v,1,y) generates a vector which is just repeating v, y times
    % NUMBER_OF_NON_TARGET_SETS_PER_CAT is the number of times each
    % stimulus ID has to repeat in order to fill the number of for all the
    % miniblocks. We divide by 2 since this is only for face-object
    % miniblocks. These variables will be reassigned for the
    % letter-false-font miniblocks

    % Non-targets
    % These are not used anymore since we use pseudorandomization for
    % non-targets
    %faces = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + FACE;
    %objects = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + OBJECT; 
    
    chars = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + LETTER;
    falses = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + FALSE_FONT;        
    
    if VERBOSE    
        disp('These arrays should contain 20 unique IDs. They should be 80 long for ECoG');
        display(size(falses,2), 'size of falses');
        display(chars, 'chars');
        display(size(chars,2), 'size of chars');
    end 
    
    
    % NUM_OF_STIM_TYPE_PER_MINIBLOCK (8)
    % The miniBlockSize
    miniBlockSz = (transpose(miniBlockSize(1:(size(miniBlockSize,1)/2),:)) - (NUM_OF_STIM_TYPE_PER_MINIBLOCK * NUM_OF_CATEGORIES));
    if VERBOSE display(miniBlockSz, 'miniBlockSz'); end
    

    % To do that, we first we need to decide the targets for each miniblock:
    
    face_targets_for_specific_mb = [];
    object_targets_for_specific_mb = [];
    
    n_miniblocks_face_objects =  NUM_OF_MINI_BLOCKS_PER_BLOCK * NUM_OF_BLOCKS/2;  %2 for mb type (face-object)
    
    % Assigning targets to each miniblock
    if VERBOSE disp(' ---- Adding targets to face-object mb ----'); end
    for mb = 1 : size(miniBlockSz,2)
        
        if VERBOSE
            disp('Handling MiniBlock nr (should be 10 in total for ECoG) ');
            disp(mb);
        end
        % Randomizing the target for this miniblock
        
        
        i = ceil(rand()*size(faces_target,2)); % faces_target is modified at each loop
        target1 = faces_target(i);
        
        face_targets_for_specific_mb(mb) = target1;
        if VERBOSE 
            disp('target1 (face) (the targetID): ');
            disp(target1);
        end
     
           
        % This is dropping the element from the array so that we won't
        % choose the same target ID twice. There are 20 targets and
        % 20 miniblocks so they won't repeat.
        faces_target(i) = [];
        
        % objects:
        i = ceil(rand()*size(objects_target,2));
        target2 = objects_target(i);
        
        % The object target should be independently drawn
       
        object_targets_for_specific_mb(mb) = target2;
        objects_target(i) = [];
        if VERBOSE
            disp('target2 (object) (the targetID): ');
            disp(target2);
        end
        
    end % end for miniblocks assigning targets
    if VERBOSE
        display(face_targets_for_specific_mb, 'face_targets_for_specific_mb should be all unique IDs and 10 long for ECoG (10 miniblocks)');
        display(object_targets_for_specific_mb, 'objects_targets_for_specific_mb should all unique IDs and 10 long for ECoG (10 miniblocks)');
    end
    
    % Then we can decide where the non-targets should go.
    % These structures are going to be used instead of the faces and
    % objects arrays
    if VERBOSE
        disp('This should be the nr of face-object miniblocks (10 for ECoG)');
        display(size(miniBlockSz,2), 'size(miniBlockSz,2)');
    end
    faces_for_face_and_object_mb_specific_mini_blocks = zeros(size(miniBlockSz,2), NUM_OF_STIM_TYPE_PER_MINIBLOCK); % first index is the mb nr, second is the stim 
    objects_for_face_and_object_mb_specific_mini_blocks = zeros(size(miniBlockSz,2),NUM_OF_STIM_TYPE_PER_MINIBLOCK); 
    
    
    if VERBOSE disp(' ---- Now assigning non-targets for face-object mbs -----'); end
    
    % Assigning non-targets
    for mb = 1 : size(miniBlockSz,2)
        
        if VERBOSE
            disp('Handling MiniBlock nr ');
            disp(mb);
        end

        % Getting the targets
        target1 = face_targets_for_specific_mb(mb);
        target2 = object_targets_for_specific_mb(mb);
        
        if VERBOSE
            display(target1, 'target1');
            display(target2, 'target2');
        end
        
        % Fetch the pseudorandomized non-targets
        faces_for_face_and_object_mb_specific_mini_blocks(mb,:) = ...
        NON_TARGETS_FACES_PER_MB(int2str(target1)); % this is a container with vectors
        
        objects_for_face_and_object_mb_specific_mini_blocks(mb,:) = ...
        NON_TARGETS_OBJECTS_PER_MB(int2str(target2));
                  
        % Collecting the target stimuli within this miniblock
        targetss = [];
        stimuli = [];

        %% Placing the target stimuli within a miniblock %%
        % iterating over the stimuli
        % miniBlock is a vector holding the number of targets that each
        % miniblock should have (varied between 2 and 6) with a mean of 4
        % so that j is the nr of targets for miniblock mb
        
        if VERBOSE
            disp('-------------start loop-------------------------');
            display(miniBlockSz(mb), 'miniBlockSz(mb)');
        end
        % miniBlockSz = number of targets
        n_target_stim_for_this_mb = miniBlockSz(mb);
        
        for j = 1 : n_target_stim_for_this_mb
            
            if VERBOSE display(j, 'target stim nr (total should vary between 2 and 6)'); end
            % round(rand()) is a random number (0 or 1). This is adding the
            % randomness to whether a face or a object target stimuli is added
            if round(rand()) && face_num_targets > 0 % if face targets left
                
                 if VERBOSE  disp('into placing face target stim'); end
                
                 if VERBOSE  display(targetss, 'targetss'); end
                    target = assignOrientation(target1,'t','f');
                 if VERBOSE 
                    display(counters_no_duration_info('tfC'), 'counters_no_duration_info(tfC)');
                    display(counters_no_duration_info('tfL'), 'counters_no_duration_info(tfL)');
                    display(counters_no_duration_info('tfR'), 'counters_no_duration_info(tfR)');
                end
                target = assignDuration(target, 't', 'f', getOrientationAsLetter(target));
                if VERBOSE 
                    display(target, 'target'); 
                    display(getOrientationAsLetter(target), 'getOrientationAsLetter(target)')
                end
                targetss = [targetss target];
                face_num_targets = face_num_targets - 1;
            elseif object_num_targets > 0
                
                if VERBOSE
                    disp('into placing obj target stim');
                    display(target2, 'target before assignOrientation');
                    display(counters_no_duration_info('toC'), 'counters_no_duration_info(toC)');
                    display(counters_no_duration_info('toL'), 'counters_no_duration_info(toL)');
                    display(counters_no_duration_info('toR'), 'counters_no_duration_info(toR)');
                end
                target = assignOrientation(target2,'t','o');
                
                if VERBOSE display(target, 'target after assignOrientation'); end
                target = assignDuration(target,'t','o', getOrientationAsLetter(target));
                if VERBOSE display(target, 'target after assignDuration'); end
                targetss = [targetss target];
                object_num_targets = object_num_targets - 1;
            elseif face_num_targets > 0 % Here, even though it looks like it is a repetition of what we have above, it isn't. Well it is but it is necessary. The first if statement goes into the objects if the rand is equal to 1. But if there are not objects left, we need to go back to the faces
                
                if VERBOSE disp('into placing face target stim'); end
                target = assignOrientation(target1, 't', 'f');
                if VERBOSE
                    display(counters_no_duration_info('tfC'), 'counters_no_duration_info(tfC)');
                    display(counters_no_duration_info('tfL'), 'counters_no_duration_info(tfL)');
                    display(counters_no_duration_info('tfR'), 'counters_no_duration_info(tfR)');
                end
                target = assignDuration(target, 't','f', getOrientationAsLetter(target));
                if VERBOSE display(target, 'target'); end
                targetss = [targetss target];              
                face_num_targets = face_num_targets - 1;                
            end  % if elseif                  
        end % end for target stimuli
        
        if VERBOSE
            disp('-----------------end loop------------------');
            disp(targetss);
        end
        
        %% Placing the non-target and irrelevant stimuli %%
         if VERBOSE disp('-------------looping over the non-targets faces to place them and assign orient and dur -------'); end
           
        %faces
        for j =1:NUM_OF_STIM_TYPE_PER_MINIBLOCK
            
            %display(j, 'j');
            stim =  faces_for_face_and_object_mb_specific_mini_blocks(mb,j);
            %display(stim, 'now placing the face target stim');
            stim = assignOrientation(stim, 'n', 'f'); 
            stim = assignDuration(stim,'n','f', getOrientationAsLetter(stim)); % f for face, n for non-target
            stimuli = [stimuli stim];
            j = j + 1;   
            
        end % for faces in  faces_for_face_and_object_mb_specific_mini_blocks vector
        
        %objects
        for j =1:NUM_OF_STIM_TYPE_PER_MINIBLOCK
            
            %display(j, 'j');
            stim =  objects_for_face_and_object_mb_specific_mini_blocks(mb,j);
            %display(stim, 'now placing the non-target object target stim');
            stim = assignOrientation(stim, 'n', 'o'); 
            stim = assignDuration(stim,'n','o', getOrientationAsLetter(stim)); % f for face, n for non-target
            stimuli = [stimuli stim];
            j = j + 1;   
            
        end % for faces in  faces_for_face_and_object_mb_specific_mini_blocks vector      
         
        if VERBOSE
            display(stimuli, 'stimuli (this should now contain 8 the non-target objects). You can compare to the containers');
            display(target1, 'target 1');
            display(target2, 'target 2');
        end
        
        if VERBOSE disp('-------------looping to place the irrelevant objects and assign orient and dur -------'); end
        % chars and falses
        j = 1;
        while j <= NUM_OF_STIM_TYPE_PER_MINIBLOCK
            k = ceil(rand()*size(falses,2));
            stim = falses(k);
            falses(k) = [];            
            stim = assignOrientation(stim, 'i','s'); 
            stim = assignDuration(stim,'i','s', getOrientationAsLetter(stim));
            stimuli = [stimuli stim];
            k = ceil(rand()*size(chars,2));
            stim = chars(k);
            chars(k) = [];            
            stim = assignOrientation(stim,'i','c'); 
            stim = assignDuration(stim,'i','c', getOrientationAsLetter(stim));
            stimuli = [stimuli stim];
            j = j + 1;
        end % while
        if VERBOSE display(stimuli, 'stimuli including only non targets and irrelevants for fo mb'); end

        testStimuli(stimuli);
        stimuli_no_target_test_face_object = [stimuli_no_target_test_face_object; stimuli];        

        % Shuffling all stimuli: targets, non-targets and irrelevant    
        stimuli = ShuffleVector([stimuli targetss]);


        if length(stimuli) ~= (miniBlockSz(mb) + NUM_OF_STIM_TYPE_PER_MINIBLOCK * NUM_OF_CATEGORIES)
            warning('Miniblock of wrong length!');
            if ~NO_ERROR error('Miniblock of wrong length!'); end
        end % if
        
        while verifyBlock(stimuli) == FALSE
            stimuli = ShuffleVector(stimuli);
            if VERBOSE display(stimuli); end
        end % while
        
        jit = padArray(getJitter(size(stimuli,2)), MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);
        duration = padArray(getTime(stimuli), MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);
        stimuli = padArray(stimuli, MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);
        
        if VERBOSE
            disp('round(stimuli)');
            disp(round(stimuli));
            disp('stimuli including targets as well');
            disp(stimuli);
        end
        miniBlocks = [miniBlocks; round(stimuli)];
        
        times = [times; duration];
        jitter = [jitter; jit];
        targets = [targets; getType(target1), getType(target2)];
        targetType = [targetType; FACE_OBJECT_MINIBLOCK];        
        miniBlocks_test = [miniBlocks_test; stimuli];
        target_test = [target_test; padArray(targetss, MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);];

        
    end % Face-object miniblocks
    

    
        
    %% CHAR FALSES MINIBLOCKS %%
    
    % Non-targets
    
    % Characters
    counters_no_duration_info('ncC') = NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ncC05') = NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ncC10') = NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ncC15') = NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('ncL') = NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ncL05') = NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ncL10') = NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ncL15') = NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('ncR') = NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ncR05') = NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ncR10') = NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ncR15') = NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    % False-fonts
    counters_no_duration_info('nsC') = NUM_NON_TARGETS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('nsC05') = NUM_NON_TARGETS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nsC10') = NUM_NON_TARGETS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nsC15') = NUM_NON_TARGETS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('nsL') = NUM_NON_TARGETS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('nsL05') = NUM_NON_TARGETS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nsL10') = NUM_NON_TARGETS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nsL15') = NUM_NON_TARGETS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('nsR') = NUM_NON_TARGETS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('nsR05') = NUM_NON_TARGETS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nsR10') = NUM_NON_TARGETS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('nsR15') = NUM_NON_TARGETS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    
    % Irrelevants
    
    % Faces
    counters_no_duration_info('ifC') = NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ifC05') = NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ifC10') = NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ifC15') = NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('ifL') = NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ifL05') = NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ifL10') = NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ifL15') = NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('ifR') = NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ifR05') = NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ifR10') = NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ifR15') = NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    % RIGHT
    counters_no_duration_info('ioC') = NUM_IRRELEVANTS_CENTER_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ioC05') = NUM_IRRELEVANTS_CENTER_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ioC10') = NUM_IRRELEVANTS_CENTER_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ioC15') = NUM_IRRELEVANTS_CENTER_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('ioL') = NUM_IRRELEVANTS_LEFT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ioL05') = NUM_IRRELEVANTS_LEFT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ioL10') = NUM_IRRELEVANTS_LEFT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ioL15') = NUM_IRRELEVANTS_LEFT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    counters_no_duration_info('ioR') = NUM_IRRELEVANTS_RIGHT_PER_CAT_PER_MB_TYPE;
    
    counters_duration_info('ioR05') = NUM_IRRELEVANTS_RIGHT_DUR_05_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ioR10') = NUM_IRRELEVANTS_RIGHT_DUR_1_PER_CAT_PER_MB_TYPE;
    counters_duration_info('ioR15') = NUM_IRRELEVANTS_RIGHT_DUR_15_PER_CAT_PER_MB_TYPE;
    
    % For debugging. Making sure the numbers are correct:
    if VERBOSE_PLUS
        
        display(version_duration, '----- All targets for version_duration for letter and falsefont mbs with verison duration  : ---------');
      
        i05l = counters_duration_info('tcC05') + counters_duration_info('tcL05') + counters_duration_info('tcR05'); 
        i10l = counters_duration_info('tcC10') + counters_duration_info('tcL10') + counters_duration_info('tcR10'); 
        i15l = counters_duration_info('tcC15') + counters_duration_info('tcL15') + counters_duration_info('tcR15'); 
        i = [i05l, i10l, i15l];
        display(i,'Letters 0.5 1.0 1.5 s : ');
        
        i05s = counters_duration_info('tsC05') + counters_duration_info('tsL05') + counters_duration_info('tsR05'); 
        i10s = counters_duration_info('tsC10') + counters_duration_info('tsL10') + counters_duration_info('tsR10'); 
        i15s = counters_duration_info('tsC15') + counters_duration_info('tsL15') + counters_duration_info('tsR15'); 
        i = [i05s, i10s, i15s];
        display(i,'False-font 0.5 1.0 1.5 s: ');
        
        display(version_duration, '----- All non-targets for version_duration for letter and falsefont mbs with verison duration  : ---------');
        
        i05l = counters_duration_info('ncC05') + counters_duration_info('ncL05') + counters_duration_info('ncR05'); 
        i10l = counters_duration_info('ncC10') + counters_duration_info('ncL10') + counters_duration_info('ncR10'); 
        i15l = counters_duration_info('ncC15') + counters_duration_info('ncL15') + counters_duration_info('ncR15'); 
        i = [i05l, i10l, i15l];
        display(i,'Letters 0.5 1.0 1.5 s : ');
        
        i05s = counters_duration_info('nsC05') + counters_duration_info('nsL05') + counters_duration_info('nsR05'); 
        i10s = counters_duration_info('nsC10') + counters_duration_info('nsL10') + counters_duration_info('nsR10'); 
        i15s = counters_duration_info('nsC15') + counters_duration_info('nsL15') + counters_duration_info('nsR15'); 
        i = [i05s, i10s, i15s];
        display(i,'False-font 0.5 1.0 1.5 s: ');
        
        display(version_duration, '----- All irrelevants for version_duration for letter and falsefont mbs with verison duration  : ---------');
        
        i05f = counters_duration_info('ifC05') + counters_duration_info('ifL05') + counters_duration_info('ifR05'); 
        i10f = counters_duration_info('ifC10') + counters_duration_info('ifL10') + counters_duration_info('ifR10'); 
        i15f = counters_duration_info('ifC15') + counters_duration_info('ifL15') + counters_duration_info('ifR15'); 
        i = [i05f, i10f, i15f];
        display(i,'Face 0.5 1.0 1.5 s: ');
        
        i05o = counters_duration_info('ioC05') + counters_duration_info('ioL05') + counters_duration_info('ioR05'); 
        i10o = counters_duration_info('ioC10') + counters_duration_info('ioL10') + counters_duration_info('ioR10'); 
        i15o = counters_duration_info('ioC15') + counters_duration_info('ioL15') + counters_duration_info('ioR15'); 
        i = [i05o, i10o, i15o];
        display(i,'Object 0.5 s: ');
        
    end % if verbose_plus
    
    % These are not used anymore since we use pseudorandomization for
    % non-targets
    %chars = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + LETTER;
    %falses = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + FALSE_FONT; 
    faces = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + FACE;
    objects = ShuffleVector(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + OBJECT; 
    
    % NUM_OF_STIM_TYPE_PER_MINIBLOCK (8)
    % The miniBlockSize
    % TODO: Why is this different from the Face-Object version: ALEX: TO
    % WHOEVER PUT THAT THERE, it is because the miniblockSize contains all
    % the miniblocks. For the face-object, only the first half is taken,
    % and for here, the last half. 
    miniBlockSz = (transpose(miniBlockSize((size(miniBlockSize,1)/2)+1:(size(miniBlockSize,1)),:)) - NUM_OF_STIM_TYPE_PER_MINIBLOCK * NUM_OF_CATEGORIES);
    
    if VERBOSE
        display(miniBlockSz, 'miniBlockSz');
        disp('These arrays should contain 20 unique IDs. They should be 0 long for ECoG');
        display(faces, 'faces');
        display(size(faces,2), 'size of faces');
        display(objects, 'objects');
        display(size(objects,2), 'size of objects');     
    end

    % To do that, we first we need to decide the targets for each miniblock:
    char_targets_for_specific_mb = [];
    false_targets_for_specific_mb = [];
    
    n_miniblocks_char_falses =  NUM_OF_MINI_BLOCKS_PER_BLOCK * NUM_OF_BLOCKS/2;  %2 for mb type (char-false)
    
    if VERBOSE disp('----- Adding targets to char-false mb'); end
    for mb = 1 : size(miniBlockSz,2)
        
        if VERBOSE
            disp('Handling MiniBlock nr (should be 10 in total for ECoG) ');
            disp(mb);
        end
        % Randomizing the target for this miniblock
     
        
        i = ceil(rand()*size(chars_target,2)); % chars_target is modified at each loop
        target1 = chars_target(i);
        
        char_targets_for_specific_mb(mb) = target1;
        if VERBOSE
            disp('target1 (char) (the targetID): ');
            disp(target1);
        end
     
        chars_target(i) = [];
        
        % This is dropping the element from the array so that we won't
        % choose the same target ID twice. There are 20 targets and
        % 20 miniblocks so they won't repeat.
       
        % The false target should be independently drawn
        i = ceil(rand()*size(falses_target,2));
        target2 = falses_target(i);
        false_targets_for_specific_mb(mb) = target2;
        falses_target(i) = [];
        if VERBOSE
            disp('target2 (false) (the targetID): ');
            disp(target2);
        end
        
    end % end for miniblocks assigning targets
    
    if VERBOSE
        display(char_targets_for_specific_mb, 'char_targets_for_specific_mb should be all unique IDs and 10 long for ECoG (10 miniblocks)');
        display(false_targets_for_specific_mb, 'falses_targets_for_specific_mb should all unique IDs and 10 long for ECoG (10 miniblocks)');
    end
    
    % Then we can decide where the non-targets should go.
    % These structures are going to be used instead of the chars and
    % falses arrays
    if VERBOSE
        disp('This should be the nr of char-false miniblocks (10 for ECoG)');
        display(size(miniBlockSz,2), 'size(miniBlockSz,2)');
    end
    chars_for_char_and_false_mb_specific_mini_blocks = zeros(size(miniBlockSz,2), NUM_OF_STIM_TYPE_PER_MINIBLOCK); % first index is the mb nr, second is the stim 
    falses_for_char_and_false_mb_specific_mini_blocks = zeros(size(miniBlockSz,2),NUM_OF_STIM_TYPE_PER_MINIBLOCK); 
    
    
    if VERBOSE disp('Now assigning non-targets for char-false mbs'); end
    
   
    
    % First place all targets and non-targets by placing the targets
    
    for mb = 1 : size(miniBlockSz,2)
        
        if VERBOSE
            disp('Handling MiniBlock nr ');
            disp(mb);
        end

        % Getting the targets
        target1 = char_targets_for_specific_mb(mb);
        target2 = false_targets_for_specific_mb(mb);
        
        if VERBOSE
            display(target1, 'target1');
            display(target2, 'target2');
        end
        
        % Fetch the pseudorandomized non-targets
        chars_for_char_and_false_mb_specific_mini_blocks(mb,:) = ...
            NON_TARGETS_CHARS_PER_MB(int2str(target1)); % this  be a vector
        
        falses_for_char_and_false_mb_specific_mini_blocks(mb,:) = ...
            NON_TARGETS_FALSES_PER_MB(int2str(target2));
                  
        % Collecting the target stimuli within this miniblock
        targetss = [];
        stimuli = [];

        %% Placing the target stimuli within a miniblock %%
        % iterating over the stimuli
        % miniBlock is a vector holding the number of targets that each
        % miniblock should have (varied between 2 and 6) with a mean of 4
        % so that j is the nr of targets for miniblock mb
        
        if VERBOSE
            disp('-------------start loop-------------------------');
            display(miniBlockSz(mb), 'miniBlockSz(mb)');
        end
        % miniBlockSz = number of targets
        n_target_stim_for_this_mb = miniBlockSz(mb);
        for j = 1 : n_target_stim_for_this_mb
                        
            if VERBOSE display(j, 'target stim nr (total should vary between 2 and 6)'); end
            % round(rand()) is a random number (0 or 1). This is adding the
            % randomness to whether a char or a false target stimuli is added
            if round(rand()) && char_num_targets > 0 % if char targets left
                
                if VERBOSE 
                    disp('into placing char target stim');
                    display(targetss, 'targetss');
                end
                target = assignOrientation(target1,'t','c');
                target = assignDuration(target, 't', 'c', getOrientationAsLetter(target));
                if VERBOSE
                    display(target, 'target');
                    display(getOrientationAsLetter(target), 'getOrientationAsLetter(target)')
                end
                targetss = [targetss target];
                char_num_targets = char_num_targets - 1;
            elseif false_num_targets > 0
                
                if VERBOSE
                    disp('into placing obj target stim');
                    display(target2, 'target before assignOrientation');
                end
                target = assignOrientation(target2,'t','s');
                if VERBOSE display(target, 'target after assignOrientation'); end
                target = assignDuration(target,'t','s', getOrientationAsLetter(target));
                if VERBOSE display(target, 'target after assignDuration'); end
                targetss = [targetss target];
                false_num_targets = false_num_targets - 1;
            elseif char_num_targets > 0 
                
                if VERBOSE disp('into placing char target stim'); end
                target = assignOrientation(target1, 't', 'c');
                target = assignDuration(target, 't','c', getOrientationAsLetter(target));
                if VERBOSE display(target, 'target'); end
                targetss = [targetss target];              
                char_num_targets = char_num_targets - 1;                
            end % if elseif
            
        end % end for target stimuli
        
        if VERBOSE
            disp('-----------------end loop------------------');
            disp(targetss);
        end
        
        %% Placing the non-target and irrelevant stimuli %%
        if VERBOSE disp('-------------looping over the non-targets chars to place them and assign orient and dur -------'); end
           
        %chars
        for j =1:NUM_OF_STIM_TYPE_PER_MINIBLOCK
            
            if VERBOSE display(j, 'j'); end
            stim =  chars_for_char_and_false_mb_specific_mini_blocks(mb,j);
            if VERBOSE display(stim, 'now placing the char target stim'); end
            stim = assignOrientation(stim, 'n', 'c'); 
            stim = assignDuration(stim,'n','c', getOrientationAsLetter(stim)); % f for char, n for non-target
            stimuli = [stimuli stim];
            j = j + 1;   
            
        end % for chars in  chars_for_char_and_false_mb_specific_mini_blocks vector
        
        %falses
        for j =1:NUM_OF_STIM_TYPE_PER_MINIBLOCK
            
            if VERBOSE display(j, 'j'); end
            stim =  falses_for_char_and_false_mb_specific_mini_blocks(mb,j);
            if VERBOSE display(stim, 'now placing the false target stim'); end
            stim = assignOrientation(stim, 'n', 's'); 
            stim = assignDuration(stim,'n','s', getOrientationAsLetter(stim)); % f for char, n for non-target
            stimuli = [stimuli stim];
            j = j + 1;   
            
        end % for chars in  chars_for_char_and_false_mb_specific_mini_blocks vector      
    
        if VERBOSE
            display(stimuli, 'stimuli (this should now contain 8 non-target falses)');
            disp('-------------looping to place the irrelevant falses and assign orient and dur -------');
        end
        % Faces and objects
        j = 1;
        % For the task irrelevant, we just need to randomly pick from the
        % vector created previously:
        while j <= NUM_OF_STIM_TYPE_PER_MINIBLOCK
            k = ceil(rand()*size(objects,2));
            stim = objects(k);
            objects(k) = [];            
            stim = assignOrientation(stim, 'i','o'); 
            stim = assignDuration(stim,'i','o', getOrientationAsLetter(stim));
            stimuli = [stimuli stim];
            k = ceil(rand()*size(faces,2));
            stim = faces(k);
            faces(k) = [];            
            stim = assignOrientation(stim,'i','f'); 
            stim = assignDuration(stim,'i','f', getOrientationAsLetter(stim));
            stimuli = [stimuli stim];
            j = j + 1;
        end % while
        
        if VERBOSE display(stimuli, 'stimuli irrelevants and non-relevants for lf mbs'); end
        
        testStimuli(stimuli); % Testing that there is a correct amount of stimuli
        stimuli_no_target_test_char_false = [stimuli_no_target_test_char_false; stimuli];        

        % Shuffling all stimuli: targets, non-targets and irrelevant    
        stimuli = ShuffleVector([stimuli targetss]);

        if VERBOSE display(stimuli, 'stimuli final with targets as well for lf mb');  end  

        if length(stimuli) ~= (miniBlockSz(mb) + NUM_OF_STIM_TYPE_PER_MINIBLOCK * NUM_OF_CATEGORIES)
            warning('Miniblock of wrong length!');
            if ~NO_ERROR error('Miniblock of wrong length!'); end
        end % if
        % Checking that there is never the same stimulus twice in a row, if
        % yes, reshuffle
        while verifyBlock(stimuli) == FALSE
            stimuli = ShuffleVector(stimuli);
            if VERBOSE disp(stimuli); end
        end % while
        
        % Here we add some NaNs to the stimulus array so that it will
        % fit into the large miniblocks structure
        jit = padArray(getJitter(size(stimuli,2)), MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);
        duration = padArray(getTime(stimuli), MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);
        stimuli = padArray(stimuli, MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);
        

        if VERBOSE
            disp('round(stimuli)');
            disp(round(stimuli));
            disp('stimuli');
            disp(stimuli);
            disp('---------------------------');
            display(size(stimuli), 'SIZE');
            disp('---------------------------');
        end
        miniBlocks = [miniBlocks; round(stimuli)];
        
        % Then, putting everything back together:
        times = [times; duration];
        jitter = [jitter; jit];
        targets = [targets; getType(target1), getType(target2)];
        targetType = [targetType; CHAR_FALSE_MINIBLOCK];        
        miniBlocks_test = [miniBlocks_test; stimuli];
        target_test = [target_test; padArray(targetss, MAX_NUM_OF_TRIALS_PER_MINI_BLOCK);];
        
    end % Char-false-font miniblocks
    
    if VERBOSE
        disp('----------------- ALL COUNTERS-----------------');
        disp('------------------------------------------------');
        disp('');
        disp('');
        disp('');

        disp('============== Targets ================');

         % Faces

        disp('--------- face targets ---------');

        display(counters_no_duration_info('tfC'), 'counters_no_duration_info(tfC)');

        display(counters_duration_info('tfC05'), 'counters_duration_info(tfC05)');
        display(counters_duration_info('tfC10'), 'counters_duration_info(tfC10)');
        display(counters_duration_info('tfC15') ,'counters_duration_info(tfC15)'); 

        display(counters_duration_info('tfL05'),'counters_duration_info(tfL05)');
        display(counters_duration_info('tfL10') ,'counters_duration_info(tfL10)');
        display(counters_duration_info('tfL15'),'counters_duration_info(tfL15)');


        display(counters_duration_info('tfR05') ,'counters_duration_info(tfR05)');
        display(counters_duration_info('tfR10') , 'counters_duration_info(tfR10)'); 
        display(counters_duration_info('tfR15') ,'counters_duration_info(tfR15)');

        % Objects
        disp('---------object targets ---------');

        display(counters_no_duration_info('toC'), 'counters_no_duration_info(toC)');

        display(counters_duration_info('toC05'), 'counters_duration_info(toC05)');
        display(counters_duration_info('toC10'), 'counters_duration_info(toC10)');
        display(counters_duration_info('toC15') ,'counters_duration_info(toC15)'); 

        display(counters_duration_info('toL05'),'counters_duration_info(toL05)');
        display(counters_duration_info('toL10') ,'counters_duration_info(toL10)');
        display(counters_duration_info('toL15'),'counters_duration_info(toL15)');


        display(counters_duration_info('toR05') ,'counters_duration_info(toR05)');
        display(counters_duration_info('toR10') , 'counters_duration_info(toR10)'); 
        display(counters_duration_info('toR15') ,'counters_duration_info(toR15)');

        % Characters
        disp('--------- char targets ---------');

        display(counters_no_duration_info('tcC'), 'counters_no_duration_info(tcC)');

        display(counters_duration_info('tcC05'), 'counters_duration_info(tcC05)');
        display(counters_duration_info('tcC10'), 'counters_duration_info(tcC10)');
        display(counters_duration_info('tcC15') ,'counters_duration_info(tcC15)'); 

        display(counters_duration_info('tcL05'),'counters_duration_info(tcL05)');
        display(counters_duration_info('tcL10') ,'counters_duration_info(tcL10)');
        display(counters_duration_info('tcL15'),'counters_duration_info(tcL15)');

        disp('--------- false font targets ---------');
        display(counters_duration_info('tcR05') ,'counters_duration_info(tcR05)');
        display(counters_duration_info('tcR10') , 'counters_duration_info(tcR10)'); 
        display(counters_duration_info('tcR15') ,'counters_duration_info(tcR15)');

        % False-fonts

        display(counters_no_duration_info('tsC'), 'counters_no_duration_info(tsC)');

        display(counters_duration_info('tsC05'), 'counters_duration_info(tsC05)');
        display(counters_duration_info('tsC10'), 'counters_duration_info(tsC10)');
        display(counters_duration_info('tsC15') ,'counters_duration_info(tsC15)'); 

        display(counters_duration_info('tsL05'),'counters_duration_info(tsL05)');
        display(counters_duration_info('tsL10') ,'counters_duration_info(tsL10)');
        display(counters_duration_info('tsL15'),'counters_duration_info(tsL15)');


        display(counters_duration_info('tsR05') ,'counters_duration_info(tsR05)');
        display(counters_duration_info('tsR10') , 'counters_duration_info(tsR10)'); 
        display(counters_duration_info('tsR15') ,'counters_duration_info(tsR15)');

        disp('============ Faces-objects ============');
        disp('--------- non-target faces ---------');

        display(counters_no_duration_info('nfC'), 'counters_no_duration_info(nfC)');

        display(counters_no_duration_info('nfC'), 'counters_no_duration_info(nfC)');
        display(counters_duration_info('nfC10'), 'counters_duration_info(nfC10)');
        display(counters_duration_info('nfC15') ,'counters_duration_info(nfC15)');

        display(counters_no_duration_info('nfL') ,'counters_no_duration_info(nfL)');

        display(counters_duration_info('nfL05'),'counters_duration_info(nfL05)');
        display(counters_duration_info('nfL10') ,'counters_duration_info(nfL10)');
        display(counters_duration_info('nfL15'),'counters_duration_info(nfL15)');

        display(counters_no_duration_info('nfR'), 'counters_no_duration_info(nfR)');

        display(counters_duration_info('nfR05') ,'counters_duration_info(nfR05)');
        display(counters_duration_info('nfR10') , 'counters_duration_info(nfR10)'); 
        display(counters_duration_info('nfR15') ,'counters_duration_info(nfR15)');


        disp('--------- non-target objects ---------');
        % Objects
        display(counters_no_duration_info('noC'), 'counters_no_duration_info(noC)');

        display(counters_no_duration_info('noC'), 'counters_no_duration_info(noC)');
        display(counters_duration_info('noC10'), 'counters_duration_info(noC10)');
        display(counters_duration_info('noC15') ,'counters_duration_info(noC15)');

        display(counters_no_duration_info('noL') ,'counters_no_duration_info(noL)');

        display(counters_duration_info('noL05'),'counters_duration_info(noL05)');
        display(counters_duration_info('noL10') ,'counters_duration_info(noL10)');
        display(counters_duration_info('noL15'),'counters_duration_info(noL15)');

        display(counters_no_duration_info('noR'), 'counters_no_duration_info(noR)');

        display(counters_duration_info('noR05') ,'counters_duration_info(noR05)');
        display(counters_duration_info('noR10') , 'counters_duration_info(noR10)'); 
        display(counters_duration_info('noR15') ,'counters_duration_info(noR15)');

        disp('--------- Irrelevant chars ---------');
        % Irrelevants

        % Characters
        display(counters_no_duration_info('icC'), 'counters_no_duration_inoo(icC)');

        display(counters_no_duration_info('icC'), 'counters_no_duration_info(icC)');
        display(counters_duration_info('icC10'), 'counters_duration_info(icC10)');
        display(counters_duration_info('icC15') ,'counters_duration_info(icC15)');

        display(counters_no_duration_info('icL') ,'counters_no_duration_info(icL)');

        display(counters_duration_info('icL05'),'counters_duration_info(icL05)');
        display(counters_duration_info('icL10') ,'counters_duration_info(icL10)');
        display(counters_duration_info('icL15'),'counters_duration_info(icL15)');

        display(counters_no_duration_info('icR'), 'counters_no_duration_info(icR)');

        display(counters_duration_info('icR05') ,'counters_duration_info(icR05)');
        display(counters_duration_info('icR10') , 'counters_duration_info(icR10)'); 
        display(counters_duration_info('icR15') ,'counters_duration_info(icR15)');

        disp('--------- Irrelevant false-fonts ---------');
        % False-fonts
        display(counters_no_duration_info('isC'), 'counters_no_duration_inoo(isC)');

        display(counters_no_duration_info('isC'), 'counters_no_duration_info(isC)');
        display(counters_duration_info('isC10'), 'counters_duration_info(isC10)');
        display(counters_duration_info('isC15') ,'counters_duration_info(isC15)');

        display(counters_no_duration_info('isL') ,'counters_no_duration_info(isL)');

        display(counters_duration_info('isL05'),'counters_duration_info(isL05)');
        display(counters_duration_info('isL10') ,'counters_duration_info(isL10)');
        display(counters_duration_info('isL15'),'counters_duration_info(isL15)');

        display(counters_no_duration_info('isR'), 'counters_no_duration_info(isR)');

        display(counters_duration_info('isR05') ,'counters_duration_info(isR05)');
        display(counters_duration_info('isR10') , 'counters_duration_info(isR10)'); 
        display(counters_duration_info('isR15') ,'counters_duration_info(isR15)');
  
    
    
        disp('------------------------------------------------')
        disp('non-target chars')
        disp('------------------------------------------------')
        disp('============ Letter-False fonts ============')
        disp('------------------------------------------------')
        disp('non-target chars')
        disp('------------------------------------------------')

        display(counters_no_duration_info('ncC'), 'counters_no_duration_info(ncC)');

        display(counters_duration_info('ncC05'), 'counters_duration_info(ncC05)'); 
        display(counters_duration_info('ncC10'), 'counters_duration_info(ncC10)');
        display(counters_duration_info('ncC15'), 'counters_duration_info(ncC15)');

        display(counters_no_duration_info('ncL') , 'counters_no_duration_info(ncL)');

        display(counters_duration_info('ncL05') , 'counters_duration_info(ncL05)');
        display(counters_duration_info('ncL10'), 'counters_duration_info(ncL10)');
        display(counters_duration_info('ncL15') , 'counters_duration_info(ncL15)'); 

        display(counters_duration_info('ncL15'), 'counters_duration_info(ncL15)');   

        display(counters_duration_info('ncR05') , 'counters_duration_info(ncR05)');
        display(counters_duration_info('ncR10'), 'counters_duration_info(ncR10)');
        display(counters_duration_info('ncR15'), 'counters_duration_info(ncR15)');

        disp('------------------------------------------------')
        disp('non-target false-fonts')
        disp('------------------------------------------------')

        % False-fonts
        display(counters_no_duration_info('nsC'), 'counters_no_duration_info(nsC)');

        display(counters_no_duration_info('nsC'), 'counters_no_duration_info(nsC)');
        display(counters_duration_info('nsC10'), 'counters_duration_info(nsC10)');
        display(counters_duration_info('nsC15') ,'counters_duration_info(nsC15)');

        display(counters_no_duration_info('nsL') ,'counters_no_duration_info(nsL)');

        display(counters_duration_info('nsL05'),'counters_duration_info(nsL05)');
        display(counters_duration_info('nsL10') ,'counters_duration_info(nsL10)');
        display(counters_duration_info('nsL15'),'counters_duration_info(nsL15)');

        display(counters_no_duration_info('nsR'), 'counters_no_duration_info(nsR)');

        display(counters_duration_info('nsR05') ,'counters_duration_info(nsR05)');
        display(counters_duration_info('nsR10') , 'counters_duration_info(nsR10)'); 
        display(counters_duration_info('nsR15') ,'counters_duration_info(nsR15)');


        % Irrelevants

        disp('------------------------------------------------');
        disp('irrelevant faces');
        disp('------------------------------------------------');

        % Faces
        display(counters_no_duration_info('ifC'), 'counters_no_duration_info(ifC)');

        display(counters_no_duration_info('ifC'), 'counters_no_duration_info(ifC)');
        display(counters_duration_info('ifC10'), 'counters_duration_info(ifC10)');
        display(counters_duration_info('ifC15') ,'counters_duration_info(ifC15)');

        display(counters_no_duration_info('ifL') ,'counters_no_duration_info(ifL)');

        display(counters_duration_info('ifL05'),'counters_duration_info(ifL05)');
        display(counters_duration_info('ifL10') ,'counters_duration_info(ifL10)');
        display(counters_duration_info('ifL15'),'counters_duration_info(ifL15)');

        display(counters_no_duration_info('ifR'), 'counters_no_duration_info(ifR)');

        display(counters_duration_info('ifR05') ,'counters_duration_info(ifR05)');
        display(counters_duration_info('ifR10') , 'counters_duration_info(ifR10)'); 
        display(counters_duration_info('ifR15') ,'counters_duration_info(ifR15)');

        disp('------------------------------------------------');
        disp('Irrelevant objects');
        disp('------------------------------------------------');

        display(counters_no_duration_info('ioC'), 'counters_no_duration_info(ioC)');

        display(counters_no_duration_info('ioC'), 'counters_no_duration_info(ioC)');
        display(counters_duration_info('ioC10'), 'counters_duration_info(ioC10)');
        display(counters_duration_info('ioC15') ,'counters_duration_info(ioC15)');

        display(counters_no_duration_info('ioL') ,'counters_no_duration_info(ioL)');

        display(counters_duration_info('ioL05'),'counters_duration_info(ioL05)');
        display(counters_duration_info('ioL10') ,'counters_duration_info(ioL10)');
        display(counters_duration_info('ioL15'),'counters_duration_info(ioL15)');

        display(counters_no_duration_info('ioR'), 'counters_no_duration_info(ioR)');

        display(counters_duration_info('ioR05') ,'counters_duration_info(ioR05)');
        display(counters_duration_info('ioR10') , 'counters_duration_info(ioR10)'); 
        display(counters_duration_info('ioR15') ,'counters_duration_info(ioR15)');
        
    end
    
 

    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    function [out] = assignOrientation(in,relevance,type)
        
        %display(in, 'in');
        %display(type, 'type');
       
        % NOTE: This function changes the face_num_center etc variables. So it
        % does not only give the orientation, it also changes the counters
        
        % 'f' for face, 'o' for false, 's' for false-font and c for char
        
        %disp('---')
        if VERBOSE 
            disp('WELCOME TO assignOrientation with ')
            display(type, 'type')
            display(in, 'in')
            display(relevance, 'relevance')
        end
        %disp('---')
        
        % Check the values of the counters. Which ones are > 0, meaning
        % there are places left in this class
      
        
        orientation_center_counter = counters_no_duration_info(strcat(relevance,type,'C')); % Center
        if VERBOSE  display(orientation_center_counter, 'orientation_center_counter'); end
        orientation_left_counter = counters_no_duration_info(strcat(relevance,type,'L')); % Left
        if VERBOSE display(orientation_left_counter, 'orientation_left_counter'); end
        orientation_right_counter = counters_no_duration_info(strcat(relevance,type,'R')); % Right
        if VERBOSE display(orientation_right_counter, 'orientation_right_counter'); end


        orientations_2_choose_from = [];
        ctr = 1;
        if orientation_center_counter > 0
            if VERBOSE 
                disp('into center');
                disp(ctr);
            end
            orientations_2_choose_from(ctr) = 1; % center
            ctr = ctr + 1;
            orientations_2_choose_from(ctr) = 1; % add twice since twice as many stim with this orientation
            ctr = ctr + 1;
            if VERBOSE  display(orientations_2_choose_from, 'orientation_2_choose_from'); end
        end
        if orientation_left_counter > 0
            if VERBOSE 
                disp('into left');
                disp(ctr);
            end
            orientations_2_choose_from(ctr) = 2;
            ctr = ctr + 1;
            if VERBOSE  display(orientations_2_choose_from, 'orientation_2_choose_from'); end
        end
        if orientation_right_counter > 0
            if VERBOSE 
                disp('into right');
                disp(ctr);
            end
            orientations_2_choose_from(ctr) = 3;
            ctr = ctr + 1;
            if VERBOSE  display(orientations_2_choose_from, 'orientation_2_choose_from'); end
        end

        if VERBOSE display(orientations_2_choose_from, 'orientations_2_choose_from'); end
        % Choose randomly from available orientations:
        or_rand_vector = Shuffle(randperm(size(orientations_2_choose_from,2)));
        or_rand_nr = or_rand_vector(1);
        if VERBOSE display(or_rand_nr,'or_rand_nr'); end

        switch orientations_2_choose_from(or_rand_nr)

            case 1 % center
                out = in + CENTER;
                %display(in, 'in');
                %display(out, 'out');
                %display(CENTER, 'in');
                
                %disp('CHECK:');
                %disp(strcat(relevance,type,'C'));

                counters_no_duration_info(strcat(relevance,type,'C')) = ...
                    counters_no_duration_info(strcat(relevance,type,'C')) - 1;
                
                if counters_no_duration_info(strcat(relevance,type,'C')) < 1
                    if VERBOSE 
                        disp('counters no duration less than zero!!! ');
                        disp( counters_no_duration_info(strcat(relevance,type,'C')));
                        disp(strcat(relevance,type,'C'));
                    end
                end
                
            case 2 % left
                out = in + LEFT;
                %disp('CHECK:');
                %disp(strcat(relevance,type,'L'));
                counters_no_duration_info(strcat(relevance,type,'L')) = ...
                    counters_no_duration_info(strcat(relevance,type,'L')) - 1;
                
                if counters_no_duration_info(strcat(relevance,type,'L')) < 1
                    if VERBOSE 
                        disp('counters no duration less than zero!!! ');
                        disp( counters_no_duration_info(strcat(relevance,type,'L')));
                        disp(strcat(relevance,type,'L'));
                    end
                end
             
            case 3 % right
                out = in + RIGHT;
                %disp('CHECK:');
                %disp(strcat(relevance,type,'R'));
                counters_no_duration_info(strcat(relevance,type,'R')) = ...
                    counters_no_duration_info(strcat(relevance,type,'R')) - 1;
                
                if counters_no_duration_info(strcat(relevance,type,'R')) < 1
                    if VERBOSE 
                        disp('counters no duration less than zero!!! ');
                        disp( counters_no_duration_info(strcat(relevance,type,'R')));
                        disp(strcat(relevance,type,'R'));
                    end
                end
        end % end switch
        
        
    end % end function assignOrientation
   
    
    function [out] = assignDuration(in, relevance,type,orientation)
        
        %disp('---')
        if VERBOSE 
            disp('WELCOME TO assignDuration with ');
            display(type, 'type');
            display(in, 'in');
            display(orientation, 'orientation');
        end
        %disp('---');
        
        % Check the values of the counters. Which ones are > 0, meaning
        % there are places left in this class
        
        %disp(strcat(relevance,type,orientation,'05'));
        duration_05_counter = counters_duration_info(strcat(relevance,type,orientation,'05'));
        if VERBOSE 
            display(duration_05_counter, 'duration_05_counter');
            disp(strcat(relevance,type,orientation,'10'));
        end
        duration_1_counter = counters_duration_info(strcat(relevance,type,orientation,'10'));
        if VERBOSE 
            display(duration_1_counter, 'duration_1_counter');
            disp(strcat(relevance,type,orientation,'15'));
        end
        duration_15_counter = counters_duration_info(strcat(relevance,type,orientation,'15'));
        if VERBOSE  display(duration_15_counter, 'duration_15_counter'); end

        durations_2_choose_from = [];
        ctr = 1;
        if duration_05_counter > 0
            durations_2_choose_from(ctr) = 5;
            ctr = ctr + 1;
        end
        if duration_1_counter > 0
            durations_2_choose_from(ctr) = 10;
            ctr = ctr + 1;
        end
        if duration_15_counter > 0
            durations_2_choose_from(ctr) = 15;
            ctr = ctr + 1;
        end

        if VERBOSE display( durations_2_choose_from,  'durations_2_choose_from'); end
        % Choose randomly from available durations:
        dur_rand_vector = Shuffle(randperm(size(durations_2_choose_from,2)));
        if VERBOSE  display( dur_rand_vector,  'dur_rand_vector'); end
        dur_rand_nr = dur_rand_vector(1);
        %display(dur_rand_vector, 'dur_rand_vector');
        %display(dur_rand_nr, 'dur_rand_nr');

        switch durations_2_choose_from(dur_rand_nr)

            case 5 % duration 0.5
                %disp('in case 5');
                out = in + DURATION_VEC(1);
                     
                %display(in, 'in');
                %display(out, 'out');
                %display(DURATION_VEC(1), 'DURATION_VEC(1)');
                counters_duration_info(strcat(relevance,type,orientation,'05')) = ...
                    counters_duration_info(strcat(relevance,type,orientation,'05')) - 1;

            case 10 % duration 1.0
                %disp('in case 1.0');
                out = in + DURATION_VEC(2);
                %display(in, 'in');
                %display(out, 'out');
                %display(DURATION_VEC(2), 'DURATION_VEC(2)');
                counters_duration_info(strcat(relevance,type,orientation,'10')) = ...
                    counters_duration_info(strcat(relevance,type,orientation,'10')) - 1;

            case 15 % duration 1.5
                %disp('in case 1.5');
                out = in + DURATION_VEC(3);
                %display(in, 'in');
                %display(out, 'out');
                %display(DURATION_VEC(3), 'DURATION_VEC(3)');
                counters_duration_info(strcat(relevance,type,orientation,'15')) = ...
                    counters_duration_info(strcat(relevance,type,orientation,'15')) - 1;

        end % end switch

    end % function assignDuration  
    
end % function createTrials
