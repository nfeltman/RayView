using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RBVH2
    {
        private RBVH2Node _root;
        private int _numBranches;
        public RBVH2Node Root { get { return _root; } }
        public int NumNodes { get { return _numBranches * 2 + 1; } }
        public int NumBranch { get { return _numBranches; } }
        public int NumLeaves { get { return _numBranches + 1; } }

        public RBVH2(RBVH2Node root, int numBranches) 
        { 
            _root = root; 
            _numBranches = numBranches; 
        }

        public Ret Accept<Ret>(RBVH2Visitor<Ret> visitor)
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

    public interface RBVH2Node
    {
        Box3 BBox { get; set; }
        int Depth { get; set; }
        Ret Accept<Ret>(RBVH2Visitor<Ret> visitor);
        Ret Accept<Ret>(Func<RBVH2Branch, Ret> forBranch, Func<RBVH2Leaf, Ret> forLeaf);
        void PrefixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf);
        void PostfixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf);
        T RollUp<T>(Func<RBVH2Branch, T, T, T> forBranch, Func<RBVH2Leaf, T> forLeaf);
    }

    public class RBVH2Branch : RBVH2Node
    {
        public RBVH2Node Left { get; set; }
        public RBVH2Node Right { get; set; }
        public float PLeft { get; set; }
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }

        public Ret Accept<Ret>(RBVH2Visitor<Ret> visitor)
        {
            return visitor.ForBranch(this);
        }
        public Ret Accept<Ret>(Func<RBVH2Branch, Ret> forBranch, Func<RBVH2Leaf, Ret> forLeaf)
        {
            return forBranch(this);
        }
        public void PrefixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf)
        {
            forBranch(this);
            Left.PrefixEnumerate(forBranch, forLeaf);
            Right.PrefixEnumerate(forBranch, forLeaf);
        }
        public void PostfixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf)
        {
            Left.PostfixEnumerate(forBranch, forLeaf);
            Right.PostfixEnumerate(forBranch, forLeaf);
            forBranch(this);
        }
        public T RollUp<T>(Func<RBVH2Branch,T,T,T> forBranch, Func<RBVH2Leaf,T> forLeaf)
        {
            return forBranch(this, Left.RollUp(forBranch, forLeaf), Right.RollUp(forBranch, forLeaf));
        }
    }

    public class RBVH2Leaf : RBVH2Node
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public Triangle[] Primitives { get; set; }

        public Ret Accept<Ret>(RBVH2Visitor<Ret> visitor)
        {
            return visitor.ForLeaf(this);
        }
        public Ret Accept<Ret>(Func<RBVH2Branch, Ret> forBranch, Func<RBVH2Leaf, Ret> forLeaf)
        {
            return forLeaf(this);
        }
        public void PrefixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf)
        {
            forLeaf(this);
        }
        public void PostfixEnumerate(Action<RBVH2Branch> forBranch, Action<RBVH2Leaf> forLeaf)
        {
            forLeaf(this);
        }
        public T RollUp<T>(Func<RBVH2Branch, T, T, T> forBranch, Func<RBVH2Leaf, T> forLeaf)
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
