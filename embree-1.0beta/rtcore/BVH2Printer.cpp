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
            bbox.lower[0], bbox.upper[0],
            bbox.lower[1], bbox.upper[1],
            bbox.lower[2], bbox.upper[2]);
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

        fprintf(file,"leaf %i %i %f %f %f %f %f %f\n",
            nodeNum,
            totalTriangleCount,
            bbox.lower.v[0], bbox.upper.v[0],
            bbox.lower.v[1], bbox.upper.v[1],
            bbox.lower.v[2], bbox.upper.v[2]);
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
				fprintf(file,"tri %f %f %f %f %f %f %f %f %f\n", p1.x.v[j],p1.y.v[j],p1.z.v[j],p2.x.v[j],p2.y.v[j],p2.z.v[j],p3.x.v[j],p3.y.v[j],p3.z.v[j]);
			}
        }
      }
  }
}