open Box3;;

val readRefBVH_text : in_channel -> Bvh2.refBVH
val branchMapFold : Kernels.kernelType -> ne_box3 -> ne_box3 -> Bvh2.bvhBranch * ne_box3 
val leafMapFold : ('a -> Triangle.triangle) -> 'a array -> Bvh2.bvhLeaf * ne_box3 