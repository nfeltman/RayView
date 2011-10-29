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

#include "scene.h"
#include "image/image.h"
#include "xml_parser.h"

namespace embree
{
  extern Ref<Device> g_device;

  struct float3 {
    float3 () {}
    float3 (float x, float y, float z) : x(x), y(y), z(z) {}
    float x, y, z;
  };

  Ref<Device::RTImage>   loadRTImage(const FileName fileName);
  Ref<Device::RTTexture> loadTexture(const FileName fileName);

  class XMLImporter
  {
  private:
    std::map<std::string,Ref<Scene> > sceneMap;
    FILE* binFile;
    FileName binFileName;
    FileName path;

  public:
    XMLImporter(const FileName& fileName);
    ~XMLImporter();
    Ref<Scene> root;

    Ref<Scene> loadPointLight(const Ref<XML>& xml);
    Ref<Scene> loadSpotLight(const Ref<XML>& xml);
    Ref<Scene> loadDirectionalLight(const Ref<XML>& xml);
    Ref<Scene> loadDistantLight(const Ref<XML>& xml);
    Ref<Scene> loadAmbientLight(const Ref<XML>& xml);
    Ref<Scene> loadTriangleLight(const Ref<XML>& xml);
    Ref<Scene> loadQuadLight(const Ref<XML>& xml);
    Ref<Scene> loadHDRILight(const Ref<XML>& xml);
    Ref<Device::RTMaterial> loadMaterial(const Ref<XML>& xml);
    Ref<Scene> loadTriangleMesh(const Ref<XML>& xml);
    Ref<Scene> loadSphere(const Ref<XML>& xml);
    Ref<Scene> loadScene(const Ref<XML>& xml);
    Ref<Scene> loadTransformNode(const Ref<XML>& xml);
    Ref<Scene> loadGroupNode(const Ref<XML>& xml);

  private:
    template<typename T> T load(const Ref<XML>& xml) { return T(zero); }
    template<typename T> T load(const Ref<XML>& xml, T opt) { return T(zero); }
    template<typename T> std::vector<T> loadBinary(const Ref<XML>& xml);
  };

  //////////////////////////////////////////////////////////////////////////////
  //// Loading standard types from an XML node
  //////////////////////////////////////////////////////////////////////////////

  template<> std::string XMLImporter::load<std::string>(const Ref<XML>& xml) {
    if (xml->body.size() < 1) throw std::runtime_error(xml->loc.str()+": wrong string body");
    return xml->body[0].String();
  }

  template<> bool XMLImporter::load<bool>(const Ref<XML>& xml, bool opt) {
    if (xml == null) return opt;
    if (xml->body.size() < 1) throw std::runtime_error(xml->loc.str()+": wrong bool body");
    return xml->body[0].Int() != 0;
  }

  template<> int XMLImporter::load<int>(const Ref<XML>& xml) {
    if (xml->body.size() < 1) throw std::runtime_error(xml->loc.str()+": wrong int body");
    return xml->body[0].Int();
  }

  template<> Vec2i XMLImporter::load<Vec2i>(const Ref<XML>& xml) {
    if (xml->body.size() < 2) throw std::runtime_error(xml->loc.str()+": wrong int2 body");
    return Vec2i(xml->body[0].Int(),xml->body[1].Int());
  }

  template<> Vec3i XMLImporter::load<Vec3i>(const Ref<XML>& xml) {
    if (xml->body.size() < 3) throw std::runtime_error(xml->loc.str()+": wrong int3 body");
    return Vec3i(xml->body[0].Int(),xml->body[1].Int(),xml->body[2].Int());
  }

  template<> Vec4i XMLImporter::load<Vec4i>(const Ref<XML>& xml) {
    if (xml->body.size() < 4) throw std::runtime_error(xml->loc.str()+": wrong int4 body");
    return Vec4i(xml->body[0].Int(),xml->body[1].Int(),xml->body[2].Int(),xml->body[3].Int());
  }

  template<> float XMLImporter::load<float>(const Ref<XML>& xml) {
    if (xml->body.size() < 1) throw std::runtime_error(xml->loc.str()+": wrong float body");
    return xml->body[0].Float();
  }

  template<> Vec2f XMLImporter::load<Vec2f>(const Ref<XML>& xml) {
    if (xml->body.size() < 2) throw std::runtime_error(xml->loc.str()+": wrong float2 body");
    return Vec2f(xml->body[0].Float(),xml->body[1].Float());
  }

  template<> Vec3f XMLImporter::load<Vec3f>(const Ref<XML>& xml) {
    if (xml->body.size() < 3) throw std::runtime_error(xml->loc.str()+": wrong float3 body");
    return Vec3f(xml->body[0].Float(),xml->body[1].Float(),xml->body[2].Float());
  }

  template<> float3 XMLImporter::load<float3>(const Ref<XML>& xml) {
    if (xml->body.size() < 3) throw std::runtime_error(xml->loc.str()+": wrong float3 body");
    return float3(xml->body[0].Float(),xml->body[1].Float(),xml->body[2].Float());
  }

  template<> Vec4f XMLImporter::load<Vec4f>(const Ref<XML>& xml) {
    if (xml->body.size() < 4) throw std::runtime_error(xml->loc.str()+": wrong float4 body");
    return Vec4f(xml->body[0].Float(),xml->body[1].Float(),xml->body[2].Float(),xml->body[3].Float());
  }

  template<> Col3f XMLImporter::load<Col3f>(const Ref<XML>& xml) {
    if (xml->body.size() < 3) throw std::runtime_error(xml->loc.str()+": wrong color body");
    return Col3f(xml->body[0].Float(),xml->body[1].Float(),xml->body[2].Float());
  }

  template<> AffineSpace XMLImporter::load<AffineSpace>(const Ref<XML>& xml) {
    if (xml->body.size() < 12) throw std::runtime_error(xml->loc.str()+": wrong AffineSpace body");
    return AffineSpace(LinearSpace3f(xml->body[0].Float(),xml->body[1].Float(),xml->body[2].Float(),
                                     xml->body[4].Float(),xml->body[5].Float(),xml->body[6].Float(),
                                     xml->body[8].Float(),xml->body[9].Float(),xml->body[10].Float()),
                       Vec3f(xml->body[3].Float(),xml->body[7].Float(),xml->body[11].Float()));
  }

  template<typename T> std::vector<T> XMLImporter::loadBinary(const Ref<XML>& xml)
  {
    index_t ofs = atol(xml->parm("ofs").c_str());
    index_t size = atol(xml->parm("size").c_str());
    if (!binFile) throw std::runtime_error("cannot open file "+binFileName.str()+" for reading");
    fseek(binFile,long(ofs),SEEK_SET);
    std::vector<T> vec(size);
    for (index_t i=0; i<size; i++) 
      if (fread(&vec[i],sizeof(T),1,binFile) != 1)
        throw std::runtime_error("Error reading "+binFileName.str());
    return vec;
  }

  template<> std::vector<Vec2f> XMLImporter::load<std::vector<Vec2f> >(const Ref<XML>& xml)
  {
    if (xml->parm("ofs") != "") return loadBinary<Vec2f>(xml);

    index_t size = xml->body.size();
    if (size % 2 != 0) throw std::runtime_error(xml->loc.str()+": wrong vector<float2> body");
    std::vector<Vec2f> vec(size/2);
    for (index_t i=0; i<size/2; i++) vec[i] = Vec2f(xml->body[2*i+0].Float(),xml->body[2*i+1].Float());
    return vec;
  }

  template<> std::vector<float3> XMLImporter::load<std::vector<float3> >(const Ref<XML>& xml)
  {
    if (xml->parm("ofs") != "") return loadBinary<float3>(xml);

    index_t size = xml->body.size();
    if (size % 3 != 0) throw std::runtime_error(xml->loc.str()+": wrong vector<float3> body");
    std::vector<float3> vec(size/3);
    for (index_t i=0; i<size/3; i++) vec[i] = float3(xml->body[3*i+0].Float(),xml->body[3*i+1].Float(),xml->body[3*i+2].Float());
    return vec;
  }

  template<> std::vector<Vec3i> XMLImporter::load<std::vector<Vec3i> >(const Ref<XML>& xml)
  {
    if (xml->parm("ofs") != "") return loadBinary<Vec3i>(xml);

    index_t size = xml->body.size();
    if (size % 3 != 0) throw std::runtime_error(xml->loc.str()+": wrong vector<int3> body");
    std::vector<Vec3i> vec(size/3);
    for (index_t i=0; i<size/3; i++) vec[i] = Vec3i(xml->body[3*i+0].Int(),xml->body[3*i+1].Int(),xml->body[3*i+2].Int());
    return vec;
  }

  //////////////////////////////////////////////////////////////////////////////
  //// Loading of objects from XML file
  //////////////////////////////////////////////////////////////////////////////

  Ref<Scene> XMLImporter::loadPointLight(const Ref<XML>& xml)
  {
    AffineSpace space = load<AffineSpace>(xml->child("AffineSpace"));
    Ref<Device::RTLight> light = g_device->rtNewLight("pointlight");
    light->rtSetFloat3("P",space.p);
    light->rtSetFloat3("I",load<Col3f>(xml->child("I")));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Scene> XMLImporter::loadSpotLight(const Ref<XML>& xml)
  {
    AffineSpace space = load<AffineSpace>(xml->child("AffineSpace"));
    Ref<Device::RTLight> light = g_device->rtNewLight("spotlight");
    light->rtSetFloat3("P",space.p);
    light->rtSetFloat3("D",space.l.vz);
    light->rtSetFloat3("I",load<Col3f>(xml->child("I")));
    light->rtSetFloat1("angleMin",load<float>(xml->child("angleMin")));
    light->rtSetFloat1("angleMax",load<float>(xml->child("angleMax")));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Scene> XMLImporter::loadDirectionalLight(const Ref<XML>& xml)
  {
    AffineSpace space = load<AffineSpace>(xml->child("AffineSpace"));
    Ref<Device::RTLight> light = g_device->rtNewLight("directionallight");
    light->rtSetFloat3("D",space.l.vz);
    light->rtSetFloat3("E",load<Col3f>(xml->child("E")));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Scene> XMLImporter::loadDistantLight(const Ref<XML>& xml)
  {
    AffineSpace space = load<AffineSpace>(xml->child("AffineSpace"));
    Ref<Device::RTLight> light = g_device->rtNewLight("distantlight");
    light->rtSetFloat3("D",space.l.vz);
    light->rtSetFloat3("L",load<Col3f>(xml->child("L")));
    light->rtSetFloat1("halfAngle",load<float>(xml->child("halfAngle")));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Scene> XMLImporter::loadAmbientLight(const Ref<XML>& xml)
  {
    Ref<Device::RTLight> light = g_device->rtNewLight("ambientlight");
    light->rtSetFloat3("L",load<Col3f>(xml->child("L")));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Scene> XMLImporter::loadTriangleLight(const Ref<XML>& xml)
  {
    AffineSpace space = load<AffineSpace>(xml->child("AffineSpace"));
    Ref<Device::RTLight> light = g_device->rtNewLight("trianglelight");
    light->rtSetFloat3("L",load<Col3f>(xml->child("L")));
    light->rtSetFloat3("v0",xfmPoint(space, Vec3f(1,0,0)));
    light->rtSetFloat3("v1",xfmPoint(space, Vec3f(0,1,0)));
    light->rtSetFloat3("v2",xfmPoint(space, Vec3f(0,0,0)));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Scene> XMLImporter::loadQuadLight(const Ref<XML>& xml)
  {
    AffineSpace space = load<AffineSpace>(xml->child("AffineSpace"));
    Col3f L = load<Col3f>(xml->child("L"));
    Vec3f v0 = xfmPoint(space, Vec3f(0,0,0));
    Vec3f v1 = xfmPoint(space, Vec3f(0,1,0));
    Vec3f v2 = xfmPoint(space, Vec3f(1,1,0));
    Vec3f v3 = xfmPoint(space, Vec3f(1,0,0));

    Ref<Device::RTLight> light0 = g_device->rtNewLight("trianglelight");
    light0->rtSetFloat3("L",L);
    light0->rtSetFloat3("v0",v1);
    light0->rtSetFloat3("v1",v3);
    light0->rtSetFloat3("v2",v0);
    light0->rtCommit();

    Ref<Device::RTLight> light1 = g_device->rtNewLight("trianglelight");
    light1->rtSetFloat3("L",L);
    light1->rtSetFloat3("v0",v2);
    light1->rtSetFloat3("v1",v3);
    light1->rtSetFloat3("v2",v1);
    light1->rtCommit();

    return new GroupNode(new LightNode(light0), new LightNode(light1));
  }

  Ref<Scene> XMLImporter::loadHDRILight(const Ref<XML>& xml)
  {
    Ref<Device::RTLight> light = g_device->rtNewLight("hdrilight");
    light->rtSetTransform("local2world",load<AffineSpace>(xml->child("AffineSpace")));
    light->rtSetFloat3("L",load<Col3f>(xml->child("L")));
    light->rtSetImage("image",loadRTImage(path+load<std::string>(xml->child("image"))));
    light->rtCommit();
    return new LightNode(light);
  }

  Ref<Device::RTMaterial> XMLImporter::loadMaterial(const Ref<XML>& xml)
  {
    Ref<Device::RTMaterial> material = g_device->rtNewMaterial(load<std::string>(xml->child("code")).c_str());

    Ref<XML> parms = xml->child("parameters");
    for (index_t i=0; i < (index_t)parms->children.size(); i++) {
      Ref<XML> entry = parms->children[i];
    std::string name = entry->parm("name");
      if      (entry->name == "int"    ) material->rtSetInt1(name.c_str(),load<int>(entry));
      else if (entry->name == "int2"   ) material->rtSetInt2(name.c_str(),load<Vec2i>(entry));
      else if (entry->name == "int3"   ) material->rtSetInt3(name.c_str(),load<Vec3i>(entry));
      else if (entry->name == "int4"   ) material->rtSetInt4(name.c_str(),load<Vec4i>(entry));
      else if (entry->name == "float"  ) material->rtSetFloat1(name.c_str(),load<float>(entry));
      else if (entry->name == "float2" ) material->rtSetFloat2(name.c_str(),load<Vec2f>(entry));
      else if (entry->name == "float3" ) material->rtSetFloat3(name.c_str(),load<Vec3f>(entry));
      else if (entry->name == "float4" ) material->rtSetFloat4(name.c_str(),load<Vec4f>(entry));
      else if (entry->name == "texture") material->rtSetTexture(name.c_str(),loadTexture(path+load<std::string>(entry)));
      else throw std::runtime_error(entry->loc.str()+": invalid type: "+entry->name);
    }
    material->rtCommit();
    return material;
  }

  Ref<Scene> XMLImporter::loadTriangleMesh(const Ref<XML>& xml)
  {
    Ref<Device::RTMaterial> material  = loadMaterial(xml->child("material"));
    std::vector<float3> positions = load<std::vector<float3> >(xml->child("positions"));
    std::vector<float3> normals   = load<std::vector<float3> >(xml->child("normals"));
    std::vector<Vec2f> texcoords  = load<std::vector<Vec2f> >(xml->child("texcoords"));
    std::vector<Vec3i> triangles  = load<std::vector<Vec3i> >(xml->child("triangles"));

    Ref<Device::RTShape> mesh = g_device->rtNewShape("trianglemesh");
    mesh->rtSetArray("positions","float3",&positions[0],positions.size(),sizeof(float3));
    mesh->rtSetArray("indices"  ,"int3"  ,&triangles[0],triangles.size(),sizeof(Vec3i));
    if (normals.size()  ) mesh->rtSetArray("normals"  ,"float3",&normals  [0],normals.size()  ,sizeof(float3));
    if (texcoords.size()) mesh->rtSetArray("texcoords","float2",&texcoords[0],texcoords.size(),sizeof(Vec2f));
    mesh->rtSetBool1("consistentNormals",load<bool>(xml->childOpt("consistentNormals"),false));
    mesh->rtCommit();

    return new ShapeNode(mesh,material);
  }

  Ref<Scene> XMLImporter::loadSphere(const Ref<XML>& xml)
  {
    Ref<Device::RTMaterial> material  = loadMaterial(xml->child("material"));

    Ref<Device::RTShape> sphere = g_device->rtNewShape("sphere");
    sphere->rtSetFloat3("P",load<Vec3f>(xml->child("position")));
    sphere->rtSetFloat1("r",load<float>(xml->child("radius")));
    sphere->rtSetInt1  ("numTheta",load<int>(xml->child("numTheta")));
    sphere->rtSetInt1  ("numPhi",load<int>(xml->child("numPhi")));
    sphere->rtSetBool1("consistentNormals",load<bool>(xml->childOpt("consistentNormals"),false));
    sphere->rtCommit();

    return new ShapeNode(sphere,material);
  }

  Ref<Scene> XMLImporter::loadTransformNode(const Ref<XML>& xml)
  {
    TransformNode* node = new TransformNode(load<AffineSpace>(xml->child("AffineSpace")));
    for (index_t i=1; i < (index_t)xml->children.size(); i++)
      node->children.push_back(loadScene(xml->children[i]));
    return node;
  }

  Ref<Scene> XMLImporter::loadGroupNode(const Ref<XML>& xml)
  {
    GroupNode* node = new GroupNode;
    for (index_t i=0; i < (index_t)xml->children.size(); i++)
      node->children.push_back(loadScene(xml->children[i]));
    return node;
  }

  //////////////////////////////////////////////////////////////////////////////
  //// Loading of scene graph node from XML file
  //////////////////////////////////////////////////////////////////////////////

  Ref<Scene> XMLImporter::loadScene(const Ref<XML>& xml)
  {
    Ref<Scene> scene;

    if      (xml->name == "reference"       ) scene = sceneMap[xml->parm("name")];
    else if (xml->name == "obj"             ) scene = loadObj (path + xml->parm("src"));
    else if (xml->name == "xml"             ) scene = loadXML (path + xml->parm("src"));
    else if (xml->name == "PointLight"      ) scene = loadPointLight      (xml);
    else if (xml->name == "SpotLight"       ) scene = loadSpotLight       (xml);
    else if (xml->name == "DirectionalLight") scene = loadDirectionalLight(xml);
    else if (xml->name == "DistantLight"    ) scene = loadDistantLight    (xml);
    else if (xml->name == "AmbientLight"    ) scene = loadAmbientLight    (xml);
    else if (xml->name == "TriangleLight"   ) scene = loadTriangleLight   (xml);
    else if (xml->name == "QuadLight"       ) scene = loadQuadLight       (xml);
    else if (xml->name == "HDRILight"       ) scene = loadHDRILight       (xml);
    else if (xml->name == "TriangleMesh"    ) scene = loadTriangleMesh    (xml);
    else if (xml->name == "Sphere"          ) scene = loadSphere          (xml);
    else if (xml->name == "Group"           ) scene = loadGroupNode       (xml);
    else if (xml->name == "Transform"       ) scene = loadTransformNode   (xml);
    else throw std::runtime_error(xml->loc.str()+": unknown tag: "+xml->name);

    if (xml->parm("id") != "") sceneMap[xml->parm("id")] = scene;
    scene->name = xml->parm("name");

    return scene;
  }

  XMLImporter::XMLImporter(const FileName& fileName) : binFile(NULL)
  {
    path = fileName.path();
    binFileName = fileName.setExt(".bin");
    binFile = fopen(binFileName.c_str(),"rb");

    Ref<XML> xml = parseXML(fileName);
    if (xml->name != "scene") throw std::runtime_error(xml->loc.str()+": invalid scene tag");
    for (index_t i=0; i < (index_t)xml->children.size(); i++)
      root = loadScene(xml->children[i]);
  }

  XMLImporter::~XMLImporter() {
    if (binFile) fclose(binFile);
  }

  Ref<Scene> loadXML(const FileName& fileName) {
    XMLImporter loader(fileName); return loader.root;
  }
}
