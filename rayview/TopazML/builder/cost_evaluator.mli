open Triangle_aggregator;;
open ArrayUtil;;
open Build_triangle;;

type 'mem eval_result = { cost : float; memo : 'mem; build_left_first : bool }
type 'tran finished_report = { build_info : Kernels.kernelType; left_transition : 'tran; right_transition : 'tran }
	
module type CostEvaluator = sig

	type uniform_data
	type transition_data
	type stack_data
	type memo_data	
	
	val get_initial_transition : uniform_data -> transition_data
	val begin_evaluations : range -> agg -> uniform_data -> transition_data -> stack_data
	val evaluate_split : stack_data -> agg -> agg -> 'a left_filter -> memo_data eval_result
	val finish_evaluations : memo_data eval_result -> uniform_data -> stack_data -> transition_data finished_report

end