using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public interface Centerable
    {
        CVector3 Center { get; }
    }

    public interface TriangleContainer
    {
        Triangle T { get; }
    }

    public struct SimpleBounds
    {
        public Vector4f upper;
        public Vector4f lower;
    }

    public interface Bounded
    {
        SimpleBounds Bounds { get; }
    }

    public interface Indexable
    {
        int Index { get; set; }
    }

    public interface CenterIndexable : Indexable, Centerable
    {}

    public class BasicBuildTriangle : TriangleContainer, CenterIndexable, Bounded
    {
        public Triangle T { get { return _t; } }
        public CVector3 Center { get { return _center; } }
        public int Index { get { return _index; } set { _index = value; } }
        public SimpleBounds Bounds
        {
            get
            {
                Vector4f point1 = new Vector4f(_t.p1.x, _t.p1.y, _t.p1.z, 0f);
                Vector4f point2 = new Vector4f(_t.p2.x, _t.p2.y, _t.p2.z, 0f);
                Vector4f point3 = new Vector4f(_t.p3.x, _t.p3.y, _t.p3.z, 0f);

                return new SimpleBounds() { lower = point1.Min(point2).Min(point3), upper = point1.Max(point2).Max(point3) };
            }
        }

        private Triangle _t;
        private CVector3 _center;
        private int _index;

        public BasicBuildTriangle(Triangle init, int buildIndex)
        {
            _t = init;
            _index = buildIndex;
            Box3 box = new Box3(init.p1, init.p2, init.p3);
            _center = box.GetCenter();
        }
    }

    public interface OBJBacked
    {
        int OBJIndex { get; }
    }

    public class OBJBackedBuildTriangle : CenterIndexable, OBJBacked, Bounded
    {
        public CVector3 Center { get { return _center; } }
        public int Index { get { return _index; } set { _index = value; } }
        public int OBJIndex { get { return _objIndex; } }
        public SimpleBounds Bounds { get { return _bounds; } }

        private SimpleBounds _bounds;
        private CVector3 _center;
        private int _index;
        private int _objIndex;

        public OBJBackedBuildTriangle(int buildIndex, Triangle init, int objBackingIndex)
        {
            _index = buildIndex;
            _objIndex = objBackingIndex;

            Vector4f point1 = new Vector4f(init.p1.x, init.p1.y, init.p1.z, 0f);
            Vector4f point2 = new Vector4f(init.p2.x, init.p2.y, init.p2.z, 0f);
            Vector4f point3 = new Vector4f(init.p3.x, init.p3.y, init.p3.z, 0f);

            Vector4f triMax = point1.Max(point2).Max(point3);
            Vector4f triMin = point1.Min(point2).Min(point3);

            _center = new CVector3((triMin + triMax) * 0.5f);

            _bounds = new SimpleBounds() { lower = triMin, upper = triMax };
        }
    }
}
