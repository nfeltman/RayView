using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;

namespace RayVisualizer
{
    class NullCrossplane : CrossplaneBehavior
    {
        public void UpdateCrossPlane(SceneData scene, float crossPlaneDistance, RaySet intersectionSet)
        {
            //do nothing
        }

        public void DrawResults(SceneData scene)
        {
            //do nothing
        }
    }
}
