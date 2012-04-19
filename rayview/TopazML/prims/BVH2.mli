
type bvhBranch = { boundsB : Box3.ne_box3; kernel : Kernels.kernelType }
type bvhLeaf = { boundsL : Box3.ne_box3 }
type bvh = (bvhBranch, bvhLeaf) Trees.tree
type refBVH = (Kernels.kernelType, int array) Trees.tree

val measureCost : bvh -> Vectors.seg3 -> bool * float * float