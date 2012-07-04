open BuildTriangle;;
open Box3;;
open ArrayUtil;;

type boxCount = { box : Box3.ne_box3; count : int }

module A = struct
	
	type ne_agg = boxCount
	type agg = EmptyAgg | NonEmptyAgg of ne_agg
	
	let ne_combine a1 a2 = { box = ne_join a1.box a2.box; count = a1.count + a2.count }
	let combine a1 a2 =
		match a1, a2 with
		| EmptyAgg, a2 -> a2
		| a1, EmptyAgg -> a1
		| NonEmptyAgg(ne1), NonEmptyAgg(ne2) -> NonEmptyAgg(ne_combine ne1 ne2)
	;;
	
	let add_triangle tri a =
		match a with
		| EmptyAgg -> NonEmptyAgg({ box = (getBounds tri); count = 1 })
		| NonEmptyAgg(ne) -> NonEmptyAgg({ box = ne_join (getBounds tri) ne.box; count = ne.count + 1 })
	;;
	
	let roll range arr = { box = calcBoundMap getTriangle arr range; count = rangeSize range }
	let rollList list = { box = calcBoundMapList getTriangle list; count = List.length list }
	
end