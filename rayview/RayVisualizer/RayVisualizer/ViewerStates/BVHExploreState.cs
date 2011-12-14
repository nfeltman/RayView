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
        private BVH2Node _currentView;
        private Stack<BVH2Node> _ancestor;

        public BVHExploreState(BVH2 bvh)
        {
            _currentView = bvh.Root;
            _ancestor = new Stack<BVH2Node>();
        }

        public IEnumerable<Viewable> CollectViewables(SceneData scene)
        {
            return new Viewable[] { new BVHTriangleViewer(_currentView) };
        }

        public void OnUpdateFrame(SceneData scene, MyKeyboard keyboard)
        {
            if (keyboard.IsFirstPress(Key.Left))
            {
                _currentView.Accept(branch =>
                {
                    _ancestor.Push(_currentView);
                    _currentView = branch.Left;
                    return 0;
                },
                leaf => 0);
            }
            if (keyboard.IsFirstPress(Key.Right))
            {
                _currentView.Accept(branch =>
                {
                    _ancestor.Push(_currentView);
                    _currentView = branch.Right;
                    return 0;
                },
                leaf => 0);
            }
            if (keyboard.IsFirstPress(Key.Up))
            {
                if (_ancestor.Count > 0)
                {
                    _currentView = _ancestor.Pop();
                }
            }
        }
    }
}
