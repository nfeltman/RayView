open ArrayUtil;;
open Triangle_aggregator;;
open Cost_evaluator;;
open Build_triangle;;
open Cost_evaluator;;

type 'a best_partition = {left_tris : bTri list; right_tris : bTri list; left_aggregate : agg; right_aggregate : agg; build_data : 'a eval_result}

val perform_best_partition : 'a evaluator -> bTri list -> 'a best_partition