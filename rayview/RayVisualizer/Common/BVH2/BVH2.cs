using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public class BVH2
    {
        private BVH2Node _root;
        private int _numBranches;
        public BVH2Node Root { get { return _root; } }
        public int NumNodes { get { return _numBranches * 2 + 1; } }
        public int NumBranch { get { return _numBranches; } }
        public int NumLeaves { get { return _numBranches + 1; } }

        public BVH2(BVH2Node root, int numBranches) 
        { 
            _root = root; 
            _numBranches = numBranches; 
        }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
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

        
    }

    public interface BVH2Node
    {
        Box3 BBox { get; set; }
        int Depth { get; set; }
        Ret Accept<Ret>(BVH2Visitor<Ret> visitor);
        Ret Accept<Ret>(Func<BVH2Branch, Ret> forBranch, Func<BVH2Leaf, Ret> forLeaf);
        void PrefixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf);
        void PostfixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf);
        T RollUp<T>(Func<BVH2Branch, T, T, T> forBranch, Func<BVH2Leaf, T> forLeaf);
    }

    public class BVH2Branch : BVH2Node
    {
        public BVH2Node Left { get; set; }
        public BVH2Node Right { get; set; }
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return visitor.ForBranch(this);
        }
        public Ret Accept<Ret>(Func<BVH2Branch, Ret> forBranch, Func<BVH2Leaf, Ret> forLeaf)
        {
            return forBranch(this);
        }
        public void PrefixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf)
        {
            forBranch(this);
            Left.PrefixEnumerate(forBranch, forLeaf);
            Right.PrefixEnumerate(forBranch, forLeaf);
        }
        public void PostfixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf)
        {
            Left.PostfixEnumerate(forBranch, forLeaf);
            Right.PostfixEnumerate(forBranch, forLeaf);
            forBranch(this);
        }
        public T RollUp<T>(Func<BVH2Branch,T,T,T> forBranch, Func<BVH2Leaf,T> forLeaf)
        {
            return forBranch(this, Left.RollUp(forBranch, forLeaf), Right.RollUp(forBranch, forLeaf));
        }
    }

    public class BVH2Leaf : BVH2Node
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public Triangle[] Primitives { get; set; }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return visitor.ForLeaf(this);
        }
        public Ret Accept<Ret>(Func<BVH2Branch, Ret> forBranch, Func<BVH2Leaf, Ret> forLeaf)
        {
            return forLeaf(this);
        }
        public void PrefixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf)
        {
            forLeaf(this);
        }
        public void PostfixEnumerate(Action<BVH2Branch> forBranch, Action<BVH2Leaf> forLeaf)
        {
            forLeaf(this);
        }
        public T RollUp<T>(Func<BVH2Branch, T, T, T> forBranch, Func<BVH2Leaf, T> forLeaf)
        {
            return forLeaf(this);
        }

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
