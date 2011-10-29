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
#include "sys/filename.h"
#include "sys/stl/string.h"

#include <iostream>
#include <string>

namespace embree
{
  void Scene::iterator::next()
  {
    while (!stack.empty())
    {
      StackItem item = stack.top();

      if (Ref<TransformNode> tnode = item.node.dynamicCast<TransformNode>()) {
        const std::vector<Ref<Scene> >& children = tnode->children;
        if (item.i < children.size()) {
          stack.top().i++;
          stack.push(StackItem(item.space*tnode->space,children[item.i]));
        }
        else {
          stack.pop();
        }
      } else if (Ref<GroupNode> gnode = item.node.dynamicCast<GroupNode>()) {
        const std::vector<Ref<Scene> >& children = gnode->children;
        if (item.i < children.size()) {
          stack.top().i++;
          stack.push(StackItem(item.space,children[item.i]));
        }
        else {
          stack.pop();
        }
      } else {
        if (item.i == 0) {
          stack.top().i++;
          return;
        }
        else {
          stack.pop();
        }
      }
    }
  }

  /* loads a scene from a file with auto-detection of format */
  Ref<Scene> load(const FileName& fileName)
  {
    std::string ext = strlwr(fileName.ext());
    if (ext == "obj" ) return loadObj (fileName);
    if (ext == "xml" ) return loadXML (fileName);
    throw std::runtime_error("file format " + ext + " not supported");
  }
}
