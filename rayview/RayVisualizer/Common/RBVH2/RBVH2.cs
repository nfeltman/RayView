using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RBVH2
    {
        private TreeNode<RBVH2Branch,RBVH2Leaf> _root;
        private int _numBranches;
        public TreeNode<RBVH2Branch, RBVH2Leaf> Root { get { return _root; } }
        public int NumNodes { get { return _numBranches * 2 + 1; } }
        public int NumBranch { get { return _numBranches; } }
        public int NumLeaves { get { return _numBranches + 1; } }

        public RBVH2(TreeNode<RBVH2Branch, RBVH2Leaf> root, int numBranches) 
        { 
            _root = root; 
            _numBranches = numBranches; 
        }

        public Ret Accept<Ret>(NodeVisitor<Ret, RBVH2Branch, RBVH2Leaf> visitor)
        {
            return _root.Accept(visitor);
        }
        public void PrefixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf)
        {
            _root.PrefixEnumerate(forBranch, forLeaf);
        }
        public void PostfixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf)
        {
            _root.PostfixEnumerate(forBranch, forLeaf);
        }
        public T RollUp<T>(Func<RBVH2Branch, T, T, T> forBranch, Func<RBVH2Leaf, T> forLeaf)
        {
            return _root.RollUp(forBranch, forLeaf);
        }

        
    }

    public struct RBVH2Branch 
    {
        public float PLeft { get; set; }
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }
    }

    public struct RBVH2Leaf
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public Triangle[] Primitives { get; set; }

        public HitRecord FindIntersection(CVector3 origin, CVector3 direction, ClosedInterval tInterval)
        {
            Triangle[] triangles = Primitives;
            for (int k = 0; k < triangles.Length; k++)
            {
                float intersection = triangles[k].IntersectRay(origin, direction);
                if (!float.IsNaN(intersection) && intersection>0.001 && tInterval.Contains(intersection))
                {
                    return new HitRecord(triangles[k], intersection, ID);
                }
            }
            return null;
        }
    }
}
