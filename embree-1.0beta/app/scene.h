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

#ifndef __EMBREE_SCENE_H__
#define __EMBREE_SCENE_H__

#include <map>
#include <string>
#include <vector>
#include <stack>

#include "embreedevice.h"
#include "sys/filename.h"
#include "math/affinespace.h"

namespace embree
{
  extern Ref<Device> g_device;

  class Scene : public RefCount
  {
  public:
    virtual ~Scene() {}

    struct Instance {
      Instance (const AffineSpace& space, Ref<Scene> node) : space(space), node(node) {};
      AffineSpace space;
      Ref<Scene> node;
    };

    class iterator
    {
      friend class Scene;

      iterator () {}
      iterator (const Ref<Scene>& node) { stack.push(StackItem(one,node)); next(); }

    public:

      void operator++()    { next(); }; // ++prefix
      void operator++(int) { next(); }; // postfix++

      bool operator==(const iterator a) { return stack.empty() && a.stack.empty(); }
      bool operator!=(const iterator a) { return !stack.empty() || !a.stack.empty(); }

      Instance&       operator->()       { return stack.top(); };
      const Instance& operator->() const { return stack.top(); };

      Instance&       operator*()       { return stack.top(); };
      const Instance& operator*() const { return stack.top(); };

    private:
      void next();

      struct StackItem : public Instance {
        StackItem (const AffineSpace& space, Ref<Scene> node) : Instance(space,node), i(0) {};
        size_t i;
      };

      std::stack<StackItem> stack;
    };

  public:
    std::string name;
    iterator begin()  const { return iterator((Scene*)this); }
    iterator end()    const { return iterator(); }
  };

  class LightNode : public Scene {
  public:
    LightNode (const Ref<Device::RTLight>& light) : light(light) {}
    Ref<Device::RTLight> light;
  };

  class ShapeNode : public Scene {
  public:
    ShapeNode (const Ref<Device::RTShape>& shape, const Ref<Device::RTMaterial>& material) : shape(shape), material(material) {}
    Ref<Device::RTShape> shape;
    Ref<Device::RTMaterial> material;
  };

  class GroupNode : public Scene {
  public:
    GroupNode () {}
    GroupNode (const Ref<Scene>& node0                         ) { children.push_back(node0);                            }
    GroupNode (const Ref<Scene>& node0, const Ref<Scene>& node1) { children.push_back(node0); children.push_back(node1); }
    size_t size() { return children.size(); }
    void operator+=(const Ref<Scene>& node) { children.push_back(node); }
    std::vector<Ref<Scene> > children;
  };

  inline Ref<GroupNode> operator+(const Ref<Scene>& node0, const Ref<Scene>& node1){
    return new GroupNode(node0,node1);
  }

  class TransformNode : public GroupNode {
  public:
    TransformNode (const AffineSpace& space) : space(space) {}
    TransformNode (const AffineSpace& space, const Ref<Scene>& child) : GroupNode(child), space(space) {}
    AffineSpace space;
  };

  inline Ref<Scene> operator*(const AffineSpace& space, const Ref<Scene>& node) {
    return new TransformNode(space,node);
  }

  /* scene loaders */
  Ref<Scene> loadObj (const FileName& fileName);
  Ref<Scene> loadXML (const FileName& fileName);
  Ref<Scene> load    (const FileName& fileName);
}

#endif
