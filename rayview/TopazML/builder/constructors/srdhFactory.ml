open BuildTriangle;;

module F =
struct
	type leafType = int array
	type branchType = Kernels.kernelType
	type branchBuildData = Kernels.kernelType
	
	let makeLeaf list = Trees.Leaf(Array.of_list (List.map getObjIndex list))
	
	let makeBranch left right kernel agg = Trees.Branch(kernel, left, right)
end