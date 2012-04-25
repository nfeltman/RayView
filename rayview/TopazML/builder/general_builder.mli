open Bvh2;;
open Build_triangle;;

type build_parameters = { leaf_size : int }

module type B = 
	sig
		type uniform_data
		type tree
		val build_bvh : build_parameters -> bTri array -> uniform_data -> tree
	end

module Builder (CE : Cost_evaluator.CostEvaluator) (NC : Node_constructor.NodeFactory) : (B with type uniform_data = CE.uniform_data)