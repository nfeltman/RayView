open ArrayUtil;;
open Vectors;;
open Triangle_aggregator;;

module SplitSeries :
sig
	type series
	type dim3 = X | Y | Z
	type extraParams = dim3
	val makeSeries : extraParams -> agg -> int -> series
	val getBucket : series -> vec3 -> int
	val getFilter : series -> int -> Build_triangle.bTri left_filter
end