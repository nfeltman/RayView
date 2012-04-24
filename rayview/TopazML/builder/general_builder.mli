open Bvh2;;
open Build_triangle;;

type build_parameters = { leaf_size : int }

module type B =
functor (CE : Cost_evaluator.CostEvaluator) ->
	sig
		val build_bvh : build_parameters -> bTri array -> CE.uniform_data -> refBVH
	end

module Make (CE : Cost_evaluator.CostEvaluator) : B