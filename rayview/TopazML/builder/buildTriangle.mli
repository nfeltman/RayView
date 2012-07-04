type bTri

val defaultTri : bTri
val getCenter : bTri -> Vectors.vec3
val getBuildIndex : bTri -> int
val getObjIndex : bTri -> int
val getBounds : bTri -> Box3.ne_box3
val getTriangle : bTri -> Triangle.triangle
val createBuildTriangleArray : Triangle.triangle array -> bTri array
val createBuildTriangleList : Triangle.triangle array -> bTri list