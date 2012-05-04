open Cost_evaluator;;
open Ray_compiler;;

module E : CostEvaluator
with type uniform_data = unit
with type kernel_data = Kernels.kernelType

val getInitialTransition : ray_compile_output -> E.transition_data