function Analyze_Full_TU_vs_PU_nu( d )

depths = d(:,1);
max_depth = max(depths);
colors = hsv(max_depth);

figure('OuterPosition',[100,100,1000,800]);
figure(1);
for k = 1:max_depth,
    f = d(depths==k,:);
    loglog(f(:,2),f(:,3).*f(:,4),'.','Color',colors(k,:));
    hold on
end
hold off
xlabel('$\tilde{T}_U(b)$','Interpreter','latex', 'FontSize', 18);
ylabel('$P_U(b)\cdot \nu(b)$','Interpreter','latex', 'FontSize', 18);
title('$\tilde{T}_U(b)$ vs $P_U(b)\cdot \nu(b)$','Interpreter','latex', 'FontSize', 18);

figure('OuterPosition',[100,100,1000,800]);
figure(2);
for k = 1:max_depth,
    f = d(depths==k,:);
    loglog(f(:,2),(f(:,3)-1).*f(:,4),'.','Color',colors(k,:));
    hold on
end
hold off
xlabel('$\tilde{T}_U(b)$','Interpreter','latex', 'FontSize', 18);
ylabel('$P_U(b)\cdot \mu(b)$','Interpreter','latex', 'FontSize', 18);
title('$\tilde{T}_U(b)$ vs $P_U(b)\cdot \mu(b)$','Interpreter','latex', 'FontSize', 18);

figure('OuterPosition',[100,100,1000,800]);
figure(3);
hold on
for k = 1:max_depth,
    f = d(depths==k,:);
    plot(f(:,5)./(f(:,6).*f(:,7)),f(:,8)./(f(:,9).*f(:,10)),'.','Color',colors(k,:));
end
hold off
xlabel('$\Psi_{U/\nu U}(left)$','Interpreter','latex', 'FontSize', 18);
ylabel('$\Psi_{U/\nu U}(right)$','Interpreter','latex', 'FontSize', 18);
title('$\Psi_{U/\nu U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

figure('OuterPosition',[100,100,1000,800]);
figure(4);
hold on
for k = 1:max_depth,
    f = d(depths==k,:);
    plot(f(:,5)./((f(:,6)-1).*f(:,7)),f(:,8)./((f(:,9)-1).*f(:,10)),'.','Color',colors(k,:));
end
hold off
xlabel('$\Psi_{U/\mu U}(left)$','Interpreter','latex', 'FontSize', 18);
ylabel('$\Psi_{U/\mu U}(right)$','Interpreter','latex', 'FontSize', 18);
title('$\Psi_{U/\mu U}$: Left vs Right','Interpreter','latex', 'FontSize', 18);

end