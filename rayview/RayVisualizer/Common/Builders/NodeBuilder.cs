using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface NodeFactory<Tri, TBranch, TLeaf, BranchData, Aggregate>
    {
        TBranch BuildBranch(TreeNode<TBranch, TLeaf> left, TreeNode<TBranch, TLeaf> right, BranchData branchData, int branchCounter, int depth, Aggregate boundingBox);
        TLeaf BuildLeaf<Tri2>(Tri2[] tris, int start, int end, int leafCounter, int depth, Aggregate boundingBox) where Tri2 : Tri;
    }
    public interface NodeFactory<Tri, TBranch, TLeaf, TreeType, BranchData, Aggregate> : NodeFactory<Tri, TBranch, TLeaf, BranchData, Aggregate>
    {
        TreeType BuildTree(TreeNode<TBranch, TLeaf> root, int numBranches);
    }
}
