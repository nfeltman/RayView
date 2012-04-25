open Trees;;

module type NodeFactory =
sig
	type leafType
	type branchType
	type branchBuildData = Kernels.kernelType
	
	val makeLeaf : Build_triangle.bTri array -> ArrayUtil.range -> (branchType, leafType) tree
	val makeBranch : (branchType, leafType) tree -> (branchType, leafType) tree -> branchBuildData -> (branchType, leafType) tree
end