using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using OpenTK.Input;
using OpenTK;

namespace RayVisualizer
{
    class BVHExploreState : ViewerState
    {
        private Stack<TreeNode<BVH2Branch, BVH2Leaf>> _ancestor;
        private BVHTriangleViewer _viewer;

        public BVHExploreState(BVHTriangleViewer viewer)
        {
            _ancestor = new Stack<TreeNode<BVH2Branch, BVH2Leaf>>();
            _viewer = viewer;
        }

        public void OnUpdateFrame(SceneData scene, MyKeyboard keyboard)
        {
            if (keyboard.IsFirstPress(Key.Left))
            {
                _viewer.BVH.Accept(branch =>
                {
                    _ancestor.Push(branch);
                    _viewer.BVH = branch.Left;
                    return 0;
                },
                leaf => 0);
            }
            if (keyboard.IsFirstPress(Key.Right))
            {
                _viewer.BVH.Accept(branch =>
                {
                    _ancestor.Push(branch);
                    _viewer.BVH = branch.Right;
                    return 0;
                },
                leaf => 0);
            }
            if (keyboard.IsFirstPress(Key.Up))
            {
                if (_ancestor.Count > 0)
                {
                    _viewer.BVH = _ancestor.Pop();
                }
            }
        }


        public void HibernateState(SceneData scene)
        {
        }

        public void ActivateState(SceneData scene)
        {
        }
    }

    class RBVHExploreState : ViewerState
    {
        private Stack<TreeNode<RBVH2Branch, RBVH2Leaf>> _ancestor;
        private RBVHTriangleViewer _viewer;

        public RBVHExploreState(RBVHTriangleViewer viewer)
        {
            _ancestor = new Stack<TreeNode<RBVH2Branch, RBVH2Leaf>>();
            _viewer = viewer;
        }

        public void OnUpdateFrame(SceneData scene, MyKeyboard keyboard)
        {
            if (keyboard.IsFirstPress(Key.Left))
            {
                _viewer.RBVH.Accept(branch =>
                {
                    _ancestor.Push(branch);
                    _viewer.RBVH = branch.Left;
                    return 0;
                },
                leaf => 0);
            }
            if (keyboard.IsFirstPress(Key.Right))
            {
                _viewer.RBVH.Accept(branch =>
                {
                    _ancestor.Push(branch);
                    _viewer.RBVH = branch.Right;
                    return 0;
                },
                leaf => 0);
            }
            if (keyboard.IsFirstPress(Key.Up))
            {
                if (_ancestor.Count > 0)
                {
                    _viewer.RBVH = _ancestor.Pop();
                }
            }
        }


        public void HibernateState(SceneData scene)
        {
        }

        public void ActivateState(SceneData scene)
        {
        }
    }
}
