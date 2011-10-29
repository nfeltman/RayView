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

#include "default.h"
#include "sys/tasking.h"
#define RT_API_SYMBOL __dllexport

/* include general stuff */
#include "api/parms.h"
#include "api/scene.h"
#include "sys/sync/mutex.h"

/* include all cameras */
#include "cameras/pinholecamera.h"

/* include all lights */
#include "lights/ambientlight.h"
#include "lights/pointlight.h"
#include "lights/spotlight.h"
#include "lights/directionallight.h"
#include "lights/distantlight.h"
#include "lights/hdrilight.h"
#include "lights/trianglelight.h"

/* include all materials */
#include "materials/matte.h"
#include "materials/plastic.h"
#include "materials/dielectric.h"
#include "materials/thindielectric.h"
#include "materials/mirror.h"
#include "materials/metal.h"
#include "materials/metallicpaint.h"
#include "materials/matte_textured.h"
#include "materials/obj.h"
#include "materials/velvet.h"

/* include all shapes */
#include "shapes/triangle.h"
#include "shapes/trianglemesh.h"
#include "shapes/trianglemesh_normals.h"
#include "shapes/trianglemesh_consistent_normals.h"

/* include all textures */
#include "textures/nearestneighbor.h"

/* include all renderers */
#include "renderers/debugrenderer.h"
#include "renderers/integratorrenderer.h"

/* include ray tracing core interface */
#include "rtcore/rtcore.h"

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#define strcasecmp lstrcmpiA
#pragma warning(disable:4297) // function assumed not to throw an exception but does
#endif

namespace embree
{
  /*******************************************************************
                  type definitions
  *******************************************************************/

  /* generic handle interface */
  struct _RTHandle {
  public:

    /*! Virtual destructor for handles. */
    virtual ~_RTHandle() {}

    /*! Recreated the object the handle refers to. */
    virtual void create() = 0;

    /*! Sets a parameter of the handle. */
    virtual void set(const std::string& property, const embree::Variant& data) = 0;
  };
  typedef _RTHandle* RTHandle;

  /* camera handle */
  typedef struct _RTCamera : public _RTHandle { }* RTCamera;

  /* image handle */
  typedef struct _RTImage : public _RTHandle { }* RTImage;

  /* texture handle */
  typedef struct _RTTexture : public _RTHandle { }* RTTexture;

  /* material handle */
  typedef struct _RTMaterial : public _RTHandle { }* RTMaterial;

  /* shape handle */
  typedef struct _RTShape : public _RTHandle { }* RTShape;

  /* light handle */
  typedef struct _RTLight : public _RTHandle { }* RTLight;

  /* primitive handle */
  typedef struct _RTPrimitive : public _RTHandle { }* RTPrimitive;

  /* scene handle */
  typedef struct _RTScene : public _RTHandle { }* RTScene;

  /* renderer handle */
  typedef struct _RTRenderer : public _RTHandle { }* RTRenderer;

  /* framebuffer handle */
  typedef struct _RTFrameBuffer : public _RTHandle { }* RTFrameBuffer;

  /*! Ray structure. Describes ray layout for ray queries. */
  struct RTRay {
    struct vec3 { float x,y,z; };
    vec3 org;    //!< origin of ray
    float _near; //!< start of ray segment
    vec3 dir;    //!< direction of ray
    float _far;  //!< end of ray segment
  };

  /*! Hit structure. Describes hit layout for ray queries. */
  struct RTHit {
    int prim;    //!< ID of hit primitive
    float dist;  //!< distance of hit
  };

  /*******************************************************************
                             globals
  *******************************************************************/

  static bool initialized = false;
  static MutexSys* mutex = new MutexSys;

  /*******************************************************************
                 generic handle implementations
  *******************************************************************/

  /*! Base Handle */
  template<typename B> class BaseHandle : public _RTHandle {
  public:
    Parms parms;     //!< Parameter container for handle.
    Ref<B> instance; //!< Referenced object.
  };

  /*! Normal handle. A normal handle type that buffers parameters and
   *  can create the underlying object. */
  template<typename T, typename B> class NormalHandle : public BaseHandle<B> {
  public:

    /*! Creates a new object. */
    void create() { this->instance = new T(this->parms); }

    /*! Sets a new parameter. */
    void set(const std::string& property, const Variant& data) { this->parms.add(property,data); }
  };

  /*! Constant handle. A special handle type that does not allow
   *  setting parameters. */
  template<typename T> class ConstHandle : public _RTHandle {
  public:

    /*! Creates a constant handle from the object to reference. */
    ConstHandle(const Ref<T>& ptr) : instance(ptr) {}

    /*! Recreating the underlying object is not allowed. */
    void create() { throw std::runtime_error("cannot modify constant handle"); }

    /*! Setting parameters is not allowed. */
    void set(const std::string& property, const Variant& data) { throw std::runtime_error("cannot modify constant handle"); }

  public:
    Ref<T> instance;  //!< Referenced object.
  };

  /*! Safe handle cast. Casts safely to a specified output type.  */
  template<typename T> T* castHandle(RTHandle handle_i, const char* name) {
    T* handle = dynamic_cast<T*>(handle_i);
    if (!handle          ) throw std::runtime_error("invalid "+std::string(name)+" handle");
    if (!handle->instance) throw std::runtime_error("invalid "+std::string(name)+" value");
    return handle;
  }

  /*******************************************************************
                 shape handle implementations
  *******************************************************************/

  /*! Shape handles. A shape handle is a special handle for holding
   * shapes. */
  class ShapeHandle : public _RTHandle {
  public:
    Ref<Shape> instance; //!< Referenced shape.
  };

  /*! creates triangle meshes */
  Ref<Shape> createTriangleMesh(bool consistentNormals,

                                size_t numVertices,     /*!< Number of mesh vertices.                 */
                                const char* position,   /*!< Pointer to vertex positions.             */
                                size_t stridePositions, /*!< Stride of vertex positions.              */

                                const char* normal,     /*!< Optional poiner to vertex normals.       */
                                size_t strideNormals,   /*!< Stride of vertex positions.              */

                                const char* texcoord,   /*!< Optional pointer to texture coordinates. */
                                size_t strideTexCoords, /*!< Stride of vertex positions.              */

                                size_t numTriangles,    /*!< Number of mesh triangles.                */
                                const char* triangles,  /*!< Pointer to triangle indices.             */
                                size_t strideTriangles  /*!< Stride of vertex positions.              */)
  {
    if (consistentNormals)
      return new TriangleMeshConsistentNormals(numVertices, position,stridePositions, normal,strideNormals, texcoord,strideTexCoords,
                                               numTriangles,triangles,strideTriangles);
    else {
      if (triangles && position && normal && !texcoord)
        return new TriangleMeshWithNormals(numVertices, position,stridePositions, normal,strideNormals,
                                           numTriangles,triangles,strideTriangles);
      else
        return new TriangleMesh(numVertices, position,stridePositions, normal,strideNormals, texcoord,strideTexCoords,
                              numTriangles,triangles,strideTriangles);
    }
  }

  /*! Triangle Handle */
  class TriangleHandle : public ShapeHandle {
  public:

    /*! Default construction. */
    TriangleHandle () : v0(zero), v1(zero), v2(zero) { }

    /*! Creates a new triangle. */
    void create() { instance = new Triangle(v0,v1,v2); }

    /*! Sets triangle parameters. */
    void set(const std::string& property, const Variant& data) {
      if      (property == "v0") v0 = data.getVec3f();
      else if (property == "v1") v1 = data.getVec3f();
      else if (property == "v2") v2 = data.getVec3f();
      else throw std::runtime_error("unknown triangle property: "+property);
    }
  public:
    Vec3f v0;   //!< 1st vertex of triangle
    Vec3f v1;   //!< 2nd vertex of triangle
    Vec3f v2;   //!< 3rd vertex of triangle
  };

  /*! Sphere Handle */
  class SphereHandle : public ShapeHandle {
  public:

    /*! Default constructor. */
    SphereHandle () : pos(zero), radius(one), numTheta(10), numPhi(10), consistentNormals(false) { }

    /*! Creates a triangulated sphere. */
    void create()
    {
      /* temporary arrays that hold geometry */
      std::vector<Vec3f> positions;
      std::vector<Vec3f> normals;
      std::vector<Vec2f> texcoords;
      std::vector<Vec3i> triangles;

      /* triangulate sphere */
      for (int theta=0; theta<=numTheta; theta++)
      {
        for (int phi=0; phi<numPhi; phi++)
        {
          Vec3f p = Vec3f(sinf(theta*float(pi)/float(numTheta))*cosf(phi*2.0f*float(pi)/float(numPhi)),
                          sinf(theta*float(pi)/float(numTheta))*sinf(phi*2.0f*float(pi)/float(numPhi)),
                          cosf(theta*float(pi)/float(numTheta)));
          positions.push_back(radius*p+pos);
          normals.push_back(p);
          texcoords.push_back(Vec2f(phi*2.0f*float(pi)/float(numPhi),theta*float(pi)/float(numTheta)));
        }
        if (theta == 0) continue;
        for (int phi=1; phi<=numPhi; phi++) {
          int p00 = (theta-1)*numPhi+phi-1;
          int p01 = (theta-1)*numPhi+phi%numPhi;
          int p10 = theta*numPhi+phi-1;
          int p11 = theta*numPhi+phi%numPhi;
          if (theta > 1) triangles.push_back(Vec3i(p10,p01,p00));
          if (theta < numTheta) triangles.push_back(Vec3i(p11,p01,p10));
        }
      }
      instance = createTriangleMesh(
        consistentNormals,
        positions.size(),
        positions.size() ? (const char*)&positions[0] : NULL, sizeof(Vec3f),
        normals  .size() ? (const char*)&normals  [0] : NULL, sizeof(Vec3f),
        texcoords.size() ? (const char*)&texcoords[0] : NULL, sizeof(Vec2f),
        triangles.size(),
        triangles.size() ? (const char*)&triangles[0]: NULL,  sizeof(TriangleMesh::Triangle));
    }

    /*! Sets shere parameters. */
    void set(const std::string& property, const Variant& data) {
      if      (property == "P" ) pos    = data.getVec3f();
      else if (property == "r" ) radius = data.getFloat();
      else if (property == "numTheta") numTheta = data.getInt();
      else if (property == "numPhi"  ) numPhi   = data.getInt();
      else if (property == "consistentNormals") consistentNormals = data.getBool();
      else throw std::runtime_error("unknown sphere property: "+property);
    }

  public:
    Vec3f pos;       //!< Center of the sphere.
    float radius;    //!< Radius of the sphere.
    int numTheta;    //!< Subdivisions from north to south pole.
    int numPhi;      //!< Subdivisions from east to west.
    bool consistentNormals; //!< Activates consistent normal interpolation.
  };

  /*! Triangle Mesh Handle */
  class TriangleMeshHandle : public ShapeHandle
  {
  public:

    /*! Default constructor. */
    TriangleMeshHandle ()
      : numPositions(0), numNormals(0), numTexCoords(0), numTriangles(0),
        stridePositions(0), strideNormals(0), strideTexCoords(0), strideTriangles(0),
        position(NULL), normal(NULL), texcoord(NULL), triangle(NULL),
        consistentNormals(false) {}

    /*! Destroys all temporary arrays. */
    ~TriangleMeshHandle () {
      position = NULL;
      normal   = NULL;
      texcoord = NULL;
      triangle = NULL;
    }

    /*! Creates a new triangle mesh. */
    void create() {
      if (!triangle && numTriangles) throw std::runtime_error("invalid triangle pointer");
      if (!position && numPositions) throw std::runtime_error("invalid position pointer");
      if (!normal   && numNormals  ) throw std::runtime_error("invalid normal pointer");
      if (!texcoord && numTexCoords) throw std::runtime_error("invalid texcoord pointer");
      if (normal   && numNormals   != numPositions) throw std::runtime_error("number of normals does not match");
      if (texcoord && numTexCoords != numPositions) throw std::runtime_error("number of texcoords does not match");

      instance = createTriangleMesh(consistentNormals,
                                    numPositions, position,stridePositions, normal,strideNormals, texcoord,strideTexCoords,
                                    numTriangles,triangle,strideTriangles);
    }

    /*! Sets an array of the triangle mesh. */
    void set(const std::string& property, const Variant& data)
    {
      if (property == "positions") {
        if (data.type != Variant::FLOAT3) throw std::runtime_error("wrong position format");
        position = data.ptr;
        numPositions = data.size;
        stridePositions = data.stride;
      }
      else if (property == "normals") {
        if (data.type != Variant::FLOAT3) throw std::runtime_error("wrong normal format");
        normal = data.ptr;
        numNormals = data.size;
        strideNormals = data.stride;
      }
      else if (property == "texcoords" || property == "texcoords0") {
        if (data.type != Variant::FLOAT2) throw std::runtime_error("wrong texcoord0 format");
        texcoord = data.ptr;
        numTexCoords = data.size;
        strideTexCoords = data.stride;
      }
      else if (property == "indices") {
        if (data.type != Variant::INT3) throw std::runtime_error("wrong triangle format");
        triangle = data.ptr;
        numTriangles = data.size;
        strideTriangles = data.stride;
      }
      else if (property == "consistentNormals")
        consistentNormals = data.getBool();

      else throw std::runtime_error("unknown triangle mesh property: "+property);
    }

  public:
    size_t numPositions;     //!< Number of positions stored in position array.
    size_t numNormals;       //!< Number of normals stored in normal array.
    size_t numTexCoords;     //!< Number of texture coordinates stored in texcoord array.
    size_t numTriangles;     //!< Number of triangles stored in triangle array.
    size_t stridePositions;  //!< Stride of positions stored in position array.
    size_t strideNormals;    //!< Stride of normals stored in normal array.
    size_t strideTexCoords;  //!< Stride of texture coordinates stored in texcoord array.
    size_t strideTriangles;  //!< Stride of triangles stored in triangle array.
    const char* position;    //!< Array containing all positions.
    const char* normal;      //!< Array containing all normals.
    const char* texcoord;    //!< Array containging all texture coordinates.
    const char* triangle;    //!< Array containing all triangles.
    bool consistentNormals;  //!< Activates consistent normal interpolation.
  };

  /*! Primitive Handle */
  class PrimitiveHandle : public _RTHandle {
  public:

    /*! Constructs shape primitive. */
    PrimitiveHandle (const Ref<Shape>& shape, const Ref<Material>& material, const AffineSpace& transform)
      : shape(shape), material(material), transform(transform) {}

    /*! Constructs light primitive. */
    PrimitiveHandle (const Ref<Light>& light, const AffineSpace& transform)
      : light(light), transform(transform) {}

    /*! Creation is not allowed. */
    void create() { throw std::runtime_error("cannot modify constant handle"); }

    /*! Setting parameters is not allowed. */
    void set(const std::string& property, const Variant& data) { throw std::runtime_error("cannot modify constant handle"); }
  public:
    Ref<Shape> shape;       //!< Shape in case of a shape primitive
    Ref<Light> light;       //!< Light in case of a light primitive
    Ref<Material> material; //!< Material of shape primitive
    AffineSpace transform;  //!< Transformation of primitive
  };

  /*******************************************************************
                  initialization / cleanup
  *******************************************************************/

  void verifyInitialized() {
    if (!initialized) throw std::runtime_error("embree not initalized");
  }

  RT_API_SYMBOL void rtInit() {
    Lock<MutexSys> lock(*mutex);
    if (initialized) throw std::runtime_error("embree already initialized");
    //TaskScheduler::init(1); std::cout << "Using only one thread !!!!!!!" << std::endl;
    //TaskScheduler::init(2); std::cout << "Using only two threads !!!!!!!" << std::endl;
    TaskScheduler::init();
    initialized = true;
  }

  RT_API_SYMBOL void rtExit() {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    TaskScheduler::cleanup();
    initialized = false;
  }

  /*******************************************************************
                    creation of handles
  *******************************************************************/

  RT_API_SYMBOL RTCamera rtNewCamera(const char* type)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!strcasecmp(type,"pinhole")) return (RTCamera) new NormalHandle<PinholeCamera,Camera>;
    else throw std::runtime_error("unknown camera type: "+std::string(type));
  }

  RT_API_SYMBOL RTImage rtNewImage(const char* type, size_t width, size_t height, const void* data)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!strcasecmp(type,"RGB8")) {
      Ref<Image3c> image = new Image3c(width,height);
      memcpy(&image->data[0],data,width*height*3*sizeof(char));
      return (RTImage) new ConstHandle<Image3c>(image);
    }
    else if (!strcasecmp(type,"RGB_FLOAT32")) {
      Ref<Image3f> image = new Image3f(width,height);
      memcpy(&image->data[0],data,width*height*sizeof(Col3f));
      return (RTImage) new ConstHandle<Image3f>(image);
    }
    else throw std::runtime_error("unknown image type: "+std::string(type));
  }

  RT_API_SYMBOL RTTexture rtNewTexture(const char* type) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!strcasecmp(type,"nearest")) return (RTTexture) new NormalHandle<NearestNeighbor,Texture>;
    else if (!strcasecmp(type,"image")) return (RTTexture) new NormalHandle<NearestNeighbor,Texture>;
    else throw std::runtime_error("unsupported texture type: "+std::string(type));
  }

  RT_API_SYMBOL RTMaterial rtNewMaterial(const char* type)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if      (!strcasecmp(type,"Matte")         ) return (RTMaterial) new NormalHandle<Matte,Material>;
    else if (!strcasecmp(type,"Plastic")       ) return (RTMaterial) new NormalHandle<Plastic,Material>;
    else if (!strcasecmp(type,"Dielectric")    ) return (RTMaterial) new NormalHandle<Dielectric,Material>;
    else if (!strcasecmp(type,"Glass")         ) return (RTMaterial) new NormalHandle<Dielectric,Material>;
    else if (!strcasecmp(type,"ThinDielectric")) return (RTMaterial) new NormalHandle<ThinDielectric,Material>;
    else if (!strcasecmp(type,"ThinGlass")     ) return (RTMaterial) new NormalHandle<ThinDielectric,Material>;
    else if (!strcasecmp(type,"Mirror")        ) return (RTMaterial) new NormalHandle<Mirror,Material>;
    else if (!strcasecmp(type,"Metal")         ) return (RTMaterial) new NormalHandle<Metal,Material>;
    else if (!strcasecmp(type,"MetallicPaint") ) return (RTMaterial) new NormalHandle<MetallicPaint,Material>;
    else if (!strcasecmp(type,"MatteTextured") ) return (RTMaterial) new NormalHandle<MatteTextured,Material>;
    else if (!strcasecmp(type,"Obj")           ) return (RTMaterial) new NormalHandle<Obj,Material>;
    else if (!strcasecmp(type,"Velvet")        ) return (RTMaterial) new NormalHandle<Velvet,Material>;
    else throw std::runtime_error("unknown material type: "+std::string(type));
  }

  RT_API_SYMBOL RTShape rtNewShape(const char* type) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if      (!strcasecmp(type,"trianglemesh")) return (RTShape) new TriangleMeshHandle;
    else if (!strcasecmp(type,"triangle")    ) return (RTShape) new TriangleHandle;
    else if (!strcasecmp(type,"sphere")      ) return (RTShape) new SphereHandle;
    else throw std::runtime_error("unknown shape type: "+std::string(type));
  }

  RT_API_SYMBOL RTLight rtNewLight(const char* type)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if      (!strcasecmp(type,"ambientlight"    )) return (RTLight) new NormalHandle<AmbientLight,Light>;
    else if (!strcasecmp(type,"pointlight"      )) return (RTLight) new NormalHandle<PointLight,Light>;
    else if (!strcasecmp(type,"spotlight"       )) return (RTLight) new NormalHandle<SpotLight,Light>;
    else if (!strcasecmp(type,"directionallight")) return (RTLight) new NormalHandle<DirectionalLight,Light>;
    else if (!strcasecmp(type,"distantlight"    )) return (RTLight) new NormalHandle<DistantLight,Light>;
    else if (!strcasecmp(type,"hdrilight"       )) return (RTLight) new NormalHandle<HDRILight,Light>;
    else if (!strcasecmp(type,"trianglelight"   )) return (RTLight) new NormalHandle<TriangleLight,Light>;
    else throw std::runtime_error("unknown light type: "+std::string(type));
  }

  RT_API_SYMBOL RTPrimitive rtNewShapePrimitive(RTShape shape_i, RTMaterial material_i, float* xfm)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    ShapeHandle*          shape    = castHandle<ShapeHandle          >(shape_i   ,"shape"   );
    BaseHandle<Material>* material = castHandle<BaseHandle<Material> >(material_i,"material");
    AffineSpace space(one);
    if (xfm) {
      space.l.vx = Vec3f(xfm[0],xfm[1],xfm[2]);
      space.l.vy = Vec3f(xfm[3],xfm[4],xfm[5]);
      space.l.vz = Vec3f(xfm[6],xfm[7],xfm[8]);
      space.p    = Vec3f(xfm[9],xfm[10],xfm[11]);
    }
    return (RTPrimitive) new PrimitiveHandle(shape->instance,material->instance,space);
  }

  RT_API_SYMBOL RTPrimitive rtNewLightPrimitive(RTLight light_i, float* xfm)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    BaseHandle<Light>* light = castHandle<BaseHandle<Light> >(light_i,"light");
    AffineSpace space(one);
    if (xfm) {
      space.l.vx = Vec3f(xfm[0],xfm[1],xfm[2]);
      space.l.vy = Vec3f(xfm[3],xfm[4],xfm[5]);
      space.l.vz = Vec3f(xfm[6],xfm[7],xfm[8]);
      space.p  = Vec3f(xfm[9],xfm[10],xfm[11]);
    }
    return (RTPrimitive) new PrimitiveHandle(light->instance,space);
  }

  RT_API_SYMBOL RTScene rtNewScene(const char* type, const FileName& traceFile, RTPrimitive* prims, size_t size)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();

    Ref<BackendScene> scene = new BackendScene;
    vector_t<BuildTriangle> triangles;

    /* extract all primitives */
    for (size_t i=0; i<size; i++)
    {
      PrimitiveHandle* prim = dynamic_cast<PrimitiveHandle*>((RTHandle)prims[i]);
      if (!prim) throw std::runtime_error("invalid primitive");

      /* extract geometry */
      if (prim->shape)
      {
        Ref<Shape> shape = prim->shape->transform(prim->transform);
        size_t id = scene->add(new Instance(i,shape,prim->material,null));

        /* extract triangle mesh */
        if (Ref<TriangleMesh> mesh = shape.dynamicCast<TriangleMesh>()) {
          for (size_t j=0; j<mesh->triangles.size(); j++) {
            const TriangleMesh::Triangle& tri = mesh->triangles[j];
            triangles.push_back(BuildTriangle(mesh->position[tri.v0],mesh->position[tri.v1],mesh->position[tri.v2],(int)id,(int)j));
          }
        }

        /* extract triangle mesh with position and normals */
        else if (Ref<TriangleMeshWithNormals> nmesh = shape.dynamicCast<TriangleMeshWithNormals>()) {
          for (size_t j=0; j<nmesh->triangles.size(); j++) {
            const TriangleMeshWithNormals::Triangle& tri = nmesh->triangles[j];
            triangles.push_back(BuildTriangle(nmesh->vertices[tri.v0].p,nmesh->vertices[tri.v1].p,nmesh->vertices[tri.v2].p,(int)id,(int)j));
          }
        }

        /* extract consistent normal triangle mesh */
        else if (Ref<TriangleMeshConsistentNormals> cmesh = shape.dynamicCast<TriangleMeshConsistentNormals>()) {
          for (size_t j=0; j<cmesh->triangles.size(); j++) {
            const TriangleMeshConsistentNormals::Triangle& tri = cmesh->triangles[j];
            triangles.push_back(BuildTriangle(cmesh->position[tri.v0],cmesh->position[tri.v1],cmesh->position[tri.v2],(int)id,(int)j));
          }
        }

        /* extract single triangles */
        else if (Ref<Triangle> tri = prim->shape.dynamicCast<Triangle>()) {
          triangles.push_back(BuildTriangle(tri->v0,tri->v1,tri->v2,(int)id));
        }
      }

      /* extract lights */
      else if (prim->light)
      {
        Ref<Light> light = prim->light->transform(prim->transform);
        scene->add(prim->light);
        if (Ref<TriangleLight> trilight = light.dynamicCast<TriangleLight>()) {
          size_t id = scene->add(new Instance(i,trilight->shape(),null,trilight.cast<AreaLight>()));
          triangles.push_back(BuildTriangle(trilight->v0,trilight->v1,trilight->v2,(int)id));
        }
      }
      else throw std::runtime_error("invalid primitive");
    }

    /* build acceleration structure */
    scene->accel = rtcCreateAccel(type, traceFile, (const BuildTriangle*)triangles.begin(),triangles.size());

    return (RTScene) new ConstHandle<BackendScene>(scene);
  }

  RT_API_SYMBOL RTRenderer rtNewRenderer(const char* type)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if      (!strcasecmp(type,"debug")) return (RTRenderer) new NormalHandle<DebugRenderer,Renderer>;
    else if (!strcasecmp(type,"pathtracer")) {
      NormalHandle<IntegratorRenderer,Renderer>* handle = new NormalHandle<IntegratorRenderer,Renderer>;
      handle->set("integrator",Variant("pathtracer"));
      return (RTRenderer) handle;
    }
    else throw std::runtime_error("unknown renderer type: "+std::string(type));
  }

  RT_API_SYMBOL RTFrameBuffer rtNewFrameBuffer(const char* type, size_t width, size_t height) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!strcasecmp(type,"RGB_FLOAT32")) return (RTFrameBuffer) new ConstHandle<Film>(new Film(width,height));
    else throw std::runtime_error("unknown framebuffer type: "+std::string(type));
  }

  RT_API_SYMBOL void* rtMapFrameBuffer(RTFrameBuffer frameBuffer_i)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    ConstHandle<Film>* frameBuffer = castHandle<ConstHandle<Film> >(frameBuffer_i,"framebuffer");
    return &frameBuffer->instance->data[0];
  }

  RT_API_SYMBOL void rtUnmapFrameBuffer(RTFrameBuffer frameBuffer_i) {
    verifyInitialized();
    Lock<MutexSys> lock(*mutex);
    castHandle<ConstHandle<Film> >(frameBuffer_i,"framebuffer");
  }

  RT_API_SYMBOL void rtDelete(RTHandle handle) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    delete handle;
  }

  /*******************************************************************
                  setting of parameters
  *******************************************************************/

  RT_API_SYMBOL void rtSetBool1(RTHandle handle, const char* property, bool x) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x));
  }

  RT_API_SYMBOL void rtSetBool2(RTHandle handle, const char* property, bool x, bool y)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y));
  }

  RT_API_SYMBOL void rtSetBool3(RTHandle handle, const char* property, bool x, bool y, bool z) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y,z));
  }

  RT_API_SYMBOL void rtSetBool4(RTHandle handle, const char* property, bool x, bool y, bool z, bool w)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y,z,w));
  }

  RT_API_SYMBOL void rtSetInt1(RTHandle handle, const char* property, int x)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x));
  }

  RT_API_SYMBOL void rtSetInt2(RTHandle handle, const char* property, int x, int y)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y));
  }

  RT_API_SYMBOL void rtSetInt3(RTHandle handle, const char* property, int x, int y, int z)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y,z));
  }

  RT_API_SYMBOL void rtSetInt4(RTHandle handle, const char* property, int x, int y, int z, int w)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y,z,w));
  }

  RT_API_SYMBOL void rtSetFloat1(RTHandle handle, const char* property, float x)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x));
  }

  RT_API_SYMBOL void rtSetFloat2(RTHandle handle, const char* property, float x, float y)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y));
  }

  RT_API_SYMBOL void rtSetFloat3(RTHandle handle, const char* property, float x, float y, float z)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y,z));
  }

  RT_API_SYMBOL void rtSetFloat4(RTHandle handle, const char* property, float x, float y, float z, float w)  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(x,y,z,w));
  }

  RT_API_SYMBOL void rtSetArray(RTHandle handle, const char* property, const char* type, const void* ptr, size_t size, size_t stride)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    if      (!strcasecmp(type,"bool1" )) handle->set(property,Variant(ptr,Variant::BOOL1 ,size,stride == size_t(-1) ? 1*sizeof(bool ) : stride));
    else if (!strcasecmp(type,"bool2" )) handle->set(property,Variant(ptr,Variant::BOOL2 ,size,stride == size_t(-1) ? 2*sizeof(bool ) : stride));
    else if (!strcasecmp(type,"bool3" )) handle->set(property,Variant(ptr,Variant::BOOL3 ,size,stride == size_t(-1) ? 3*sizeof(bool ) : stride));
    else if (!strcasecmp(type,"bool4" )) handle->set(property,Variant(ptr,Variant::BOOL4 ,size,stride == size_t(-1) ? 4*sizeof(bool ) : stride));
    else if (!strcasecmp(type,"int1"  )) handle->set(property,Variant(ptr,Variant::INT1  ,size,stride == size_t(-1) ? 1*sizeof(int  ) : stride));
    else if (!strcasecmp(type,"int2"  )) handle->set(property,Variant(ptr,Variant::INT2  ,size,stride == size_t(-1) ? 2*sizeof(int  ) : stride));
    else if (!strcasecmp(type,"int3"  )) handle->set(property,Variant(ptr,Variant::INT3  ,size,stride == size_t(-1) ? 3*sizeof(int  ) : stride));
    else if (!strcasecmp(type,"int4"  )) handle->set(property,Variant(ptr,Variant::INT4  ,size,stride == size_t(-1) ? 4*sizeof(int  ) : stride));
    else if (!strcasecmp(type,"float1")) handle->set(property,Variant(ptr,Variant::FLOAT1,size,stride == size_t(-1) ? 1*sizeof(float) : stride));
    else if (!strcasecmp(type,"float2")) handle->set(property,Variant(ptr,Variant::FLOAT2,size,stride == size_t(-1) ? 2*sizeof(float) : stride));
    else if (!strcasecmp(type,"float3")) handle->set(property,Variant(ptr,Variant::FLOAT3,size,stride == size_t(-1) ? 3*sizeof(float) : stride));
    else if (!strcasecmp(type,"float4")) handle->set(property,Variant(ptr,Variant::FLOAT4,size,stride == size_t(-1) ? 4*sizeof(float) : stride));
    else throw std::runtime_error("unknown array type: "+std::string(type));
  }

  RT_API_SYMBOL void rtSetString(RTHandle handle, const char* property, const char* str) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    handle->set(property,Variant(str));
  }

  RT_API_SYMBOL void rtSetImage(RTHandle handle, const char* property, RTImage img) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    if (ConstHandle<Image3c>* cimage = dynamic_cast<ConstHandle<Image3c>*>((RTHandle)img)) {
      if (!cimage->instance) throw std::runtime_error("invalid image value");
      handle->set(property,Variant(cimage->instance.cast<Image>()));
    } else if (ConstHandle<Image3f>* fimage = dynamic_cast<ConstHandle<Image3f>*>((RTHandle)img)) {
      if (!fimage->instance) throw std::runtime_error("invalid image value");
      handle->set(property,Variant(fimage->instance.cast<Image>()));
   } else
      throw std::runtime_error("invalid image handle");
  }

  RT_API_SYMBOL void rtSetTexture(RTHandle handle, const char* property, RTTexture tex) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    BaseHandle<Texture>* texture = castHandle<BaseHandle<Texture> >(tex,"texture");
    handle->set(property,Variant(texture->instance));
  }

  RT_API_SYMBOL void rtSetTransform(RTHandle handle, const char* property,
                                    float vxx, float vxy, float vxz,
                                    float vyx, float vyy, float vyz,
                                    float vzx, float vzy, float vzz,
                                    float px, float py, float pz)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle  ) throw std::runtime_error("invalid handle"  );
    if (!property) throw std::runtime_error("invalid property");
    AffineSpace xfm(LinearSpace3f(Vec3f(vxx,vxy,vxz),Vec3f(vyx,vyy,vyz),Vec3f(vzx,vzy,vzz)),Vec3f(px,py,pz));
    handle->set(property,Variant(xfm));
  }

  RT_API_SYMBOL void rtCommit(RTHandle handle) {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();
    if (!handle) throw std::runtime_error("invalid handle");
    handle->create();
  }

  /*******************************************************************
                            render call
  *******************************************************************/

  RT_API_SYMBOL void rtRenderFrame(RTRenderer renderer_i, RTCamera camera_i, RTScene scene_i, RTFrameBuffer frameBuffer_i)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();

    /* extract objects from handles */
    BaseHandle<Renderer>*      renderer = castHandle<BaseHandle<Renderer> >(renderer_i,"renderer");
    BaseHandle<Camera>*        camera   = castHandle<BaseHandle<Camera> >(camera_i,"camera");
    ConstHandle<BackendScene>* scene    = castHandle<ConstHandle<BackendScene> >(scene_i,"scene");
    ConstHandle<Film>*         frameBuffer = castHandle<ConstHandle<Film> >(frameBuffer_i,"framebuffer");

    /* start rendering */
    renderer->instance->renderFrame(camera->instance,scene->instance,frameBuffer->instance);
  }

  RT_API_SYMBOL void rtTraceRays(const RTRay* rays, RTScene scene_i, RTHit* hits, size_t numRays)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();

    /* extract scene */
    ConstHandle<BackendScene>* scene = castHandle<ConstHandle<BackendScene> >(scene_i,"scene");

    /* trace rays */
    for (size_t i=0; i<numRays; i++)
    {
      const RTRay& r = rays[i];
      Ray ray(Vec3f(r.org.x,r.org.y,r.org.z),Vec3f(r.dir.x,r.dir.y,r.dir.z),r._near,r._far);
      Hit hit; scene->instance->accel->intersect(ray,hit,0);
      if (hit.id0 == -1) { hits[i].prim = -1; hits[i].dist = hit.t; }
      else { hits[i].prim = (int)scene->instance->geometry[hit.id0]->id; hits[i].dist = hit.t; }
    }
  }

  RT_API_SYMBOL bool rtPick(float x, float y, Vec3f& p, RTCamera camera_i, RTScene scene_i)
  {
    Lock<MutexSys> lock(*mutex);
    verifyInitialized();

    /* extract objects from handles */
    BaseHandle<Camera>*        camera = castHandle<BaseHandle<Camera> >(camera_i,"camera");
    ConstHandle<BackendScene>* scene  = castHandle<ConstHandle<BackendScene> >(scene_i,"scene");

    /* trace ray */
    Ray ray;
    camera->instance->ray(Vec2f(x,y), Vec2f(0.5f, 0.5f), ray);
    Hit hit; scene->instance->accel->intersect(ray,hit,-3);
    if (hit) { p = ray.org + hit.t * ray.dir; return true; }
    else return false;
  }
}
