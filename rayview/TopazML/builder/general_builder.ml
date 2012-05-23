open Build_triangle;;
open Bvh2;;
open ArrayUtil;;
open Splitter;;
open Cost_evaluator;;
open BoxCountAgg.A;;

type build_parameters = { leaf_size : int }

module type B =
sig
	type uniform_data
	type transition_data
	type tree
	val build_bvh : build_parameters -> bTri list -> uniform_data -> transition_data -> tree
end

exception BuildException of string

module Builder (CE : Cost_evaluator.CostEvaluator) (NC : Node_constructor.NodeFactory with type branchBuildData = CE.kernel_data) =
struct
	
	type uniform_data = CE.uniform_data
	type transition_data = CE.transition_data
	type tree = (NC.branchType, NC.leafType) Trees.tree
	
	let is_completely_degenerate first rest =
		let rec degen list center =
			match list with 
				| [] -> true
				| f::r -> if (getCenter f) <> center then false	else degen r center
		in degen rest (getCenter first)
	
	let rec sizeBoundedBy n l = 
		match l with
		| [] -> 0 <= n
		| _::t -> if n <= 0 then false else sizeBoundedBy (n-1) t
	
	
	let build_bvh params tris unif initialTransition =
		let rec buildNode tris total_agg trans_in =
			match tris with
			| [] ->	raise (BuildException "Somehow building a 0 size array.")
			| tris when sizeBoundedBy params.leaf_size tris -> NC.makeLeaf tris
			| first::rest ->
					let newState = CE.begin_evaluations total_agg unif trans_in in
					let buildData, leftAgg, rightAgg, leftTris, rightTris =
						if is_completely_degenerate first rest then
							let (leftHalf, rightHalf) = splitList tris in
							let (leftAgg, rightAgg) = (rollList leftHalf), (rollList rightHalf) in
							let midpoint = getBuildIndex (List.hd rightHalf) in
							(CE.evaluate_split newState (NonEmptyAgg leftAgg) (NonEmptyAgg rightAgg) (fun bt -> getBuildIndex bt < midpoint)), leftAgg, rightAgg, leftHalf, rightHalf
						else
							let bestP = Splitter.perform_best_partition (CE.evaluate_split newState) tris in
							bestP.build_data, bestP.left_aggregate, bestP.right_aggregate, bestP.left_tris, bestP.right_tris
					in
					let report = CE.finish_evaluations buildData unif newState in
					if report.build_left_first then
						let leftChild = buildNode leftTris leftAgg report.left_transition in
						let rightChild = buildNode rightTris rightAgg report.right_transition in
						NC.makeBranch leftChild rightChild report.build_info total_agg
					else
						let rightChild = buildNode rightTris rightAgg report.right_transition in
						let leftChild = buildNode leftTris leftAgg report.left_transition in
						NC.makeBranch leftChild rightChild report.build_info total_agg
		in
		buildNode tris (rollList tris) initialTransition
end