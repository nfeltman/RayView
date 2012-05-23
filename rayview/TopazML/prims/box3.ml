open Vectors;;
open Interval;;
module I = Interval;;

type ne_box3 = { bx: I.ne_interval; by: I.ne_interval; bz: I.ne_interval }
type box3 = Empty | NotEmpty of ne_box3
let center neb = { Vectors.x = I.center neb.bx; Vectors.y = I.center neb.by; Vectors.z = I.center neb.bz }

let ne_surfaceArea neb = let (lx, ly, lz) = (ne_length neb.bx, ne_length neb.by, ne_length neb.bz) in lx *. ly +. ly *. lz +. lz *. lx

let surfaceArea b = match b with
	| Empty -> 0.0
	| NotEmpty(neb) -> ne_surfaceArea neb

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

let ne1_join ne1 b2 = match b2 with
	| Empty -> ne1
	| NotEmpty(ne2) -> ne_join ne1 ne2

let join b1 b2 = match b1, b2 with
	| b1, Empty -> b1
	| Empty, b2 -> b2
	| NotEmpty(ne1), NotEmpty(ne2) -> NotEmpty(ne_join ne1 ne2)

exception BoundsError of string

let calcPointBoundMap map tris range =
	if ArrayUtil.rangeSize range <= 0 then
		raise (BoundsError "Expected non-empty array.")
	else
		let p0 = map(tris.(fst(range))) in
		let (xMin, xMax, yMin, yMax, zMin, zMax) = (ref p0.x, ref p0.x, ref p0.y, ref p0.y, ref p0.z, ref p0.z) in
		let bound p =
			xMin := min !xMin p.x; xMax := max !xMax p.x;
			yMin := min !yMin p.y; yMax := max !yMax p.y;
			zMin := min !zMin p.z; zMax := max !zMax p.z in
		ArrayUtil.iterRange (fun t -> bound(map(t))) (ArrayUtil.incrBottom range) tris;
		{ bx = I.ne_make !xMin !xMax; by = I.ne_make !yMin !yMax; bz = I.ne_make !zMin !zMax }

let calcPointBoundMapList map tris = match tris with
	| [] -> raise (BoundsError "Expected non-empty array.")
	| h:: t ->	let rec bound l xMin xMax yMin yMax zMin zMax = match l with
				| [] -> { bx = I.ne_make xMin xMax; by = I.ne_make yMin yMax; bz = I.ne_make zMin zMax }
				| h:: t -> let p = map h in	bound t (min xMin p.x) (max xMax p.x) (min yMin p.y) (max yMax p.y) (min zMin p.z) (max zMax p.z)
			in let p0 = map h in bound t p0.x p0.x p0.y p0.y p0.z p0.z

let calcBoundMap map tris range =
	if ArrayUtil.rangeSize range <= 0 then
		raise (BoundsError "Expected non-empty array.")
	else
		let (f1, f2, f3) = map(tris.(fst(range))) in
		let (xMin, xMax, yMin, yMax, zMin, zMax) = (ref f1.x, ref f1.x, ref f1.y, ref f1.y, ref f1.z, ref f1.z) in
		let bound p =
			xMin := min !xMin p.x; xMax := max !xMax p.x;
			yMin := min !yMin p.y; yMax := max !yMax p.y;
			zMin := min !zMin p.z; zMax := max !zMax p.z in
		bound(f2); bound(f3);
		ArrayUtil.iterRange (fun t -> let (p1, p2, p3) = map(t) in bound(p1); bound(p2); bound(p3)) (ArrayUtil.incrBottom range) tris;
		{ bx = I.ne_make !xMin !xMax; by = I.ne_make !yMin !yMax; bz = I.ne_make !zMin !zMax }

let calcBoundMapList map tris = match tris with
	| [] -> raise (BoundsError "Expected non-empty array.")
	| h:: t ->	let (f1, f2, f3) = map h in
			let (xMin, xMax, yMin, yMax, zMin, zMax) = (ref f1.x, ref f1.x, ref f1.y, ref f1.y, ref f1.z, ref f1.z) in
			let bound p =
				xMin := min !xMin p.x; xMax := max !xMax p.x;
				yMin := min !yMin p.y; yMax := max !yMax p.y;
				zMin := min !zMin p.z; zMax := max !zMax p.z in
			bound(f2); bound(f3);
			List.iter (fun t -> let (p1, p2, p3) = map(t) in bound(p1); bound(p2); bound(p3)) t;
			{ bx = I.ne_make !xMin !xMax; by = I.ne_make !yMin !yMax; bz = I.ne_make !zMin !zMax }

let calcBound tris range =
	if ArrayUtil.rangeSize range <= 0 then
		raise (BoundsError "Expected non-empty array.")
	else
		let (f1, f2, f3) = tris.(fst(range)) in
		let (xMin, xMax, yMin, yMax, zMin, zMax) = (ref f1.x, ref f1.x, ref f1.y, ref f1.y, ref f1.z, ref f1.z) in
		let bound p =
			xMin := min !xMin p.x; xMax := max !xMax p.x;
			yMin := min !yMin p.y; yMax := max !yMax p.y;
			zMin := min !zMin p.z; zMax := max !zMax p.z in
		bound(f2); bound(f3);
		ArrayUtil.iterRange (fun (p1, p2, p3) -> bound(p1); bound(p2); bound(p3)) (ArrayUtil.incrBottom range) tris;
		{ bx = I.ne_make !xMin !xMax; by = I.ne_make !yMin !yMax; bz = I.ne_make !zMin !zMax }

let calcBoundAll tris = calcBound tris (0, Array.length tris)

let fromTri (p1, p2, p3) =
	let minX, maxX = min (min p1.x p2.x) p3.x, max (max p1.x p2.x) p3.x in
	let minY, maxY = min (min p1.y p2.y) p3.y, max (max p1.y p2.y) p3.y in
	let minZ, maxZ = min (min p1.z p2.z) p3.z, max (max p1.z p2.z) p3.z in
	{ bx = I.ne_make minX maxX; by = I.ne_make minY maxY; bz = I.ne_make minZ maxZ }