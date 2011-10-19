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

#ifndef __EMBREE_TRIANGLE_MESH_H__
#define __EMBREE_TRIANGLE_MESH_H__

#include "shapes/shape.h"

namespace embree
{
  /*! Implements a triangle mesh. The mesh supports optional
   *  vertex normals and texture coordinates. */
  class TriangleMesh : public Shape
  {
  public:

    /*! Triangle indices description. */
    struct Triangle {
      Triangle () {}
      Triangle (uint32 v0, uint32 v1, uint32 v2) : v0(v0), v1(v1), v2(v2) {}
      uint32 v0;  //!< index of first triangle vertex
      uint32 v1;  //!< index of second triangle vertex
      uint32 v2;  //!< index of third triangle vertex
    };

  private:
    TriangleMesh() {}

  public:

    /*! Construction from vertex data and triangle index data. */
    TriangleMesh(size_t numVertices,     /*!< Number of mesh vertices.                 */
                 const char* position,   /*!< Pointer to vertex positions.             */
                 size_t stridePositions, /*!< Stride of vertex positions.              */

                 const char* normal,     /*!< Optional poiner to vertex normals.       */
                 size_t strideNormals,   /*!< Stride of vertex positions.              */

                 const char* texcoord,   /*!< Optional pointer to texture coordinates. */
                 size_t strideTexCoords, /*!< Stride of vertex positions.              */

                 size_t numTriangles,    /*!< Number of mesh triangles.                */
                 const char* triangles,  /*!< Pointer to triangle indices.             */
                 size_t strideTriangles  /*!< Stride of vertex positions.              */);


  public:
    Ref<Shape> transform(const AffineSpace& xfm) const;
    void postIntersect(const Ray& ray, DifferentialGeometry& dg) const;

  public:
    vector_t<Vec3f> position;      //!< Position array.
    vector_t<Vec3f> normal;        //!< Normal array (can be empty).
    vector_t<Vec2f> texcoord;      //!< Texture coordinates array (can be empty).
    vector_t<Triangle> triangles;  //!< Triangle indices array.
  };
}

#endif
