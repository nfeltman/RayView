using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class SAH5050Factory: NodeFactory<RBVH2Branch, RBVH2Leaf, RBVH2, Unit>
    {
        public static readonly SAH5050Factory ONLY = new SAH5050Factory();

        private SAH5050Factory() { }

        public RBVH2Branch BuildBranch(TreeNode<RBVH2Branch, RBVH2Leaf> left, TreeNode<RBVH2Branch, RBVH2Leaf> right, Unit unit, int branchCounter, int depth, Box3 boundingBox)
        {
            return new RBVH2Branch() { ID = branchCounter, BBox = boundingBox, Depth = depth, PLeft = 0.5f };
        }

        public RBVH2Leaf BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, Box3 boundingBox)
        {
            Triangle[] prims = new Triangle[end-start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].t;
            return new RBVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox };
        }

        public RBVH2 BuildTree(TreeNode<RBVH2Branch, RBVH2Leaf> root, int numBranches)
        {
            return new RBVH2(root, numBranches);
        }
    }
}
