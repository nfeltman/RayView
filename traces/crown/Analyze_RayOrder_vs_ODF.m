function Analyze_RayOrder_vs_ODF( n )
g = n(:,1);
b = n(:,2);
d = b-g;

fprintf('Number of nodes: %i\n', length(g));
fprintf('Number of nodes ever hit: %i vs %i\n', sum(g~=0), sum(b~=0));
fprintf('Number of nodes hit by RO and not ODF: %i\n', sum(g~=0 & b==0));
fprintf('Number of nodes hit by ODF and not RO: %i\n', sum(g==0 & b~=0));
fprintf('Total node inspections: %i - %i = %i\n', sum(b), sum(g), sum(d));

subplot(3,1,1);
hist(log(g(g~=0)),40);
title('Distribution of node hits (RayOrder)');
subplot(3,1,2);
hist(log(b(b~=0)),40);
title('Distribution of node hits (ODF)');
subplot(3,1,3);
hist(log(d(d~=0)),40);
title('Distribution of node hits (diff)');
end

