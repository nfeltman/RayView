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
        protected float Less;
        protected float Times;

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
        protected float Less;
        protected float Times;

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
        protected float Less;
        protected float Times;

        public ZAASplitSeries(float less, float times)
        {
            Less = less;
            Times = times;
        }

        public int GetBucket<Tri>(Tri tri)
            where Tri : CenterIndexable
        {
            return (int)((tri.Center.z - Less) * Times);
        }
        public int PerformPartition<Tri>(Tri[] tri, int start, int end, int threshold)
            where Tri : CenterIndexable
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].Center.z - Less) * Times < threshold)
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
            return t => ((t.Center.z - Less) * Times < threshold);
        }
    }
}
