open ArrayUtil;;
open Vectors;;
open Triangle_aggregator;;
open Box3;;
open Interval;;

exception SplitFromEmptyAggError

module SplitSeries =
struct
	type dim3 = X | Y | Z
	type extraParams = dim3
	type series = { less : float; times : float; dim : dim3 }
	let makeSeries dim agg numBins =
		let box = match agg.box with
			| Box3.NotEmpty(ne) -> ne
			| Box3.Empty -> raise SplitFromEmptyAggError
		in match dim with
		| X -> { less = box.bx.min ; times = (float_of_int numBins) /. Interval.ne_length box.bx; dim = X }
		| Y -> { less = box.by.min ; times = (float_of_int numBins) /. Interval.ne_length box.by; dim = Y }
		| Z -> { less = box.bz.min ; times = (float_of_int numBins) /. Interval.ne_length box.bz; dim = Z }
	
	let getBucket series p = let v = match series.dim with
			| X -> p.x
			| Y -> p.y
			| Z -> p.z
		in int_of_float ((v -. series.less) *. series.times)
	
	let getFilter series threshold = match series.dim with
		| X -> fun p -> int_of_float (((Build_triangle.getCenter p).x -. series.less) *. series.times) < threshold
		| Y -> fun p -> int_of_float (((Build_triangle.getCenter p).y -. series.less) *. series.times) < threshold
		| Z -> fun p -> int_of_float (((Build_triangle.getCenter p).z -. series.less) *. series.times) < threshold
end