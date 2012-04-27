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
						if agg == defaultAgg then
							Skip(leftAcc)
						else
							let rightAcc, filter = backwardAccumulator.(index +1), getFilter splitSeries (index +1) in
							let res = eval leftAcc rightAcc filter in
							NoSkip((res.cost, (filter, leftAcc, rightAcc, res)), leftAcc)
			end defaultAgg bins in
	if (bestLeft == defaultAgg || bestRight == defaultAgg)	then Degen
	else NotDegen ({ filt = bestFilter ; lAgg = bestLeft ; rAgg = bestRight ; res = bestRes })

exception DegenerateError

let perform_best_partition eval range tris wholeAgg =
	let binCount = max 32 (wholeAgg.count / 20 + 4) in
	let xSeries, ySeries, zSeries = makeSeries X wholeAgg binCount, makeSeries Y wholeAgg binCount, makeSeries Z wholeAgg binCount in
	let xBins, yBins, zBins = Array.make binCount defaultAgg, Array.make binCount defaultAgg, Array.make binCount defaultAgg in
	iterRange (fun bTri ->
					let center = getCenter bTri in
					let x, y, z = getBucket xSeries center, getBucket ySeries center, getBucket zSeries center in
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
	{ pivot_index = pivot ; left_aggregate = bestResults.lAgg; right_aggregate = bestResults.rAgg; build_data = bestResults.res }