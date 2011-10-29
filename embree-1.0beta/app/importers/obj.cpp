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

#include <string.h>
#include <map>
#include <vector>
#include <fstream>
#include <iostream>

#include "sys/platform.h"
#include "sys/filename.h"
#include "image/image.h"
#include "scene.h"
#include "embreedevice.h"

namespace embree
{
  /*! device to create meshes and materials */
  extern Ref<Device> g_device;

  /*! texture loading interface */
  Ref<Device::RTTexture> loadTexture(const FileName fileName);

  /*! three-index vertex, indexing start at 0, -1 means invalid vertex */
  struct Vertex {
    int v, vt, vn;
    Vertex() {};
    Vertex(int v) : v(v), vt(v), vn(v) {};
    Vertex(int v, int vt, int vn) : v(v), vt(vt), vn(vn) {};
  };

  static inline bool operator < ( const Vertex& a, const Vertex& b ) {
    if (a.v  != b.v)  return a.v  < b.v;
    if (a.vn != b.vn) return a.vn < b.vn;
    if (a.vt != b.vt) return a.vt < b.vt;
    return false;
  }

  struct OBJReader
  {
    /*! directory of input file */
    FileName path;

    /* output model */
    Ref<GroupNode> model;

    /* geometry buffer */
    std::vector<Vec3f> v;
    std::vector<Vec3f> vn;
    std::vector<Vec2f> vt;
    std::vector<std::vector<Vertex> > curGroup;

    /*! material handling */
    Ref<Device::RTMaterial> curMaterial;
    std::map<std::string,Ref<Device::RTMaterial> > material;

    /*! Fill space at the end of the token with 0s. */
    static inline const char* trimEnd(const char* token) {
      size_t len = strlen(token);
      if (len == 0) return token;
      char* pe = (char*)(token + len - 1);
      while ((*pe == ' ' || *pe == '\t' || *pe == '\r') && pe >= token) *pe-- = 0;
      return token;
    }

    static inline bool isSep(const char c) {
      return (c == ' ') || (c == '\t');
    }

    /*! parse separator */
    static inline const char* parseSep(const char*& token) {
      size_t sep = strspn(token, " \t");
      if (!sep) throw std::runtime_error("separator expected");
      return token+=sep;
    }

    /*! parse optional separator */
    static inline const char* parseSepOpt(const char*& token) {
      return token+=strspn(token, " \t");
    }

    /*! Read float from a string */
    static inline float getFloat(const char*& token) {
      token += strspn(token, " \t");
      float n = (float)atof(token);
      token += strcspn(token, " \t\r");
      return n;
    }

    /*! Read Vec2f from a string */
    static inline Vec2f getVec2f(const char*& token) {
      float x = getFloat(token);
      float y = getFloat(token);
      return Vec2f(x,y);
    }

    /*! Read Vec3f from a string */
    static inline Vec3f getVec3f(const char*& token) {
      float x = getFloat(token);
      float y = getFloat(token);
      float z = getFloat(token);
      return Vec3f(x,y,z);
    }

    /*! Parse differently formated triplets like: n0, n0/n1/n2, n0//n2, n0/n1
      All indices are converted to C-style (from 0). Missing entries are assigned -1. */
    inline Vertex getInt3(const char*& token)
    {
      Vertex v(-1);
      v.v = fix_v(atoi(token));
      token += strcspn(token, "/ \t\r");
      if (token[0] != '/') return v;
      token++;

      // it is i//n
      if (token[0] == '/') {
        token++;
        v.vn = fix_vn(atoi(token));
        token += strcspn(token, " \t\r");
        return v;
      }

      // it is i/t/n or i/t
      v.vt = fix_vt(atoi(token));
      token += strcspn(token, "/ \t\r");
      if (token[0] != '/') return v;
      token++;

      // it is i/t/n
      v.vn = fix_vn(atoi(token));
      token += strcspn(token, " \t\r");
      return v;
    }

    OBJReader (const FileName& fileName)
      : path(fileName.path()), model(new GroupNode)
    {
      /* open file */
      std::ifstream cin;
      cin.open(fileName.c_str());
      if (!cin.is_open()) {
        std::cerr << "cannot open " << fileName.str() << std::endl;
        return;
      }

      /* generate default material */
      Ref<Device::RTMaterial> defaultMaterial = g_device->rtNewMaterial("matte");
      defaultMaterial->rtSetFloat3("reflectance",Vec3f(0.5f));
      defaultMaterial->rtCommit();
      curMaterial = defaultMaterial;

      char line[10000];
      memset(line,0,sizeof(line));

      while (cin.peek() != -1)
      {
        /* load next multiline */
        char* pline = line;
        while (true) {
          cin.getline(pline, sizeof(line) - (pline - line) - 16, '\n');
          index_t last = strlen(pline) - 1;
          if (last < 0 || pline[last] != '\\') break;
          pline += last;
          *pline++ = ' ';
        }

        const char* token = trimEnd(line + strspn(line, " \t"));
        if (token[0] == 0) continue;

        /*! parse position */
        if (token[0] == 'v' && isSep(token[1]))                    { v.push_back(getVec3f(token+=2)); continue; }

        /* parse normal */
        if (token[0] == 'v' && token[1] == 'n' && isSep(token[2])) { vn.push_back(getVec3f(token+=3)); continue; }

        /* parse texcoord */
        if (token[0] == 'v' && token[1] == 't' && isSep(token[2])) { vt.push_back(getVec2f(token+=3)); continue; }

        /*! parse face */
        if (token[0] == 'f' && isSep(token[1]))
        {
          parseSep(token+=1);

          std::vector<Vertex> face;
          while (token[0]) {
            face.push_back(getInt3(token));
            parseSepOpt(token);
          }
          curGroup.push_back(face);
          continue;
        }

        /*! use material */
        if (!strncmp(token, "usemtl", 6) && isSep(token[6]))
        {
          flushFaceGroup();
          std::string name(parseSep(token+=6));
          if (material.find(name) == material.end()) curMaterial = defaultMaterial;
          else curMaterial = material[name];
          continue;
        }

        /* load material library */
        if (!strncmp(token, "mtllib", 6) && isSep(token[6])) {
          loadMtl(path + std::string(parseSep(token+=6)));
          continue;
        }

        // ignore unknown stuff
      }
      flushFaceGroup();
      cin.close();
    }

    /* load material file */
    void loadMtl(const FileName& fileName)
    {
      std::ifstream cin;
      cin.open(fileName.c_str());
      if (!cin.is_open()) {
        std::cerr << "cannot open " << fileName.str() << std::endl;
        return;
      }

      char line[10000];
      memset(line,0,sizeof(line));

      Ref<Device::RTMaterial> cur = null;
      while (cin.peek() != -1)
      {
        /* load next multiline */
        char* pline = line;
        while (true) {
          cin.getline(pline, sizeof(line) - (pline - line) - 16, '\n');
          index_t last = strlen(pline) - 1;
          if (last < 0 || pline[last] != '\\') break;
          pline += last;
          *pline++ = ' ';
        }
        const char* token = trimEnd(line + strspn(line, " \t"));

        if (token[0] == 0  ) continue; // ignore empty lines
        if (token[0] == '#') continue; // ignore comments

        if (!strncmp(token, "newmtl", 6)) {
          parseSep(token+=6);
          if (cur) cur->rtCommit();
          std::string name(token);
          material[name] = cur = g_device->rtNewMaterial("obj");
          continue;
        }

        if (!cur) throw std::runtime_error("invalid material file: newmtl expected first");

        if (!strncmp(token,"illum",5)) { parseSep(token+=5); continue; }

        if (!strncmp(token,"d",1))     { parseSep(token+=1); cur->rtSetFloat1("d" ,getFloat(token)); continue; }
        if (!strncmp(token,"Ns",2))    { parseSep(token+=2); cur->rtSetFloat1("Ns",getFloat(token)); continue; }
        if (!strncmp(token,"Ni",2))    { parseSep(token+=2); cur->rtSetFloat1("Ni",getFloat(token)); continue; }

        if (!strncmp(token,"Ka",2))    { parseSep(token+=2); cur->rtSetFloat3("Ka",getVec3f(token)); continue; }
        if (!strncmp(token,"Kd",2))    { parseSep(token+=2); cur->rtSetFloat3("Kd",getVec3f(token)); continue; }
        if (!strncmp(token,"Ks",2))    { parseSep(token+=2); cur->rtSetFloat3("Ks",getVec3f(token)); continue; }
        if (!strncmp(token,"Tf",2))    { parseSep(token+=2); cur->rtSetFloat3("Tf",getVec3f(token)); continue; }

        if (!strncmp(token,"map_d" ,5)) { parseSep(token+=5); cur->rtSetTexture("map_d" ,loadTexture(path+std::string(token))); continue; }
        if (!strncmp(token,"map_Ns",6)) { parseSep(token+=6); cur->rtSetTexture("map_Ns",loadTexture(path+std::string(token))); continue; }
        if (!strncmp(token,"map_Ka",6)) { parseSep(token+=6); cur->rtSetTexture("map_Ka",loadTexture(path+std::string(token))); continue; }
        if (!strncmp(token,"map_Kd",6)) { parseSep(token+=6); cur->rtSetTexture("map_Kd",loadTexture(path+std::string(token))); continue; }
        if (!strncmp(token,"map_Ks",6)) { parseSep(token+=6); cur->rtSetTexture("map_Ks",loadTexture(path+std::string(token))); continue; }

        /*! the following are extensions to the standard */
        if (!strncmp(token,"map_Refl",8)) { parseSep(token+=8); cur->rtSetTexture("map_Refl",loadTexture(path+std::string(token))); continue; }
        if (!strncmp(token,"map_Bump",8)) { parseSep(token+=8); cur->rtSetTexture("map_Bump",loadTexture(path+std::string(token))); continue; }
      }
      if (cur) cur->rtCommit();
      cin.close();
    }

    /*! handles relative indices and starts indexing from 0 */
    int fix_v (int index) { return index>0 ? index-1 : index == 0 ? 0 : (int)v .size()+index; }
    int fix_vt(int index) { return index>0 ? index-1 : index == 0 ? 0 : (int)vt.size()+index; }
    int fix_vn(int index) { return index>0 ? index-1 : index == 0 ? 0 : (int)vn.size()+index; }

    uint32 getVertex(std::map<Vertex,uint32>& vertexMap, std::vector<Vec3f>& positions, std::vector<Vec3f>& normals, std::vector<Vec2f>& texcoords, const Vertex& i)
    {
      const std::map<Vertex, uint32>::iterator& entry = vertexMap.find(i);
      if (entry != vertexMap.end()) return entry->second;

      positions.push_back(v[i.v]);
      if (i.vn >= 0) normals.push_back(vn[i.vn]);
      if (i.vt >= 0) texcoords.push_back(vt[i.vt]);
      return vertexMap[i] = int(positions.size()) - 1;
    }

    /*! end current facegroup and append to mesh */
    void flushFaceGroup()
    {
      if (curGroup.empty()) return;

      // temporary data arrays
      std::vector<Vec3f> positions;
      std::vector<Vec3f> normals;
      std::vector<Vec2f> texcoords;
      std::vector<Vec3i> triangles;
      std::map<Vertex, uint32> vertexMap;

      // merge three indices into one
      for (size_t j=0; j<curGroup.size(); j++)
      {
        /* iterate over all faces */
        const std::vector<Vertex>& face = curGroup[j];
        Vertex i0 = face[0], i1 = Vertex(-1), i2 = face[1];

        /* triangulate the face with a triangle fan */
        for (size_t k=2; k<face.size(); k++) {
          i1 = i2; i2 = face[k];
          uint32 v0 = getVertex(vertexMap, positions, normals, texcoords, i0);
          uint32 v1 = getVertex(vertexMap, positions, normals, texcoords, i1);
          uint32 v2 = getVertex(vertexMap, positions, normals, texcoords, i2);
          triangles.push_back(Vec3i(v0,v1,v2));
        }
      }
      curGroup.clear();

      /* create triangle mesh */
      Ref<Device::RTShape> mesh = g_device->rtNewShape("trianglemesh");
      mesh->rtSetArray("positions","float3",&positions[0],positions.size(),sizeof(Vec3f));
      mesh->rtSetArray("indices"  ,"int3"  ,&triangles[0],triangles.size(),sizeof(Vec3i));
      if (normals.size()  ) mesh->rtSetArray("normals"  ,"float3",&normals[0]  ,normals.size()  ,sizeof(Vec3f));
      if (texcoords.size()) mesh->rtSetArray("texcoords","float2",&texcoords[0],texcoords.size(),sizeof(Vec2f));
      mesh->rtCommit();

      *model += new ShapeNode(mesh,curMaterial);
    }
  };

  Ref<Scene> loadObj(const FileName& fileName) {
    return OBJReader(fileName).model.cast<Scene>();
  }
}
