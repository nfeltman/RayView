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

#ifndef __EMBREE_DEBUG_RENDERER_H__
#define __EMBREE_DEBUG_RENDERER_H__

#include "renderers/renderer.h"

namespace embree
{
  /*! Simple renderer for testing the ray shooting performance. The
   *  renderer performs a series of diffuse bounces when given a
   *  recursion depth greater than 1. */
  class DebugRenderer : public Renderer
  {
    /* tile configuration */
    enum { TILE_SIZE_X = 16, TILE_SIZE_Y = 16 };

  public:

    /*! Construction from parameters. */
    DebugRenderer (const Parms& parms);

    /*! Renders a single frame. */
    void renderFrame(const Ref<Camera>& camera, const Ref<BackendScene>& scene, Ref<Film>& film);

  private:

    /*! Render function called once for each thread and frame. */
    void renderThread();
    static void run_renderThread(size_t tid, DebugRenderer* This, size_t) { This->renderThread(); }

    /*! Configuration */
  private:
    int maxDepth;                  //!< Maximal recursion depth

    /*! Arguments of renderFrame function */
  private:
    Ref<Camera> camera;            //!< Camera to render from
    Ref<BackendScene> scene;       //!< Scene to render
    Ref<Film> film;                //!< Framebuffer to render into

    /*! Precomputations. */
  private:
    float rcpWidth;                //!< Reciprocal width of framebuffer.
    float rcpHeight;               //!< Reciprocal height of framebuffer.
    int numTiles;                  //!< Number of tiles of the framebuffer.
    int numTilesX;                 //!< Number of tiles in x direction.
    int numTilesY;                 //!< Number of tiles in y direction.

  private:
    Atomic tileID;                 //!< ID of current tile
    Atomic atomicNumRays;          //!< for counting number of shoot rays
  };
}

#endif
