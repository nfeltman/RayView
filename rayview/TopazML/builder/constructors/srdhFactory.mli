

module F : Node_constructor.NodeFactory 
with type leafType = int array
with type branchType = Kernels.kernelType
with type branchBuildData = Kernels.kernelType