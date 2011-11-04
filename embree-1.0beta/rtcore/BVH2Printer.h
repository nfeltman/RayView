#ifndef __EMBREE_BVH2_PRINTER_H__
#define __EMBREE_BVH2_PRINTER_H__

#include "bvh2/bvh2.h"
#include "bvh4/triangle4.h"

namespace embree{

class BVH2Printer
{
public:
    static void printBVH2ToFile(Ref<BVH2<Triangle4> > bvh, FileName& bvhOutput);
    static void printNode(int nodeNum, Box bbox, Ref<BVH2<Triangle4> > bvh, FILE* file);
};

}


#endif