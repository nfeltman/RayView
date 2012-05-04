open Trees;;
open Node_constructor;;

module F : NodeFactory
with type leafType = Box3.ne_box3 * Build_triangle.bTri array
with type branchType = Box3.ne_box3
with type branchBuildData = unit