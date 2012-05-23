open ArrayUtil;;
open BoxCountAgg.A;;
open Cost_evaluator;;
open Build_triangle;;
open Cost_evaluator;;

type 'a best_partition = {left_tris : bTri list; right_tris : bTri list; left_aggregate : ne_agg; right_aggregate : ne_agg; build_data : 'a eval_result}

val perform_best_partition : 'a evaluator -> bTri list -> 'a best_partition