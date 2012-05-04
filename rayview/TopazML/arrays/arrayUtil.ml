type range = int * int (* left-inclusive, right-exclusive *)

type 'a left_filter = 'a -> bool

let split_range (s, e) i = (s, i), (i, e)
let incrBottom (s, e) = (s +1, e)
let entireRange a = (0, Array.length a)
let rangeSize (s, e) = e - s
let rangeMidpoint (s, e) = (e + s) /2
let rangeIncludes (s, e) v = v >= s && v < e

let iterRange action (s, e) a =	for i = s to e - 1 do	action(Array.get a i) done

let partition filt (st, e) arr =
	let rec part s k =
		if s >= e then k
		else
			let temp = arr.(s) in
			if filt(temp) then
				(arr.(s) <- arr.(k); arr.(k) <- temp;
					part (s +1) (k +1))
			else part (s +1) k
	in part st st

let smartPartition filt notifier (st, e) arr =
	let rec part s k =
		if s >= e then k
		else
			let temp = arr.(s) in
			if filt(temp) then
				(arr.(s) <- arr.(k); arr.(k) <- temp; notifier temp k; notifier arr.(s) s;
					part (s +1) (k +1))
			else part (s +1) k
	in part st st

let pickMin objective arr =
	let e = Array.length arr in
	let rec findMin index bestCost bestEntry =
		if index >= e then
			(bestCost, bestEntry)
		else
			let (cost, entry) = objective arr.(index) in
			if cost < bestCost then
				findMin (index + 1) cost entry
			else
				findMin (index + 1) bestCost bestEntry
	in let (c0, e0) = objective arr.(0) in findMin 1 c0 e0

exception AllSkipsError

type ('retVal, 'foldVal) skipable = Skip of 'foldVal | NoSkip of 'retVal * 'foldVal

let pickMinFoldLeft objective seed (s, e) arr =
	let rec findMin index bestCost bestEntry foldVal =
		if index >= e then (bestCost, bestEntry, foldVal)
		else match objective index arr.(index) foldVal with
			| NoSkip((cost, entry), nextFold) ->
					if cost < bestCost then	findMin (index + 1) cost entry nextFold
					else findMin (index + 1) bestCost bestEntry nextFold
			| Skip(nextFold) -> findMin (index + 1) bestCost bestEntry nextFold
	in
	let rec findMinBootstrap index foldVal =
		if index >= e then raise AllSkipsError
		else match objective index arr.(index) foldVal with
			| NoSkip((cost, entry), nextFold) -> findMin (index + 1) cost entry nextFold
			| Skip(nextFold) -> findMinBootstrap (index + 1) nextFold
	in
	findMinBootstrap s seed
	
exception ImpossibleSplittingError
	let splitList l =
		let rec adv front back l = match back, l with
			| (_, [] | _, _::[]) -> List.rev front, back
			| h::t1, _::_::t2 -> adv (h::front) t1 t2
			| [], _::_::_ -> raise ImpossibleSplittingError
		in adv [] l l