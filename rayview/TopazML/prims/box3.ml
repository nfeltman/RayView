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