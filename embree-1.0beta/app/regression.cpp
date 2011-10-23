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

#include "regression.h"
#include <vector>

namespace embree
{
  extern std::string g_accel;

  Ref<Device::RTImage> createRandomImage(Ref<Device> device, size_t width, size_t height)
  {
    if (random<bool>())
    {
      char* data = new char[3*width*height];
      for (size_t y=0; y<height; y++) {
        for (size_t x=0; x<width; x++) {
          size_t ofs = y*width+x;
          data[3*ofs+0] = char(x*y);
          data[3*ofs+1] = char(y*x);
          data[3*ofs+2] = char(x+y);
        }
      }
      Ref<Device::RTImage> image = device->rtNewImage("RGB8",width,height,data);
      delete[] data;
      return image;
    }
    else {
      Col3f* data = new Col3f[3*width*height];
      for (size_t y=0; y<height; y++) {
        for (size_t x=0; x<width; x++) {
          size_t ofs = y*width+x;
          data[ofs].r = char(x*y)/255.0f;
          data[ofs].g = char(y*x)/255.0f;
          data[ofs].b = char(x+y)/255.0f;
        }
      }
      Ref<Device::RTImage> image = device->rtNewImage("RGB_FLOAT32",width,height,data);
      delete[] data;
      return image;
    }
  }

  Ref<Device::RTTexture> createRandomTexture(Ref<Device> device, size_t width, size_t height)
  {
    Ref<Device::RTTexture> texture = device->rtNewTexture("image");
    texture->rtSetImage("image",createRandomImage(device,width,height));
    texture->rtCommit();
    return texture;
  }

  Ref<Device::RTMaterial> createRandomMaterial(Ref<Device> device)
  {
    //switch (random<int>() % 9)
    switch (random<int>() % 8)
    {
    case 0: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("Matte");
      material->rtSetFloat3("reflectance",Col3f(random<float>(),random<float>(),random<float>()));
      material->rtCommit();
      return material;
    }
    case 1: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("Plastic");
      material->rtSetFloat3("pigmentColor",Col3f(random<float>(),random<float>(),random<float>()));
      material->rtSetFloat1("eta",1.0f+random<float>());
      material->rtSetFloat1("roughness",0.1f*random<float>());
      material->rtCommit();
      return material;
    }
    case 2: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("Dielectric");
      material->rtSetFloat3("transmission",0.5f*Col3f(random<float>(),random<float>(),random<float>())+Col3f(0.5f));
      material->rtSetFloat1("etaOutside",1.0f);
      material->rtSetFloat1("etaInside",1.0f+random<float>());
      material->rtCommit();
      return material;
    }
    case 3: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("ThinDielectric");
      material->rtSetFloat3("transmission",0.5f*Col3f(random<float>(),random<float>(),random<float>())+Col3f(0.5f));
      material->rtSetFloat1("eta",1.0f+random<float>());
      material->rtSetFloat1("thickness",0.5f*random<float>());
      material->rtCommit();
      return material;
    }
    case 4: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("Mirror");
      material->rtSetFloat3("reflectance",0.5f*Col3f(random<float>(),random<float>(),random<float>())+Col3f(0.5f));
      material->rtCommit();
      return material;
    }
    case 5: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("Metal");
      material->rtSetFloat3("reflectance",0.5f*Col3f(random<float>(),random<float>(),random<float>())+Col3f(0.5f));
      material->rtSetFloat3("eta",Col3f(1.0f)+Col3f(random<float>(),random<float>(),random<float>()));
      material->rtSetFloat3("k",0.3f*Col3f(random<float>(),random<float>(),random<float>()));
      material->rtSetFloat1("roughness",0.3f*random<float>());
      material->rtCommit();
      return material;
    }
    case 6: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("MetallicPaint");
      material->rtSetFloat3("shadeColor",0.5f*Col3f(random<float>(),random<float>(),random<float>())+Col3f(0.5f));
      material->rtSetFloat3("glitterColor",Col3f(random<float>(),random<float>(),random<float>()));
      material->rtSetFloat1("glitterSpread",0.5f+random<float>());
      material->rtSetFloat1("eta",1.0f+random<float>());
      material->rtCommit();
      return material;
    }
    case 7: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("MatteTextured");
      material->rtSetTexture("Kd",createRandomTexture(device,32,32));
      material->rtSetFloat2("s0",Vec2f(random<float>(),random<float>()));
      material->rtSetFloat2("ds",5.0f*Vec2f(random<float>(),random<float>()));
      material->rtCommit();
      return material;
    }
    case 8: {
      Ref<Device::RTMaterial> material = device->rtNewMaterial("Obj");
      material->rtSetFloat1("d",random<float>());
      if (random<bool>()) material->rtSetTexture("map_d",createRandomTexture(device,32,32));
      material->rtSetFloat3("Tf",Col3f(random<float>(),random<float>(),random<float>()));
      material->rtSetFloat3("Kd",Col3f(random<float>(),random<float>(),random<float>()));
      if (random<bool>()) material->rtSetTexture("map_Kd",createRandomTexture(device,32,32));
      material->rtSetFloat3("Ks",Col3f(random<float>(),random<float>(),random<float>()));
      if (random<bool>()) material->rtSetTexture("map_Ks",createRandomTexture(device,32,32));
      material->rtSetFloat1("Ns",10.0f*random<float>());
      if (random<bool>()) material->rtSetTexture("map_Ns",createRandomTexture(device,32,32));
      material->rtCommit();
      return material;
    }
    }
    return null;
  }

  Ref<Device::RTLight> createRandomLight(Ref<Device> device)
  {
    Ref<Device::RTLight> light = device->rtNewLight("hdrilight");
    light->rtSetFloat3("L",Col3f(one));
    light->rtCommit();
    return light;
  }

  Ref<Device::RTShape> createRandomShape(Ref<Device> device, size_t numTriangles)
  {
    if (numTriangles < 20)
    {
      std::vector<Vec3f> positions;
      std::vector<Vec2f> texcoords;
      std::vector<Vec3i>   indices;

      Vec3f pos = 2.0f*Vec3f(random<float>(),random<float>(),random<float>())-Vec3f(1.0f);
      for (size_t i=0; i<numTriangles; i++) {
        positions.push_back(pos+0.3f*Vec3f(random<float>(),random<float>(),random<float>()));
        texcoords.push_back(Vec2f(random<float>(),random<float>()));
        indices  .push_back(Vec3i(random<int>()%numTriangles,random<int>()%numTriangles,random<int>()%numTriangles));
      }

      Ref<Device::RTShape> shape = device->rtNewShape("trianglemesh");
    shape->rtSetArray("positions","float3",positions.size() ? &positions[0] : NULL, positions.size(),sizeof(Vec3f));
    shape->rtSetArray("texcoords","float2",texcoords.size() ? &texcoords[0] : NULL, texcoords.size(),sizeof(Vec2f));
    shape->rtSetArray("indices"  ,"int3"  ,indices.size  () ? &indices  [0] : NULL, indices.size  (),sizeof(Vec3i));
      shape->rtCommit();
      return shape;
    }
    else
    {
      Ref<Device::RTShape> shape = device->rtNewShape("sphere");
      shape->rtSetFloat3("P",2.0f*Vec3f(random<float>(),random<float>(),random<float>())-Vec3f(1.0f));
      shape->rtSetFloat1("r",0.2f*random<float>());
      shape->rtSetInt1("numTheta",(int)numTriangles/20);
      shape->rtSetInt1("numPhi",20);
      shape->rtCommit();
      return shape;
    }
  }

  Ref<Device::RTScene> createRandomScene(Ref<Device> device, size_t numLights, size_t numObjects, size_t numTriangles)
  {
    std::vector<Ref<Device::RTPrimitive> > prims;

    //for (size_t i=0; i<numLights; i++)
    prims.push_back(device->rtNewPrimitive(createRandomLight(device),AffineSpace(one)));

    for (size_t i=0; i<numObjects; i++) {
      size_t s = numTriangles ? random<int>()%numTriangles : 0;
      prims.push_back(device->rtNewPrimitive(createRandomShape(device,s),createRandomMaterial(device),AffineSpace(one)));
    }

    return device->rtNewScene(g_accel.c_str(), FileName(""),&prims[0],prims.size());
  }
}

