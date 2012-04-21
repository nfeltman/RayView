open Vectors;;

type triangle = vec3 * vec3 * vec3
val intersectsSegment : triangle -> seg3 -> bool
val intersectsAny : triangle array -> seg3 -> bool