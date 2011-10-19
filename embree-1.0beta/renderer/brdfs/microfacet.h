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

#ifndef __EMBREE_MICROFACET_BRDF_H__
#define __EMBREE_MICROFACET_BRDF_H__

#include "brdfs/brdf.h"
#include "brdfs/optics.h"

namespace embree
{
  /*! Fresnel term for perfect reflection. */
  class FresnelNone {
  public:

    /*! Evaluation simply returns white. */
    __forceinline Col3f eval(float cosTheta) const { return one; }
  };

  /*! Fresnel term of a dielectric surface. A dielectric surface is
   *  for instance glass or water. */
  class FresnelDielectric {
  public:

    /*! Dielectric fresnel term constructor. \param etai is the
     *  refraction index of the medium the incident ray travels in
     *  \param etat is the refraction index of the opposite medium */
    __forceinline FresnelDielectric(float etai, float etat) : etai(etai), etat(etat) {}

    /*! Evaluates the fresnel term. \param cosTheta is the cosine
     *  between the facet normal (half vector) and the viewing
     *  vector. */
    __forceinline Col3f eval(float cosTheta) const {
      return Col3f(fresnelDielectric(cosTheta,etai*rcp(etat)));
    }

  private:

    /*! refraction index of the medium the incident ray travels in */
    float etai;

    /*! refraction index of the medium the outgoing transmission rays
     *  travels in */
    float etat;
  };

  /*! Fresnel term for a metal surface. */
  class FresnelConductor {
  public:

    /*! Conductor fresnel term constructor. \param eta is the real part of
     *  the refraction index \param k is the imaginary part of the
     *  refraction index */
    __forceinline FresnelConductor(const Col3f& eta, const Col3f& k) : eta(eta), k(k) {}

    /*! Evaluates the fresnel term. \param cosTheta is the cosine
     *  between the facet normal (half vector) and the viewing
     *  vector. */
    __forceinline Col3f eval(float cosTheta) const { return fresnelConductor(cosTheta,eta,k); }
  private:
    Col3f eta;  //!< Real part of refraction index
    Col3f k;    //!< Imaginary part of refraction index
  };

  /*! Power cosine microfacet distribution. */
  class PowerCosineDistribution {
  public:

    /*! Power cosine distribution constructor. */
    __forceinline PowerCosineDistribution(float exp) : exp(exp) {}

    /*! Evaluates the power cosine distribution. \param cosThetaH is
     *  the cosine between half vector and the normal. */
    __forceinline float eval(float cosThetaH) const {
      return (exp+2) * (1.0f/(2.0f*float(pi))) * pow(abs(cosThetaH), exp);
    }

    /*! Samples the power cosine distribution. */
    __forceinline void sample(const Vec3f               & wo,  /*!< Direction light is reflected into.                 */
                              const DifferentialGeometry& dg,  /*!< Shade location on a surface to sample the BRDF at. */
                              Sample3f                  & wi,  /*!< Returns sampled incoming light direction and PDF.  */
                              const Vec2f               & s)   /*!< Sample locations are provided by the caller.       */ const
    {
      Sample3f wh = powerCosineSampleHemisphere(s.x,s.y,dg.Ns,exp);
      wi = Sample3f(reflect(wo,wh),wh.pdf/(4.0f*abs(dot(wo,wh.value))));
    }

    /*! Evaluates the sampling PDF. \returns the probability density */
    __forceinline float pdf(const Vec3f               & wo,    /*!< Direction light is reflected into.               */
                            const DifferentialGeometry& dg,    /*!< Shade location on a surface to evaluate PDF for. */
                            const Vec3f               & wi)    /*!< Direction light is coming from.                  */ const
    {
      if (dot(wo,dg.Ns) < 0.0f || dot(wi,dg.Ns) < 0.0f) return zero;
      Vec3f wh = normalize(wo+wi);
      return powerCosineSampleHemispherePDF(wh,dg.Ns,exp)/(4.0f*abs(dot(wo,wh)));
    }

  private:

    /*! Exponent that determines the glossiness. The range is
     *  [0,infinity[ where 0 means a diffuse surface, and the
     *  specularity increases towards infinity. */
    float exp;
  };

  /*! Microfacet BRDF model. The class is templated over the fresnel
   *  term and microfacet distribution to use. */
  template<typename Fresnel, typename Distribution>
    class Microfacet : public BRDF
  {
  public:

    /*! Microfacet BRDF constructor. This is a glossy BRDF. */
    __forceinline Microfacet (const Col3f& R, const Fresnel& fresnel, const Distribution& distribution)
      : BRDF(GLOSSY_REFLECTION), R(R), fresnel(fresnel), distribution(distribution) {}

    __forceinline Col3f eval(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const
    {
      float cosThetaO = dot(wo,dg.Ns);
      float cosThetaI = dot(wi,dg.Ns);
      if (cosThetaI <= 0.0f || cosThetaO <= 0.0f) return zero;
      Vec3f wh = normalize(wi + wo);
      float cosThetaH = dot(wh, dg.Ns);
      float cosTheta = dot(wi, wh); // = dot(wo, wh);
      Col3f F = fresnel.eval(cosTheta);
      float D = distribution.eval(cosThetaH);
      float G = min(1.0f, 2.0f * cosThetaH * cosThetaO / cosTheta, 2.0f * cosThetaH * cosThetaI / cosTheta);
      return R * D * G * F / (4.0f*cosThetaO);
    }

    Col3f sample(const Vec3f& wo, const DifferentialGeometry& dg, Sample3f& wi, const Vec2f& s) const
    {
      if (dot(wo,dg.Ns) <= 0.0f) return zero;
      distribution.sample(wo,dg,wi,s);
      if (dot(wi.value,dg.Ns) <= 0.0f) return zero;
      return eval(wo,dg,wi);
    }

    float pdf(const Vec3f& wo, const DifferentialGeometry& dg, const Vec3f& wi) const {
      return distribution.pdf(wo,dg,wi);
    }

  private:

    /*! Reflectivity of the microfacets. The range is [0,1] where 0
     *  means no reflection at all, and 1 means full reflection. */
    Col3f R;

    /*! Fresnel term to use. */
    Fresnel fresnel;

    /*! Microfacet distribution to use. */
    Distribution distribution;
  };
}

#endif
