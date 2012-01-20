using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface NodeFactory<NodeType, TreeType, BranchData>
    {
        NodeType BuildBranch(NodeType left, NodeType right, BranchData branchData, int branchCounter, int depth, Box3 boundingBox);
        NodeType BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, Box3 boundingBox);
        TreeType BuildTree(NodeType root, int numBranches);
    }
}
