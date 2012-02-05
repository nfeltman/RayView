using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface TreeNode<TBranch, TLeaf>
    {
        Ret Accept<Ret>(NodeVisitor<Ret, TBranch, TLeaf> visitor);
        Ret Accept<Ret, TParam>(NodeVisitor<Ret, TParam, TBranch, TLeaf> visitor, TParam param);
        Ret Accept<Ret>(Func<Branch<TBranch, TLeaf>, Ret> forBranch, Func<Leaf<TBranch, TLeaf>, Ret> forLeaf);
        void PrefixEnumerate(Action<TBranch> forBranch, Action<TLeaf> forLeaf);
        void PrefixEnumerateNodes(Action<Branch<TBranch, TLeaf>> forBranch, Action<Leaf<TBranch, TLeaf>> forLeaf);
        void PostfixEnumerate(Action<TBranch> forBranch, Action<TLeaf> forLeaf);
        void PostfixEnumerateNodes(Action<Branch<TBranch, TLeaf>> forBranch, Action<Leaf<TBranch, TLeaf>> forLeaf);
        T RollUp<T>(Func<TBranch, T, T, T> forBranch, Func<TLeaf, T> forLeaf);
        T RollUpNodes<T>(Func<Branch<TBranch, TLeaf>, T, T, T> forBranch, Func<Leaf<TBranch, TLeaf>, T> forLeaf);
    }

    public static class TreeNodeExtension
    {
        public static int ID(this TreeNode<BVH2Branch, BVH2Leaf> node)
        {
            return node.Accept(b => b.Content.ID, l => l.Content.ID);
        }
        public static Box3 BBox(this TreeNode<BVH2Branch, BVH2Leaf> node)
        {
            return node.Accept(b => b.Content.BBox, l => l.Content.BBox);
        }
    }

    public class Branch<TBranch, TLeaf> : TreeNode<TBranch, TLeaf>
    {
        public TreeNode<TBranch, TLeaf> Left { get; set; }
        public TreeNode<TBranch, TLeaf> Right { get; set; }
        public TBranch Content { get; set; }

        public Branch(TreeNode<TBranch, TLeaf> left, TreeNode<TBranch, TLeaf> right, TBranch content)
        {
            Left = left;
            Right = right;
            Content = content;
        }

        public Ret Accept<Ret>(NodeVisitor<Ret, TBranch, TLeaf> visitor)
        {
            return visitor.ForBranch(this);
        }
        public Ret Accept<Ret, TParam>(NodeVisitor<Ret, TParam, TBranch, TLeaf> visitor, TParam param)
        {
            return visitor.ForBranch(this, param);
        }
        public Ret Accept<Ret>(Func<Branch<TBranch, TLeaf>, Ret> forBranch, Func<Leaf<TBranch, TLeaf>, Ret> forLeaf)
        {
            return forBranch(this);
        }
        public void PrefixEnumerate(Action<TBranch> forBranch, Action<TLeaf> forLeaf)
        {
            forBranch(Content);
            Left.PrefixEnumerate(forBranch, forLeaf);
            Right.PrefixEnumerate(forBranch, forLeaf);
        }
        public void PrefixEnumerateNodes(Action<Branch<TBranch, TLeaf>> forBranch, Action<Leaf<TBranch, TLeaf>> forLeaf)
        {
            forBranch(this);
            Left.PrefixEnumerateNodes(forBranch, forLeaf);
            Right.PrefixEnumerateNodes(forBranch, forLeaf);
        }
        public void PostfixEnumerate(Action<TBranch> forBranch, Action<TLeaf> forLeaf)
        {
            Left.PostfixEnumerate(forBranch, forLeaf);
            Right.PostfixEnumerate(forBranch, forLeaf);
            forBranch(Content);
        }
        public void PostfixEnumerateNodes(Action<Branch<TBranch, TLeaf>> forBranch, Action<Leaf<TBranch, TLeaf>> forLeaf)
        {
            Left.PostfixEnumerateNodes(forBranch, forLeaf);
            Right.PostfixEnumerateNodes(forBranch, forLeaf);
            forBranch(this);
        }
        public T RollUp<T>(Func<TBranch, T, T, T> forBranch, Func<TLeaf, T> forLeaf)
        {
            return forBranch(Content, Left.RollUp(forBranch, forLeaf), Right.RollUp(forBranch, forLeaf));
        }
        public T RollUpNodes<T>(Func<Branch<TBranch, TLeaf>, T, T, T> forBranch, Func<Leaf<TBranch, TLeaf>, T> forLeaf)
        {
            return forBranch(this, Left.RollUpNodes(forBranch, forLeaf), Right.RollUpNodes(forBranch, forLeaf));
        }
    }

    public class Leaf<TBranch, TLeaf> : TreeNode<TBranch, TLeaf>
    {
        public TLeaf Content { get; set; }

        public Leaf(TLeaf content)
        {
            Content = content;
        }

        public Ret Accept<Ret>(NodeVisitor<Ret, TBranch, TLeaf> visitor)
        {
            return visitor.ForLeaf(this);
        }
        public Ret Accept<Ret, TParam>(NodeVisitor<Ret, TParam, TBranch, TLeaf> visitor, TParam param)
        {
            return visitor.ForLeaf(this, param);
        }
        public Ret Accept<Ret>(Func<Branch<TBranch, TLeaf>, Ret> forBranch, Func<Leaf<TBranch, TLeaf>, Ret> forLeaf)
        {
            return forLeaf(this);
        }
        public void PrefixEnumerateNodes(Action<Branch<TBranch, TLeaf>> forBranch, Action<Leaf<TBranch, TLeaf>> forLeaf)
        {
            forLeaf(this);
        }

        public void PostfixEnumerateNodes(Action<Branch<TBranch, TLeaf>> forBranch, Action<Leaf<TBranch, TLeaf>> forLeaf)
        {
            forLeaf(this);
        }

        public T RollUpNodes<T>(Func<Branch<TBranch, TLeaf>, T, T, T> forBranch, Func<Leaf<TBranch, TLeaf>, T> forLeaf)
        {
            return forLeaf(this);
        }

        public void PrefixEnumerate(Action<TBranch> forBranch, Action<TLeaf> forLeaf)
        {
            forLeaf(Content);
        }

        public void PostfixEnumerate(Action<TBranch> forBranch, Action<TLeaf> forLeaf)
        {
            forLeaf(Content);
        }

        public T RollUp<T>(Func<TBranch, T, T, T> forBranch, Func<TLeaf, T> forLeaf)
        {
            return forLeaf(Content);
        }
    }

    public interface NodeVisitor<Ret, TBranch, TLeaf>
    {
        Ret ForBranch(Branch<TBranch, TLeaf> branch);
        Ret ForLeaf(Leaf<TBranch, TLeaf> leaf);
    }

    public interface NodeVisitor<Ret, TParam, TBranch, TLeaf>
    {
        Ret ForBranch(Branch<TBranch, TLeaf> branch, TParam param);
        Ret ForLeaf(Leaf<TBranch, TLeaf> leaf, TParam param);
    }
}
