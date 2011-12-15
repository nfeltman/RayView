using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;

namespace RayVisualizer
{
    public interface CrossplaneBehavior
    {
        void UpdateCrossPlane(SceneData scene, float crossPlaneDistance, RaySet intersectionSet);
        void DrawResults(SceneData scene);
    }
}
