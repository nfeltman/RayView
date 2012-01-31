using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface NodeFactory<TBranch, TLeaf, BranchData>
    {
        TBranch BuildBranch(TreeNode<TBranch, TLeaf> left, TreeNode<TBranch, TLeaf> right, BranchData branchData, int branchCounter, int depth, Box3 boundingBox);
        TLeaf BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, Box3 boundingBox);
    }
    public interface NodeFactory<TBranch, TLeaf, TreeType, BranchData> : NodeFactory<TBranch, TLeaf, BranchData>
    {
        TreeType BuildTree(TreeNode<TBranch, TLeaf> root, int numBranches);
    }
}
