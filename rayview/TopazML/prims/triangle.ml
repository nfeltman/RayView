open Vectors

type triangle = vec3 * vec3 * vec3

let t_EPSILON = 0.0001;;

let intersectsSegment triangle seg = let (p1, p2, p3) = triangle in
	let (edge0, edge1, edge2) = (p1 - p3, p3 - p2, p3 - seg.originS) in
	let (normal) = (edge0 ^ edge1) in
	let rcp = 1.0 /. (dot normal seg.difference) in
	let t = (dot normal edge2) *. rcp in
	(t >= t_EPSILON && t <= (1.0 -. t_EPSILON)) && let interm = edge2 ^ seg.difference in
	let u = (dot interm edge1) *. rcp in
	(u >= 0.0) && let v = (dot interm edge0) *. rcp in
	(u +. v) <= 1.0 && v >= 0.0

let intersectsAny tris seg = Array.fold_left (fun alreadyHits tri -> alreadyHits || intersectsSegment tri seg) false tris