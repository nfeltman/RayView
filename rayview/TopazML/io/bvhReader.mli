open Box3;;

val readRefBVH : in_channel -> Bvh2.refBVH
val branchMapFold : Kernels.kernelType -> ne_box3 -> ne_box3 -> Bvh2.bvhBranch * ne_box3 