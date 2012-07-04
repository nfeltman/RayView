open BoxCountAgg.A;;
open BoxCountAgg;;
open ArrayUtil;;
open BuildTriangle;;
open CostEvaluator;;


module E =
struct
	type uniform_data = unit
	type transition_data = unit
	type stack_data = unit
	type memo_data = unit
	type kernel_data = unit
	
	let get_initial_transition unif = ()
	
	let begin_evaluations total_agg unif trans = ()
	
	let evaluate_split stack leftAgg rightAgg filter =
		let calcHalf agg = match agg with
			| EmptyAgg -> 0.0
			| NonEmptyAgg(ne) -> (Box3.ne_surfaceArea ne.box) *. (float_of_int ne.count)
		in
		{ cost = (calcHalf leftAgg) +. (calcHalf rightAgg); memo = () }
		
	let finish_evaluations res unif stack = { build_info = () ; left_transition = (); right_transition = (); build_left_first = true }
end