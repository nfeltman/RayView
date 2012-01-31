using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class AASplitSeries
    {
        private SplitDimension Dim;
        private float Less;
        private float Times;

        public AASplitSeries(SplitDimension dim, float less, float times)
        {
            Dim = dim;
            Less = less;
            Times = times;
        }

        public int GetPartition(float val)
        {
            return (int)((val - Less) * Times);
        }

        public int PerformPartition(BuildTriangle[] tri, int start, int end, int threshold)
        {
            int partLoc = start; // the first larger-than-partVal element

            if (Dim == SplitDimension.SplitX)
            {
                for (int k = start; k < end; k++)
                {
                    if ((tri[k].center.x - Less) * Times < threshold)
                    {
                        Swap(tri,k,partLoc);
                        partLoc++;
                    }
                }
            }
            else if (Dim == SplitDimension.SplitY)
            {
                for (int k = start; k < end; k++)
                {
                    if ((tri[k].center.y - Less) * Times < threshold)
                    {
                        Swap(tri, k, partLoc);
                        partLoc++;
                    }
                }
            }
            else
            {
                for (int k = start; k < end; k++)
                {
                    if ((tri[k].center.z - Less) * Times < threshold)
                    {
                        Swap(tri, k, partLoc);
                        partLoc++;
                    }
                }
            }
            if (partLoc >= end || partLoc <= start)
            {
                throw new Exception("This shouldn't happen.");
            }

            return partLoc;
        }

        private static void Swap(BuildTriangle[] tri, int loc1, int loc2)
        {
            BuildTriangle temp = tri[loc1];
            tri[loc1] = tri[loc2];
            tri[loc2] = temp;
            tri[loc1].index = loc1;
            tri[loc2].index = loc2;
        }

        public InteractionCombination GetInteractionType(BuildTriangle[] points, int max, int threshold)
        {
            if (max == 0)
                return InteractionCombination.HitNeither;
            if (Dim == SplitDimension.SplitX)
            {
                if ((points[0].center.x - Less) * Times < threshold)
                {
                    for (int k = 1; k < max; k++)
                        if ((points[k].center.x - Less) * Times >= threshold)
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyLeft;
                }
                else
                {
                    for (int k = 1; k < max; k++)
                        if ((points[k].center.x - Less) * Times < threshold)
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyRight;
                }
            }
            else if (Dim == SplitDimension.SplitY)
            {
                if ((points[0].center.y - Less) * Times < threshold)
                {
                    for (int k = 1; k < max; k++)
                        if ((points[k].center.y - Less) * Times >= threshold)
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyLeft;
                }
                else
                {
                    for (int k = 1; k < max; k++)
                        if ((points[k].center.y - Less) * Times < threshold)
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyRight;
                }
            }
            else
            {
                if ((points[0].center.z - Less) * Times < threshold)
                {
                    for (int k = 1; k < max; k++)
                        if ((points[k].center.z - Less) * Times >= threshold)
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyLeft;
                }
                else
                {
                    for (int k = 1; k < max; k++)
                        if ((points[k].center.z - Less) * Times < threshold)
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyRight;
                }
            }
        }
    }

    public enum SplitDimension
    {
        SplitX = 0, SplitY = 1, SplitZ = 2
    }

    public enum InteractionCombination
    {
        HitOnlyLeft, HitOnlyRight, HitBoth, HitNeither
    }
}
