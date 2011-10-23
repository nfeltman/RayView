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

#ifndef __EMBREE_API_H__
#define __EMBREE_API_H__

#include <stddef.h>
#include "sys/filename.h"

#ifndef RT_API_SYMBOL
#define RT_API_SYMBOL extern "C"  //!< makes API functions use C name mangling
#endif

/*! \file api.h This file implements the interface to the renderer
 *  backend. The library has to get initialized by calling rtInit() at
 *  the beginning and rtExit() at the end of your application. The API
 *  is completely functional, meaning that objects can NOT be
 *  modified. Handles are references to objects, and objects are
 *  internally reference counted, thus destroyed when no longer
 *  needed. Calling rtDelete does only delete the handle, not the
 *  object the handle references. The first parameter of every
 *  rtNewXXX function is a type that selects the object to create. For
 *  an overview of the parameters supported by a specific object see
 *  the header file of the object implementation. Using the rtSetXXX
 *  functions buffers the set values inside the handle. A subsequent
 *  call to rtCommit will set the handle reference to a new object
 *  with the changed parameters. The original object is not changed by
 *  this process. The semantics of modifying an object A used by
 *  another object B can only be achieved by creating A' and a new B'
 *  that uses A'. The RTImage, RTFramebuffer, and RTScene handles are
 *  constant, thus the rtSetXXX and rtCommit function cannot be used
 *  for them. */

namespace embree
{

  /*******************************************************************
                     type definitions
  *******************************************************************/

  /*! Generic handle. */
  typedef struct _RTHandle {}* RTHandle;

  /*! Transformation handle. */
  typedef struct _RTTransform : public _RTHandle { }* RTTransform;

  /*! Camera handle. */
  typedef struct _RTCamera : public _RTHandle { }* RTCamera;

  /*! Image handle (constant). */
  typedef struct _RTImage : public _RTHandle { }* RTImage;

  /*! Texture handle. */
  typedef struct _RTTexture : public _RTHandle { }* RTTexture;

  /*! Material handle. */
  typedef struct _RTMaterial : public _RTHandle { }* RTMaterial;

  /*! Shape handle. */
  typedef struct _RTShape : public _RTHandle { }* RTShape;

  /*! Light handle. */
  typedef struct _RTLight : public _RTHandle { }* RTLight;

  /*! Primitive handle. */
  typedef struct _RTPrimitive : public _RTHandle { }* RTPrimitive;

  /*! Scene handle (constant). */
  typedef struct _RTScene : public _RTHandle { }* RTScene;

  /*! Renderer handle. */
  typedef struct _RTRenderer : public _RTHandle { }* RTRenderer;

  /*! Framebuffer handle (constant). */
  typedef struct _RTFrameBuffer : public _RTHandle { }* RTFrameBuffer;

  /*! Ray structure. Describes ray layout for ray queries. */
  struct RTRay {
    struct vec3 { float x,y,z; };
    vec3 org;   //!< origin of ray
    float near; //!< start of ray segment
    vec3 dir;   //!< direction of ray
    float far;  //!< end of ray segment
  };

  /*! Hit structure. Describes hit layout for ray queries. */
  struct RTHit {
    int prim;    //!< ID of hit primitive
    float dist;  //!< distance of hit
  };

  /*******************************************************************
                    initialization / cleanup
  *******************************************************************/

  /*! Initialized the Embree library. */
  RT_API_SYMBOL void rtInit();

  /*! Cleanup of the Embree library. */
  RT_API_SYMBOL void rtExit();


  /*******************************************************************
                      creation of objects
  *******************************************************************/

  /*! Creates a new camera. \param type is the type of camera to
   *  create (e.g. "pinhole"). \returns camera handle. */
  RT_API_SYMBOL RTCamera rtNewCamera(const char* type);

  /*! Creates a new image. The data gets directly copied by this
   *  function. \param type is the type of image to create
   *  (e.g. "RGB8", "RGB_FLOAT32"). \param width is the width of the
   *  image \param height is the height of the image \param data is a
   *  pointer to the image data. \returns image handle */
  RT_API_SYMBOL RTImage rtNewImage(const char* type, size_t width, size_t height, const void* data);

  /*! Creates a texture. \param type is the type of texture to create
   *  (e.g. "image" mapped). \returns texture handle */
  RT_API_SYMBOL RTTexture rtNewTexture(const char* type);

  /*! Creates a new material. \param type is the type of material to
   *  create (e.g. "Matte", "Plastic", "Dielectric", "ThinDielectric",
   *  "Mirror", "Metal", "MetallicPaint", "MatteTextured",
   *  "Obj"). \returns material handle */
  RT_API_SYMBOL RTMaterial rtNewMaterial(const char* type);

  /*! Creates a new shape. \param type is the type of shape to create
   *  (e.g. "trianglemesh", "triangle", "sphere") \returns shape
   *  handle */
  RT_API_SYMBOL RTShape rtNewShape(const char* type);

  /*! Creates a new light source. \param type is the type of shape to
   *  create (e.g. "ambientlight", "pointlight", "spotlight",
   *  "directionallight", "distantlight", "hdrilight",
   *  "trianglelight"). \returns light handle */
  RT_API_SYMBOL RTLight rtNewLight(const char* type);

  /*! Creates a new shape primitive. \param shape is the shape to
   *  instantiate \param material is the material to attach to the
   *  shape \param transform is an optional pointer to a
   *  transformation to transform the shape \returns primitive
   *  handle */
  RT_API_SYMBOL RTPrimitive rtNewShapePrimitive(RTShape shape, RTMaterial material, float* transform = NULL);

  /*! Creates a new light primitive. \param light is the light to
   *  instantiate \param transform is an optional pointer to a
   *  transformation to transform the shape \returns primitive
   *  handle */
  RT_API_SYMBOL RTPrimitive rtNewLightPrimitive(RTLight light, float* transform = NULL);

  /*! Creates a new scene. \param type is the type of acceleration
   *  structure of the scene (e.g. "bvh2", "bvh4", "bvh4.spatial")
   *  \param prims is a pointer to an array of primitives
   *  \param size is the number of primitives in that array \returns
   *  scene handle */
  RT_API_SYMBOL RTScene rtNewScene(const char* type, const FileName& traceFile, RTPrimitive* prims, size_t size);

  /*! Creates a new renderer. \param type is the type of renderer to
   *  create (e.g. "debug", "pathtracer"). \returns renderer handle */
  RT_API_SYMBOL RTRenderer rtNewRenderer(const char* type);

  /*! Creates a new framebuffer. \param type is the type of
   *  framebuffer to create (e.g. "RGB_FLOAT32"). \returns framebuffer
   *  handle */
  RT_API_SYMBOL RTFrameBuffer rtNewFrameBuffer(const char* type, size_t width, size_t height);

  /*! Map the framebuffer data. \param frameBuffer is the framebuffer
   *  to map \returns pointer to framebuffer data */
  RT_API_SYMBOL void* rtMapFrameBuffer(RTFrameBuffer frameBuffer);

  /*! Unmap the framebuffer data. \param frameBuffer is the
   *  framebuffer to unmap. */
  RT_API_SYMBOL void rtUnmapFrameBuffer(RTFrameBuffer frameBuffer);

  /*! Deletes a handle. The referenced object is not necessarily
   *  deleted too. */
  RT_API_SYMBOL void rtDelete(RTHandle handle);


  /*******************************************************************
                    setting of parameters
  *******************************************************************/

  /*! Sets a boolean parameter of the handle. */
  RT_API_SYMBOL void rtSetBool1(RTHandle handle, const char* property, bool x);

  /*! Sets a bool2 parameter of the handle. */
  RT_API_SYMBOL void rtSetBool2(RTHandle handle, const char* property, bool x, bool y);

  /*! Sets a bool3 parameter of the handle. */
  RT_API_SYMBOL void rtSetBool3(RTHandle handle, const char* property, bool x, bool y, bool z);

  /*! Sets a bool4 parameter of the handle. */
  RT_API_SYMBOL void rtSetBool4(RTHandle handle, const char* property, bool x, bool y, bool z, bool w);

  /*! Sets an integer parameter of the handle. */
  RT_API_SYMBOL void rtSetInt1(RTHandle handle, const char* property, int x);

  /*! Sets an int2 parameter of the handle. */
  RT_API_SYMBOL void rtSetInt2(RTHandle handle, const char* property, int x, int y);

  /*! Sets an int3 parameter of the handle. */
  RT_API_SYMBOL void rtSetInt3(RTHandle handle, const char* property, int x, int y, int z);

  /*! Sets an int4 parameter of the handle. */
  RT_API_SYMBOL void rtSetInt4(RTHandle handle, const char* property, int x, int y, int z, int w);

  /*! Sets a float parameter of the handle. */
  RT_API_SYMBOL void rtSetFloat1(RTHandle handle, const char* property, float x);

  /*! Sets a float2 parameter of the handle. */
  RT_API_SYMBOL void rtSetFloat2(RTHandle handle, const char* property, float x, float y);

  /*! Sets a float3 parameter of the handle. */
  RT_API_SYMBOL void rtSetFloat3(RTHandle handle, const char* property, float x, float y, float z);

  /*! Sets a float4 parameter of the handle. */
  RT_API_SYMBOL void rtSetFloat4(RTHandle handle, const char* property, float x, float y, float z, float w);

  /*! Sets an typed array parameter of the handle. The data is copied when calling rtCommit. */
  RT_API_SYMBOL void rtSetArray(RTHandle handle, const char* property, const char* type, const void* ptr, size_t size, size_t stride = size_t(-1));

  /*! Sets a string parameter of the handle. */
  RT_API_SYMBOL void rtSetString(RTHandle handle, const char* property, const char* str);

  /*! Sets an image parameter of the handle. */
  RT_API_SYMBOL void rtSetImage(RTHandle handle, const char* property, RTImage img);

  /*! Sets a texture parameter of the handle. */
  RT_API_SYMBOL void rtSetTexture(RTHandle handle, const char* property, RTTexture tex);

  /*! Sets a transformation of the handle. */
  RT_API_SYMBOL void rtSetTransform(RTHandle handle, const char* property,
                                    float vxx, float vxy, float vxz,
                                    float vyx, float vyy, float vyz,
                                    float vzx, float vzy, float vzz,
                                    float px, float py, float pz);

  /*! Commits all changes by setting the reference of the handle to a
   *  new object with specified parameters. */
  RT_API_SYMBOL void rtCommit(RTHandle handle);


  /*******************************************************************
                          render calls
  *******************************************************************/

  /*! Renders a frame. \param renderer is the renderer to use \param
   *  camera is the camera to use \param scene is the scene to render
   *  \parm frameBuffer is the framebuffer to render into */
  RT_API_SYMBOL void rtRenderFrame(RTRenderer renderer, RTCamera camera, RTScene scene, RTFrameBuffer frameBuffer);

  /*! Traces rays. \param rays is an array of rays to trace \parm
   *  scene is the scene to trace the rays in \param hits is an array
   *  that is overwritten with the result \param numRays are the
   *  number of rays to shoot */
  RT_API_SYMBOL void rtTraceRays(const RTRay* rays, RTScene scene, RTHit* hits, size_t numRays);

  /*! Pick a 3D point. \returns true if a point was picked, false otherwise
   *  \parm x is the x coordinate [0:1] in the image plane
   *  \parm y is the y coordinate [0:1] in the image plane
   *  \parm p is the world space position of the picked point, if any
   *  \parm camera is the camera to use \param scene is the scene for picking */
  RT_API_SYMBOL bool rtPick(float x, float y, Vec3f& p, RTCamera cmaera, RTScene scene);
}

#endif

