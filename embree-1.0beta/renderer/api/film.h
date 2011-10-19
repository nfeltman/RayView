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

#ifndef __EMBREE_FILM_H__
#define __EMBREE_FILM_H__

#include "image/image.h"
#include "sys/stl/array2d.h"
#include "filters/filter.h"

namespace embree
{
  /*! Framebuffer with filtering and accumulation features. The film
   *  implements the image interface, but contains an additional
   *  accumulation and weight image. */
  class Film : public Image3f
  {
  public:

    /*! Construction of a new film of specified size. */
    Film (size_t width, size_t height, float gamma = 1.0f, bool vignetting = true)
      : Image3f(width, height), accu(new Vec4f[width*height]), rcpGamma(rcp(gamma)), vignetting(vignetting), iteration(0)
    {
      clear(Vec2i(0,0),Vec2i((int)width-1,(int)height-1));
    }

    /*! Destruction of film. */
    ~Film() {
      if (accu) delete[] accu; accu = NULL;
    }

    /*! Clear a part of the film. */
    void clear(Vec2i start, Vec2i end)
    {
      for (index_t y=start.y; y<=end.y; y++) {
        for (index_t x=start.x; x<=end.x; x++) {
          set(x, y, zero);
          accu[y*width+x] = Vec4f(0.0f,0.0f,0.0f,1E-10f);
        }
      }
    }

  public:

    /*! Get the current accumulation iteration. */
    int getIteration() const { return iteration; }

    /*! Set the current accumulation iteration. */
    void setIteration(const int i) { iteration = i; }

    /*! Goto next accumulation iteration. */
    void incIteration() { iteration++; }

    /*! Set the gamma value for the film. */
    void setGamma(const float g) { rcpGamma = rcp(g); }

    /*! Turn the vignetting effect on/of. */
    void setVignetting(const bool v) { vignetting = v; }

  public:

    /*! Add a new sample to the film. \param p is the location of the
     *  sample \param start is the beginning of the tile \param end is
     *  the end of the tile \param color is the color of the sample
     *  \param weight is the weight of the sample */
    void accumulate(const Vec2f& p, Vec2i start, Vec2i end,
                    const Col3f& color, const float weight)
    {
      /*! ignore samples outside the tile */
      int x = (int)p.x, y = (int)p.y;
      if (x < start.x || x > end.x || y < start.y || y > end.y) return;

      /*! accumulate color and weight */
      accu[y*width+x] += Vec4f(color.r,color.g,color.b,weight);
    }

    /*! Normalizes a tile of the accumulation buffer and copies the
     *  result into the frame buffer. */
    void normalize(Vec2i start, Vec2i end)
    {
      for (index_t y=start.y; y<=end.y; y++) {
        for (index_t x=start.x; x<=end.x; x++) {
          const Vec4f& a = accu[y*width+x];
          Col3f color = Col3f(a.x,a.y,a.z) * rcp(a.w);
          if (rcpGamma != 1.0f) color = pow(color,rcpGamma);
          if (vignetting) {
            float d = length((Vec2f(float(x),float(y)) - Vec2f(float(width/2),float(height/2))) / float(width/2));
            color *= pow(cos(d*0.5f),3.0f);
          }
          set(x,y,color);
        }
      }
    }

  private:
    Vec4f* accu;     //!< Accumulation buffer.
    float rcpGamma;  //!< Reciprocal gamma value.
    bool vignetting; //!< Add a vignetting effect.
    int iteration;   //!< Current accumulation iteration.
  };
}

#endif
