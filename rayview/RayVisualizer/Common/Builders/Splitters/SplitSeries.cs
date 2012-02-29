using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface SplitSeries
    {
        int PerformPartition<Tri>(Tri[] tri, int start, int end, int threshold)
            where Tri : CenterIndexable;
        int GetBucket<Tri>(Tri tri)
            where Tri : CenterIndexable;
        Func<Tri, bool> GetFilter<Tri>(int threshold)
            where Tri : CenterIndexable;
    }

    public class XAASplitSeries : SplitSeries
    {
        private float Less;
        private float Times;

        public XAASplitSeries(float less, float times)
        {
            Less = less;
            Times = times;
        }

        public int GetBucket<Tri>(Tri tri)
            where Tri : CenterIndexable
        {
            return (int)((tri.Center.x - Less) * Times);
        }
        public int PerformPartition<Tri>(Tri[] tri, int start, int end, int threshold)
            where Tri : CenterIndexable
        {
            int partLoc = start; // the first larger-than-partVal element
            for (int k = start; k < end; k++)
            {
                if ((tri[k].Center.x - Less) * Times < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }

            return partLoc;
        }
        public Func<Tri, bool> GetFilter<Tri>(int threshold)
            where Tri : CenterIndexable
        {
            return t => ((t.Center.x - Less) * Times < threshold);
        }
    }

    public class YAASplitSeries : SplitSeries
    {
        private float Less;
        private float Times;

        public YAASplitSeries(float less, float times)
        {
            Less = less;
            Times = times;
        }

        public int GetBucket<Tri>(Tri tri)
            where Tri : CenterIndexable
        {
            return (int)((tri.Center.y - Less) * Times);
        }
        public int PerformPartition<Tri>(Tri[] tri, int start, int end, int threshold)
            where Tri : CenterIndexable
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].Center.y - Less) * Times < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }

            return partLoc;
        }
        public Func<Tri, bool> GetFilter<Tri>(int threshold)
            where Tri : CenterIndexable
        {
            return t => ((t.Center.y - Less) * Times < threshold);
        }
    }

    public class ZAASplitSeries : SplitSeries
    {
        private float _offset;
        private float _factor;

        public ZAASplitSeries(float offset, float factor)
        {
            _offset = offset;
            _factor = factor;
        }

        public int GetBucket<Tri>(Tri tri)
            where Tri : CenterIndexable
        {
            return (int)((tri.Center.z - _offset) * _factor);
        }
        public int PerformPartition<Tri>(Tri[] tri, int start, int end, int threshold)
            where Tri : CenterIndexable
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].Center.z - _offset) * _factor < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }

            return partLoc;
        }
        public Func<Tri, bool> GetFilter<Tri>(int threshold)
            where Tri : CenterIndexable
        {
            return t => ((t.Center.z - _offset) * _factor < threshold);
        }
    }

    public class RadialSplitSeries : SplitSeries
    {
        private CVector3 Center;
        private float _factor;
        private float _offset;

        public RadialSplitSeries(CVector3 center, float offset, float factor)
        {
            Center = center;
            _factor = factor;
            _offset = offset;
        }

        public int PerformPartition<Tri>(Tri[] tri, int start, int end, int threshold) where Tri : CenterIndexable
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if (((tri[k].Center - Center).Length() - _offset) * _factor < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }
            if (partLoc <= start || partLoc >= end)
            {
                Console.WriteLine("bad part. specs: {0} {1} {2} {3} {4} {5}", partLoc, start, end, Center, _offset, _factor);
                Console.WriteLine("threshold "+threshold);
                for (int k = start; k < end; k++)
                {
                    Console.WriteLine("val [" + k + "] = " + (((tri[k].Center - Center).Length() - _offset) * _factor) + "; center= " + tri[k].Center);
                }
            }
            return partLoc;
        }

        public int GetBucket<Tri>(Tri tri) where Tri : CenterIndexable
        {
            return (int)(((tri.Center - Center).Length() - _offset) * _factor); 
        }

        public Func<Tri, bool> GetFilter<Tri>(int threshold) where Tri : CenterIndexable
        {
            return t => (((t.Center - Center).Length() - _offset) * _factor < threshold);
        }
    }
}
