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

#ifndef __EMBREE_PINHOLE_CAMERA_H__
#define __EMBREE_PINHOLE_CAMERA_H__

#include "camera.h"

namespace embree
{
  /*! Implements the pinhole camera model. */
  class PinholeCamera : public Camera
  {
  public:

    /*! Construction from parameter container. */
    PinholeCamera(const Parms& parms) {
      localToWorld      = parms.getTransform("local2world");
      float angle       = parms.getFloat("angle",64.0f);
      float aspectRatio = parms.getFloat("aspectRatio",1.0f);
      lensRadius        = parms.getFloat("lensRadius",0.0f);
      focalDistance     = parms.getFloat("focalDistance");
      Vec3f W           = xfmVector(localToWorld, Vec3f(-0.5f*aspectRatio,-0.5f,0.5f/tanf(deg2rad(0.5f*angle))));
      xfm = AffineSpace(aspectRatio*localToWorld.l.vx,localToWorld.l.vy,W,localToWorld.p);
    }

    void ray(const Vec2f& pixel, const Vec2f& sample, Ray& ray_o) const 
    {
      // Special case for pinhole camera and rays through center of lens.
      if (lensRadius == 0.0f || sample == Vec2f(0.5f,0.5f)) {
        new (&ray_o) Ray(xfm.p,normalize(pixel.x*xfm.l.vx + (1.0f-pixel.y)*xfm.l.vy + xfm.l.vz));
        return;
      }

      // Sample uniform location on a disc with r = lensRadius
      float r, theta;
      Vec2f s = 2.0f * sample - Vec2f(1.0f, 1.0f);

      if (s.x >= -s.y) {
        if (s.x > s.y) {
          r = s.x;
          if (s.y > 0.0) theta = s.y/r;
          else theta = 8.0f + s.y/r;
        }
        else {
          r = s.y;
          theta = 2.0f - s.x/r;
        }
      }
      else {
        if (s.x <= s.y) {
          r = -s.x;
          theta = 4.0f - s.y/r;
        }
        else {
          r = -s.y;
          theta = 6.0f + s.x/r;
        }
      }
      theta *= float(pi) / 4.0f;

      Vec3f org = xfmPoint(localToWorld, Vec3f(lensRadius*r*cosf(theta), lensRadius*r*sinf(theta), 0.0f));
      Vec3f dir = normalize(pixel.x*xfm.l.vx + (1.0f-pixel.y)*xfm.l.vy + xfm.l.vz);
      Vec3f centerDir = normalize(0.5f*xfm.l.vx + 0.5f*xfm.l.vy + xfm.l.vz);
      Vec3f p = xfm.p + focalDistance*dir/dot(dir,centerDir);
      new (&ray_o) Ray(org, normalize(p - org));
      return;
    }

  private:

    /*! Transformation that transforms a pixel location directly into
     *  the corresponding ray. */
    AffineSpace xfm;
    AffineSpace localToWorld;
    float lensRadius;
    float focalDistance;
  };
}

#endif
