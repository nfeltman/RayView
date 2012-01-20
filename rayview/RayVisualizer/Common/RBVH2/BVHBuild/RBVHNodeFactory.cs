using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RBVHNodeFactory : NodeFactory<RBVH2Node, RBVH2, float>
    {
        public static readonly RBVHNodeFactory ONLY = new RBVHNodeFactory();

        private RBVHNodeFactory() { }

        public RBVH2Node BuildBranch(RBVH2Node left, RBVH2Node right, float pLeft, int branchCounter, int depth, Box3 boundingBox)
        {
            return new RBVH2Branch() { Left = left, Right = right, ID = branchCounter, BBox = boundingBox, Depth = depth, PLeft = pLeft };
        }

        public RBVH2Node BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, Box3 boundingBox)
        {
            Triangle[] prims = new Triangle[end-start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].t;
            return new RBVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox };
        }

        public RBVH2 BuildTree(RBVH2Node root, int numBranches)
        {
            return new RBVH2(root, numBranches);
        }
    }
}
