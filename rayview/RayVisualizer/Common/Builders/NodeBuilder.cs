using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface NodeFactory<TBranch, TLeaf, BranchData, Aggregate>
    {
        TBranch BuildBranch(TreeNode<TBranch, TLeaf> left, TreeNode<TBranch, TLeaf> right, BranchData branchData, int branchCounter, int depth, Aggregate boundingBox);
        TLeaf BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, Aggregate boundingBox);
    }
    public interface NodeFactory<TBranch, TLeaf, TreeType, BranchData, Aggregate> : NodeFactory<TBranch, TLeaf, BranchData, Aggregate>
    {
        TreeType BuildTree(TreeNode<TBranch, TLeaf> root, int numBranches);
    }
}
