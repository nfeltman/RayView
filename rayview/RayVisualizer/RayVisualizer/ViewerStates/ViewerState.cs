using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace RayVisualizer
{
    interface ViewerState
    {
        void OnUpdateFrame(SceneData scene, MyKeyboard keyboard);
        void HibernateState(SceneData scene);
        void ActivateState(SceneData scene);
    }
}
