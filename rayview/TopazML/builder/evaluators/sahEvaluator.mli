open TriangleAggregator;;
open ArrayUtil;;
open BuildTriangle;;
open CostEvaluator;;

module E : CostEvaluator
with type uniform_data = unit
with type transition_data = unit
with type kernel_data = unit
