open BuildTriangle;;

module F =
struct
	type leafType = int array
	type branchType = Kernels.kernelType
	type branchBuildData = unit
	
	let makeLeaf list = Trees.Leaf(Array.of_list (List.map getObjIndex list))
	
	let makeBranch left right kernel agg = Trees.Branch(Kernels.UniformRandom, left, right)
end