
type vec3 = { x: float; y: float; z: float }
type seg3 = { originS: vec3; difference: vec3 }
type shadowQuery = Vectors.seg3 * bool 
type ray3 = { originR: vec3; direction: vec3 }
val origin : vec3
val length2 : vec3 -> float
val dot : vec3 -> vec3 -> float
val ( * ) : vec3 -> float -> vec3
val (+) : vec3 -> vec3 -> vec3
val (-) : vec3 -> vec3 -> vec3
val (^) : vec3 -> vec3 -> vec3
val readVector : in_channel -> vec3