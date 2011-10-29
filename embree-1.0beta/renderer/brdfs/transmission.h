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

#ifndef __EMBREE_TRANSMISSION_BRDF_H__
#define __EMBREE_TRANSMISSION_BRDF_H__

#include "brdfs/brdf.h"
#include "brdfs/optics.h"

namespace embree
{
  /*! BRDF of transmissive material. */
  class Transmission : public BRDF
  {
  public:

    /*! Transmissive BRDF constructor. \param T is the transmission coefficient */
    __forceinline Transmission(const Col3f& T) : BRDF(SPECULAR_TRANSMISSION), T(T) {}

    __forceinline Col3f eval(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      return zero;
    }

    Col3f sample(const Vec3f& wo, const DifferentialGeometry& dg, Sample3f& wi, const Vec2f& s) const {
      wi = -wo; return T;
    }

    float pdf(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      return zero;
    }

  private:

    /*! Transmission coefficient of the material. The range is [0,1]
     *  where 0 means total absorption and 1 means total
     *  transmission. */
    Col3f T;
  };
}

#endif
