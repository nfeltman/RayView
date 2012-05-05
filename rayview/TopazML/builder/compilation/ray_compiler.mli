
type cRay_n = { ray : Vectors.seg3 ; intersectedFirst : Build_triangle.bTri; intersectedRest : Build_triangle.bTri list}

type ray_compile_output = { connected : Vectors.seg3 list ; broken : cRay_n list}

val compileRays : Bvh2.tempBVH -> Vectors.seg3 array -> ray_compile_output