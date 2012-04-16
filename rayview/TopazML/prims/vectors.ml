
module Vec3 =
struct
	type vec3 = { x: float; y: float; z: float }
	type seg3 = { origin: vec; difference: vec }
	type ray3 = { origin: vec; direction: vec }
	let length2 v = v.x * v.x + v.y * v.y + v.z * v.z
	let dot (a: vec3) (b: vec3) = a.x *. b.x +. a.y *. b.y +. a.z *. b.z
	let ( * ) s (r: vec3) = { x = s *. r.x; y = s *. r.y; z = s *. r.z }
	let (+) (a: vec3) (b: vec3) = { x = a.x +. b.x; y = a.y +. b.y; z = a.z +. b.z }
	let (-) (a: vec3) (b: vec3) = { x = a.x -. b.x; y = a.y -. b.y; z = a.z -. b.z }
	let (^) (a: vec3) (b: vec3) = { x = a.y*b.z -. a.z*b.y; y = a.z*b.x -. a.x*b.z; z = a.x*b.y -. a.y*b.x }
end