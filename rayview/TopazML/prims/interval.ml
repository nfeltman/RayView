type ne_interval = { min: float; max: float }
type interval = Empty | NotEmpty of ne_interval
let isNonEmpty i = match i with
	| Empty -> false
	| NotEmpty -> true
let isEmpty i = not isNonEmpty i
let center ne_i = (min + max) / 2
let length i = match i with
	| Empty -> 0
	| NotEmpty(min, max) -> max - min;;
let contains i v = match i with
	| Empty -> false
	| NotEmpty(min, max) -> v >= min && v <= max
let intersects i1 i2 = match i1, i2 with
	| (Empty, Empty) -> false
	| (Empty, i) -> false
	| (i, Empty) -> false
	| (NotEmpty(min1, max1), NotEmpty(min2, max2)) when max1 < min2 || max2 < min1 -> false
	| (NotEmpty(min1, max1), NotEmpty(min2, max2)) -> true
let (+) i v = match i with
	| Empty -> Empty
	| NotEmpty(minI, maxI) -> { min = minI + v, max = maxI + v };;
let (-) i v = match i with
	| Empty -> Empty
	| NotEmpty(minI, maxI) -> { min = minI - v, max = maxI - v };;
let ( * ) i v = match i with
	| Empty -> Empty
	| NotEmpty(minI, maxI) when v < 0 -> { min = maxI * v, max = minI * v }
	| NotEmpty(minI, maxI) -> { min = minI * v, max = maxI * v }
let (/) i v = match i with
	| Empty -> Empty
	| NotEmpty(minI, maxI) when v > 0 -> { min = maxI / v, max = minI / v }
	| NotEmpty(minI, maxI) when v < 0 -> { min = maxI / v, max = minI / v }
	| NotEmpty(minI, maxI) -> if minI <=0 && maxI >=0 then { min = neg_infinity, max = infinity } else Empty
let (&) i1 i2 = match i1, i2 with
	| (Empty, Empty) -> Empty
	| (Empty, i) -> Empty
	| (i, Empty) -> Empty
	| (NotEmpty(min1, max1), NotEmpty(min2, max2)) when max1 < min2 || max2 < min1 -> Empty
	| (NotEmpty(min1, max1), NotEmpty(min2, max2)) -> { min = max(min1, min2), max = min(max1, max2) }
let ( || ) i1 i2 = match i1, i2 with
	| (Empty, Empty) -> Empty
	| (Empty, i) -> i
	| (i, Empty) -> i
	| (NotEmpty(min1, max1), NotEmpty(min2, max2)) -> { min = min(min1, min2), max = max(max1, max2) };;