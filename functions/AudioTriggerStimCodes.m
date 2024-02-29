global matrix_LUT inverse_matrix_LUT

matrix_LUT = zeros(4,3,3,3);
inverse_matrix_LUT = containers.Map;

% Fill with values from 1 to 108:
ctr = 1;

for cat=1:4
    for rel=1:3
        for ori=1:3
            for dur=1:3

                matrix_LUT(cat,rel,ori,dur) = ctr;
                % Assign 'face', 'rel'...
                % This matrix can be used to retrieve the code again
                 % from the trigger values
                inverse_matrix_LUT(int2str(ctr)) = [cat, rel, ori, dur];
                ctr = ctr + 1;

            end
        end
    end
end

% test the function:

% face, target, left, 1.5
cat = 1;
rel = 1;
ori = 2;
dur = 3;

bit_code = getStimTrigAudBitCode(cat, rel, ori, dur);

display(bit_code);

disp('Before calling getCatRel...');

[category, relevance, orientation, duration] = getCatRelOriDur(bit_code);

display(category);
display(relevance);
display(orientation);
display(duration);


function [bit_code] = getStimTrigAudBitCode(cat, rel, ori, dur)

    global matrix_LUT

    % Construct a mapping between category, relevance, orientaion and duration

    dec_code = matrix_LUT(cat,rel,ori, dur);
    bit_code = dec2bin(dec_code,7);
    
        
end % end getStimTrigAudCode


function [category, relevance, orientation, duration] = getCatRelOriDur(bit_code)

    global inverse_matrix_LUT
    
    dec_code = bin2dec(bit_code);
    id_vec = inverse_matrix_LUT(int2str(dec_code));
    cat = id_vec(1);
    rel = id_vec(2);
    ori = id_vec(3);
    dur = id_vec(4);
    
    category_vec = ["face", "object", "letter", "false"];
    relevance_vec = ["target", "non-target", "irrelevant"];
    orientation_vec = ["center", "left", "right"];  
    duration_vec = ["0.5", "1.5", "2.0"];
    
    category = category_vec(cat);
    relevance = relevance_vec(rel);
    orientation = orientation_vec(ori);
    duration = duration_vec(dur);
    
    display(id_vec);
    
end % end getCatRelOriDur

