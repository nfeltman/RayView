open Triangle_aggregator;;
open ArrayUtil;;
open Build_triangle;;
open Cost_evaluator;;

module E : CostEvaluator
with type uniform_data = unit
with type transition_data = unit
with type kernel_data = unit
