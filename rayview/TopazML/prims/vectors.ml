
module Vec3 =
struct
	type vec3 = { x: float; y: float; z: float }
	type seg3 = { origin: vec; difference: vec }
	type ray3 = { origin: vec; direction: vec }
	let length2 v = v.x * v.x + v.y * v.y + v.z * v.z
	let dot (a: vec3) (b: vec3) = a.x *. b.x +. a.y *. b.y +. a.z *. b.z
	let ( * ) (s: vec3) (r: vec3) = { x = s *. r.x; y = s *. r.y; z = s *. r.z }
	let (+) (a: vec3) (b: vec3) = { x = a.x +. b.x; y = a.y +. b.y; z = a.z +. b.z }
	let (-) (a: vec3) (b: vec3) = { x = a.x -. b.x; y = a.y -. b.y; z = a.z -. b.z }
end

module Interval =
struct
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
end

module Box3 =
struct
	type ne_box3 = { x: Interval.ne_interval; y: Interval.ne_interval; z: Interval.ne_interval }
	type box3 = Empty | NotEmpty of ne_box3
	let center neb = { x = center x; y = center y; z = center z }
	let surfaceArea b = match b with
		| Empty -> 0
		| NotEmpty(x, y, z) -> let lx = length x, ly = length y, lz = length z in lx * ly + ly * lz + lz * lx
	let contains b x y z = (contains b.x x) && (contains b.y y) && (contains b.z z)
	let intersects b origin direction t =
		let t_x = ((b.x - origin) / direction.x) & t in (isNonEmpty t_x) &&
		let t_y = ((b.y - origin) / direction.y) & t_x in (isNonEmpty t_y) &&
		intsersects ((b.z - origin) / direction.z) t_y
	let intersects (b: box3) (s: seg3) = intersects b seg.origin seg.direction { min = 0; max = 1 }
	let intersects (b: box3) (r: ray3) = intersects b seg.origin seg.difference { min = 0; max = infinity }
end
