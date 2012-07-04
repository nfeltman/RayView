open BuildTriangle;;
open BoxCountAgg.A;;
open BoxCountAgg;;
open Box3;;

module F =
struct
	type leafType = ne_box3 * bTri array
	type branchType = ne_box3
	type branchBuildData = unit
	
	let makeLeaf list =
		let refs = Array.of_list list in
		Trees.Leaf(calcBoundMapList getTriangle list, refs);;
	
	let makeBranch left right kernel ne_agg = Trees.Branch(ne_agg.box, left, right)
	
end