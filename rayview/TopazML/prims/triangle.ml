module Triangle =
struct
	type triangle = vec3 * vec3 * vec3
	let intersects triangle seg = let (p1, p2, p3) = triangle in
		let (edge0, edge1, edge2) = (p1 - p3, p3 - p2, p3 - seg.origin) in
		let (normal) = (edge0 ^ edge1) in
		let rcp = 1.0f / (normal * seg.difference) in
		let t = (normal * edge2) * rcp in
		(t >= 0.0 && t <= 1.0) && let interm = edge2 ^ seg.difference in
		let u = (interm * edge1) * rcp in
		(u >= 0.0) && let v = (interm * edge0) * rcp in
		(u + v <= 1.0 && v >= 0.0)
end