open Trees;;
open BoxCountAgg.A;;

module type NodeFactory =
sig
	type leafType
	type branchType
	type branchBuildData
	
	val makeLeaf : Build_triangle.bTri list -> (branchType, leafType) tree
	val makeBranch : (branchType, leafType) tree -> (branchType, leafType) tree -> branchBuildData -> ne_agg -> (branchType, leafType) tree
end