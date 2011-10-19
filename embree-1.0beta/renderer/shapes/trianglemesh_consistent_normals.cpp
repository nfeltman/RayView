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

#include "shapes/trianglemesh_consistent_normals.h"

#include <algorithm>

namespace embree
{
  // Input data wrapper allowing
  // -- data access as p(i), n(i), t(i);
  // -- sorting using operator().
  struct InputData3 {
    InputData3(const char* _position, size_t _stridePositions,
               const char* _normal  , size_t _strideNormals,
               const char* _texcoord, size_t _strideTexCoords) :
      position(_position), stridePositions(_stridePositions),
      normal    (_normal), strideNormals  (_strideNormals),
      texcoord(_texcoord), strideTexCoords(_strideTexCoords)
    {}

    // used by std::sort
    bool operator()(const Vec2i& pair0, const Vec2i& pair1) const {
      // This operator facilitates indirect sort by input data.
      int i = pair0.x, j = pair1.x; // use only 1st component (original index)
      // return (p(i) != p(j))? (p(i) < p(j)) : (n(i) != n(j))? (n(i) < n(j)) : (t(i) < t(j));
      return
        (p(i).x != p(j).x)? (p(i).x < p(j).x) :
        (p(i).y != p(j).y)? (p(i).y < p(j).y) :
        (p(i).z != p(j).z)? (p(i).z < p(j).z) :
        (n(i).x != n(j).x)? (n(i).x < n(j).x) :
        (n(i).y != n(j).y)? (n(i).y < n(j).y) :
        (n(i).z != n(j).z)? (n(i).z < n(j).z) :
        (t(i).x != t(j).x)? (t(i).x < t(j).x) :
        (t(i).y < t(j).y);
    }

    const Vec3f p(int i) const { float* d = (float*)(position + i*stridePositions); return Vec3f(d[0],d[1],d[2]); }
    const Vec3f n(int i) const { float* d = (float*)(normal   + i*strideNormals  ); return Vec3f(d[0],d[1],d[2]); }
    const Vec2f t(int i) const { float* d = (float*)(texcoord + i*strideTexCoords); return Vec2f(d[0],d[1]);      }

    const char* position; size_t stridePositions;
    const char* normal  ; size_t strideNormals;
    const char* texcoord; size_t strideTexCoords;
  };

  int TriangleMeshConsistentNormals::CopyUniqueVertices(size_t numVertices,
                                                        const char* _position , size_t stridePositions,
                                                        const char* _normal   , size_t strideNormals,
                                                        const char* _texcoord , size_t strideTexCoords, size_t numTriangles,
                                                        const char* _triangles, size_t strideTriangles)
  {

    // Copy unique input data to these vectors (this function will be called from constructor).
    InputData3 inputdata(_position, stridePositions,
                         _normal  , strideNormals,
                         _texcoord, strideTexCoords);

    // Each sorted_vertices element contains Vec2i(original_index, unique_index) pair.
    std::vector<Vec2i> sorted_vertices;
    sorted_vertices.reserve(numVertices);
    // Set up original indices (x).
    for (size_t i = 0; i < numVertices; i++)
      sorted_vertices.push_back(Vec2i((int)i,0));

    // Sort by position, normal, texture coordinates.
    std::sort(sorted_vertices.begin(), sorted_vertices.end(), inputdata);

    // Compute unique indices (y).
    // sorted_vertices[0].y was already set to 0.
    int n_unique_vertices = 0;
    for (size_t i = 1; i < numVertices; i++) {
      // Check if two vertices are different.
      if (inputdata(sorted_vertices[i-1], sorted_vertices[i]))
        n_unique_vertices++;
      sorted_vertices[i].y = n_unique_vertices;
    }
    n_unique_vertices++;

    // Copy unique data to these vectors.
    position.resize(n_unique_vertices);
    normal.resize(n_unique_vertices);
    if (_texcoord) texcoord.resize(n_unique_vertices);
    for (size_t il = (size_t)-1, i = 0; i < numVertices; i++) {
      int io = sorted_vertices[i].x; // original
      int iu = sorted_vertices[i].y; // unique
      if (il == (size_t)iu) continue;
      il = iu;
      position[iu] = inputdata.p(io);
      normal[iu] = inputdata.n(io);
      if (_texcoord) texcoord[iu] = inputdata.t(io);
    }

    // Compute original -> unique mapping.
    std::vector<int> unique_index;
    unique_index.resize(numVertices);
    for (size_t i = 0; i < numVertices; i++)
      unique_index[sorted_vertices[i].x] = sorted_vertices[i].y;

    sorted_vertices.clear();

    // Set triangles.
    triangles.reserve(numTriangles);
    for (size_t i = 0; i < numTriangles; i++) {
      int* ptr = (int*)(_triangles+i*strideTriangles);
      triangles.push_back(Triangle(unique_index[ptr[0]],
                                   unique_index[ptr[1]],
                                   unique_index[ptr[2]]));
    }

    return n_unique_vertices;
  }


  TriangleMeshConsistentNormals::TriangleMeshConsistentNormals(size_t numVertices,
                             const char* _position, size_t stridePositions,
                             const char* _normal, size_t strideNormals,
                             const char* _texcoord, size_t strideTexCoords,
                             size_t numTriangles, const char* _triangles, size_t strideTriangles)
  {

    // trianglemesh_normals.cpp has the similar block (see description there).

    if (_normal == 0)
      return;

    // Phase 1: copy unique vertices into this data.
    // =============================================
    numVertices = CopyUniqueVertices(numVertices,
                                     _position , stridePositions,
                                     _normal   , strideNormals,
                                     _texcoord , strideTexCoords, numTriangles,
                                     _triangles, strideTriangles);

    // Phase 2:
    // First, compute cos(maxangle), then real angle.
    // ==============================================

    // Initialize maxangle
    // (min(maxangle, dot(nv,nf)) will be computed later and then converted to angle).
    maxangle.resize(numVertices);
    for (size_t i = 0; i < numVertices; i++)
      maxangle[i] = 1; // cos(0)

    for (size_t i = 0; i < numTriangles; i++) {
      const Triangle& tri = triangles[i];

      // Face normal (not normalized).
      Vec3f nf(cross(position[tri.v2] - position[tri.v0],
                     position[tri.v1] - position[tri.v0]));
      float len = dot(nf, nf);
      if (len) len = rsqrt(len);

      // Use abs() to ignore triangle orientation and/or correctness of per-vertex normals.
      maxangle[tri.v0] = min(maxangle[tri.v0], len * abs(dot(nf, normal[tri.v0])));
      maxangle[tri.v1] = min(maxangle[tri.v1], len * abs(dot(nf, normal[tri.v1])));
      maxangle[tri.v2] = min(maxangle[tri.v2], len * abs(dot(nf, normal[tri.v2])));
    }

    // cos(angle) -> adjusted angle, see
    // http://visual-computing.intel-research.net/publications/papers/2010/cni/Consistent%20Normal%20Interpolation.pdf
    for (size_t i = 0; i < numVertices; i++) {
      float cosa = maxangle[i];
      float adjust = 1 + 0.03632f*(1-cosa)*(1-cosa);
      maxangle[i] = min(acos(cosa) * adjust, 3.14159265358979f/2);
    }

  }

  Ref<Shape> TriangleMeshConsistentNormals::transform(const AffineSpace& xfm) const
  {
    /*! do nothing for identity matrix */
    if (xfm == AffineSpace(one))
      return (Shape*)this;

    /*! create transformed */
    TriangleMeshConsistentNormals* mesh = new TriangleMeshConsistentNormals;
    mesh->position.resize(position.size());
    for (size_t i=0; i<position.size(); i++) mesh->position[i] = xfmPoint(xfm,position[i]);
    mesh->normal.resize(normal.size()  );
    for (size_t i=0; i<normal.size();   i++) {
      mesh->normal[i] = xfmNormal(xfm,normal[i]);
      // It might not be necessary...
      float len2 = dot(mesh->normal[i], mesh->normal[i]);
      if (len2 != 0 && len2 != 1) mesh->normal[i] *= rsqrt(len2);
    }
    mesh->texcoord  = texcoord;
    mesh->triangles = triangles;
    mesh->maxangle = maxangle;
    return (Shape*)mesh;
  }

  void TriangleMeshConsistentNormals::postIntersect(const Ray& ray, DifferentialGeometry& dg) const
  {
    const Triangle& tri = triangles[dg.id1];
    Vec3f p0 = position[tri.v0], p1 = position[tri.v1], p2 = position[tri.v2];
    float u = dg.u, v = dg.v, w = 1.0f-u-v, t = dg.t;
    dg.P  = ray.org+t*ray.dir;
    dg.Ng = normalize(cross(p2 - p0,p1 - p0));

    if (normal.size()) {
      Vec3f n0 = normal[tri.v0], n1 = normal[tri.v1], n2 = normal[tri.v2];
      Vec3f Ns = w*n0 + u*n1 + v*n2;
      float len2 = dot(Ns,Ns);
      Ns = len2 > 0 ? Ns*rsqrt(len2) : dg.Ng;

      if (dot(ray.dir,dg.Ng) > 0) dg.Ng = -dg.Ng; // set Ng orientation by ray.dir
      if (dot(Ns,dg.Ng) < 0) Ns = -Ns;            // set Ns orientation by Ng

      // Compute interpolated angle f.
      float a0 = maxangle[tri.v0], a1 = maxangle[tri.v1], a2 = maxangle[tri.v2];
      float f  = w*a0 + u*a1 + v*a2;

      float x  = dot(ray.dir, Ns);
      const float pi = 3.14159265358979f;
      float qi = ((1.0f - (2.0f/pi)*f)*(1.0f - (2.0f/pi)*f))/((1.0f + (7.0f/8.0f)*f)*(1.0f + (9.0f/8.0f-4.0f/pi)*f));
      float y = 1.0f - qi * (1.0f + x); // lerp
      float sinr = sqrt(qi*(1 + y)/(1 - x));
      Vec3f reflection = (y - sinr * x) * Ns + sinr * ray.dir;
      // compute consistent normal
      Ns = normalize(reflection - ray.dir);

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
