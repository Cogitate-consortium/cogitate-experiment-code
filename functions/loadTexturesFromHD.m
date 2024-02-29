
% LOADTEXTURESFROMHD loads all the stimuli in all types and orientation from HD. Wasteful and should be used before the experiment is running.
% output:
% -------
% loads all textures from HD, and puts them into the global textures'
% vectors.
function [ ] = loadTexturesFromHD()
global TEXTURES_C_objects TEXTURES_C_falses TEXTURES_C_chars TEXTURES_L_objects TEXTURES_L_falses TEXTURES_L_chars VERBOSE
global FACES_R_FOL FACES_L_FOL TEXTURES_R_faces TEXTURES_C_faces TEXTURES_L_faces TEXTURES_R_faces_Female TEXTURES_R_faces_Male TEXTURES_C_faces_Female TEXTURES_C_faces_Male TEXTURES_L_faces_Female TEXTURES_L_faces_Male FEMALE_FOLDER MALE_FOLDER TEXTURES_R_objects TEXTURES_R_falses TEXTURES_R_chars OBJECTS_R_FOL FALSES_R_FOL CHARS_R_FOL OBJECTS_C_FOL FALSES_C_FOL CHARS_C_FOL FACES_C_FOL OBJECTS_L_FOL FALSES_L_FOL CHARS_L_FOL
global PRACTICE_L_FOL PRACTICE_R_FOL PRACTICE_C_FOL PRACTICE_TEXTURES_L PRACTICE_TEXTURES_R PRACTICE_TEXTURES_C DIODETEST_FOL TEXTURE_DIODE_TEST ANIMAL_REWARD_FOL TEXTURES_ANIMAL_REWARD

if VERBOSE
    disp(' loadTexturesFromHD');
end

try
    PRACTICE_TEXTURES_L = getTexturesFromHD(PRACTICE_L_FOL);
    PRACTICE_TEXTURES_R = getTexturesFromHD(PRACTICE_R_FOL);
    PRACTICE_TEXTURES_C = getTexturesFromHD(PRACTICE_C_FOL);
    
    % Left stimuli:
    TEXTURES_L_faces_Male = getTexturesFromHD(fullfile(FACES_L_FOL,MALE_FOLDER));
    TEXTURES_L_faces_Female = getTexturesFromHD(fullfile(FACES_L_FOL,FEMALE_FOLDER));
    if VERBOSE
        display(fullfile(FACES_L_FOL,MALE_FOLDER), 'fullfile(FACES_L_FOL,MALE_FOLDER)');
        display(TEXTURES_L_faces_Male, 'TEXTURES_L_faces_Male');
        display(TEXTURES_L_faces_Female, 'TEXTURES_L_faces_Female');
    end
    TEXTURES_L_faces = [TEXTURES_L_faces_Male TEXTURES_L_faces_Female];
    TEXTURES_L_chars = getTexturesFromHD(CHARS_L_FOL);
    TEXTURES_L_falses = getTexturesFromHD(FALSES_L_FOL);
    TEXTURES_L_objects = getTexturesFromHD(OBJECTS_L_FOL);
    
    % Center stimuli
    TEXTURES_C_faces_Male = getTexturesFromHD(fullfile(FACES_C_FOL,MALE_FOLDER));
    TEXTURES_C_faces_Female = getTexturesFromHD(fullfile(FACES_C_FOL,FEMALE_FOLDER));
    TEXTURES_C_faces = [TEXTURES_C_faces_Male TEXTURES_C_faces_Female];
    TEXTURES_C_chars = getTexturesFromHD(CHARS_C_FOL);
    TEXTURES_C_falses = getTexturesFromHD(FALSES_C_FOL);
    TEXTURES_C_objects = getTexturesFromHD(OBJECTS_C_FOL);
    
    % Right stimuli
    TEXTURES_R_faces_Male = getTexturesFromHD(fullfile(FACES_R_FOL,MALE_FOLDER));
    TEXTURES_R_faces_Female = getTexturesFromHD(fullfile(FACES_R_FOL,FEMALE_FOLDER));
    TEXTURES_R_faces = [TEXTURES_R_faces_Male TEXTURES_R_faces_Female];
    TEXTURES_R_chars = getTexturesFromHD(CHARS_R_FOL);
    TEXTURES_R_falses = getTexturesFromHD(FALSES_R_FOL);
    TEXTURES_R_objects = getTexturesFromHD(OBJECTS_R_FOL);
    %Animal pics
    TEXTURES_ANIMAL_REWARD = load_animal_imag(ANIMAL_REWARD_FOL);
    
catch
    PRACTICE_TEXTURES_L = getTexturesFromHD(PRACTICE_L_FOL);
    PRACTICE_TEXTURES_R = getTexturesFromHD(PRACTICE_R_FOL);
    PRACTICE_TEXTURES_C = getTexturesFromHD(PRACTICE_C_FOL);
    
    TEXTURES_L_faces_Male = getTexturesFromHD(fullfile(FACES_L_FOL,MALE_FOLDER));
    TEXTURES_L_faces_Female = getTexturesFromHD(fullfile(FACES_L_FOL,FEMALE_FOLDER));
    TEXTURES_L_faces = [TEXTURES_L_faces_Male TEXTURES_L_faces_Female];
    TEXTURES_L_chars = getTexturesFromHD(CHARS_L_FOL);
    TEXTURES_L_falses = getTexturesFromHD(FALSES_L_FOL);
    TEXTURES_L_objects = getTexturesFromHD(OBJECTS_L_FOL);
    
    TEXTURES_C_faces_Male = getTexturesFromHD(fullfile(FACES_C_FOL,MALE_FOLDER));
    TEXTURES_C_faces_Female = getTexturesFromHD(fullfile(FACES_C_FOL,FEMALE_FOLDER));
    TEXTURES_C_faces = [TEXTURES_C_faces_Male TEXTURES_C_faces_Female];
    TEXTURES_C_chars = getTexturesFromHD(CHARS_C_FOL);
    TEXTURES_C_falses = getTexturesFromHD(FALSES_C_FOL);
    TEXTURES_C_objects = getTexturesFromHD(OBJECTS_C_FOL);
    
    TEXTURES_R_faces_Male = getTexturesFromHD(fullfile(FACES_R_FOL,MALE_FOLDER));
    TEXTURES_R_faces_Female = getTexturesFromHD(fullfile(FACES_R_FOL,FEMALE_FOLDER));
    TEXTURES_R_faces = [TEXTURES_R_faces_Male TEXTURES_R_faces_Female];
    TEXTURES_R_chars = getTexturesFromHD(CHARS_R_FOL);
    TEXTURES_R_falses = getTexturesFromHD(FALSES_R_FOL);
    TEXTURES_R_objects = getTexturesFromHD(OBJECTS_R_FOL);
    
    
    TEXTURE_DIODE_TEST = getTexturesFromHD(DIODETEST_FOL);
end
end