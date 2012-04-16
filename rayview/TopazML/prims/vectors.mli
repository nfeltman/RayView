type vec3 = { x: float; y: float; z: float }
type seg3 = { origin: vec; difference: vec }
type ray3 = { origin: vec; direction: vec }
val length2 : vec3 -> float
val dot : vec3 -> vec3 -> float
val ( * ) : vec3 -> float -> vec3
val (+) : vec3 -> vec3 -> vec3
val (-) : vec3 -> vec3 -> vec3
val (^) : vec3 -> vec3 -> vec3
