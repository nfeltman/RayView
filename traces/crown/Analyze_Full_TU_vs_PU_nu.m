function Analyze_Full_TU_vs_PU_nu( d )

depths = d(:,1);
max_depth = max(depths);
colors = hsv(max_depth);
fig_count = 1;

% FIGURE ~T vs P*nu
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
for k = 1:max_depth,
    f = d(depths==k,:);
    loglog(f(:,2),f(:,3).*f(:,4),'.','Color',colors(k,:));
    hold on
end
hold off
xlabel('$\tilde{T}_U(b)$','Interpreter','latex', 'FontSize', 18);
ylabel('$P_U(b)\cdot \nu(b)$','Interpreter','latex', 'FontSize', 18);
title('$\tilde{T}_U(b)$ vs $P_U(b)\cdot \nu(b)$','Interpreter','latex', 'FontSize', 18);

% FIGURE ~T vs P*mu
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
for k = 1:max_depth,
    f = d(depths==k,:);
    loglog(f(:,2),(f(:,3)-1).*f(:,4),'.','Color',colors(k,:));
    hold on
end
hold off
xlabel('$\tilde{T}_U(b)$','Interpreter','latex', 'FontSize', 18);
ylabel('$P_U(b)\cdot \mu(b)$','Interpreter','latex', 'FontSize', 18);
title('$\tilde{T}_U(b)$ vs $P_U(b)\cdot \mu(b)$','Interpreter','latex', 'FontSize', 18);

% FIGURE ~T vs P*lam
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
for k = 1:max_depth,
    f = d(depths==k,:);
    loglog(f(:,2),log(f(:,3)).*f(:,4),'.','Color',colors(k,:));
    hold on
end
hold off
xlabel('$\tilde{T}_U(b)$','Interpreter','latex', 'FontSize', 18);
ylabel('$P_U(b)\cdot \lambda(b)$','Interpreter','latex', 'FontSize', 18);
title('$\tilde{T}_U(b)$ vs $P_U(b)\cdot \lambda(b)$','Interpreter','latex', 'FontSize', 18);

% FIGURE Left/Right Psi over nu
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
hold on
phi1 = d(:,5)./(d(:,6).*d(:,7));
phi2 = d(:,8)./(d(:,9).*d(:,10));
for k = 1:max_depth,
    filter = depths==k;
    plot(phi1(filter,:),phi2(filter,:),'.','Color',colors(k,:));
end
hold off
xlabel('$\Psi_{U/\nu U}(left)$','Interpreter','latex', 'FontSize', 18);
ylabel('$\Psi_{U/\nu U}(right)$','Interpreter','latex', 'FontSize', 18);
title('$\Psi_{U/\nu U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

% FIGURE Min/Max PDF Psi over nu
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
factors = min(phi1,phi2)./max(phi1,phi2);
factors = factors(factors >= 0 & factors <= 1);
plot(linspace(0,1,length(factors)),sort(factors));
title('Min/Max $\Psi_{U/\nu U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

% FIGURE Left/Right Psi over mu
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
hold on
phi1 = d(:,5)./((d(:,6)-1).*d(:,7));
phi2 = d(:,8)./((d(:,9)-1).*d(:,10));
for k = 1:max_depth,
    filter = depths==k;
    plot(phi1(filter,:),phi2(filter,:),'.','Color',colors(k,:));
end
hold off
xlabel('$\Psi_{U/\mu U}(left)$','Interpreter','latex', 'FontSize', 18);
ylabel('$\Psi_{U/\mu U}(right)$','Interpreter','latex', 'FontSize', 18);
title('$\Psi_{U/\mu U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

% FIGURE Min/Max PDF Psi over mu
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
factors = min(phi1,phi2)./max(phi1,phi2);
factors = factors(factors >= 0 & factors <= 1);
plot(linspace(0,1,length(factors)),sort(factors));
title('Min/Max $\Psi_{U/\mu U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

% FIGURE Left/Right Psi over lambda
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
hold on
phi1 = d(:,5)./(log(d(:,6)).*d(:,7));
phi2 = d(:,8)./(log(d(:,9)).*d(:,10));
for k = 1:max_depth,
    filter = depths==k;
    plot(phi1(filter,:),phi2(filter,:),'.','Color',colors(k,:));
end
hold off
xlabel('$\Psi_{U/\lambda U}(left)$','Interpreter','latex', 'FontSize', 18);
ylabel('$\Psi_{U/\lambda U}(right)$','Interpreter','latex', 'FontSize', 18);
title('$\Psi_{U/\lambda U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

% FIGURE Min/Max PDF Psi over lambda
figure(fig_count);
fig_count = fig_count + 1;
set(gcf,'OuterPosition',[100,100,1000,800]);
factors = min(phi1,phi2)./max(phi1,phi2);
factors = factors(factors >= 0 & factors <= 1);
plot(linspace(0,1,length(factors)),sort(factors));
title('Min/Max $\Psi_{U/\lambda U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);


end