type kernelType = UniformRandom | LeftFirst | RightFirst | FrontToBack | BackToFront;;
type bvhBranch = { bounds : ne_box3; kernel : kernelType }
type bvhLeaf = { bounds : ne_box3 }
type bvh = (bvhBranch, bvhLeaf) tree

val measureCost : bvh -> seg3 -> bool * float * float