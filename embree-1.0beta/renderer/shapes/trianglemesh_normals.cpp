// ======================================================================== //
// Copyright 2009-2011 Intel Corporation                                    //
//                                                                          //
// Licensed under the Apache License, Version 2.0 (the "License");          //
// you may not use this file except in compliance with the License.         //
// You may obtain a copy of the License at                                  //
//                                                                          //
//     http://www.apache.org/licenses/LICENSE-2.0                           //
//                                                                          //
// Unless required by applicable law or agreed to in writing, software      //
// distributed under the License is distributed on an "AS IS" BASIS,        //
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. //
// See the License for the specific language governing permissions and      //
// limitations under the License.                                           //
// ======================================================================== //

#include "shapes/trianglemesh_normals.h"

namespace embree
{
  TriangleMeshWithNormals::TriangleMeshWithNormals(size_t numVertices,
                                                   const char* position, size_t stridePositions,
                                                   const char* normal, size_t strideNormals,
                                                   size_t numTriangles, const char* triangles_i, size_t strideTriangles)
  {
    vertices.resize(numVertices);
    for (size_t i=0; i<numVertices; i++) {
      float* v = (float*)(position+i*stridePositions);
      vertices[i].p = Vec3f(v[0],v[1],v[2]);
      float* n = (float*)(normal+i*strideNormals);
      vertices[i].n = Vec3f(n[0],n[1],n[2]);
    }
    triangles.resize(numTriangles);
    for (size_t i=0; i<numTriangles; i++) {
      int* ptr = (int*)(triangles_i+i*strideTriangles);
      triangles[i] = Triangle(ptr[0],ptr[1],ptr[2]);
    }
  }

  Ref<Shape> TriangleMeshWithNormals::transform(const AffineSpace& xfm) const
  {
    /*! do nothing for identity matrix */
    if (xfm == AffineSpace(one))
      return (Shape*)this;

    /*! create transformed mesh */
    TriangleMeshWithNormals* mesh = new TriangleMeshWithNormals;
    mesh->vertices.resize(vertices.size());
    for (size_t i=0; i<vertices.size(); i++) mesh->vertices[i].p = xfmPoint (xfm,*(Vec3f*)&vertices[i].p);
    for (size_t i=0; i<vertices.size(); i++) mesh->vertices[i].n = xfmNormal(xfm,*(Vec3f*)&vertices[i].n);
    mesh->triangles = triangles;
    return mesh;
  }

  void TriangleMeshWithNormals::postIntersect(const Ray& ray, DifferentialGeometry& dg) const
  {
    const Triangle& tri = triangles[dg.id1];
    const Vertex& v0 = vertices[tri.v0];
    const Vertex& v1 = vertices[tri.v1];
    const Vertex& v2 = vertices[tri.v2];
    float u = dg.u, v = dg.v, w = 1.0f-u-v, t = dg.t;
    Vec3f dPdu = v1.p - v0.p, dPdv = v2.p - v0.p;
    dg.P = ray.org+t*ray.dir;
    dg.Ng = normalize(cross(dPdv,dPdu));
    Vec3f Ns = w*v0.n + u*v1.n + v*v2.n;
    float len2 = dot(Ns,Ns);
    Ns = len2 > 0 ? Ns*rsqrt(len2) : dg.Ng;
    if (dot(Ns,dg.Ng) < 0) Ns = -Ns;
    dg.Ns = Ns;
    dg.error = max(abs(dg.t),reduce_max(abs(dg.P)));
  }
}
