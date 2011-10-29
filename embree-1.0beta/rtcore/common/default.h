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

#ifndef __EMBREE_RTCORE_DEFAULT_H__
#define __EMBREE_RTCORE_DEFAULT_H__

#include <cstring>
#include <iostream>
#include <vector>
#include <map>

#include "sys/platform.h"
#include "sys/tasking.h"
#include "sys/sync/atomic.h"
#include "sys/intrinsics.h"
#include "sys/stl/vector.h"

#include "math/math.h"
#include "math/vec2.h"
#include "math/vec3.h"
#include "math/vec4.h"
#include "math/bbox.h"

#include "simd/sse.h"

namespace embree
{
  /*! Box to use in the builders. */
  typedef BBox<ssef> Box;

  /*! Computes half surface area of box. */
  __forceinline float halfArea(const Box& box) {
    ssef d = size(box);
    ssef a = d*shuffle<1,2,0,3>(d);
    return extract<0>(reduce_add(a));
  }

  typedef Vec2<sseb> sse2b;
  typedef Vec3<sseb> sse3b;
  typedef Vec2<ssei> sse2i;
  typedef Vec3<ssei> sse3i;
  typedef Vec2<ssef> sse2f;
  typedef Vec3<ssef> sse3f;
}

#if !defined(__NO_AVX__)

#include "simd/avx.h"

namespace embree
{
  typedef Vec2<avxb> avx2b;
  typedef Vec3<avxb> avx3b;
  typedef Vec2<avxi> avx2i;
  typedef Vec3<avxi> avx3i;
  typedef Vec2<avxf> avx2f;
  typedef Vec3<avxf> avx3f;
}
#endif

#endif
