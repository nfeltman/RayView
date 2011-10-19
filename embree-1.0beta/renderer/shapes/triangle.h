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

#ifndef __EMBREE_TRIANGLE_H__
#define __EMBREE_TRIANGLE_H__

#include "shapes/shape.h"

namespace embree
{
  /*! Implements a single triangle. */
  class Triangle : public Shape
  {
  public:

    /*! Triangle construction from its vertices. */
    Triangle (const Vec3f& v0, const Vec3f& v1, const Vec3f& v2)
      : v0(v0), v1(v1), v2(v2), Ng(normalize(cross(v2-v0,v1-v0))) {}

    /*! Transforms the triangle. */
    Ref<Shape> transform(const AffineSpace& xfm) const {
      return new Triangle(xfmPoint(xfm,v0),xfmPoint(xfm,v1),xfmPoint(xfm,v2));
    }

    /*! Post intersection to compute shading data. */
    void postIntersect(const Ray& ray, DifferentialGeometry& dg) const {
      dg.P = ray.org+dg.t*ray.dir;
      dg.Ng = this->Ng;
      dg.Ns = this->Ng;
      dg.st = Vec2f(dg.u,dg.v);
      dg.error = max(abs(dg.t),reduce_max(abs(dg.P)));
    }

  public:
    Vec3f v0;   //!< 1st vertex of triangle.
    Vec3f v1;   //!< 2nd vertex of triangle.
    Vec3f v2;   //!< 3rd vertex of triangle.
    Vec3f Ng;   //!< Geometry normal of triangle.
  };
}

#endif
