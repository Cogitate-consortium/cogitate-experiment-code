function type = getType(num)
    type = floor(mod(num,100))+floor(num/1000)*1000;
end