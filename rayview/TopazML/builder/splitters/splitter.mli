open ArrayUtil;;
open Triangle_aggregator;;
open Cost_evaluator;;
open Build_triangle;;

type 'a best_partition = {pivot_index : int; left_aggregate : agg; right_aggregate : agg; build_data : 'a eval_result}

val perform_best_partition : bTri array -> range -> 'a best_partition