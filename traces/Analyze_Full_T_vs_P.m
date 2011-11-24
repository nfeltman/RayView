function Analyze_Full_T_vs_P( d )

depths = d(:,1);
P_U = d(:,2);
P_R = d(:,3);

left_nu = d(:,4);
left_P_U = d(:,5);
left_T_U = d(:,6);
left_P_R = d(:,7);
left_T_R = d(:,8);

right_nu = d(:,9);
right_P_U = d(:,10);
right_T_U = d(:,11);
right_P_R = d(:,12);
right_T_R = d(:,13);

nu = left_nu + right_nu;
T_U = P_U + left_T_U + right_T_U;
T_R = P_R + left_T_R + right_T_R;

set(0,'DefaultFigureWindowStyle','docked') 
MuNuLambda_Modulator(T_U,P_U,left_nu,left_T_U,left_P_U,right_nu,right_T_U,right_P_U,depths,'U','U','UU');
MuNuLambda_Modulator(T_R,P_R,left_nu,left_T_R,left_P_R,right_nu,right_T_R,right_P_R,depths,'R','R','RR');
MuNuLambda_Modulator(T_R,P_U,left_nu,left_T_R,left_P_U,right_nu,right_T_R,right_P_U,depths,'R','U','RU');
figure('Name','T_U vs T_R');
PlotMultiColorLoglog(T_U,T_R,depths);
figure('Name','P_U vs P_R');
PlotMultiColorLoglog(P_U,P_R,depths);
end

function MuNuLambda_Modulator(T,P,left_nu,left_T,left_P,right_nu,right_T,right_P,depths,T_char,E_char,label)
nu = left_nu + right_nu;
PredictorAccuracyGraphs(T,nu.*P,left_T./(left_nu.*left_P),right_T./(right_nu.*right_P),T_char,E_char,'\nu',sprintf('%s-N',label),depths);
PredictorAccuracyGraphs(T,(nu-1).*P,left_T./((left_nu-1).*left_P),right_T./((right_nu-1).*right_P),T_char,E_char,'\mu',sprintf('%s-M',label),depths);
PredictorAccuracyGraphs(T,log(nu).*P,left_T./(log(left_nu).*left_P),right_T./(log(right_nu).*right_P),T_char,E_char,'\lambda',sprintf('%s-L',label),depths);
end

function PredictorAccuracyGraphs(actual,predictor,phi1,phi2,T_char,E_char,gl,label,depths)

% FIGURE T vs P*gl
figure('Name',sprintf('%s Corr',label),'NumberTitle','off');
PlotMultiColorLoglog(actual,predictor,depths);
xlabel(sprintf('$T_%s(b)$',T_char),'Interpreter','latex', 'FontSize', 18);
ylabel(sprintf('$P_%s(b)\\cdot %s(b)$',E_char,gl),'Interpreter','latex', 'FontSize', 18);
title(sprintf('$T_%s(b)$ vs $P_%s(b)\\cdot %s(b)$',T_char,E_char,gl),'Interpreter','latex', 'FontSize', 18);

% FIGURE Left/Right Psi
figure('Name',sprintf('%s LR',label),'NumberTitle','off');
PlotMultiColorLoglog(phi1,phi2,depths);
xlabel(sprintf('$\\Psi_{%s/%s %s}(left)$',T_char,gl,E_char),'Interpreter','latex', 'FontSize', 18);
ylabel(sprintf('$\\Psi_{%s/%s %s}(right)$',T_char,gl,E_char),'Interpreter','latex', 'FontSize', 18);
title(sprintf('$\\Psi_{%s/%s %s}$: Left vs Right',T_char,gl,E_char),'Interpreter','latex', 'FontSize', 18);

% FIGURE MinPsi/MaxPsi CDF
figure('Name',sprintf('%s CDF',label),'NumberTitle','off');
factors = min(phi1,phi2)./max(phi1,phi2);
factors = factors(factors >= 0 & factors <= 1);
plot(linspace(0,1,length(factors)),sort(factors));
title(sprintf('Min/Max $\\Psi_{%s/%s %s}$: Left vs Right',T_char,gl,E_char),'Interpreter','latex', 'FontSize', 18);
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