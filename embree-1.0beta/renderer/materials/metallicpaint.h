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

#ifndef __EMBREE_METALLIC_PAINT_H__
#define __EMBREE_METALLIC_PAINT_H__

#include "materials/material.h"
#include "brdfs/dielectric.h"
#include "brdfs/dielectriclayer.h"
#include "brdfs/lambertian.h"

namespace embree
{
  /*! Implements a car paint BRDF. Models a dielectric layer over a
   *  diffuse ground layer. Additionally the ground layer may contain
   *  metallic glitter. */
  class MetallicPaint : public Material
  {
    typedef Microfacet<FresnelConductor,PowerCosineDistribution> MicrofacetGlitter;

  public:

    /*! Construction from parameters. */
    MetallicPaint (const Parms& parms)
    {
      /*! extract parameters */
      Col3f shadeColor    = parms.getCol3f("shadeColor",one);
      Col3f glitterColor  = parms.getCol3f("glitterColor",zero);
      float glitterSpread = parms.getFloat("glitterSpread",1.0f);
      float eta           = parms.getFloat("eta",1.4f);

      /*! Use the fresnel relfectance of Aluminium for the flakes. Modulate with glitterColor. */
      Col3f etaAluminium(0.62f,0.62f,0.62f);
      Col3f kAluminium(4.8,4.8,4.8);

      /*! precompute BRDF component for performance reasons */
      reflection = new DielectricReflection(1.0f, eta);
      paint      = new DielectricLayer<Lambertian >(one, 1.0f, eta, Lambertian(shadeColor));
      if (glitterSpread != 0 && glitterColor != Col3f(zero))
        glitter = new DielectricLayer<MicrofacetGlitter >(one, 1.0f, eta, MicrofacetGlitter(glitterColor,
                                                          FresnelConductor(etaAluminium,kAluminium),
                                                          PowerCosineDistribution(rcp(glitterSpread))));
      else
        glitter = NULL;
    }

  /*! Destructor */
  ~MetallicPaint ()
  {
      if (reflection) delete reflection; reflection = NULL;
      if (paint     ) delete paint     ; paint      = NULL;
      if (glitter   ) delete glitter   ; glitter    = NULL;
  }


    void shade(const Ray& ray, const Medium& currentMedium, const DifferentialGeometry& dg, CompositedBRDF& brdfs) const
    {
      brdfs.add(reflection);
      brdfs.add(paint);
      if (glitter) brdfs.add(glitter);
    }

  private:
    DielectricReflection*               reflection;  //!< Precomputed dielectric reflection component.
    DielectricLayer<Lambertian>*        paint;       //!< Diffuse layer covered by dielectricum.
    DielectricLayer<MicrofacetGlitter>* glitter;     //!< Glitter layer covered by dielectricum.
  };
}

#endif
