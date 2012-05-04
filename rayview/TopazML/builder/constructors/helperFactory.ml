open Build_triangle;;
open Triangle_aggregator;;
open Box3;;

module F =
struct
	type leafType = Box3.ne_box3 * Build_triangle.bTri array
	type branchType = Box3.ne_box3
	type branchBuildData = unit
	
	let makeLeaf list =
		let refs = Array.of_list list in
		Trees.Leaf(Box3.calcBoundMapList getTriangle list, refs);;
	
	exception BuildingFromEmptyAgg
	
	let makeBranch left right kernel agg = 
		match agg.box with
			| Empty -> raise BuildingFromEmptyAgg
			| NotEmpty(ne) -> Trees.Branch(ne, left, right);;
end