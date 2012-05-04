open Trees;;
open Triangle_aggregator;;

module type NodeFactory =
sig
	type leafType
	type branchType
	type branchBuildData
	
	val makeLeaf : Build_triangle.bTri list -> (branchType, leafType) tree
	val makeBranch : (branchType, leafType) tree -> (branchType, leafType) tree -> branchBuildData -> agg -> (branchType, leafType) tree
end