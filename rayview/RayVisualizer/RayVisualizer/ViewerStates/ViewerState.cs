using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace RayVisualizer
{
    interface ViewerState
    {
        IEnumerable<Viewable> CollectViewables(SceneData scene);
        void OnUpdateFrame(SceneData scene, MyKeyboard keyboard);
    }
}
