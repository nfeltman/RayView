open Build_triangle;;

type agg = {box : Box3.box3; count : int}
val combine : agg -> agg -> agg
val add_triangle : bTri -> agg -> agg
val roll : ArrayUtil.range -> bTri array -> agg
val defaultAgg : agg