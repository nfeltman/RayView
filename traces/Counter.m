function numtrees = Counter(leaves, offlims)
if(contains(leaves, offlims))
    numtrees = 0;
    return;
end    
if length(leaves)==1
   numtrees = 1;
   return;
end
splits = GetSplits(leaves);
accum = 0;
for k=1:size(splits,1)
    accum = accum + Counter(splits{k,1},offlims)*Counter(splits{k,2},offlims);
end
numtrees = accum;
end

function poss = GetSplits(tosplit)
poss = cell(0,2);
len = length(tosplit);
max = 2^len - 1;
index = 1;
for k = 1:((max+1)/2-1)
    maybe1 = tosplit(GetBitArray(k,len));
    maybe2 = tosplit(GetBitArray(max-k,len));
%     if(~contains(maybe1,offlims) || ~contains(maybe1,offlims))
        poss{index,1} = maybe1;
        poss{index,2} = maybe2;
        index = index +1;
%     end
end
end

function a = GetBitArray(k,n)
    a = false(n,1);
    for b= 1:n,
        if (bitget(k,b))
           a(b) = true; 
        end
    end
end

function b = contains(elem, arr)

b = false;
for k=1:length(arr)
    if isequal(arr{k},elem)
        b = true;
        return;
    end
end

end