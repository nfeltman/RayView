open Triangle_aggregator;;
open ArrayUtil;;
open Build_triangle;;

module E : Cost_evaluator.CostEvaluator =
struct
	type uniform_data = unit
	type transition_data = unit
	type stack_data = unit
	type memo_data = unit
	type kernel_data = Kernels.kernelType
	
	type eval_result = { cost : float; memo : memo_data; build_left_first : bool }
	type finished_report = { build_info : kernel_data; left_transition : transition_data; right_transition : transition_data }
	
	let get_initial_transition unif = ()
	let begin_evaluations range total_agg unif trans = ()
	let evaluate_split stack leftAgg rightAgg filter = (Box3.surfaceArea leftAgg.box) *. (float_of_int leftAgg.count) +. (Box3.surfaceArea rightAgg.box) *. (float_of_int rightAgg.count)
	let finish_evaluations res unif stack = { build_info = Kernels.UniformRandom ; left_transition = (); right_transition = () }
end