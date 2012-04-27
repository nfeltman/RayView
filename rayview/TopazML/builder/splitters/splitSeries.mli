open ArrayUtil;;
open Vectors;;

module SplitSeries :
sig
	type series
	type dim3 = X | Y | Z
	type extraParams = dim3
	val makeSeries : extraParams -> Box3.ne_box3 -> int -> series
	val getBucket : series -> vec3 -> int
	val getFilter : series -> int -> Build_triangle.bTri left_filter
end