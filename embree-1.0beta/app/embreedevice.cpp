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

#include "embreedevice.h"
#include "sys/sync/atomic.h"
#include "../renderer/api/api.h"

namespace embree
{
  /*******************************************************************
                    construction / destruction
  *******************************************************************/

  static Atomic initialized(0);

  Device::Device() {
    if (initialized++) return;
    rtInit();
  }

  Device::~Device() {
    if (--initialized) return;
    rtExit();
  }

  /*******************************************************************
                    creation of objects
  *******************************************************************/

  /** create a camera */
  Ref<Device::RTCamera> Device::rtNewCamera(const char* type) {
    return new Device::RTCamera (this,embree::rtNewCamera(type));
  }

  /** creates an image */
  Ref<Device::RTImage> Device::rtNewImage(const char* type, size_t width, size_t height, const void* data) {
    return new Device::RTImage (this,embree::rtNewImage(type,width,height,data));
  }

  /** creates a texture */
  Ref<Device::RTTexture> Device::rtNewTexture(const char* type) {
    return new Device::RTTexture (this,embree::rtNewTexture(type));
  }

  /** create a new material */
  Ref<Device::RTMaterial> Device::rtNewMaterial(const char* type) {
    return new Device::RTMaterial (this,embree::rtNewMaterial(type));
  }

  /** create a new shape */
  Ref<Device::RTShape> Device::rtNewShape(const char* type) {
    return new Device::RTShape (this,embree::rtNewShape(type));
  }

  /** create a new light source. */
  Ref<Device::RTLight> Device::rtNewLight(const char* type) {
    return new Device::RTLight (this,embree::rtNewLight(type));
  }

  /** create a new primitive */
  Ref<Device::RTPrimitive> Device::rtNewPrimitive(const Ref<RTShape>& shape, const Ref<RTMaterial>& material, const AffineSpace& transform) {
    float xfm[12];
    xfm[0] = transform.l.vx.x; xfm[1] = transform.l.vx.y; xfm[2] = transform.l.vx.z;
    xfm[3] = transform.l.vy.x; xfm[4] = transform.l.vy.y; xfm[5] = transform.l.vy.z;
    xfm[6] = transform.l.vz.x; xfm[7] = transform.l.vz.y; xfm[8] = transform.l.vz.z;
    xfm[9] = transform.p   .x; xfm[10]= transform.p   .y; xfm[11]= transform.p   .z;
    return new Device::RTPrimitive (this,embree::rtNewShapePrimitive((embree::RTShape)shape->handle,(embree::RTMaterial)material->handle,(float*)xfm));
  }

  /** create a new primitive */
  Ref<Device::RTPrimitive> Device::rtNewPrimitive(const Ref<RTLight>& light, const AffineSpace& transform) {
    float xfm[12];
    xfm[0] = transform.l.vx.x; xfm[1] = transform.l.vx.y; xfm[2] = transform.l.vx.z;
    xfm[3] = transform.l.vy.x; xfm[4] = transform.l.vy.y; xfm[5] = transform.l.vy.z;
    xfm[6] = transform.l.vz.x; xfm[7] = transform.l.vz.y; xfm[8] = transform.l.vz.z;
    xfm[9] = transform.p   .x; xfm[10]= transform.p   .y; xfm[11]= transform.p   .z;
    return new Device::RTPrimitive (this,embree::rtNewLightPrimitive((embree::RTLight)light->handle,(float*)xfm));
  }

  /** create a new scene. */
  Ref<Device::RTScene> Device::rtNewScene(const char* type, TraceData traceFile, Ref<RTPrimitive>* prims_i, size_t size) {
    embree::RTPrimitive* prims = new embree::RTPrimitive[size];
    for (size_t i=0; i<size; i++) prims[i] = (embree::RTPrimitive) prims_i[i]->handle;
    Ref<Device::RTScene> scene = new Device::RTScene (this,embree::rtNewScene(type,traceFile,prims,size));
    delete[] prims; prims = NULL;
    return scene;
  }

  /** creates a renderer */
  Ref<Device::RTRenderer> Device::rtNewRenderer(const char* type) {
    return new Device::RTRenderer (this,embree::rtNewRenderer(type));
  }

  /** creates a framebuffer */
  Ref<Device::RTFrameBuffer> Device::rtNewFrameBuffer(const char* type, size_t width, size_t height) {
    return new Device::RTFrameBuffer (this,embree::rtNewFrameBuffer(type,width,height));
  }

  /** map and unmap the framebuffer data */
  void* Device::rtMapFrameBuffer(const Ref<RTFrameBuffer>& frameBuffer) {
    return embree::rtMapFrameBuffer((embree::RTFrameBuffer)frameBuffer->handle);
  }

  void Device::rtUnmapFrameBuffer(const Ref<RTFrameBuffer>& frameBuffer) {
    return embree::rtUnmapFrameBuffer((embree::RTFrameBuffer)frameBuffer->handle);
  }


  /*******************************************************************
                  setting of parameters
  *******************************************************************/

  Device::RTHandle::~RTHandle() {
    embree::rtDelete((embree::RTHandle)handle);
  }

  void Device::RTHandle::rtSetBool1(const char* property, bool x) {
    embree::rtSetBool1((embree::RTHandle)handle,property,x);
  }

  void Device::RTHandle::rtSetBool2(const char* property, const Vec2b& v) {
    embree::rtSetBool2((embree::RTHandle)handle,property,v.x,v.y);
  }

  void Device::RTHandle::rtSetBool3(const char* property, const Vec3b& v) {
    embree::rtSetBool3((embree::RTHandle)handle,property,v.x,v.y,v.z);
  }

  void Device::RTHandle::rtSetBool4(const char* property, const Vec4b& v) {
    embree::rtSetBool4((embree::RTHandle)handle,property,v.x,v.y,v.z,v.w);
  }

  void Device::RTHandle::rtSetInt1(const char* property, int x) {
    embree::rtSetInt1((embree::RTHandle)handle,property,x);
  }

  void Device::RTHandle::rtSetInt2(const char* property, const Vec2i& v) {
    embree::rtSetInt2((embree::RTHandle)handle,property,v.x,v.y);
  }

  void Device::RTHandle::rtSetInt3(const char* property, const Vec3i& v) {
    embree::rtSetInt3((embree::RTHandle)handle,property,v.x,v.y,v.z);
  }

  void Device::RTHandle::rtSetInt4(const char* property, const Vec4i& v) {
    embree::rtSetInt4((embree::RTHandle)handle,property,v.x,v.y,v.z,v.w);
  }

  void Device::RTHandle::rtSetFloat1(const char* property, float x) {
    embree::rtSetFloat1((embree::RTHandle)handle,property,x);
  }

  void Device::RTHandle::rtSetFloat2(const char* property, const Vec2f& v) {
    embree::rtSetFloat2((embree::RTHandle)handle,property,v.x,v.y);
  }

  void Device::RTHandle::rtSetFloat3(const char* property, const Vec3f& v) {
    embree::rtSetFloat3((embree::RTHandle)handle,property,v.x,v.y,v.z);
  }

  void Device::RTHandle::rtSetFloat3(const char* property, const Col3f& v) {
    embree::rtSetFloat3((embree::RTHandle)handle,property,v.r,v.g,v.b);
  }

  void Device::RTHandle::rtSetFloat4(const char* property, const Vec4f& v) {
    embree::rtSetFloat4((embree::RTHandle)handle,property,v.x,v.y,v.z,v.w);
  }

  void Device::RTHandle::rtSetArray(const char* property, const char* type, const void* ptr, size_t size, size_t stride) {
    embree::rtSetArray((embree::RTHandle)handle,property,type,ptr,size,stride);
  }

  void Device::RTHandle::rtSetString(const char* property, const char* str) {
    embree::rtSetString((embree::RTHandle)handle,property,str);
  }

  void Device::RTHandle::rtSetImage(const char* property, const Ref<RTImage>& img) {
    embree::rtSetImage((embree::RTHandle)handle,property,(embree::RTImage)img->handle);
  }

  void Device::RTHandle::rtSetTexture(const char* property, const Ref<RTTexture>& tex) {
    embree::rtSetTexture((embree::RTHandle)handle,property,(embree::RTTexture)tex->handle);
  }

  void Device::RTHandle::rtSetTransform(const char* property, const AffineSpace& space) {
    embree::rtSetTransform((embree::RTHandle)handle,property,
                           space.l.vx.x,space.l.vx.y,space.l.vx.z,
                           space.l.vy.x,space.l.vy.y,space.l.vy.z,
                           space.l.vz.x,space.l.vz.y,space.l.vz.z,
                           space.p   .x,space.p   .y,space.p   .z);
  }

  void Device::RTHandle::rtCommit() {
    embree::rtCommit((embree::RTHandle)handle);
  }

  /*******************************************************************
                      render call
  *******************************************************************/

  /** render a frame using current renderer settings. */
  void Device::rtRenderFrame(const Ref<RTRenderer>& renderer, const Ref<RTCamera>& camera, const Ref<RTScene>& scene, const Ref<RTFrameBuffer>& frameBuffer) {
    embree::rtRenderFrame((embree::RTRenderer)renderer->handle,
                          (embree::RTCamera)camera->handle,
                          (embree::RTScene)scene->handle,
                          (embree::RTFrameBuffer)frameBuffer->handle);
  }

  /** pick the 3D point at the give location in the image plane */
  bool Device::rtPick(float x, float y, Vec3f& p, const Ref<RTCamera>& camera, const Ref<RTScene>& scene) {
    return embree::rtPick(x, y, p, (embree::RTCamera)camera->handle, (embree::RTScene)scene->handle);
  }
}
