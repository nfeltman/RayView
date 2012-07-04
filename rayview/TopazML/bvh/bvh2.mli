
type bvhBranch = { boundsB : Box3.ne_box3; kernel : Kernels.kernelType }
type bvhLeaf = { boundsL : Box3.ne_box3; prims : Triangle.triangle array }
type bvh = (bvhBranch, bvhLeaf) Trees.tree
type refBVH = (Kernels.kernelType, int array) Trees.tree
type tempBVH = (Box3.ne_box3, Box3.ne_box3 * BuildTriangle.bTri array) Trees.tree

type agreementReport = { th_mh : int ; th_mm : int ; tm_mh : int ; tm_mm : int }
type traversalCost = { spineCost : float; sideCost : float; missCost : float }

val measureCost : bvh -> Vectors.shadowQuery array -> traversalCost * agreementReport