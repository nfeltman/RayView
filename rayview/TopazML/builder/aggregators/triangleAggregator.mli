open Build_triangle;;

module type Aggregator = sig
	type ne_agg
	type agg = EmptyAgg | NonEmptyAgg of ne_agg
	val combine : agg -> agg -> agg
	val ne_combine : ne_agg -> ne_agg -> ne_agg
	val add_triangle : bTri -> agg -> agg
	val roll : ArrayUtil.range -> bTri array -> ne_agg
	val rollList : bTri list -> ne_agg
end