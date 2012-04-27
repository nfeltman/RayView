type bTri

val getCenter : bTri -> Vectors.vec3
val getBuildIndex : bTri -> int ref
val getObjIndex : bTri -> int
val getBounds : bTri -> Box3.ne_box3
val getTriangle : bTri -> Triangle.triangle
val createBuildTriangles : Triangle.triangle array -> bTri array