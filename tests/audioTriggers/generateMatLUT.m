% This function creates the matrices mapping the audio 7 bit triggers to
% the experimental conditions.
% Author: Katarina Bendtz
% Date: 28/05/2020
% Modified by Alex Lepauvre, 28/05/2020, making it a function of its own#
% Input:
%
% Output:
% matrix_LUT: matrix mapping the 7bit audio triggers to the condition
% inverse_matrix_LUT: container map mapping the conditions back to the 7
% bit triggers
function [matrix_LUT,inverse_matrix_LUT] = generateMatLUT()
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
end
