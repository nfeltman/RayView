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

#ifndef __EMBREE_MEDIUM_H__
#define __EMBREE_MEDIUM_H__

#include "default.h"

namespace embree
{
  /*! Medium description. The integrator tracks the current medium for
   *  computing volumetric effects. */
  class Medium {
  public:

    /*! Medium constructor. */
    __forceinline Medium (const Col3f& transmission = one, float eta = 1.0f)
     : transmission(transmission), eta(eta) {}

    /*! Builds a vacuum. */
    static Medium Vacuum() {
      return Medium(one, 1.0f);
    }

    /*! Comparision of two media. */
    __forceinline bool operator==(const Medium& m) const {
      return transmission == m.transmission && eta == m.eta;
    }

  public:
    Col3f transmission; //!< Transmissivity of medium.
    float eta;          //!< Refraction index of medium.
  };
}

#endif
