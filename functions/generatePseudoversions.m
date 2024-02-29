% Generate pseudoversions
function [] = generatePseudoversions(type)

    % call it like this: generatePseudoversions(type) where type = 'f', 'o', 'c' or 's' for
    % face, object, char or false-font, respectively.
    
    % This function generates pseudorandomized non-targets for miniblocks.
    % It can run into an infinite loop. If so just run it again until it
    % works. It is meant to be run only a couple of times to generate working
    % pseudorandomizations, therefore it doesn't matter if it sometimes fails.
    % We did not want to integrate this into the experiment script, because then
    % we could risk failing to run the experiment.
    
    disp('NOTE: This function can go into an infinite loop. If it does, then run it again until it does not.');

    %global fMRI ECoG NUM_OF_STIMULI_EACH FACE OBJECT FALSE_FONT LETTER NUMBER_OF_NON_TARGET_SETS_PER_CAT NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB
    ECoG = 1;
    
    NUM_OF_STIMULI_EACH = 20; % nr of unique stimuli (e.g. different human faces)
    FACE = 1000;  OBJECT = 2000; LETTER = 3000; FALSE_FONT = 4000;
    NUMBER_OF_NON_TARGET_SETS_PER_CAT = 16; % number of ech stimulus appearing as a non-target
    NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB = 8; % number of ech non-targets per 
    
    if ECoG
         NUMBER_OF_NON_TARGET_SETS_PER_CAT = 16/2;
        
    end


    % Vectors to use for non-targets:
    non_targets_faces_per_mb = containers.Map;
    non_targets_objects_per_mb = containers.Map;
    non_targets_chars_per_mb = containers.Map;
    non_targets_falses_per_mb = containers.Map;
    

    
    % NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2 = 16 /2 = 8 (4 for ECoG)

    faces = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + FACE;
    objects = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + OBJECT; 
    chars = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + LETTER; 
    falses = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,NUMBER_OF_NON_TARGET_SETS_PER_CAT / 2)) + FALSE_FONT; 

    display(size(faces,2), 'size(faces,2)');    
    % Vectors to use as targets:
    % Size = 20
    faces_target = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + FACE; % FACE = 1000 so that all faces will start with 10
    objects_target = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + OBJECT;
    chars_target = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + LETTER;
    falses_target = Shuffle(repmat(randperm(NUM_OF_STIMULI_EACH),1,1)) + FALSE_FONT;
    
    if ECoG
        
        % TODO: update when Alex is finished with these:
        
        FACES_TARGET_ECoG = [1, 2, 4, 5, 9, 11, 12, 14, 15, 18]; % 1
        OBJECTS_TARGET_ECoG = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]; % 2
        CHARS_TARGET_ECoG = [1, 3, 5, 7, 8, 10, 11, 13, 15, 19]; % 3
        FALSES_TARGET_ECoG = [1, 3, 5, 7, 8, 10, 11, 13, 15, 19]; % 4
        
        faces_target = Shuffle(FACES_TARGET_ECoG) + FACE;
        objects_target = Shuffle(OBJECTS_TARGET_ECoG) + OBJECT;
        chars_target = Shuffle(CHARS_TARGET_ECoG) + LETTER;
        falses_target = Shuffle(FALSES_TARGET_ECoG) + FALSE_FONT;
        
    end
    
    display(faces_target);
    display(objects_target);
    display(chars_target);
    display(falses_target);
    

    if type == 'f'

        for j = 1:size(faces_target,2)

            display(j, 'j');
            target = faces_target(j);

            display(target, 'target');
            non_target_faces = [];

            for i = 1:NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB  

                display(size(faces,2), 'size(faces,2)')
                % Pick a random ind from the faces vector
                faces = Shuffle(faces);
                non_target_face = faces(1);
                while non_target_face == target
                    display(non_target_face, 'in while with non_target_face');
                    % rand is not truly random
                    faces = Shuffle(faces);
                    non_target_face = faces(1);

                end % while
                display(non_target_face, 'decided for this non_target_face');
                non_target_faces = [non_target_faces non_target_face];
                faces(1) = [];
            end % for

            non_targets_faces_per_mb(int2str(target)) = non_target_faces;

        end % for faces_target

        % Print the non_targets vectors
        for i = 1:size(faces_target,2)
            str_key = int2str(faces_target(i));
            value = non_targets_faces_per_mb(str_key);
            print_str = strcat('NON_TARGETS_FACES_PER_MB(', str_key, ') = [');
            disp(print_str);
            print_str = '';
            
            for j = 1:size(value,2)
                print_str = strcat(int2str(value(j)), ',' , print_str);
            end % for
            disp(strcat(print_str,']'));
        end % for
    
    elseif type == 'o'

        for j = 1:size(objects_target,2)

            display(j, 'j');
            target = objects_target(j);

            display(target, 'target');
            non_target_objects = [];

            for i = 1:NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB  

                display(size(objects,2), 'size(objects,2)')
                % Pick a random ind from the objects vector
                objects = Shuffle(objects);
                non_target_object = objects(1);
                while non_target_object == target
                    display(non_target_object, 'in while with non_target_object');
                    % rand is not truly random
                    objects = Shuffle(objects);
                    non_target_object = objects(1);

                end % while
                display(non_target_object, 'decided for this non_target_object');
                non_target_objects = [non_target_objects non_target_object];
                objects(1) = [];
            end % for

            non_targets_objects_per_mb(int2str(target)) = non_target_objects;

        end % for objects_target

        % Print the non_targets vectors
        for i = 1:size(objects_target,2)
            str_key = int2str(objects_target(i));
            value = non_targets_objects_per_mb(str_key);
            print_str = strcat('NON_TARGETS_OBJECTS_PER_MB(', str_key, ') = [');
            disp(print_str);
            print_str = '';

            for j = 1:size(value,2)
                print_str = strcat(int2str(value(j)), ',' , print_str);
                
            end % for
            disp(strcat(print_str,']'));
        end % for
        
         elseif type == 'c'

        for j = 1:size(chars_target,2)

            display(j, 'j');
            target = chars_target(j);

            display(target, 'target');
            non_target_chars = [];

            for i = 1:NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB  

                display(size(chars,2), 'size(chars,2)')
                % Pick a random ind from the chars vector
                chars = Shuffle(chars);
                non_target_char = chars(1);
                while non_target_char == target
                    display(non_target_char, 'in while with non_target_char');
                    % rand is not truly random
                    chars = Shuffle(chars);
                    non_target_char = chars(1);

                end % while
                display(non_target_char, 'decided for this non_target_char');
                non_target_chars = [non_target_chars non_target_char];
                chars(1) = [];
            end % for

            non_targets_chars_per_mb(int2str(target)) = non_target_chars;

        end % for chars_target

        % Print the non_targets vectors
        for i = 1:size(chars_target,2)
            str_key = int2str(chars_target(i));
            value = non_targets_chars_per_mb(str_key);
            print_str = strcat('NON_TARGETS_CHARS_PER_MB(', str_key, ') = [');
            disp(print_str);
            print_str = '';

            for j = 1:size(value,2)
                print_str = strcat(int2str(value(j)), ',' , print_str);
                
            end % for
            disp(strcat(print_str,']'));
        end % for
        
    else % falses
        
        for j = 1:size(falses_target,2)

            display(j, 'j');
            target = falses_target(j);

            display(target, 'target');
            non_target_falses = [];

            for i = 1:NUMBER_OF_NON_TARGETS_PER_CAT_PER_MB  

                display(size(falses,2), 'size(falses,2)')
                % Pick a random ind from the falses vector
                falses = Shuffle(falses);
                non_target_false = falses(1);
                while non_target_false == target
                    display(non_target_false, 'in while with non_target_false');
                    % rand is not truly random
                    falses = Shuffle(falses);
                    non_target_false = falses(1);

                end % while
                display(non_target_false, 'decided for this non_target_false');
                non_target_falses = [non_target_falses non_target_false];
                falses(1) = [];
            end % for

            non_targets_falses_per_mb(int2str(target)) = non_target_falses;

        end % for falses_target

        % Print the non_targets vectors
        for i = 1:size(falses_target,2)
            str_key = int2str(falses_target(i));
            value = non_targets_falses_per_mb(str_key);
            print_str = strcat('NON_TARGETS_FALSES_PER_MB(', str_key, ') = [');
            disp(print_str);
            print_str = '';

            for j = 1:size(value,2)
                print_str = strcat(int2str(value(j)), ',', print_str);
                
            end % for
            disp(strcat(print_str,']'));
        end % for
        
    end % if type
    
end % function
    
    