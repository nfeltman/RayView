type leafType = Bvh2.bvhLeaf
type branchType = Bvh2.bvhBranch
type branchBuildData = Kernels.kernelType

val makeLeaf : Build_triangle.bTri array -> ArrayUtil.range -> leafType
val makeBranch : (leafType branchType) tree -> (leafType branchType) tree -> branchBuildData -> branchType