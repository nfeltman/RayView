using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer
{
    interface CrossplaneBehavior
    {
        void UpdateCrossPlane(SceneData scene);
        void DrawResults(SceneData scene);
    }
}
