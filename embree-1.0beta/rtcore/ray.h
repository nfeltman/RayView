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

#ifndef __EMBREE_RAY_H__
#define __EMBREE_RAY_H__

#include "common/default.h"

namespace embree
{
  /*! Ray structure. Contains all information about a ray including
   *  precomputed reciprocal direction. */
  struct Ray
  {
    /*! Default construction does nothing. */
    __forceinline Ray() {}

    /*! Constructs a ray from origin, direction, and ray segment. Near
     *  has to be smaller than far. */
    __forceinline Ray(const Vec3f& org, const Vec3f& dir, const float& near = zero, const float& far = inf)
      : org(org), dir(dir), rdir(1.0f/dir), near(near), far(far) {
	
	}

  public:
    Vec3f org;     //!< Ray origin
    Vec3f dir;     //!< Ray direction
    Vec3f rdir;    //!< Reciprocal ray direction
    float near;    //!< Start of ray segment
    float far;     //!< End of ray segment
  };

  /*! Outputs ray to stream. */
  inline std::ostream& operator<<(std::ostream& cout, const Ray& ray) {
    return cout << "{ org = " << ray.org << ", dir = " << ray.dir << ", near = " << ray.near << ", far = " << ray.far << " }";
  }
}

#endif
