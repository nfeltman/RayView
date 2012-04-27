open ArrayUtil;;
open Triangle_aggregator;;
open Cost_evaluator;;
open Build_triangle;;
open Box3;;
open SplitSeries.SplitSeries;;

type 'a best_partition = { pivot_index : int; left_aggregate : agg; right_aggregate : agg; build_data : 'a eval_result }
type 'a nd_scoreResult = { filt : bTri left_filter ; lAgg : agg ; rAgg : agg ; res : 'a eval_result }
type 'a scoreResult = Degen | NotDegen of 'a nd_scoreResult

(* Paula Dean would prefer I call this function pickButter *)
let pickBetter sRes1 sRes2 =
	match sRes1 with
	| NotDegen(nd1) -> (match sRes2 with
				| NotDegen(nd2) -> if nd1.res.cost < nd2.res.cost then sRes1 else sRes2
				| Degen -> sRes1)
	| Degen -> sRes2

let scoreSeries bins splitSeries eval =
	let n = (Array.length bins) in
	let backwardAccumulator = Array.make n defaultAgg in
	let _ = Array.fold_right
			(fun agg (acc, i) ->
						let nextAcc = combine agg acc in
						backwardAccumulator.(i) <- nextAcc;
						(nextAcc, i - 1)) bins (defaultAgg, n - 1) in
	let (_, (bestFilter, bestLeft, bestRight, bestRes), _) = ArrayUtil.pickMinFoldLeft
			begin fun index agg forwardAccumulator ->
						let leftAcc = combine agg forwardAccumulator in
						if false && agg.count == 0 then
							Skip(leftAcc)
						else
							let rightAcc, filter = backwardAccumulator.(index +1), getFilter splitSeries (index +1) in
							let res = eval leftAcc rightAcc filter in
							(* Printf.printf "index: %i  (%i/%i) cost: %f \n" index leftAcc.count rightAcc.count res.cost; *)
							NoSkip((res.cost, (filter, leftAcc, rightAcc, res)), leftAcc)
			end defaultAgg (0, n-1) bins in
	if (bestLeft.count == 0 || bestRight.count == 0)	then Degen
	else NotDegen ({ filt = bestFilter ; lAgg = bestLeft ; rAgg = bestRight ; res = bestRes })

exception DegenerateError
exception BadPartition of string

let perform_best_partition eval range tris =
	let binCount = min 32 ((rangeSize range) / 20 + 4) in
	let centroidBounds = Box3.calcPointBoundMap (fun bTri -> getCenter bTri) tris range in
	let xSeries, ySeries, zSeries = 
		makeSeries X centroidBounds binCount, 
		makeSeries Y centroidBounds binCount, 
		makeSeries Z centroidBounds binCount in
	let xBins, yBins, zBins = Array.make binCount defaultAgg, Array.make binCount defaultAgg, Array.make binCount defaultAgg in
	iterRange (fun bTri ->
					let center = getCenter bTri in
					let x, y, z = 
						max 0 (min (binCount - 1) (getBucket xSeries center)), 
						max 0 (min (binCount - 1) (getBucket ySeries center)),
						max 0 (min (binCount - 1) (getBucket zSeries center)) in
					(* Printf.printf "%i %i %i of %i \n" x y z binCount; *)
					xBins.(x) <- add_triangle bTri xBins.(x);
					yBins.(y) <- add_triangle bTri yBins.(y);
					zBins.(z) <- add_triangle bTri zBins.(z)
		) range tris;
	let bestX, bestY, bestZ = scoreSeries xBins xSeries eval, scoreSeries yBins ySeries eval, scoreSeries zBins zSeries eval in
	let bestResults = match pickBetter (pickBetter bestX bestY) bestZ with
		| NotDegen(nd) -> nd
		| Degen -> raise DegenerateError
	in
	let pivot = ArrayUtil.smartPartition bestResults.filt (fun tri newIndex -> (getBuildIndex tri) := newIndex) range tris in
	(* let count = ref 0 in ArrayUtil.iterRange (fun tri -> if bestResults.filt tri then incr count) range tris;
	if !count <> (pivot - (fst range)) then raise (BadPartition "waaaaat"); *)
	(* Printf.printf "numbins: %i  chose: %f  count: %i  %i  pivot: %i \n" binCount bestResults.res.cost !count (pivot - (fst range)) pivot; *)
	if pivot <= 0 || pivot >= (snd range) then raise (BadPartition (Printf.sprintf "Bad partition %i" pivot)) else
	{ pivot_index = pivot ; left_aggregate = bestResults.lAgg; right_aggregate = bestResults.rAgg; build_data = bestResults.res }
	
	