open Trees;;
open Node_constructor;;

module F : NodeFactory 
with type leafType = int array
with type branchType = Kernels.kernelType
with type branchBuildData = unit
(*
sig
	type leafType = int array
	type branchType = Kernels.kernelType
	type branchBuildData = unit
	
	val makeLeaf : Build_triangle.bTri array -> ArrayUtil.range -> (branchType, leafType) tree
	val makeBranch : (branchType, leafType) tree -> (branchType, leafType) tree -> branchBuildData -> (branchType, leafType) tree
end

*)