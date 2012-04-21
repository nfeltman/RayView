type vec3 = { x: float; y: float; z: float }
type seg3 = { originS: vec3; difference: vec3 }
type ray3 = { originR: vec3; direction: vec3 }
type shadowQuery = seg3 * bool 
let origin = { x =0.; y =0.; z =0.}
let length2 v = v.x *. v.x +. v.y *. v.y +. v.z *. v.z
let dot (a: vec3) (b: vec3) = a.x *. b.x +. a.y *. b.y +. a.z *. b.z
let ( * ) r s = { x = s *. r.x; y = s *. r.y; z = s *. r.z }
let (+) (a: vec3) (b: vec3) = { x = a.x +. b.x; y = a.y +. b.y; z = a.z +. b.z }
let (-) (a: vec3) (b: vec3) = { x = a.x -. b.x; y = a.y -. b.y; z = a.z -. b.z }
let (^) (a: vec3) (b: vec3) = { x = a.y *. b.z -. a.z *. b.y; y = a.z *. b.x -. a.x *. b.z; z = a.x *. b.y -. a.y *. b.x }
let readVector input = 
	let x = Util.read_single input in
	let y = Util.read_single input in
	let z = Util.read_single input in
	{x = x; y = y; z = z}
let firstIsCloser f s v = length2 (f-v) <= length2 (s-v)