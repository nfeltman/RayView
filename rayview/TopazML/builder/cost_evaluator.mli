open Triangle_aggregator;;
open ArrayUtil;;
open Build_triangle;;

module type CostEvaluator = sig

	type uniform_data
	type transition_data
	type stack_data
	type memo_data
	type kernel_data = Kernels.kernelType
	
	type eval_result = { cost : float; memo : memo_data; build_left_first : bool }
	type finished_report = { build_info : kernel_data; left_transition : transition_data; right_transition : transition_data }
	
	val get_initial_transition : uniform_data -> transition_data
	val begin_evaluations : range -> agg -> uniform_data -> transition_data -> stack_data
	val evaluate_split : stack_data -> agg -> agg -> 'a left_filter -> eval_result
	val finish_evaluations : eval_result -> uniform_data -> stack_data -> finished_report

end