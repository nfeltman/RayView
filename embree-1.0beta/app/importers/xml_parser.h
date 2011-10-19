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

#ifndef __EMBREE_XML_H__
#define __EMBREE_XML_H__

#include <map>
#include <string>
#include <vector>

#include "sys/platform.h"
#include "sys/filename.h"
#include "lexers/tokenstream.h"

namespace embree
{
  /* an XML node */
  class XML : public RefCount
  {
  public:
    XML (const std::string& name = "") : name(name) {}

    std::string parm(const std::string& parmID) const {
      std::map<std::string,std::string>::const_iterator i = parms.find(parmID);
      if (i == parms.end()) return ""; else return i->second;
    }

    const Ref<XML> childOpt(const std::string& childID) const {
      for (index_t i=0; i < (int)children.size(); i++)
        if (children[i]->name == childID) return children[i];
      return null;
    }

    const Ref<XML> child(const std::string& childID) const {
      for (index_t i=0; i < (int)children.size(); i++)
        if (children[i]->name == childID) return children[i];
      throw std::runtime_error (loc.str()+": XML node has no child \"" + childID + "\"");
    }

    Ref<XML> add(const Ref<XML>& xml) { children.push_back(xml); return this; }
    Ref<XML> add(const Token& tok   ) { body.push_back(tok);     return this; }
    Ref<XML> add(const std::string& name, const std::string& val) { parms[name] = val; return this; }

  public:
    ParseLocation loc;
    std::string name;
    std::map<std::string,std::string> parms;
    std::vector<Ref<XML> > children;
    std::vector<Token> body;
  };

  /*! load XML file from stream */
  std::istream& operator>>(std::istream& cin, Ref<XML>& xml);

  /*! load XML file from disk */
  Ref<XML> parseXML(const FileName& fileName);

  /* store XML to stream */
  std::ostream& operator<<(std::ostream& cout, const Ref<XML>& xml);

  /*! store XML to disk */
  void emitXML(const FileName& fileName, const Ref<XML>& xml);
}

#endif
