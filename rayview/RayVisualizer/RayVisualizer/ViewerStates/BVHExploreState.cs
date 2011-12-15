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
        private Stack<BVH2Node> _ancestor;
        private BVHTriangleViewer _viewer;

        public BVHExploreState(BVHTriangleViewer viewer)
        {
            _ancestor = new Stack<BVH2Node>();
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
}
