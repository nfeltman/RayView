open Build_triangle;;
open Bvh2;;
open ArrayUtil;;
open Splitter;;
open Cost_evaluator;;

type build_parameters = { leaf_size : int }

module type B =
sig
	type uniform_data
	type tree
	val build_bvh : build_parameters -> bTri array -> uniform_data -> tree
end

exception BuildException of string

module Builder (CE : Cost_evaluator.CostEvaluator) (NC : Node_constructor.NodeFactory) =
struct
	
	type uniform_data = CE.uniform_data
	type tree = (NC.branchType, NC.leafType) Trees.tree
	
	let is_completely_degenerate range tris =
		let (s0, e) = range in
		let rec degen s center =
			if s == e then true
			else if (getCenter tris.(s)) <> center then false
			else degen (s +1) center
		in degen s0 (getCenter tris.(s0))
	
	let build_bvh params tris unif =
		let rec buildNode range total_agg trans_in =
			let numTris = rangeSize range in
			if numTris == 0 then
				raise (BuildException "Somehow building a 0 size array.")
			else if numTris <= params.leaf_size then
				NC.makeLeaf tris range
			else
				let newState = CE.begin_evaluations range total_agg unif trans_in in
				let (buildData, leftRange, rightRange, leftAgg, rightAgg) =
					if is_completely_degenerate range tris then
						let midpoint = rangeMidpoint range in
						let (leftRange, rightRange) = split_range range midpoint in
						let (leftAgg, rightAgg) = (Triangle_aggregator.roll leftRange tris), (Triangle_aggregator.roll rightRange tris) in
						(CE.evaluate_split newState leftAgg rightAgg (fun bt -> !(getBuildIndex bt) < midpoint)), leftRange, rightRange, leftAgg, rightAgg
					else
						let bestP = Splitter.perform_best_partition (CE.evaluate_split newState) range tris in
						let (leftRange, rightRange) = split_range range bestP.pivot_index in
						(bestP.build_data, leftRange, rightRange, bestP.left_aggregate, bestP.right_aggregate)
				in
				let report = CE.finish_evaluations buildData unif newState in
				if buildData.build_left_first then
					let leftChild = buildNode leftRange leftAgg report.left_transition in
					let rightChild = buildNode rightRange rightAgg report.right_transition in
					NC.makeBranch leftChild rightChild report.build_info
				else
					let rightChild = buildNode rightRange rightAgg report.right_transition in
					let leftChild = buildNode leftRange leftAgg report.left_transition in
					NC.makeBranch leftChild rightChild report.build_info
		in
		let wholeRange = (entireRange tris) in
		buildNode wholeRange (Triangle_aggregator.roll wholeRange tris) (CE.get_initial_transition unif)
end