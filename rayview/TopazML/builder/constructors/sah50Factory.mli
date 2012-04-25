open Trees;;

module F :
sig
	type leafType = int array
	type branchType = Kernels.kernelType
	type branchBuildData = Kernels.kernelType
	
	val makeLeaf : Build_triangle.bTri array -> ArrayUtil.range -> (branchType, leafType) tree
	val makeBranch : (branchType, leafType) tree -> (branchType, leafType) tree -> branchBuildData -> (branchType, leafType) tree
end