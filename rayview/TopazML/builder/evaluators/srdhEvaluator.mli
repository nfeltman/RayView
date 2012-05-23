open Cost_evaluator;;
open RayCompiler;;

module E : CostEvaluator
with type uniform_data = unit
with type kernel_data = Kernels.kernelType

val getInitialTransition : ray_compile_output -> E.transition_data