open Build_triangle;;

type build_parameters = { leaf_size : int }

module type B = 
	sig
		type uniform_data
		type transition_data
		type tree
		val build_bvh : build_parameters -> bTri list -> uniform_data -> transition_data -> tree
	end

module Builder (CE : Cost_evaluator.CostEvaluator) (NC : Node_constructor.NodeFactory with type branchBuildData = CE.kernel_data) : (B 
with type uniform_data = CE.uniform_data 
with type transition_data = CE.transition_data
with type tree = (NC.branchType, NC.leafType) Trees.tree)