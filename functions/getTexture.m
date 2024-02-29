%GETTEXTURE takes a stimuli number and reaches into the pre-loaded
%textures and returns the texture pointer.
% input:
% ------
% stimNum - the stimuli number (e.g., 4119) for which to get the texture
% pointer.
%
% output:
% -------
% texturePtr - a pointer to the texture of a given stimuli

function [ texturePtr ] = getTexture (stimNum)

    global TEXTURES_R_falses TEXTURES_R_chars TEXTURES_R_objects FACE OBJECT TEXTURES_R_faces  LETTER  FALSE_FONT LEFT RIGHT;
    global TEXTURES_L_faces TEXTURES_C_faces TEXTURES_L_chars TEXTURES_C_chars TEXTURES_L_falses TEXTURES_C_falses TEXTURES_L_objects TEXTURES_C_objects TEXTURE_DIODE_TEST

    stimIndex = mod(stimNum,100);

    global PRACTICE PRACTICE_TEXTURES_L PRACTICE_TEXTURES_R PRACTICE_TEXTURES_C

    if stimNum > PRACTICE && stimNum < 10000 % 10000 is the address for the oscilloscope
        if (mod(stimNum,1000) >= LEFT && mod(stimNum,1000) < RIGHT) % is Left | get the 100s, left is 200 and right is 300
            texturePtr = PRACTICE_TEXTURES_L(1,stimIndex);
        elseif (mod(stimNum,1000) >= RIGHT)
            texturePtr = PRACTICE_TEXTURES_R(1,stimIndex);
        else % (FACE with CENTER)
            texturePtr = PRACTICE_TEXTURES_C(1,stimIndex);
        end
    elseif stimNum >= FACE && stimNum < OBJECT % is FACE | which is in the 1000s, while object is in the 2000s
        if (mod(stimNum,1000) >= LEFT && mod(stimNum,1000) < RIGHT) % is Left | get the 100s, left is 200 and right is 300
            texturePtr = TEXTURES_L_faces(1,stimIndex); % after finding the correct vactor (FACE with LEFT), reach by stim number to the correct cell
        elseif (mod(stimNum,1000) >= RIGHT) % (FACE with RIGHT)
            texturePtr = TEXTURES_R_faces(1,stimIndex);
        else % (FACE with CENTER)
            texturePtr = TEXTURES_C_faces(1,stimIndex);
        end
    elseif stimNum >= OBJECT && stimNum < LETTER % OBJECT
        if (mod(stimNum,1000) >= LEFT && mod(stimNum,1000) < RIGHT) % is Left
            texturePtr = TEXTURES_L_objects(1,stimIndex);
        elseif (mod(stimNum,1000) >= RIGHT)  % is Right
            texturePtr = TEXTURES_R_objects(1,stimIndex);
        else
            texturePtr = TEXTURES_C_objects(1,stimIndex); % is center
        end
    elseif stimNum >= LETTER && stimNum < FALSE_FONT % LETTER
        if (mod(stimNum,1000) >= LEFT && mod(stimNum,1000) < RIGHT) % is Left
            texturePtr = TEXTURES_L_chars(1,stimIndex);
        elseif (mod(stimNum,1000) >= RIGHT) % is Right
            texturePtr = TEXTURES_R_chars(1,stimIndex);
        else
            texturePtr = TEXTURES_C_chars(1,stimIndex);   % is center
        end
    elseif stimNum >= FALSE_FONT && stimNum < PRACTICE% FALSE_FONT
        if (mod(stimNum,1000) >= LEFT && mod(stimNum,1000) < RIGHT) % is Left
            texturePtr = TEXTURES_L_falses(1,stimIndex);
        elseif (mod(stimNum,1000) >= RIGHT) % is Right
            texturePtr = TEXTURES_R_falses(1,stimIndex);
        else
            texturePtr = TEXTURES_C_falses(1,stimIndex); % is center
        end
     else
        texturePtr = [];
    end
end
