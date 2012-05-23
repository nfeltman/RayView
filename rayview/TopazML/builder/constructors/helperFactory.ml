open Build_triangle;;
open BoxCountAgg.A;;
open BoxCountAgg;;
open Box3;;

module F =
struct
	type leafType = Box3.ne_box3 * Build_triangle.bTri array
	type branchType = Box3.ne_box3
	type branchBuildData = unit
	
	let makeLeaf list =
		let refs = Array.of_list list in
		Trees.Leaf(Box3.calcBoundMapList getTriangle list, refs);;
	
	let makeBranch left right kernel ne_agg = Trees.Branch(ne_agg.box, left, right)
	
end