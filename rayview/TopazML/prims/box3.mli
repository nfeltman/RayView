type ne_box3 = { bx: Interval.ne_interval; by: Interval.ne_interval; bz: Interval.ne_interval }
type box3 = Empty | NotEmpty of ne_box3
val center : ne_box3 -> Vectors.vec3
val surfaceArea : box3 -> float
val contains : box3 -> Vectors.vec3 -> bool
val ne_intersectsSeg : ne_box3 -> Vectors.seg3 -> bool
val intersectsSeg : box3 -> Vectors.seg3 -> bool
val ne_intersectsRay : ne_box3 -> Vectors.ray3 -> bool
val intersectsRay : box3 -> Vectors.ray3 -> bool
val ne_join : ne_box3 -> ne_box3 -> ne_box3
val ne1_join : ne_box3 -> box3 -> ne_box3
val join : box3 -> box3 -> box3
val calcBound : Triangle.triangle array -> ArrayUtil.range -> ne_box3
val calcBoundMap : ('a -> Triangle.triangle) -> 'a array -> ArrayUtil.range -> ne_box3
val calcBoundMapList : ('a -> Triangle.triangle) -> 'a list -> ne_box3
val calcPointBoundMap : ('a -> Vectors.vec3) -> 'a array -> ArrayUtil.range -> ne_box3
val calcPointBoundMapList : ('a -> Vectors.vec3) -> 'a list -> ne_box3
val calcBoundAll : Triangle.triangle array -> ne_box3
val fromTri : Triangle.triangle -> ne_box3