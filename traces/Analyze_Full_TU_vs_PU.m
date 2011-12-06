function Analyze_Full_TU_vs_PU( d )

depths = d(:,1);
P_U = d(:,4);
T_U = d(:,2);

left_nu = d(:,6);
left_P_U = d(:,7);
left_T_U = d(:,5);

right_nu = d(:,9);
right_P_U = d(:,10);
right_T_U = d(:,8);

set(0,'DefaultFigureWindowStyle','docked') 
MuNuLambda_Modulator(T_U,P_U,left_nu,left_T_U,left_P_U,right_nu,right_T_U,right_P_U,depths);
end

function MuNuLambda_Modulator(T,P,left_nu,left_T,left_P,right_nu,right_T,right_P,depths)
nu = left_nu + right_nu;
PredictorAccuracyGraphs(T,nu.*P,left_T./(left_nu.*left_P),right_T./(right_nu.*right_P),'\nu',depths);
PredictorAccuracyGraphs(T,(nu-1).*P,left_T./((left_nu-1).*left_P),right_T./((right_nu-1).*right_P),'\mu',depths);
PredictorAccuracyGraphs(T,log(nu).*P,left_T./(log(left_nu).*left_P),right_T./(log(right_nu).*right_P),'\lambda',depths);
end

function PredictorAccuracyGraphs(actual,predictor,phi1,phi2,name,depths)

% FIGURE ~T vs P*nu
figure();
PlotMultiColorLoglog(actual,predictor,depths);
xlabel('$\tilde{T}_U(b)$','Interpreter','latex', 'FontSize', 18);
ylabel(sprintf('$P_U(b)\\cdot %s(b)$',name),'Interpreter','latex', 'FontSize', 18);
title(sprintf('$\\tilde{T}_U(b)$ vs $P_U(b)\\cdot %s(b)$',name),'Interpreter','latex', 'FontSize', 18);

% FIGURE Left/Right Psi over nu
figure();
PlotMultiColor(phi1,phi2,depths);
xlabel(sprintf('$\\Psi_{U/%s U}(left)$',name),'Interpreter','latex', 'FontSize', 18);
ylabel(sprintf('$\\Psi_{U/%s U}(right)$',name),'Interpreter','latex', 'FontSize', 18);
title(sprintf('$\\Psi_{U/%s U}$: Left vs Right',name),'Interpreter','latex', 'FontSize', 18);

% FIGURE Min/Max PDF Psi over nu
figure();
factors = min(phi1,phi2)./max(phi1,phi2);
factors = factors(factors >= 0 & factors <= 1);
plot(linspace(0,1,length(factors)),sort(factors));
title(sprintf('Min/Max $\\Psi_{U/%s U}$: Left vs Right',name),'Interpreter','latex', 'FontSize', 18);

end

function PlotMultiColor(x, y, depths)
    max_depth = max(depths);
    colors = hsv(max_depth);
    for k = 1:max_depth,
        plot(x(depths==k,:),y(depths==k,:),'.','Color',colors(k,:));
        hold on
    end
    hold off
end

function PlotMultiColorLoglog(x, y, depths)
    max_depth = max(depths);
    colors = hsv(max_depth);
    for k = 1:max_depth,
        loglog(x(depths==k,:),y(depths==k,:),'.','Color',colors(k,:));
        hold on
    end
    hold off
end