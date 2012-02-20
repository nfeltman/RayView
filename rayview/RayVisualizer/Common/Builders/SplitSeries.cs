using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface SplitSeries
    {
        int PerformPartition(BuildTriangle[] tri, int start, int end, int threshold);
        int GetBucket(BuildTriangle tri);
        Func<BuildTriangle, bool> GetFilter(int threshold);
    }
    public abstract class AASplitSeries : SplitSeries
    {
        protected float Less;
        protected float Times;

        public AASplitSeries(float less, float times)
        {
            Less = less;
            Times = times;
        }

        public abstract int GetBucket(BuildTriangle tri);
        public abstract int PerformPartition(BuildTriangle[] tri, int start, int end, int threshold);
        public abstract Func<BuildTriangle, bool> GetFilter(int threshold);
    }

    public class XAASplitSeries : AASplitSeries
    {
        public XAASplitSeries(float less, float times) : base(less, times){}

        public override int GetBucket(BuildTriangle tri)
        {
            return (int)((tri.center.x - Less) * Times);
        }
        public override int PerformPartition(BuildTriangle[] tri, int start, int end, int threshold)
        {
            int partLoc = start; // the first larger-than-partVal element
            for (int k = start; k < end; k++)
            {
                if ((tri[k].center.x - Less) * Times < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }

            return partLoc;
        }
        public override Func<BuildTriangle, bool> GetFilter(int threshold)
        {
            return t => ((t.center.x - Less) * Times < threshold);
        }
    }

    public class YAASplitSeries : AASplitSeries
    {
        public YAASplitSeries(float less, float times) : base(less, times) { }

        public override int GetBucket(BuildTriangle tri)
        {
            return (int)((tri.center.y - Less) * Times);
        }
        public override int PerformPartition(BuildTriangle[] tri, int start, int end, int threshold)
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].center.y - Less) * Times < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }

            return partLoc;
        }
        public override Func<BuildTriangle, bool> GetFilter(int threshold)
        {
            return t => ((t.center.y - Less) * Times < threshold);
        }
    }

    public class ZAASplitSeries : AASplitSeries
    {
        public ZAASplitSeries(float less, float times) : base(less, times) { }

        public override int GetBucket(BuildTriangle tri)
        {
            return (int)((tri.center.z - Less) * Times);
        }
        public override int PerformPartition(BuildTriangle[] tri, int start, int end, int threshold)
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].center.z - Less) * Times < threshold)
                {
                    SplitterHelper.Swap(tri, k, partLoc);
                    partLoc++;
                }
            }

            return partLoc;
        }
        public override Func<BuildTriangle, bool> GetFilter(int threshold)
        {
            return t => ((t.center.z - Less) * Times < threshold);
        }
    }

    public enum SplitDimension
    {
        SplitX = 0, SplitY = 1, SplitZ = 2
    }
}
