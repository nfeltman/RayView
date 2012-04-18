
type bvhBranch = { boundsB : Box3.ne_box3; kernel : Kernels.kernelType }
type bvhLeaf = { boundsL : Box3.ne_box3 }
type bvh = (bvhBranch, bvhLeaf) Trees.tree

val measureCost : bvh -> Vectors.seg3 -> bool * float * float