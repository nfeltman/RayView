type ne_box3
type box3 = Empty | NotEmpty of ne_box3
val center : ne_box3 -> Vectors.vec3
val surfaceArea : box3 -> float
val contains : box3 -> Vectors.vec3 -> bool
val ne_intersectsSeg : ne_box3 -> Vectors.seg3 -> bool
val intersectsSeg : box3 -> Vectors.seg3 -> bool
val ne_intersectsRay : ne_box3 -> Vectors.ray3 -> bool
val intersectsRay : box3 -> Vectors.ray3 -> bool
val ne_join : ne_box3 -> ne_box3 -> ne_box3