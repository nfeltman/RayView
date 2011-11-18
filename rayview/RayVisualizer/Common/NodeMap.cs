using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class NodeMap<T>
    {
        private T[] branches, leaves;

        public NodeMap(int numBranches)
        {
            branches = new T[numBranches];
            leaves = new T[numBranches + 1];
        }

        public T[] Branches { get { return branches; } }
        public T[] Leaves { get { return leaves; } }

        public T this[BVH2Node index]
        {
            get { return index.Accept(b => branches[b.ID], l => leaves[l.ID]);}
            set { index.Accept(b => branches[b.ID] = value, l => leaves[l.ID] = value); }
        }
    }
}
