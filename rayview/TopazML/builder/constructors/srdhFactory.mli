

module F : NodeConstructor.NodeFactory 
with type leafType = int array
with type branchType = Kernels.kernelType
with type branchBuildData = Kernels.kernelType