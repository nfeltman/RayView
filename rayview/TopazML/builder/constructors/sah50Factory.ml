open Build_triangle;;

module F =
struct
	type leafType = int array
	type branchType = Kernels.kernelType
	type branchBuildData = Kernels.kernelType
	
	let makeLeaf arr range =
		let refs = Array.make (ArrayUtil.rangeSize range) (-1) in
		let (s, e) = range in
		for i = s to e + 1 do
			refs.(i - s) <- getObjIndex arr.(i)
		done;
		Trees.Leaf(refs);;
	
	let makeBranch left right kernel = Trees.Branch(kernel, left, right)
end