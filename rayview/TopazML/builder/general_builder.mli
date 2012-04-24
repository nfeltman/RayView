open Bvh2;;

type build_parameters = { leaf_size : int}

module type B = 
sig
	val build_bvh : build_parameters -> bTri array -> uniform_data -> refBVH
end