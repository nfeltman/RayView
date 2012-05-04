open Triangle_aggregator;;
open ArrayUtil;;

type 'mem eval_result = { cost : float; memo : 'mem }
type ('tran, 'kern) finished_report = { build_info : 'kern; left_transition : 'tran; right_transition : 'tran; build_left_first : bool }
type 'mem evaluator = agg -> agg -> Build_triangle.bTri left_filter -> 'mem eval_result
	
module type CostEvaluator = sig

	type uniform_data
	type transition_data
	type stack_data
	type memo_data
	type kernel_data
	
	val begin_evaluations : agg -> uniform_data -> transition_data -> stack_data
	val evaluate_split : stack_data -> memo_data evaluator
	val finish_evaluations : memo_data eval_result -> uniform_data -> stack_data -> (transition_data, kernel_data) finished_report

end