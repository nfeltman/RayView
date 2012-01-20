using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class OrderedDepthFirstOpCounter : OrderedDepthFirstOperations
    {
        public int rayCasts;
        public int boundingBoxTests;
        public int boundingBoxHits;
        public int primitiveNodeInspections;
        public int primitiveNodePrimitiveHits;
        public int branchNodeInspections;
        public int rayHitFound;

        public void RayCast(CVector3 origin, CVector3 direction)
        {
            rayCasts++;
        }

        public void BoundingBoxTest(BVH2Node node)
        {
            boundingBoxTests++;
        }

        public void BoundingBoxHit(BVH2Node node)
        {
            boundingBoxHits++;
        }

        public void PrimitiveNodeInspection(BVH2Leaf leaf)
        {
            primitiveNodeInspections++;
        }

        public void PrimitiveNodePrimitiveHit(BVH2Leaf leaf, HitRecord hit)
        {
            primitiveNodePrimitiveHits++;
        }

        public void BranchNodeInspection(BVH2Branch branch)
        {
            branchNodeInspections++;
        }

        public void RayHitFound(HitRecord hit)
        {
            rayHitFound++;
        }
    }
}
