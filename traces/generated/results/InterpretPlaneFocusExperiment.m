function InterpretPlaneFocusExperiment()

d = load('PlaneFocusExperiment.txt');
len = size(d,2);
numtrials = (len-1)/2;
figure();
hold on
plot(d(:,1),d(:,3:2:len),'r');
plot(d(:,1),d(:,2:2:(len-1)),'b');
hold off
title('Plane All');
xlabel('Focus');
ylabel('BVH Cost');
ax = axis;
axis([0 1 0 ax(4)]);

avg = zeros(size(d,1),2);
for k=1:numtrials
    avg = avg + d(:,(k*2):(k*2+1));
end
avg = avg/numtrials;
figure();
hold on
plot(d(:,1),avg(:,1),'b');
plot(d(:,1),avg(:,2),'r');
hold off
title('Plane: Averaged');
xlabel('Focus');
ylabel('BVH Cost');
ax = axis;
axis([0 1 0 ax(4)]);
%legend('Unspecialized', 'Specialized');

end
