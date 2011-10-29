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

#ifndef __EMBREE_SHAPE_H__
#define __EMBREE_SHAPE_H__

#include "default.h"
#include "rtcore/ray.h"
#include "differentialgeometry.h"

namespace embree
{
  /*! Interface to different shapes. A shape is the smallest geometric
   *  primitive materials and area lights can be assigned to. */
  class Shape : public RefCount {
    ALIGNED_CLASS
  public:

    /*! Shape virtual destructor. */
    virtual ~Shape() {}

    /*! Instantiates a new shape by transforming this shape to a different location. */
    virtual Ref<Shape> transform(const AffineSpace& xfm) const = 0;

    /*! Performs interpolation of shading vertex parameters. */
    virtual void postIntersect(const Ray& ray, DifferentialGeometry& dg) const = 0;
  };
}

#endif
