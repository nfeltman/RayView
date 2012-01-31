using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public class BVH2
    {
        private TreeNode<BVH2Branch, BVH2Leaf> _root;
        private int _numBranches;
        public TreeNode<BVH2Branch, BVH2Leaf> Root { get { return _root; } }
        public int NumNodes { get { return _numBranches * 2 + 1; } }
        public int NumBranch { get { return _numBranches; } }
        public int NumLeaves { get { return _numBranches + 1; } }

        public BVH2(TreeNode<BVH2Branch, BVH2Leaf> root, int numBranches) 
        { 
            _root = root; 
            _numBranches = numBranches; 
        }

        public Ret Accept<Ret>(NodeVisitor<Ret, BVH2Branch, BVH2Leaf> visitor)
        {
            return _root.Accept(visitor);
        }
        public void PrefixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf)
        {
            _root.PrefixEnumerate(forBranch, forLeaf);
        }
        public void PostfixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf)
        {
            _root.PostfixEnumerate(forBranch, forLeaf);
        }
        public T RollUp<T>(Func<BVH2Branch, T, T, T> forBranch, Func<BVH2Leaf, T> forLeaf)
        {
            return _root.RollUp(forBranch, forLeaf);
        }
        public T RollUpNodes<T>(Func<Branch<BVH2Branch, BVH2Leaf>, T, T, T> forBranch, Func<Leaf<BVH2Branch, BVH2Leaf>, T> forLeaf)
        {

            return _root.RollUpNodes(forBranch, forLeaf);
        }
        
    }

    public struct BVH2Branch
    {
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }
    }

    public struct BVH2Leaf
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public Triangle[] Primitives { get; set; }

        public HitRecord FindClosestPositiveIntersection(CVector3 origin, CVector3 direction, ClosedInterval tInterval)
        {
            // TODO : have it take in a c vector

            float closestIntersection = float.PositiveInfinity;
            int closestIndex = -1;
            Triangle[] triangles = Primitives;
            for (int k = 0; k < triangles.Length; k++)
            {
                float intersection = triangles[k].IntersectRay(origin, direction);
                if (!float.IsNaN(intersection) && intersection>0.001 && tInterval.Contains(intersection) && intersection < closestIntersection)
                {
                    closestIntersection = intersection;
                    closestIndex = k;
                }
            }
            if (closestIndex == -1)
                return null;
            return new HitRecord(triangles[closestIndex],closestIntersection, ID);
        }
    }

}
