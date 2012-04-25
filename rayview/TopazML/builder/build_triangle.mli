type bTri

val getCenter : bTri -> Vectors.vec3
val getBuildIndex : bTri -> int ref
val getObjIndex : bTri -> int
val createBuildTriangles : Triangle.triangle array -> bTri array