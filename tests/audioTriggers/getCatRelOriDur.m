% This function retrieves the experiment conditions from the audio 7 bits
% triggers
% Author: Katarina Bendtz
% Date: 28/05/2020
% Modified by Alex Lepauvre, 28/05/2020, make this an independant function
% and add the inverse amtrix as input instead of global variable
% Input:
% bit_code: the audio trigger bit code (withouth the flanks) to decode
% inverse_matrix_LUT: matrix mapping the conditions to the triggers
% Output
% category: category of the stim
% relevant: task relevant condition of the stim
% orientation: orientation of the stim
% duration: duration of the stimulus

function [category, relevance, orientation, duration] = getCatRelOriDur(bit_code,inverse_matrix_LUT)
    
    dec_code = bin2dec(bit_code);
    id_vec = inverse_matrix_LUT(int2str(dec_code));
    cat = id_vec(1);
    rel = id_vec(2);
    ori = id_vec(3);
    dur = id_vec(4);
    
    category_vec = ["face", "object", "letter", "false"];
    relevance_vec = ["target", "non-target", "irrelevant"];
    orientation_vec = ["center", "left", "right"];  
    duration_vec = ["0.5", "1.0", "1.5"];
    
    category = category_vec(cat);
    relevance = relevance_vec(rel);
    orientation = orientation_vec(ori);
    duration = duration_vec(dur);
    
end % end getCatRelOriDur