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

#ifndef __EMBREE_VELVETY_BRDF_H__
#define __EMBREE_VELVETY_BRDF_H__

#include "brdfs/brdf.h"
#include "brdfs/optics.h"

namespace embree
{
  /*! Velvety BRDF. For materials with horizon scattering (velvet, peach, etc.)
   *  The BRDF has a reflectance parameter that determines the color of
   *  the surface and a parameter to determine the falloff of horizon scattering. */
  class Velvety : public BRDF
  {
  public:

    /*! Velvety BRDF constructor. This is a diffuse reflection BRDF. */
    __forceinline Velvety(const Col3f& R, const float f) : BRDF(DIFFUSE_REFLECTION), R(R), f(f) {}

    __forceinline Col3f eval(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      float cosThetaO = clamp(dot(wo,dg.Ns));
      float cosThetaI = clamp(dot(wi,dg.Ns));
      float sinThetaO = sqrtf(1.0f - cosThetaO);
      float horizonScatter = powf(sinThetaO, f);
      return R * horizonScatter * cosThetaI / float(pi);
    }

    Col3f sample(const Vec3f& wo, const DifferentialGeometry& dg, Sample3f& wi, const Vec2f& s) const {
      return eval(wo, dg, wi = cosineSampleHemisphere(s.x,s.y,dg.Ns));
    }

    float pdf(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      return cosineSampleHemispherePDF(wi,dg.Ns);
    }

  private:

    /*! The reflectance parameter. The vale 0 means no reflection,
     *  and 1 means full reflection. */
    Col3f R;

    /*! The falloff of horizon scattering. 0 no falloff,
     *  and inf means maximum falloff. */
    float f;
  };
}

#endif
