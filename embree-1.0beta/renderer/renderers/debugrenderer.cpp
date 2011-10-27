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

#include "debugrenderer.h"
#include "math/random.h"

namespace embree
{
  DebugRenderer::DebugRenderer(const Parms& parms) {
    maxDepth = parms.getInt("maxDepth",1);
  }

  void DebugRenderer::renderThread()
  {
    size_t numRays = 0;

    /*! tile pick loop */
    while (true)
    {
      /*! pick a new tile */
      index_t tile = tileID++;
      if (tile >= numTiles) break;

      /*! compute tile location */
      Random rand(int(tile)*1024);
      size_t x0 = (tile%numTilesX)*TILE_SIZE_X;
      size_t y0 = (tile/numTilesX)*TILE_SIZE_Y;

      /*! loop over all pixels of the tile */
      for (size_t dy=0; dy<TILE_SIZE_Y; dy++)
      {
        for (size_t dx=0; dx<TILE_SIZE_X; dx++)
        {
          /*! ignore tile pixels outside framebuffer */
          size_t ix = x0+dx, iy = y0+dy;
          if (ix >= film->width || iy >= film->height) continue;

          /*! create primary ray */
          DifferentialGeometry hit;
          Ray ray; camera->ray(Vec2f(ix*rcpWidth,iy*rcpHeight), Vec2f(rand.getFloat(),rand.getFloat()), ray);

          for (index_t depth=0; depth<maxDepth; depth++)
          {
            /*! shoot current ray */
            new (&hit) DifferentialGeometry;
            scene->accel->intersect(ray,hit,0);
            scene->postIntersect(ray,hit);
            numRays++;
            if (!hit) break;

            /*! compute new ray through diffuse bounce */
            Vec3f Nf = hit.Ng;
            if (dot(-ray.dir,Nf) < 0) Nf = -Nf;
            if (depth+1<maxDepth) ray = Ray(ray.org+0.999f*hit.t*ray.dir,cosineSampleHemisphere(rand.getFloat(),rand.getFloat(),Nf),4.0f*float(ulp)*hit.error);
          }

          /*! update framebuffer */
          if (!hit) film->set(ix,iy,zero);
          else film->set(ix,iy,Col3f(hit.u,hit.v,1.0f-hit.u-hit.v));
        }
      }
    }

    /*! we access the atomic ray counter only once per tile */
    atomicNumRays += numRays;
  }

  void DebugRenderer::renderFrame(const Ref<Camera>& camera, const Ref<BackendScene>& scene,  Ref<Film>& film)
  {
    /*! flush to zero and no denormals */
    _mm_setcsr(_mm_getcsr() | /*FTZ:*/ (1<<15) | /*DAZ:*/ (1<<6));

    /*! precompute some values */
    numTilesX = ((int)film->width +TILE_SIZE_X-1)/TILE_SIZE_X;
    numTilesY = ((int)film->height+TILE_SIZE_Y-1)/TILE_SIZE_Y;
    numTiles = numTilesX * numTilesY;
    rcpWidth  = 1.0f/float(film->width);
    rcpHeight = 1.0f/float(film->height);

    /*! render frame */
    double t = getSeconds();
    this->tileID = 0;
    this->atomicNumRays = 0;
    this->camera = camera;
    this->scene = scene;
    this->film = film;
    scheduler->addTask((Task::runFunction)&run_renderThread,this,scheduler->getNumThreads());
    scheduler->go();
    this->camera = null;
    this->scene = null;
    this->film = null;
    double dt = getSeconds()-t;

    /*! print framerate */
    std::cout << 1.0f/dt << " fps, " << dt*1000.0f << " ms, " << atomicNumRays/dt*1E-6 << " Mrps" << std::endl;
  }
}
