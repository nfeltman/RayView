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

#include "sys/platform.h"
#include "sys/filename.h"
#include "image/image.h"
#include "lexers/streamfilters.h"
#include "lexers/parsestream.h"

#include "scene.h"
#include "embreedevice.h"
#include "glutdisplay.h"

namespace embree
{
  /******************************************************************************/
  /*                                  State                                     */
  /******************************************************************************/

  /* camera settings */
  Vec3f g_camPos    = Vec3f(0.0f,0.0f,0.0f);
  Vec3f g_camLookAt = Vec3f(1.0f,0.0f,0.0f);
  Vec3f g_camUp     = Vec3f(0,1,0);
  float g_camAngle  = 64.0f;
  float g_camRadius = 0.0f;

  /* device, renderer, and scene */
  Ref<Device> g_device = null;
  Ref<Device::RTRenderer> g_renderer = null;
  std::string g_accel = "default";
  Ref<GroupNode> g_scene = new GroupNode;
  int g_depth = -1;
  int g_spp = 1;

  /* output settings */
  bool g_rendered = false;
  bool g_refine = true;
  float g_gamma = 1.0f;
  bool g_fullscreen = false;
  size_t g_width = 512, g_height = 512;
  Ref<Device::RTFrameBuffer> g_frameBuffer = NULL;
  Ref<Device::RTImage> g_backplate = NULL;

  /* regression testing mode */
  bool g_regression = false;

  /******************************************************************************/
  /*                            Object Creation                                 */
  /******************************************************************************/

  Ref<Device::RTCamera> createCamera(const AffineSpace& space)
  {
    Ref<Device::RTCamera> camera = g_device->rtNewCamera("pinhole");
    camera->rtSetTransform("local2world",space);
    camera->rtSetFloat1("angle",g_camAngle);
    camera->rtSetFloat1("aspectRatio",float(g_width)/float(g_height));
    camera->rtSetFloat1("lensRadius", g_camRadius);
    camera->rtSetFloat1("focalDistance",length(g_camLookAt - g_camPos));
    camera->rtCommit();
    return camera;
  }

  Ref<Device::RTImage> loadRTImage(const FileName fileName)
  {
    static std::map<std::string,Ref<Device::RTImage> > image_map;

    if (image_map.find(fileName.str()) != image_map.end())
      return image_map[fileName.str()];

    Ref<Image> image = loadImage(fileName,true);

    if (!image) {
      int white = 0xFFFFFFFF;
      image_map[fileName.str()] = g_device->rtNewImage("RGB8",1,1,&white);
    }
    else if (Ref<Image3f> fimg = image.dynamicCast<Image3f>())
      image_map[fileName.str()] = g_device->rtNewImage("RGB_FLOAT32",fimg->width,fimg->height,&fimg->data[0]);
    else if (Ref<Image3c> cimg = image.dynamicCast<Image3c>())
      image_map[fileName.str()] = g_device->rtNewImage("RGB8",cimg->width,cimg->height,&cimg->data[0]);
    else
      throw std::runtime_error("unknown image type");

    return image_map[fileName.str()];
  }

  Ref<Device::RTTexture> loadTexture(const FileName fileName)
  {
    static std::map<std::string,Ref<Device::RTTexture> > texture_map;

    if (texture_map.find(fileName.str()) != texture_map.end())
      return texture_map[fileName.str()];

    Ref<Device::RTTexture> texture = g_device->rtNewTexture("nearest");
    texture->rtSetImage("image",loadRTImage(fileName));
    texture->rtCommit();

    return texture_map[fileName.str()] = texture;
  }

  Ref<Device::RTScene> createScene(const Ref<Scene>& root, const FileName traceFile)
  {
    std::vector<Ref<Device::RTPrimitive> > prims;
    for (Scene::iterator i=root->begin(); i!=root->end(); i++)
    {
      if (Ref<LightNode> lnode = (*i).node.dynamicCast<LightNode>())
        prims.push_back(g_device->rtNewPrimitive(lnode->light,(*i).space));
      else if (Ref<ShapeNode> snode = (*i).node.dynamicCast<ShapeNode>())
        prims.push_back(g_device->rtNewPrimitive(snode->shape,snode->material,(*i).space));
      else
        throw std::runtime_error("invalid scene graph leaf node");
    }
  Ref<Device::RTScene> scene = g_device->rtNewScene(g_accel.c_str(),traceFile, prims.size() == 0 ? NULL : &prims[0],prims.size());
    return scene;
  }

  /******************************************************************************/
  /*                      Command line parsing                                  */
  /******************************************************************************/

  static Ref<Device::RTRenderer> parseDebugRenderer(Ref<ParseStream> cin, const FileName& path)
  {
    Ref<Device::RTRenderer> renderer = g_device->rtNewRenderer("debug");
    if (g_depth >= 0) renderer->rtSetInt1("maxDepth",g_depth);

    if (cin->peek() != "{") goto finish;
    cin->drop();

    while (cin->peek() != "}") {
      std::string tag = cin->getString();
      cin->force("=");
      if (tag == "depth") renderer->rtSetInt1("maxDepth",cin->getInt());
      else std::cout << "unknown tag \"" << tag << "\" in debug renderer parsing" << std::endl;
    }
    cin->drop();

  finish:
    renderer->rtCommit();
    return renderer;
  }

  static Ref<Device::RTRenderer> parsePathTracer(Ref<ParseStream> cin, const FileName& path)
  {
    Ref<Device::RTRenderer> renderer = g_device->rtNewRenderer("pathtracer");
    renderer->rtSetFloat1("gamma",g_gamma);
    if (g_depth >= 0) renderer->rtSetInt1("maxDepth",g_depth);
    renderer->rtSetInt1("sampler.spp",g_spp);
    if (g_backplate) renderer->rtSetImage("backplate",g_backplate);

    if (cin->peek() != "{") goto finish;
    cin->drop();

    while (cin->peek() != "}") {
      std::string tag = cin->getString();
      cin->force("=");
      if      (tag == "depth"          ) renderer->rtSetInt1  ("maxDepth"       ,cin->getInt()  );
      else if (tag == "spp"            ) renderer->rtSetInt1  ("sampler.spp"    ,cin->getInt()  );
      else if (tag == "minContribution") renderer->rtSetFloat1("minContribution",cin->getFloat());
      else if (tag == "backplate"      ) renderer->rtSetImage ("backplate",loadRTImage(path + cin->getFileName()));
      else std::cout << "unknown tag \"" << tag << "\" in debug renderer parsing" << std::endl;
    }
    cin->drop();

  finish:
    renderer->rtCommit();
    return renderer;
  }

  static void displayMode()
  {
    if (!g_renderer) throw std::runtime_error("no renderer set");

    OrthonormalSpace camSpace = OrthonormalSpace::lookAtPoint(g_camPos,g_camLookAt,g_camUp);
    float speed = 0.02f * length(g_camLookAt-g_camPos);
    GLUTDisplay(camSpace,speed,createScene(g_scene.cast<Scene>(), FileName("")));
    g_rendered = true;
  }

  static void outputMode(const FileName& fileName)
  {
    if (!g_renderer) throw std::runtime_error("no renderer set");

    /* render */
    Ref<Device::RTCamera> camera = createCamera(AffineSpace::lookAtPoint(g_camPos,g_camLookAt,g_camUp));
    Ref<Device::RTScene> scene = createScene(g_scene.cast<Scene>(), FileName(""));
    g_device->rtRenderFrame(g_renderer,camera,scene,g_frameBuffer);

    /* store to disk */
    void* ptr = g_device->rtMapFrameBuffer(g_frameBuffer);
    Ref<Image3f> image = new Image3f(g_width,g_height);
    memcpy(&image->data[0],ptr,g_width*g_height*sizeof(Col3f));
    storeImage(image.cast<Image>(),fileName);
    g_device->rtUnmapFrameBuffer(g_frameBuffer);
    g_rendered = true;
  }

  static void traceMode(const FileName& fileName)
  {
    if (!g_renderer) throw std::runtime_error("no renderer set");

    /* render */
    Ref<Device::RTCamera> camera = createCamera(AffineSpace::lookAtPoint(g_camPos,g_camLookAt,g_camUp));
    Ref<Device::RTScene> scene = createScene(g_scene.cast<Scene>(), fileName);
    g_device->rtRenderFrame(g_renderer,camera,scene,g_frameBuffer);
    g_rendered = true;
  }

  static void parseCommandLine(Ref<ParseStream> cin, const FileName& path)
  {
    while (true)
    {
      std::string tag = cin->getString();
      if (tag == "") return;

      /* parse command line parameters from a file */
      if (tag == "-c") {
        FileName file = path + cin->getFileName();
        parseCommandLine(new ParseStream(new LineCommentFilter(file,"#")),file.path());
      }

      /* read model from file */
      else if (tag == "-i")
        *g_scene += load(path+cin->getFileName());

      /* triangulated sphere */
      else if (tag == "-trisphere")
      {
        Ref<Device::RTShape> sphere = g_device->rtNewShape("sphere");
        sphere->rtSetFloat3("P",cin->getVec3f());
        sphere->rtSetFloat1("r",cin->getFloat());
        sphere->rtSetInt1("numTheta",cin->getInt());
        sphere->rtSetInt1("numPhi",cin->getInt());
        sphere->rtCommit();

        Ref<Device::RTMaterial> material = g_device->rtNewMaterial("matte");
        material->rtSetFloat3("reflection",Col3f(1.0f,0.0f,0.0f));
        material->rtCommit();

        *g_scene += new ShapeNode(sphere,material);
      }

      /* ambient light source */
      else if (tag == "-ambientlight") {
        Ref<Device::RTLight> light = g_device->rtNewLight("ambientlight");
        light->rtSetFloat3("L",cin->getVec3f());
        light->rtCommit();
        *g_scene += new LightNode(light);
      }

      /* point light source */
      else if (tag == "-pointlight") {
        Ref<Device::RTLight> light = g_device->rtNewLight("pointlight");
        light->rtSetFloat3("P",cin->getVec3f());
        light->rtSetFloat3("I",cin->getVec3f());
        light->rtCommit();
        *g_scene += new LightNode(light);
      }

      /* distant light source */
      else if (tag == "-distantlight") {
        Ref<Device::RTLight> light = g_device->rtNewLight("distantlight");
        light->rtSetFloat3("D",cin->getVec3f());
        light->rtSetFloat3("L",cin->getVec3f());
        light->rtSetFloat1("halfAngle",cin->getFloat());
        light->rtCommit();
        *g_scene += new LightNode(light);
      }

      /* triangular light source */
      else if (tag == "-trianglelight") {
        Vec3f P = cin->getVec3f();
        Vec3f U = cin->getVec3f();
        Vec3f V = cin->getVec3f();
        Vec3f L = cin->getVec3f();
        Ref<Device::RTLight> light = g_device->rtNewLight("trianglelight");
        light->rtSetFloat3("v0",P);
        light->rtSetFloat3("v1",P+U);
        light->rtSetFloat3("v2",P+V);
        light->rtSetFloat3("L" ,L);
        light->rtCommit();
        *g_scene += new LightNode(light);
      }

      /* quad light source */
      else if (tag == "-quadlight")
      {
        Vec3f P = cin->getVec3f();
        Vec3f U = cin->getVec3f();
        Vec3f V = cin->getVec3f();
        Vec3f L = cin->getVec3f();

        Ref<Device::RTLight> light0 = g_device->rtNewLight("trianglelight");
        light0->rtSetFloat3("v0",P+U+V);
        light0->rtSetFloat3("v1",P+U);
        light0->rtSetFloat3("v2",P);
        light0->rtSetFloat3("L" ,L);
        light0->rtCommit();
        *g_scene += new LightNode(light0);

        Ref<Device::RTLight> light1 = g_device->rtNewLight("trianglelight");
        light1->rtSetFloat3("v0",P+U+V);
        light1->rtSetFloat3("v1",P);
        light1->rtSetFloat3("v2",P+V);
        light1->rtSetFloat3("L" ,L);
        light1->rtCommit();
        *g_scene += new LightNode(light1);
      }

      /* HDRI light source */
      else if (tag == "-hdrilight")
      {
        Ref<Device::RTLight> light = g_device->rtNewLight("hdrilight");
        light->rtSetFloat3("L",cin->getVec3f());
        light->rtSetImage("image",loadRTImage(path + cin->getFileName()));
        light->rtCommit();
        *g_scene += new LightNode(light);
      }

      /* parse camera parameters */
      else if (tag == "-vp")     g_camPos    = Vec3f(cin->getVec3f());
      else if (tag == "-vi")     g_camLookAt = Vec3f(cin->getVec3f());
      else if (tag == "-vd")     g_camLookAt = g_camPos+cin->getVec3f();
      else if (tag == "-vu")     g_camUp     = cin->getVec3f();
      else if (tag == "-angle")  g_camAngle  = cin->getFloat();
      else if (tag == "-fov")    g_camAngle  = cin->getFloat();
      else if (tag == "-radius") g_camRadius = cin->getFloat();

      /* frame buffer size */
      else if (tag == "-size") {
        g_width = cin->getInt();
        g_height = cin->getInt();
        g_frameBuffer = g_device->rtNewFrameBuffer("RGB_FLOAT32", g_width, g_height);
      }

       /* full screen mode */
      else if (tag == "-fullscreen") {
        g_fullscreen = true;
      }

      /* refine rendering when not moving */
      else if (tag == "-refine") g_refine = true;
      else if (tag == "-norefine") g_refine = false;

      /* acceleration structure to use */
      else if (tag == "-accel") g_accel = cin->getString();

      /* set renderer */
      else if (tag == "-renderer")
      {
        std::string renderer = cin->getString();
        if      (renderer == "debug"     ) g_renderer = parseDebugRenderer(cin,path);
        else if (renderer == "pt"        ) g_renderer = parsePathTracer(cin,path);
        else if (renderer == "pathtracer") g_renderer = parsePathTracer(cin,path);
        else throw std::runtime_error("unknown renderer: "+renderer);
      }

      /* set gamma */
      else if (tag == "-gamma") {
        g_renderer->rtSetFloat1("gamma",g_gamma = cin->getFloat());
        g_renderer->rtCommit();
      }

      /* set recursion depth */
      else if (tag == "-depth") {
        g_renderer->rtSetInt1("maxDepth",g_depth = cin->getInt());
        g_renderer->rtCommit();
      }

      /* set samples per pixel */
      else if (tag == "-spp") {
        g_renderer->rtSetInt1("sampler.spp",g_spp = cin->getInt());
        g_renderer->rtCommit();
      }

      /* set the backplate */
      else if (tag == "-backplate") {
        g_renderer->rtSetImage("backplate",g_backplate = loadRTImage(path + cin->getFileName()));
        g_renderer->rtCommit();
      }

      /* render frame */
      else if (tag == "-o")
        outputMode(path + cin->getFileName());
	  	  
      /* save ray traces */
      else if (tag == "-savetrace")
        traceMode(path + cin->getFileName());

      /* display image */
      else if (tag == "-display")
        displayMode();

      /* regression testing */
      else if (tag == "-regression")
      {
        g_refine = false;
        g_regression = true;
        GLUTDisplay(OrthonormalSpace::lookAtPoint(g_camPos,g_camLookAt,g_camUp),0.01f);
      }

      else if (tag == "-version") {
        std::cout << "embree renderer version 1.0" << std::endl;
      }

      else if (tag == "-h" || tag == "-?" || tag == "-help" || tag == "--help")
      {
        std::cout << std::endl;
        std::cout << "Embree Version 1.0" << std::endl;
        std::cout << std::endl;
        std::cout << "  usage: embree -i model.obj -renderer debug -display" << std::endl;
        std::cout << "         embree -i model.obj -renderer pathtracer -o out.tga" << std::endl;
        std::cout << "         embree -c model.ecs -display" << std::endl;
        std::cout << std::endl;
        std::cout << "-renderer [debug,pathtracer]" << std::endl;
        std::cout << "  Sets the renderer to use." << std::endl;
        std::cout << std::endl;
        std::cout << "-c file" << std::endl;
        std::cout << "  Parses command line parameters from file." << std::endl;
        std::cout << std::endl;
        std::cout << "-i file" << std::endl;
        std::cout << "  Loads a scene from file." << std::endl;
        std::cout << std::endl;
        std::cout << "-o file" << std::endl;
        std::cout << "  Renders and outputs the image to the file." << std::endl;
        std::cout << std::endl;
        std::cout << "-savetrace file" << std::endl;
        std::cout << "  Renders and saves the ray trace data to the file." << std::endl;
        std::cout << std::endl;
        std::cout << "-display" << std::endl;
        std::cout << "  Interactively displays the rendering into a window." << std::endl;
        std::cout << std::endl;
        std::cout << "-vp x y z" << std::endl;
        std::cout << "  Sets camera position to the location (x,y,z)." << std::endl;
        std::cout << std::endl;
        std::cout << "-vi x y z" << std::endl;
        std::cout << "  Sets camera lookat point to the location (x,y,z)." << std::endl;
        std::cout << std::endl;
        std::cout << "-vd x y z" << std::endl;
        std::cout << "  Sets camera viewing direction to (x,y,z)." << std::endl;
        std::cout << std::endl;
        std::cout << "-vu x y z" << std::endl;
        std::cout << "  Sets camera up direction to (x,y,z)." << std::endl;
        std::cout << std::endl;
        std::cout << "-fov angle" << std::endl;
        std::cout << "  Sets camera field of view in y direction to angle." << std::endl;
        std::cout << std::endl;
        std::cout << "-size width height" << std::endl;
        std::cout << "  Sets the width and height of image to render." << std::endl;
        std::cout << std::endl;
        std::cout << "-fullscreen" << std::endl;
        std::cout << "  Enables full screen display mode." << std::endl;
        std::cout << std::endl;
        std::cout << "-accel [bvh2,bvh4,bvh4.spatial]" << std::endl;
        std::cout << "  Sets the spatial index structure to use." << std::endl;
        std::cout << std::endl;
        std::cout << "-gamma v" << std::endl;
        std::cout << "  Sets gamma correction to v (only pathtracer)." << std::endl;
        std::cout << std::endl;
        std::cout << "-depth i" << std::endl;
        std::cout << "  Sets the recursion depth to i (default 16)" << std::endl;
        std::cout << std::endl;
        std::cout << "-spp i" << std::endl;
        std::cout << "  Sets the number of samples per pixel to i (default 1) (only pathtracer)." << std::endl;
        std::cout << std::endl;
        std::cout << "-backplate" << std::endl;
        std::cout << "  Sets a high resolution back ground image. (default none) (only pathtracer)." << std::endl;
        std::cout << std::endl;

        std::cout << "-ambientlight r g b" << std::endl;
        std::cout << "  Creates an ambient light with intensity (r,g,b)." << std::endl;
        std::cout << std::endl;
        std::cout << "-pointlight px py pz r g b" << std::endl;
        std::cout << "  Creates a point light with intensity (r,g,b) at position (px,py,pz)." << std::endl;
        std::cout << std::endl;
        std::cout << "-distantlight dx dy dz r g b halfAngle" << std::endl;
        std::cout << "  Creates a distant sun light with intensity (r,g,b) shining into " << std::endl;
        std::cout << "  direction (dx,dy,dz) from the cone spanned by halfAngle." << std::endl;
        std::cout << std::endl;
        std::cout << "-trianglelight px py pz ux uy uz vx vy vz r g b" << std::endl;
        std::cout << "  Creates a triangle-light with intensity (r,g,b) spanned by the point " << std::endl;
        std::cout << "  (px,py,pz) and the vectors (vx,vy,vz) and (ux,uy,uz)." << std::endl;
        std::cout << std::endl;
        std::cout << "-quadlight px py pz ux uy uz vx vy vz r g b" << std::endl;
        std::cout << "  Creates a quad-light with intensity (r,g,b) spanned by the point " << std::endl;
        std::cout << "  (px,py,pz) and the vectors (vx,vy,vz) and (ux,uy,uz)." << std::endl;
        std::cout << std::endl;
        std::cout << "-hdrilight r g b file" << std::endl;
        std::cout << "  Creates a high dynamic range environment light from the image " << std::endl;
        std::cout << "  file. The intensities are multiplies by (r,g,b)." << std::endl;
        std::cout << std::endl;
        std::cout << "-trisphere px py pz r theta phi" << std::endl;
        std::cout << "  Creates a triangulated sphere with radius r at location (px,py,pz) " << std::endl;
        std::cout << "  and triangulation rates theta and phi." << std::endl;
        std::cout << std::endl;
        std::cout << "-[no]refine" << std::endl;
        std::cout << "  Enables (default) or disables the refinement display mode." << std::endl;
        std::cout << std::endl;
        std::cout << "-regression" << std::endl;
        std::cout << "  Runs a stress test of the system." << std::endl;
        std::cout << std::endl;
        std::cout << "-version" << std::endl;
        std::cout << "  Prints version number." << std::endl;
        std::cout << std::endl;
        std::cout << "-h, -?, -help, --help" << std::endl;
        std::cout << "  Prints this help." << std::endl;
      }

      /* skip unknown command line parameter */
      else {
        std::cerr << "unknown command line parameter: " << tag << " ";
        while (cin->peek() != "" && cin->peek()[0] != '-') std::cerr << cin->getString() << " ";
        std::cerr << std::endl;
      }
    }
  }

  /* main function in embree namespace */
  int main( int argc, char** argv) {
    g_device = new Device();
    g_renderer = g_device->rtNewRenderer("pathtracer");
    g_renderer->rtSetInt1("maxDepth",10);
    g_renderer->rtSetInt1("sampler.spp",1);
    g_renderer->rtCommit();
    g_frameBuffer = g_device->rtNewFrameBuffer("RGB_FLOAT32", g_width, g_height);
    parseCommandLine(new ParseStream(new CommandLineStream(argc,argv)),FileName());

    /*! if we did no render yet but have loaded a scene, switch to display mode */
    if (!g_rendered && g_scene->size()) displayMode();

    return 0;
  }
}

/******************************************************************************/
/*                               Main Function                                */
/******************************************************************************/

int main(int argc, char** argv)
{
  try {
    return embree::main(argc,argv);
  }
  catch (const std::exception& e) {
    std::cout << "Error: " << e.what() << std::endl;
    return 1;
  }
}
