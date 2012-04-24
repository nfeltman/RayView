open ArrayUtil;;
open Triangle_aggregator;;

type best_partition = {pivot_index : int; left_aggregate : agg; right_aggregate : agg; build_data : Cost_evaluator.eval_result}

val perform_best_partition : bTri array -> range -> best_partition