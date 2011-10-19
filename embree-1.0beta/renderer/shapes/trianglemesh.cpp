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

#include "shapes/trianglemesh.h"

namespace embree
{
  TriangleMesh::TriangleMesh(size_t numVertices,
                             const char* position, size_t stridePositions,
                             const char* normal, size_t strideNormals,
                             const char* texcoord, size_t strideTexCoords,
                             size_t numTriangles, const char* triangles, size_t strideTriangles)
  {
    if (position)  {
      this->position.resize(numVertices);
      for (size_t i=0; i<numVertices; i++) {
        float* ptr = (float*)(position+i*stridePositions);
        this->position[i] = Vec3f(ptr[0],ptr[1],ptr[2]);
      }
    }
    if (normal) {
      this->normal.resize(numVertices);
      for (size_t i=0; i<numVertices; i++) {
        float* ptr = (float*)(normal+i*strideNormals);
        this->normal[i] = Vec3f(ptr[0],ptr[1],ptr[2]);
      }
    }
    if (texcoord) {
      this->texcoord.resize(numVertices);
      for (size_t i=0; i<numVertices; i++) {
        float* ptr = (float*)(texcoord+i*strideTexCoords);
        this->texcoord[i] = Vec2f(ptr[0],ptr[1]);
      }
    }
    if (triangles) {
      this->triangles.resize(numTriangles);
      for (size_t i=0; i<numTriangles; i++) {
        int* ptr = (int*)(triangles+i*strideTriangles);
        this->triangles[i] = Triangle(ptr[0],ptr[1],ptr[2]);
      }
    }
  }

  Ref<Shape> TriangleMesh::transform(const AffineSpace& xfm) const
  {
    /*! do nothing for identity matrix */
    if (xfm == AffineSpace(one))
      return (Shape*)this;

    /*! create transformed */
    TriangleMesh* mesh = new TriangleMesh;
    mesh->position.resize(position.size());
    for (size_t i=0; i<position.size(); i++) mesh->position[i] = xfmPoint(xfm,position[i]);
    mesh->normal.resize(normal.size()  );
    for (size_t i=0; i<normal.size();   i++) mesh->normal  [i] = xfmNormal(xfm,normal[i]);
    mesh->texcoord  = texcoord;
    mesh->triangles = triangles;
    return (Shape*)mesh;
  }

  void TriangleMesh::postIntersect(const Ray& ray, DifferentialGeometry& dg) const
  {
    const Triangle& tri = triangles[dg.id1];
    Vec3f p0 = position[tri.v0], p1 = position[tri.v1], p2 = position[tri.v2];
    float u = dg.u, v = dg.v, w = 1.0f-u-v, t = dg.t;
    dg.P  = ray.org+t*ray.dir;
    dg.Ng = normalize(cross(p2 - p0,p1 - p0));

    if (normal.size())
    {
      Vec3f n0 = normal[tri.v0], n1 = normal[tri.v1], n2 = normal[tri.v2];
      Vec3f Ns = w*n0 + u*n1 + v*n2;
      float len2 = dot(Ns,Ns);
      Ns = len2 > 0 ? Ns*rsqrt(len2) : dg.Ng;
      if (dot(Ns,dg.Ng) < 0) Ns = -Ns;
      dg.Ns = Ns;
    }
    else
      dg.Ns = dg.Ng;

    if (texcoord.size())
      dg.st = texcoord[tri.v0]*w + texcoord[tri.v1]*u + texcoord[tri.v2]*v;
    else
      dg.st = Vec2f(u,v);

    dg.error = max(abs(dg.t),reduce_max(abs(dg.P)));
  }
}
