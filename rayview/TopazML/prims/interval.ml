type ne_interval = { min: float; max: float }
type interval = Empty | NotEmpty of ne_interval

let make min max = NotEmpty({ min = min; max = max })
let ne_make min max = { min = min; max = max }

let isNonEmpty i = match i with
	| Empty -> false
	| NotEmpty(ne) -> true

let isEmpty i = not (isNonEmpty i)
let center ne_i = (ne_i.min +. ne_i.max) /. 2.0

let ne_length ne = ne.max -. ne.min
let length i = match i with
	| Empty -> 0.0
	| NotEmpty(ne) -> ne_length(ne)

let ne_contains ne v = v >= ne.min && v <= ne.max
let contains i v = match i with
	| Empty -> false
	| NotEmpty(ne) -> ne_contains ne v

let overlaps i1 i2 = match i1, i2 with
	| (Empty, Empty) -> false
	| (Empty, i) -> false
	| (i, Empty) -> false
	| (NotEmpty(ne1), NotEmpty(ne2)) when ne1.max < ne2.min || ne2.max < ne1.min -> false
	| (NotEmpty(ne1), NotEmpty(ne2)) -> true
let (+) i v = match i with
	| Empty -> Empty
	| NotEmpty(ne) -> NotEmpty({ min = ne.min +. v; max = ne.max +. v })

let ne_minus ne v = { min = ne.min -. v; max = ne.max -. v }
let (-) i v = match i with
	| Empty -> Empty
	| NotEmpty(ne) -> NotEmpty(ne_minus ne v)

let ( * ) i v = match i with
	| Empty -> Empty
	| NotEmpty(ne) when v < 0.0 -> NotEmpty({ min = ne.max *. v; max = ne.min *. v })
	| NotEmpty(ne) -> NotEmpty({ min = ne.min *. v; max = ne.max *. v })

let ne_divide ne v = if v > 0.0 then
		NotEmpty({ min = ne.min /. v; max = ne.max /. v })
	else if v < 0.0 then
		NotEmpty({ min = ne.max /. v; max = ne.min /. v })
	else if ne_contains ne 0.0 then
		NotEmpty({ min = neg_infinity; max = infinity })
	else
		Empty

let (/) i v = match i with
	| Empty -> Empty
	| NotEmpty(ne) -> ne_divide ne v

let ne_meet ne1 ne2 = if ne1.max < ne2.min || ne2.max < ne1.min then Empty
	else NotEmpty({ min = max ne1.min ne2.min; max = min ne1.max ne2.max })

let meet i1 i2 = match i1, i2 with
	| Empty, Empty -> Empty
	| Empty, i -> Empty
	| i, Empty -> Empty
	| NotEmpty(ne1), NotEmpty(ne2) -> ne_meet ne1 ne2

let ne_join ne1 ne2 = { min = min ne1.min ne2.min; max = max ne1.max ne2.max }

let join i1 i2 = match i1, i2 with
	| Empty, Empty -> Empty
	| Empty, i -> i
	| i, Empty -> i
	| NotEmpty(ne1), NotEmpty(ne2) -> NotEmpty(ne_join ne1 ne2)