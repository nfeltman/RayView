open Trees;;
open NodeConstructor;;

module F : NodeFactory
with type leafType = Box3.ne_box3 * BuildTriangle.bTri array
with type branchType = Box3.ne_box3
with type branchBuildData = unit