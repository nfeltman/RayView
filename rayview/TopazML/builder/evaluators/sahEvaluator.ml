open Triangle_aggregator;;
open ArrayUtil;;
open Build_triangle;;
open Cost_evaluator;;


module E =
struct
	type uniform_data = unit
	type transition_data = unit
	type stack_data = unit
	type memo_data = unit
	type kernel_data = Kernels.kernelType
	
	let get_initial_transition unif = ()
	let begin_evaluations range total_agg unif trans = ()
	let evaluate_split stack leftAgg rightAgg filter =
		let sah_cost = (Box3.surfaceArea leftAgg.box) *. (float_of_int leftAgg.count) +. (Box3.surfaceArea rightAgg.box) *. (float_of_int rightAgg.count) in
		{ cost = sah_cost; memo = (); build_left_first = true }
	let finish_evaluations res unif stack = { build_info = Kernels.UniformRandom ; left_transition = (); right_transition = () }
end