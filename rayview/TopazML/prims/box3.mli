type ne_box3 = { x: Interval.ne_interval; y: Interval.ne_interval; z: Interval.ne_interval }
type box3 = Empty | NotEmpty of ne_box3
val center : ne_box3 -> bool
val surfaceArea : box3 -> float
val contains : box3 -> vec3 -> bool
val intersects : box3 -> seg3 -> bool
val intersects : box3 -> ray3 -> bool
