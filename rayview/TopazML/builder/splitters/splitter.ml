open ArrayUtil;;
open BoxCountAgg.A;;
open Cost_evaluator;;
open Build_triangle;;
open Box3;;
open SplitSeries.SplitSeries;;

type 'a best_partition = { left_tris : bTri list; right_tris : bTri list; left_aggregate : ne_agg; right_aggregate : ne_agg; build_data : 'a eval_result }
type 'a nd_scoreResult = { filt : bTri left_filter ; lAgg : ne_agg ; rAgg : ne_agg ; res : 'a eval_result }
type 'a scoreResult = Degen | NotDegen of 'a nd_scoreResult

(* Paula Dean would prefer I call this function pickButter *)
let pickBetter sRes1 sRes2 =
	match sRes1 with
	| NotDegen(nd1) -> (match sRes2 with
				| NotDegen(nd2) -> if nd1.res.cost < nd2.res.cost then sRes1 else sRes2
				| Degen -> sRes1)
	| Degen -> sRes2

exception AllBinsEmpty

let scoreSeries bins splitSeries eval =
	let calcScore index leftAcc rightAcc =
		begin
			let filter = getFilter splitSeries (index +1) in
			let res = eval (NonEmptyAgg leftAcc) (NonEmptyAgg rightAcc) filter in
			{ filt = filter ; lAgg = leftAcc ; rAgg = rightAcc ; res = res }, res.cost
		end in
	let _, reversedNonEmptyList = Array.fold_left
			(fun (i, l) agg -> match agg with
						| EmptyAgg -> i +1, l
						| NonEmptyAgg(ne) -> i +1, (i, ne) :: l ) (0, []) bins
	in
	match reversedNonEmptyList with
	| [] -> raise AllBinsEmpty
	| (_, ne):: rest ->
			let _, backwardsAcc = List.fold_left (fun (backAcc, accList) (i, agg) -> (ne_combine backAcc agg, (i, agg, backAcc):: accList)) (ne,[]) rest in
			match backwardsAcc with
			| [] -> Degen
			| (firstIndex, agg, backAcc):: rest ->
					let (_,(bestAnswer, _)) =	List.fold_left
							(fun (forAcc, (bestRes, bestCost)) (i, agg, backAcc) ->
										let (res, cost) = calcScore i forAcc backAcc in	(ne_combine forAcc agg), if cost < bestCost then (res, cost) else (bestRes, bestCost))
							(agg, (calcScore firstIndex agg backAcc)) rest
					in NotDegen(bestAnswer)
;;

exception DegenerateError
exception BadPartition of string

let perform_best_partition eval tris =
	let binCount = min 32 ((List.length tris) / 20 + 4) in
	let centroidBounds = Box3.calcPointBoundMapList getCenter tris in
	let xSeries, ySeries, zSeries =
		makeSeries X centroidBounds binCount,
		makeSeries Y centroidBounds binCount,
		makeSeries Z centroidBounds binCount in
	let xBins, yBins, zBins = Array.make binCount EmptyAgg, Array.make binCount EmptyAgg, Array.make binCount EmptyAgg in
	List.iter (fun bTri ->
					let center = getCenter bTri in
					let x, y, z =
						max 0 (min (binCount - 1) (getBucket xSeries center)),
						max 0 (min (binCount - 1) (getBucket ySeries center)),
						max 0 (min (binCount - 1) (getBucket zSeries center)) in
					(* Printf.printf "%i %i %i of %i \n" x y z binCount; *)
					xBins.(x) <- add_triangle bTri xBins.(x);
					yBins.(y) <- add_triangle bTri yBins.(y);
					zBins.(z) <- add_triangle bTri zBins.(z)
		) tris;
	let bestX, bestY, bestZ = scoreSeries xBins xSeries eval, scoreSeries yBins ySeries eval, scoreSeries zBins zSeries eval in
	let bestResults = match pickBetter (pickBetter bestX bestY) bestZ with
		| NotDegen(nd) -> nd
		| Degen -> raise DegenerateError
	in
	let leftTris, rightTris = List.partition bestResults.filt tris in
	{ left_tris = leftTris; right_tris = rightTris; left_aggregate = bestResults.lAgg; right_aggregate = bestResults.rAgg; build_data = bestResults.res }