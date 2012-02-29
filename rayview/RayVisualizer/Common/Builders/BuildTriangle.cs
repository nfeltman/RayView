using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public interface Indexable
    {
        int Index { get; set; }
    }

    public interface CenterIndexable : Indexable, Centerable
    {}

    public class BasicBuildTriangle : TriangleContainer, CenterIndexable
    {
        public Triangle T { get { return _t; } }
        public CVector3 Center { get { return _center; } }
        public int Index { get { return _index; } set { _index = value; } }


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

    public class OBJBackedBuildTriangle : CenterIndexable, OBJBacked, TriangleContainer
    {
        public CVector3 Center { get { return _center; } }
        public int Index { get { return _index; } set { _index = value; } }
        public int OBJIndex { get { return _objIndex; } }
        public Triangle T{get { return _t; }}

        private Triangle _t;
        private CVector3 _center;
        private int _index;
        private int _objIndex;

        public OBJBackedBuildTriangle(int buildIndex, Triangle init, int objBackingIndex)
        {
            _t = init;
            _index = buildIndex;
            Box3 box = new Box3(init.p1, init.p2, init.p3);
            _center = box.GetCenter();
            _objIndex = objBackingIndex;
        }
    }
}
