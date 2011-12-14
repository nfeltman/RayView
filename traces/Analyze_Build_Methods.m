function Analyze_Build_Methods( d )
% 1  | x 
% 2  | T_U(build[P_U*nu^x]) 
% 3  | T_U(build[P_U*mu^x]) 
% 4  | T_U(build[(P_B+10)*nu^x]) 
% 5  | T_U(build[(P_E+10)*nu^x]) 
% 6  | T_B(build[P_U*nu^x]) 
% 7  | T_B(build[P_U*mu^x]) 
% 8  | T_B(build[(P_B+10)*nu^x]) 
% 9  | T_B(build[(P_E+10)*nu^x]) 
% 10 | T_E(build[P_U*nu^x]) 
% 11 | T_E(build[P_U*mu^x]) 
% 12 | T_E(build[(P_B+10)*nu^x]) 
% 13 | T_E(build[(P_E+10)*nu^x])

figure();
plot(d(:,1),d(:,2:5));
xlabel('x');
ylabel('T_U(bvh)');
legend('P_U*nu^x','P_U*mu^x','(P_B+10)*nu^x','(P_E+10)*nu^x');

figure();
plot(d(:,1),d(:,6:9));
xlabel('x');
ylabel('T_B(bvh)');
legend('P_U*nu^x','P_U*mu^x','(P_B+10)*nu^x','(P_E+10)*nu^x');

figure();
plot(d(:,1),d(:,10:13));
xlabel('x');
ylabel('T_E(bvh)');
legend('P_U*nu^x','P_U*mu^x','(P_B+10)*nu^x','(P_E+10)*nu^x');

end