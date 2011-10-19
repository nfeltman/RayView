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

#ifndef __EMBREE_LAMBERTIAN_BRDF_H__
#define __EMBREE_LAMBERTIAN_BRDF_H__

#include "brdfs/brdf.h"

namespace embree
{
  /*! Lambertian BRDF. A lambertian surface is a surface that reflects
   *  the same intensity independent of the viewing direction. The
   *  BRDF has a reflectivity parameter that determines the color of
   *  the surface. */
  class Lambertian : public BRDF
  {
  public:

    /*! Lambertian BRDF constructor. This is a diffuse reflection BRDF. */
    __forceinline Lambertian(const Col3f& R) : BRDF(DIFFUSE_REFLECTION), R(R) {}

    __forceinline Col3f eval(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      return R * (1.0f/float(pi)) * clamp(dot(wi,dg.Ns));
    }

    Col3f sample(const Vec3f& wo, const DifferentialGeometry& dg, Sample3f& wi, const Vec2f& s) const {
      return eval(wo, dg, wi = cosineSampleHemisphere(s.x,s.y,dg.Ns));
    }

    float pdf(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      return cosineSampleHemispherePDF(wi,dg.Ns);
    }

  private:

    /*! The reflectivity parameter. The vale 0 means no reflection,
     *  and 1 means full reflection. */
    Col3f R;
  };
}

#endif
