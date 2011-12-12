using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayQuery
    {
        public RayKind Kind { get; set; }
        public int Depth { get; set; }
        //public int ObjID { get; set; }
        public CVector3 Origin { get; set; }
        public CVector3 Direction { get; set; } // |Direction| == 1 if Kind == IntersectionMiss

        /*
        public bool BoxIntersect(Box3 b)
        {
            if (Kind == RayKind.IntersectionMiss)
                return !b.IntersectRay(Origin, End).IsEmpty;
            return !b.IntersectSegment(Origin, End).IsEmpty;
        }
        */
    }

    public class FHRayHit
    {
        public CVector3 Origin { get; set; }
        public CVector3 Difference { get; set; }
    }
    public class FHRayMiss
    {
        public CVector3 Origin { get; set; }
        public CVector3 Direction { get; set; }
    }

    public enum RayKind
    {
        FirstHit_Hit,
        FirstHit_Miss,
        AnyHit_Connected,
        AnyHit_Broken
    }
}
