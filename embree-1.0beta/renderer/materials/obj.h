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

#ifndef __EMBREE_OBJ_H__
#define __EMBREE_OBJ_H__

#include "materials/material.h"
#include "brdfs/lambertian.h"
#include "brdfs/specular.h"
#include "brdfs/transmission.h"
#include "textures/texture.h"

namespace embree
{
  /*! Implements an OBJ format material. Special material to model
   *  important features of the material from the OBJ file format. */
  class Obj : public Material
  {
  public:

    /*! Construction from parameters. */
    Obj (const Parms& parms)
    {
      d      = parms.getFloat("d",1.0f);
      map_d  = parms.getTexture("map_d");
      Tf     = parms.getCol3f("Tf",one);
      Kd     = parms.getCol3f("Kd",one);
      map_Kd = parms.getTexture("map_Kd");
      Ks     = parms.getCol3f("Ks",one);
      map_Ks = parms.getTexture("map_Ks");
      Ns     = parms.getFloat("Ns",10.0f);
      map_Ns = parms.getTexture("map_Ns");
    }

    void shade(const Ray& ray, const Medium& currentMedium, const DifferentialGeometry& dg, CompositedBRDF& brdfs) const
    {
      /*! combine dissolve factor. */
      float d  = this->d;
      if (map_d)  d  *= map_d ->get(dg.st).r;

      /*! adjust transmission filter */
      Col3f Tf = (1.0f-d)*this->Tf;

      /*! combine diffuse color */
      Col3f Kd = d*this->Kd;
      if (map_Kd) Kd *= map_Kd->get(dg.st);

      /*! combine specular color */
      Col3f Ks = d*this->Ks;
      if (map_Ks) Ks *= map_Ks->get(dg.st);

      /*! combine specular exponent */
      float Ns = this->Ns;
      if (map_Ns) Ns *= map_Ns->get(dg.st).r;

      /*! add all relevant BRDF components */
      if (Kd != Col3f(zero)) brdfs.add(NEW_BRDF(Lambertian)(Kd));
      if (Ks != Col3f(zero)) brdfs.add(NEW_BRDF(Specular)(Ks,Ns));
      if (Tf != Col3f(zero)) brdfs.add(NEW_BRDF(Transmission)(Tf));
    }

  private:
    float d;                  /*!< Dissolve factor. The range goes from 0 (transparent) to 1 (opaque). */
    Ref<Texture> map_d;       /*!< Dissolve factor texture. */
    Col3f Tf;                 /*!< Transmission filter. The range goes from 0 (opaque) to 1 (transmissive). */
    Col3f Kd;                 /*!< Diffuse reflectivity. The range goes from 0 (black) to 1 (white). */
    Ref<Texture> map_Kd;      /*!< Diffuse reflectivity texture. */
    Col3f Ks;                 /*!< Specular reflectivity. The range goes from 0 (black) to 1 (white). */
    Ref<Texture> map_Ks;      /*!< Specular reflectivity texture. */
    float Ns;                 /*!< Specular exponent. The range goes from 0 (diffuse) towards infinity (specular). */
    Ref<Texture> map_Ns;      /*!< Specular exponent texture. */
  };
}

#endif
