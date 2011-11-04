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

#ifndef __EMBREE_DEVICE_H__
#define __EMBREE_DEVICE_H__

#include "sys/platform.h"
#include "sys/filename.h"
#include "math/math.h"
#include "math/vec2.h"
#include "math/vec3.h"
#include "math/vec4.h"
#include "math/col3.h"
#include "math/affinespace.h"

namespace embree
{
  class Device : public RefCount
  {
  public:

    /*******************************************************************
                    construction / destruction
    *******************************************************************/

    Device();
    virtual ~Device();


    /*******************************************************************
                  type definitions
    *******************************************************************/

    struct RTImage;
    struct RTTexture;

    /** generic handle */
    struct RTHandle : public RefCount
    {
      friend class Device;

      __forceinline RTHandle(const Ref<Device>& device, void* handle) : device(device), handle(handle) {}
      virtual ~RTHandle();

      void rtSetBool1(const char* property, bool x);
      void rtSetBool2(const char* property, const Vec2b& v);
      void rtSetBool3(const char* property, const Vec3b& v);
      void rtSetBool4(const char* property, const Vec4b& v);
      void rtSetInt1(const char* property, int x);
      void rtSetInt2(const char* property, const Vec2i& v);
      void rtSetInt3(const char* property, const Vec3i& v);
      void rtSetInt4(const char* property, const Vec4i& v);
      void rtSetFloat1(const char* property, float x);
      void rtSetFloat2(const char* property, const Vec2f& v);
      void rtSetFloat3(const char* property, const Vec3f& v);
      void rtSetFloat3(const char* property, const Col3f& v);
      void rtSetFloat4(const char* property, const Vec4f& v);
      void rtSetArray(const char* property, const char* type, const void* ptr, size_t size, size_t stride = size_t(-1));
      void rtSetString(const char* property, const char* str);
      void rtSetImage(const char* property, const Ref<RTImage>& img);
      void rtSetTexture(const char* property, const Ref<RTTexture>& tex);
      void rtSetTransform(const char* property, const AffineSpace& space);
      void rtCommit();

    private:
      Ref<Device> device;
      void* handle;
    };

      /** camera handle */
      struct RTCamera : public RTHandle {
        RTCamera (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** image handle */
      struct RTImage : public RTHandle {
        RTImage (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** texture handle */
      struct RTTexture : public RTHandle {
        RTTexture (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** material handle */
      struct RTMaterial : public RTHandle {
        RTMaterial (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** shape handle */
      struct RTShape : public RTHandle {
        RTShape (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** light handle */
      struct RTLight : public RTHandle {
        RTLight (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** primitive handle */
      struct RTPrimitive : public RTHandle {
        RTPrimitive (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** scene handle */
      struct RTScene : public RTHandle {
        RTScene (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** renderer handle */
      struct RTRenderer : public RTHandle {
        RTRenderer (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };

      /** framebuffer handle */
      struct RTFrameBuffer : public RTHandle {
        RTFrameBuffer (const Ref<Device>& device, void* handle) : RTHandle(device,handle) { }
      };


    /*******************************************************************
                    creation of objects
    *******************************************************************/

    /** create a camera */
    Ref<RTCamera> rtNewCamera(const char* type);

    /** creates an image */
    Ref<RTImage> rtNewImage(const char* type, size_t width, size_t height, const void* data);

    /** creates a texture */
    Ref<RTTexture> rtNewTexture(const char* type);

    /** create a new material */
    Ref<RTMaterial> rtNewMaterial(const char* type);

    /** create a new shape */
    Ref<RTShape> rtNewShape(const char* type);

    /** create a new light source. */
    Ref<RTLight> rtNewLight(const char* type);

    /** create a new primitive */
    Ref<RTPrimitive> rtNewPrimitive(const Ref<RTShape>& shape, const Ref<RTMaterial>& material, const AffineSpace& transform);

    /** create a new primitive */
    Ref<RTPrimitive> rtNewPrimitive(const Ref<RTLight>& light, const AffineSpace& transform);

    /** create a new scene. */
    Ref<RTScene> rtNewScene(const char* type, embree::TraceData traceFile, Ref<RTPrimitive>* prims, size_t size);

    /** creates a renderer */
    Ref<RTRenderer> rtNewRenderer(const char* type);

    /** creates a framebuffer */
    Ref<RTFrameBuffer> rtNewFrameBuffer(const char* type, size_t width, size_t height);

    /** map and unmap the framebuffer data */
    void* rtMapFrameBuffer(const Ref<RTFrameBuffer>& frameBuffer);
    void rtUnmapFrameBuffer(const Ref<RTFrameBuffer>& frameBuffer);


    /*******************************************************************
                      render call
    *******************************************************************/

    /** render a frame using current renderer settings. */
    void rtRenderFrame(const Ref<RTRenderer>& renderer, const Ref<RTCamera>& camera, const Ref<RTScene>& scene, const Ref<RTFrameBuffer>& frameBuffer);

    /** pick the 3D point at the give location in the image plane */
    bool rtPick(float x, float y, Vec3f& p, const Ref<RTCamera>& camera, const Ref<RTScene>& scene);
  };
}

#endif
