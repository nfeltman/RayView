using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayOrderOpCounter : RayOrderOperations
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

        public void BoundingBoxTest(TreeNode<BVH2Branch, BVH2Leaf> node)
        {
            boundingBoxTests++;
        }

        public void BoundingBoxHit(TreeNode<BVH2Branch, BVH2Leaf> node)
        {
            boundingBoxHits++;
        }

        public void PrimitiveNodeInspection(Leaf<BVH2Branch, BVH2Leaf> leaf)
        {
            primitiveNodeInspections++;
        }

        public void PrimitiveNodePrimitiveHit(Leaf<BVH2Branch, BVH2Leaf> leaf, HitRecord hit)
        {
            primitiveNodePrimitiveHits++;
        }

        public void BranchNodeInspection(Branch<BVH2Branch, BVH2Leaf> branch)
        {
            branchNodeInspections++;
        }

        public void RayHitFound(HitRecord hit)
        {
            rayHitFound++;
        }
    }
}
