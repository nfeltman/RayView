open ArrayUtil;;
open Vectors;;
open Box3;;
open Interval;;
open BuildTriangle;;

exception SplitFromEmptyAggError

module SplitSeries =
struct
	type dim3 = X | Y | Z
	type extraParams = dim3
	type series = { less : float; times : float; dim : dim3 }
	let makeSeries dim centroidBounds numBins =
		match dim with
		| X -> { less = centroidBounds.bx.min ; times = (float_of_int numBins) /. Interval.ne_length centroidBounds.bx; dim = X }
		| Y -> { less = centroidBounds.by.min ; times = (float_of_int numBins) /. Interval.ne_length centroidBounds.by; dim = Y }
		| Z -> { less = centroidBounds.bz.min ; times = (float_of_int numBins) /. Interval.ne_length centroidBounds.bz; dim = Z }
	
	let getBucket series p = let v = match series.dim with
			| X -> p.x
			| Y -> p.y
			| Z -> p.z
		in int_of_float ((v -. series.less) *. series.times)
	
	let getFilter series threshold = match series.dim with
		| X -> fun p -> int_of_float (((getCenter p).x -. series.less) *. series.times) < threshold
		| Y -> fun p -> int_of_float (((getCenter p).y -. series.less) *. series.times) < threshold
		| Z -> fun p -> int_of_float (((getCenter p).z -. series.less) *. series.times) < threshold
end