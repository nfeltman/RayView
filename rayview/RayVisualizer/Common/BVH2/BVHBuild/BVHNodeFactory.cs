using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class BVHNodeFactory : NodeFactory<BVH2Node, BVH2, Unit>
    {
        public static readonly BVHNodeFactory ONLY = new BVHNodeFactory();

        private BVHNodeFactory() { }

        public BVH2Node BuildBranch(BVH2Node left, BVH2Node right, Unit branchData, int branchCounter, int depth, Box3 boundingBox)
        {
            return new BVH2Branch() { Left = left, Right = right, ID = branchCounter, BBox = boundingBox, Depth = depth };
        }

        public BVH2Node BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, Box3 boundingBox)
        {
            Triangle[] prims = new Triangle[end-start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].t;
            return new BVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox };
        }

        public BVH2 BuildTree(BVH2Node root, int numBranches)
        {
            return new BVH2(root, numBranches);
        }
    }
}
