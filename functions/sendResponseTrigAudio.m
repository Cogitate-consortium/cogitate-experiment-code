function [] = sendResponseTrigAudio()

    global TRG_RESPONSE_AUD NO_AUDIO

    bit_code = dec2bin(TRG_RESPONSE_AUD,7);
    if ~NO_AUDIO
        sendTrigAudio(bit_code);
    end
end