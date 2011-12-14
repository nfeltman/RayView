using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public abstract class RayQuery
    {
        public int Depth { get; set; }
    }

    public abstract class CastQuery : RayQuery
    {
        public CVector3 Origin { get; set; }
    }

    public class CastHitQuery : CastQuery
    {
        public CVector3 Difference { get; set; }
    }

    public class CastMissQuery : CastQuery
    {
        public CVector3 Direction { get; set; } // |Direction| == 1
    }

    public class ShadowQuery : RayQuery
    {
        public bool Connected { get; set; }
        public CVector3 Origin { get; set; }
        public CVector3 Difference { get; set; }
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
}
