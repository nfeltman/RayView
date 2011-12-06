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

#include "rtcore.h"
#include "bvh2/bvh2_builder.h"
#include "bvh2/bvh2_builder_spatial.h"
#include "bvh2/bvh2_to_bvh4.h"
#include "bvh2/bvh2_traverser.h"
#include "BVH2Printer.h"
#include "bvh4/bvh4_builder.h"
#include "bvh4/bvh4_traverser.h"
#include "PrintingTraverser.h"

#include <string>

namespace embree
{
	void printBVH2ToFile(Ref<BVH2<Triangle4> > bvh, FileName& bvhOutput);

  Intersector* rtcCreateAccelNoTrace(const char* type, const BuildTriangle* triangles, size_t numTriangles, FileName& bvhOutput)
  {
    if (!strcmp(type,"bvh2"        )) 	{
		Ref<BVH2<Triangle4> > bvh = BVH2Builder::build(triangles,numTriangles);
		BVH2Printer::printBVH2ToFile(bvh,bvhOutput);
		return new BVH2Traverser(bvh);
	}
    else if (!strcmp(type,"bvh2.spatial"))	{
		Ref<BVH2<Triangle4> > bvh = BVH2BuilderSpatial::build(triangles,numTriangles);
		BVH2Printer::printBVH2ToFile(bvh,bvhOutput);
		return new BVH2Traverser(bvh);
	}
    else if (!strcmp(type,"bvh4") || !strcmp(type,"default"))	{
		Ref<BVH4<Triangle4> > bvh = BVH4Builder::build(triangles,numTriangles);
		return new BVH4Traverser(bvh);
	}
    else if (!strcmp(type,"bvh4.spatial")) 	{
	  Ref<BVH4<Triangle4> > bvh = BVH2ToBVH4::convert(BVH2BuilderSpatial::build(triangles,numTriangles));
      return new BVH4Traverser(bvh);
    }
    else {
      throw std::runtime_error("invalid acceleration structure: "+std::string(type));
      return NULL;
    }
  }
  Intersector* rtcCreateAccel(const char* type, TraceData traceFile, const BuildTriangle* triangles, size_t numTriangles)
  {
      Intersector *sansTracer = rtcCreateAccelNoTrace(type,triangles,numTriangles, traceFile.bvhOutputFile);
      if(traceFile.rayTraceFile.str().length()==0)
          return sansTracer;
      return new PrintingTraverser(sansTracer, traceFile.rayTraceFile);
  }

  
}
