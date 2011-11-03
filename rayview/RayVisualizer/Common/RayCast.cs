using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayCast
    {
        public RayKind Kind { get; set; }
        public int Depth { get; set; }
        //public int ObjID { get; set; }
        public CVector3 Origin { get; set; }
        public CVector3 End { get; set; } // |End-Origin| == 1 if Kind == IntersectionMiss

        public bool BoxIntersect(Box3 b)
        {
            if (Kind == RayKind.IntersectionMiss)
                return b.IntersectRay(Origin, End);
            return b.IntersectSegment(Origin, End);
        }
    }

    public enum RayKind
    {
        IntersectionHit,
        IntersectionMiss,
        OcclusionConnect,
        OcclusionBroken
    }
}
