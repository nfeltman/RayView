open Vectors;;
open Interval;;
module I = Interval;;

type ne_box3 = { bx: I.ne_interval; by: I.ne_interval; bz: I.ne_interval }
type box3 = Empty | NotEmpty of ne_box3
let center neb = { Vectors.x = I.center neb.bx; Vectors.y = I.center neb.by; Vectors.z = I.center neb.bz }
let surfaceArea b = match b with
	| Empty -> 0.0
	| NotEmpty(neb) -> let (lx, ly, lz) = (ne_length neb.bx, ne_length neb.by, ne_length neb.bz) in lx *. ly +. ly *. lz +. lz *. lx
let contains b (v: vec3) = match b with
	| Empty -> false
	| NotEmpty(neb) -> (ne_contains neb.bx v.x) && (ne_contains neb.by v.y) && (ne_contains neb.bz v.z)
let ne_intersects b origin direction t =
	let t_x = meet (ne_divide (ne_minus b.bx origin.x) direction.x) t in (isNonEmpty t_x) &&
	let t_y = meet (ne_divide (ne_minus b.by origin.y) direction.y) t_x in (isNonEmpty t_y) &&
	overlaps (ne_divide (ne_minus b.bz origin.z) direction.z) t_y
let ne_intersectsSeg b (s: seg3) = ne_intersects b s.originS s.difference (I.make 0.0 1.0)
let ne_intersectsRay b (r: ray3) = ne_intersects b r.originR r.direction (I.make 0.0 infinity)
let intersectsSeg b (s: seg3) = match b with
	| Empty -> false
	| NotEmpty(neb) -> ne_intersectsSeg neb s
let intersectsRay b (r: ray3) = match b with
	| Empty -> false
	| NotEmpty(neb) -> ne_intersectsRay neb r

let ne_join b1 b2 = { bx = ne_join b1.bx b2.bx; by = ne_join b1.by b2.by; bz = ne_join b1.bz b2.bz }

exception BoundsError of string

let calcBound tris =
	if Array.length tris == 0 then
		raise (BoundsError "Expected non-empty array.")
	else
		let (f1, f2, f3) = tris.(0) in
		let (xMin, xMax, yMin, yMax, zMin, zMax) = (ref f1.x, ref f1.x, ref f1.y, ref f1.y, ref f1.z, ref f1.z) in
		let bound p =
			xMin := min !xMin p.x; xMax := max !xMax p.x;
			yMin := min !yMin p.y; yMax := max !yMax p.y;
			zMin := min !zMin p.z; zMax := max !zMax p.z in
		bound(f2); bound(f3);
		for i = 1 to pred (Array.length tris) do
			let (p1, p2, p3) = tris.(i) in bound(p1); bound(p2); bound(p3);
		done;
		{ bx = I.ne_make !xMin !xMax; by = I.ne_make !yMin !yMax; bz = I.ne_make !zMin !zMax }