using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace RayVisualizer
{
    interface ViewerState
    {
        void OnRenderFrame(SceneData scene, GameWindow w, FrameEventArgs e);
        void OnUpdateFrame(SceneData scene, GameWindow w, FrameEventArgs e);
    }
}
