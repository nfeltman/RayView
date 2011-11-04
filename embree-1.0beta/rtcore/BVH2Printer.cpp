#include "BVH2Printer.h"

namespace embree{


  void BVH2Printer::printBVH2ToFile(Ref<BVH2<Triangle4> > bvh, FileName& bvhOutput)
  {
      FILE* file = fopen(bvhOutput.c_str(), "wb");
      printNode(bvh->root, Box(), bvh, file);
      fclose(file);
  }

  
  void BVH2Printer::printNode(int nodeNum, Box bbox, Ref<BVH2<Triangle4> > bvh, FILE* file)
  {
      if(nodeNum >= 0)
      {
        // BRANCH
        BVH2<Triangle4>::Node n = bvh->node(nodeNum);
        fprintf(file,"bran %i %i %i %f %f %f %f %f %f\n",
            nodeNum,
            n.child[0], n.child[1],
            bbox.lower.v[2], nodeNum,bbox.upper.v[2],
            bbox.lower.v[1], nodeNum,bbox.upper.v[1],
            bbox.lower.v[0], nodeNum,bbox.upper.v[0]);
        printNode(n.child[0], n.bounds(0), bvh, file);
        printNode(n.child[1], n.bounds(1), bvh, file);
      }
      else
      {
        // LEAF
        int leadID = nodeNum ^ 0x80000000;
        
        const size_t ofs = size_t(leadID) >> 5;
        const size_t num = size_t(leadID) & 0x1F;
        
        fprintf(file,"leaf %i %i %i %f %f %f %f %f %f\n",
            nodeNum,
            ofs, num,
            bbox.lower.v[2], nodeNum,bbox.upper.v[2],
            bbox.lower.v[1], nodeNum,bbox.upper.v[1],
            bbox.lower.v[0], nodeNum,bbox.upper.v[0]);
        //each triangle block could be up to 4 triangles
        //for (size_t i=ofs; i<ofs+num; i++) bvh->triangles[i].intersect(ray,hit);
      }
  }
}