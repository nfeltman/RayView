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

#ifndef __EMBREE_BRDF_H__
#define __EMBREE_BRDF_H__

#include "shapes/differentialgeometry.h"

namespace embree
{
  /*! The type of a BRDF. It is only a hint that helps the integrator
   *  to choose the best integration technique. */
  enum BRDFType
  {
    /*! individual BRDF components */
    DIFFUSE_REFLECTION    = 1,   /*!< diffuse light reflection            */
    GLOSSY_REFLECTION     = 2,   /*!< glossy light reflection             */
    SPECULAR_REFLECTION   = 4,   /*!< perfect specular light reflection   */
    DIFFUSE_TRANSMISSION  = 8,   /*!< diffuse light transmission          */
    GLOSSY_TRANSMISSION   = 16,  /*!< glossy light transmission           */
    SPECULAR_TRANSMISSION = 32,  /*!< perfect specular light transmission */

    /*! combining all diffuse, glossy, and specular components */
    DIFFUSE      = DIFFUSE_REFLECTION   | DIFFUSE_TRANSMISSION,    /*!< diffuse reflections and transmissions          */
    GLOSSY       = GLOSSY_REFLECTION    | GLOSSY_TRANSMISSION,     /*!< glossy reflections and transmissions           */
    SPECULAR     = SPECULAR_REFLECTION  | SPECULAR_TRANSMISSION,   /*!< perfect specular reflections and transmissions */

    /*! combining all reflection and all transmission components */
    REFLECTION   = DIFFUSE_REFLECTION   | GLOSSY_REFLECTION   | SPECULAR_REFLECTION,   /*!< all possible reflections   */
    TRANSMISSION = DIFFUSE_TRANSMISSION | GLOSSY_TRANSMISSION | SPECULAR_TRANSMISSION, /*!< all possible transmissions */

    /*! no or all components set */
    NONE         = 0,                         /*!< no component set   */
    ALL          = REFLECTION | TRANSMISSION  /*!< all components set */
  };

  /*! BRDF interface definition. A BRDF can be evaluated, sampled, and
   *  the sampling PDF be evaluated. Note that in difference to the
   *  definition in the literature, our BRDFs contain the cosine term.
   *  E.g. the diffuse BRDF in our system is a/pi*cos(wi,N) instead of
   *  a/pi. This makes the formula of reflection and refraction BRDFs
   *  more natural. It further creates consistency as the sampling
   *  functionality of the BRDF class would have to include sampling
   *  of the cosine term anyway. As an optimization, the evaluation of
   *  the cosine term can even be skipped in case it is handled
   *  through sampling. */
  class BRDF
  {
  public:

    /*! BRDF constructor. The BRDF interface remembers the type
     *  hints. */
    __forceinline BRDF(const BRDFType type) : type(type) {}

    /*! Evaluates the BRDF for a given outgoing direction, shade
     *  location, and incoming light direction. Perfectly specular
     *  BRDF components cannot be evaluated and should return
     *  zero. */
    virtual Col3f eval(const Vec3f               & wo,  /*!< Direction light is reflected into.                   */
                       const DifferentialGeometry& dg,  /*!< Shade location on a surface to evaluate the BRDF at. */
                       const Vec3f               & wi)  /*!< Direction light is falling onto the surface.         */ const {
      return zero;
    }

    /*! Samples the BRDF for a point to shade and outgoing light
     *  direction. By default we perform a cosine weighted hemisphere
     *  sampling. */
    virtual Col3f sample(const Vec3f               & wo,  /*!< Direction light is reflected into.                 */
                         const DifferentialGeometry& dg,  /*!< Shade location on a surface to sample the BRDF at. */
                         Sample3f                  & wi,  /*!< Returns sampled incoming light direction and PDF.  */
                         const Vec2f               & s)   /*!< Sample locations are provided by the caller.       */ const {
      return eval(wo, dg, wi = cosineSampleHemisphere(s.x,s.y,dg.Ns));
    }

    /*! Evaluates the sampling PDF. \returns the probability density */
    virtual float pdf(const Vec3f               & wo,   /*!< Direction light is reflected into.               */
                      const DifferentialGeometry& dg,   /*!< Shade location on a surface to evaluate PDF for. */
                      const Vec3f               & wi)   /*!< Direction light is coming from.                  */ const {
      return cosineSampleHemispherePDF(wi,dg.Ns);
    }

  public:

    /*! the type hints of the BRDF */
    BRDFType type;
  };
}

#endif
