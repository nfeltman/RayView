using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface TriangleAggregator<Aggregate>
    {
        void Include(Aggregate[] arr, int index, BuildTriangle t);
        Aggregate Op(Aggregate val1, Aggregate val2);
        Aggregate GetIdentity();
    }

    public class CountAggregator : TriangleAggregator<int>
    {
        public static readonly CountAggregator ONLY = new CountAggregator();

        private CountAggregator() { }

        public void Include(int[] arr, int index, BuildTriangle t)
        {
            arr[index]++;
        }

        public int Op(int val1, int val2)
        {
            return val1 + val2;
        }

        public int GetIdentity()
        {
            return 0;
        }
    }

    public class BoundsCountAggregator : TriangleAggregator<BoundsCountAggregate>
    {

        public void Include(BoundsCountAggregate[] arr, int index, BuildTriangle t)
        {
            throw new NotImplementedException();
        }

        public BoundsCountAggregate GetIdentity()
        {
            throw new NotImplementedException();
        }


        public BoundsCountAggregate Op(BoundsCountAggregate val1, BoundsCountAggregate val2)
        {
            throw new NotImplementedException();
        }
    }

    public class BoundsCountAggregate
    {

    }
}
