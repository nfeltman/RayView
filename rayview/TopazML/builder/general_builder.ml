open Build_triangle;;

type build_parameters = { leaf_size : int}

module type B = 
sig
	val build_bvh : build_parameters -> bTri array -> uniform_data -> refBVH
end

module Builder = functor (CE : Cost_evaluator.CostEvaluator) ->
	struct
let build_bvh params tris unif = 
	let rec buildNode range total_agg trans_in = 
		let numTris = ArrayUtil.rangeSize range in
		if numTris < params.leaf_size then
			Node_constructor.makeLeaf tris range
		else
			let newState = CE.begin_evaluations range total_agg unif trans_in in 
			let (buildData, leftRange, rightRange, leftAgg, rightAgg) = 
				if is_completely_degenerate range tris then
					let midpoint = ArrayUtil.rangeMidpoint range in
					let (leftRange, rightRange) = ArrayUtil.split_range range midpoint in
					let (leftAgg, rightAgg) = (Triangle_aggregator.roll leftRange tris), (Triangle_aggregator.roll rightRange tris) in
					(CE.evaluate_split newState leftAgg rightAgg (fun bt -> bt.build_index < midpoint)), leftRange, rightRange, leftAgg, rightAgg
				else
					let bestP = Splitter.perform_best_partition tris range in
					let (leftRange, rightRange) = ArrayUtil.split_range range bestP.pivot_index in
					(bestP.build_data, leftRange, rightRange, bestP.left_aggregate, bestP.right_aggregate)
			in
			let report = CE.finish_evaluations buildData unif newState in
			if buildData.build_left_first then
				let leftChild = buildNode leftRange leftAgg report.left_transition in
				let rightChild = buildNode rightRange rightAgg report.right_transition in
				Node_constructor.makeBranch leftChild rightChild report.build_info
			else
				let rightChild = buildNode rightRange rightAgg report.right_transition in
				let leftChild = buildNode leftRange leftAgg report.left_transition in
				Node_constructor.makeBranch leftChild rightChild report.build_info
				
	in buildNode (ArrayUtil.entireRange tris) (CE.get_initial_transition unif)
	end