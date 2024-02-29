
% GETSTIMTRIGAUDBITCODE
function [bit_code] = getStimTrigAudBitCode(cat, rel, ori, dur)

    global matrix_LUT

    % Construct a mapping between category, relevance, orientaion and duration

    dec_code = matrix_LUT(cat,rel,ori, dur);
    bit_code = dec2bin(dec_code,7);
    
        
end % end getStimTrigAudCode
