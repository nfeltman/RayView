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

#include "renderers/integratorrenderer.h"

/* include all integrators */
#include "integrators/pathtraceintegrator.h"

/* include all samplers */
#include "samplers/sampler.h"

/* include all image filters */
#include "filters/boxfilter.h"
#include "filters/bsplinefilter.h"

namespace embree
{
  IntegratorRenderer::IntegratorRenderer(const Parms& parms)
  {
    /*! create integrator to use */
    std::string _integrator = parms.getString("integrator","pathtracer");
    if (_integrator == "pathtracer") integrator = new PathTraceIntegrator(parms);
    else throw std::runtime_error("unknown integrator type: "+_integrator);

    /*! create sampler to use */
    std::string _samplers = parms.getString("sampler","multijittered");
    if (_samplers == "multijittered"   ) samplers = new SamplerFactory(parms);
    else throw std::runtime_error("unknown sampler type: "+_samplers);

    /*! create pixel filter to use */
    std::string _filter = parms.getString("filter","bspline");
    if      (_filter == "none"   ) filter = NULL;
    else if (_filter == "box"    ) filter = new BoxFilter;
    else if (_filter == "bspline") filter = new BSplineFilter;
    else throw std::runtime_error("unknown filter type: "+_filter);

    /*! get framebuffer configuration */
    accumulate = parms.getBool("accumulate",false);
    gamma = parms.getFloat("gamma",1.0f);
  }

  void IntegratorRenderer::renderThread()
  {
    /*! create a new sampler */
    size_t numRays = 0;
    Sampler* sampler = samplers->create();

    /*! tile pick loop */
    while (true)
    {
      /*! pick a new tile */
      index_t tile = tileID++;
      if (tile >= numTiles) break;

      /*! compute tile pixel range */
      Vec2i start((int(tile)%numTilesX)*TILE_SIZE_X,(int(tile)/numTilesX)*TILE_SIZE_Y);
      Vec2i end (min(int(film->width),start.x+TILE_SIZE_X)-1,min(int(film->height),start.y+TILE_SIZE_Y)-1);

      /*! configure the sampler with the tile pixels */
      sampler->init(Vec2i((int)film->width, (int)film->height), start, end, iteration);
      if (!accumulate) film->clear(start,end);

      /*! process all tile samples */
      while (!sampler->finished()) {
        Vec2f rasterPos = sampler->proceed();
        Ray primary; camera->ray(rasterPos*Vec2f(rcpWidth,rcpHeight), sampler->getLens(), primary);
        Col3f L = integrator->Li(primary, scene, sampler, numRays, 0);
        if (!finite(L.r+L.g+L.b) || L.r < 0 || L.g < 0 || L.b < 0) L = zero;
        film->accumulate(sampler->getIntegerRaster(), start, end, L, 1.0f);
      }
      film->normalize(start,end);
    }

    /*! we access the atomic ray counter only once per tile */
    atomicNumRays += numRays;
    delete sampler;
  }

  void IntegratorRenderer::renderFrame(const Ref<Camera>& camera, const Ref<BackendScene>& scene, Ref<Film>& film)
  {
    /*! flush to zero and no denormals */
    _mm_setcsr(_mm_getcsr() | /*FTZ:*/ (1<<15) | /*DAZ:*/ (1<<6));

    /*! precompute some values */
    numTilesX = ((int)film->width +TILE_SIZE_X-1)/TILE_SIZE_X;
    numTilesY = ((int)film->height+TILE_SIZE_Y-1)/TILE_SIZE_Y;
    numTiles = numTilesX * numTilesY;
    rcpWidth  = 1.0f/float(film->width);
    rcpHeight = 1.0f/float(film->height);
    film->setGamma(gamma);
    if (!accumulate) film->setIteration(0);
    iteration = film->getIteration();

    /*! render frame */
    double t = getSeconds();
    this->tileID = 0;
    this->atomicNumRays = 0;
    this->samplers->reset();
    this->integrator->requestSamples(this->samplers, scene);
    this->samplers->init(film->getIteration(), filter);
    this->camera = camera;
    this->scene = scene;
    this->film = film;
    scheduler->addTask((Task::runFunction)&run_renderThread,this,scheduler->getNumThreads());
    scheduler->go();
    film->incIteration();
    this->camera = null;
    this->scene = null;
    this->film = null;
    double dt = getSeconds()-t;

    /*! print framerate */
    std::cout << 1.0f/dt << " fps, " << dt*1000.0f << " ms, " << atomicNumRays/dt*1E-6 << " Mrps" << std::endl;
  }
}
