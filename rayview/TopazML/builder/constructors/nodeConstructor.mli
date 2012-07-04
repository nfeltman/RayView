open Trees;;
open BoxCountAgg.A;;

module type NodeFactory =
sig
	type leafType
	type branchType
	type branchBuildData
	
	val makeLeaf : BuildTriangle.bTri list -> (branchType, leafType) tree
	val makeBranch : (branchType, leafType) tree -> (branchType, leafType) tree -> branchBuildData -> ne_agg -> (branchType, leafType) tree
end