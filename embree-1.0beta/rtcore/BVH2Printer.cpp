#include "BVH2Printer.h"

namespace embree{


  void BVH2Printer::printBVH2ToFile(Ref<BVH2<Triangle4> > bvh, FileName& bvhOutput)
  {
      FILE* file = fopen(bvhOutput.c_str(), "wb");
      printNode(bvh->root, Box(True), bvh, file);
      int end_sentinel = 9215;
      fwrite(&end_sentinel,sizeof(int),1,file);
      fclose(file);
  }

  
  void BVH2Printer::printNode(int nodeNum, Box bbox, Ref<BVH2<Triangle4> > bvh, FILE* file)
  {
      if(nodeNum >= 0)
      {
        // BRANCH
        BVH2<Triangle4>::Node n = bvh->node(nodeNum);
        int header = 2; // for implicit branch node
        /*
        float floats[6];
        floats[0] = bbox.lower[0];
        floats[1] = bbox.upper[0];
        floats[2] = bbox.lower[1];
        floats[3] = bbox.upper[1];
        floats[4] = bbox.lower[2];
        floats[5] = bbox.upper[2];
        */
        fwrite(&header,sizeof(int),1,file);
        //fwrite(&floats,sizeof(float),6,file);
        printNode(n.child[0], n.bounds(0), bvh, file);
        printNode(n.child[1], n.bounds(1), bvh, file);
      }
      else
      {
        // LEAF
        int leadID = nodeNum ^ 0x80000000;
        
        const size_t ofs = size_t(leadID) >> 5;
        const size_t num = size_t(leadID) & 0x1F;
        
        int totalTriangleCount = 0;
        for (size_t i=ofs; i<ofs+num; i++) totalTriangleCount += bvh->triangles[i].size();
        
        int header[2];
        header[0] = 1; //for leaf node
        header[1] = totalTriangleCount;
        float floats[9];
        floats[0] = bbox.lower.v[0];
        floats[1] = bbox.upper.v[0];
        floats[2] = bbox.lower.v[1];
        floats[3] = bbox.upper.v[1];
        floats[4] = bbox.lower.v[2];
        floats[5] = bbox.upper.v[2];
        fwrite(&header,sizeof(int),2,file);
        fwrite(&floats,sizeof(float),6,file);

        //each triangle block could be up to 4 triangles
        for (size_t i=ofs; i<ofs+num; i++)
        {
            Triangle4 t = bvh->triangles[i];
            int size = t.size();
            sse3f p1 = t.v0;
            sse3f p2 = t.v0-t.e1;
            sse3f p3 = t.v0+t.e2;
            for(int j=0;j<size;j++)
            {
                floats[0] = p1.x.v[j];
                floats[1] = p1.y.v[j];
                floats[2] = p1.z.v[j];
                floats[3] = p2.x.v[j];
                floats[4] = p2.y.v[j];
                floats[5] = p2.z.v[j];
                floats[6] = p3.x.v[j];
                floats[7] = p3.y.v[j];
                floats[8] = p3.z.v[j];
                fwrite(&floats,sizeof(float),9,file);
            }
        }
      }
  }
}