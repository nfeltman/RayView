open Triangle_aggregator;;
open ArrayUtil;;
open Build_triangle;;
open Cost_evaluator;;

module E : 
sig
	type uniform_data = unit
	type transition_data = unit
	type stack_data = unit
	type memo_data = unit
	type kernel_data = Kernels.kernelType
	
	val get_initial_transition : unit -> unit
	val begin_evaluations : range -> agg -> unit -> unit -> stack_data
	val evaluate_split : unit -> agg -> agg -> 'a left_filter -> unit eval_result
	val finish_evaluations : unit eval_result -> uniform_data -> stack_data -> unit finished_report
end