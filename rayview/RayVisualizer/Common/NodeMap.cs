using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class NodeMap<T>
    {
        private T[] _branches, _leaves;

        public NodeMap(int numBranches)
        {
            _branches = new T[numBranches];
            _leaves = new T[numBranches + 1];
        }

        public NodeMap(T[] branches, T[] leaves)
        {
            _branches = branches;
            _leaves = leaves;
        }

        public T[] Branches { get { return _branches; } }
        public T[] Leaves { get { return _leaves; } }

        public T this[BVH2Node index]
        {
            get { return index.Accept(b => _branches[b.ID], l => _leaves[l.ID]);}
            set { index.Accept(b => _branches[b.ID] = value, l => _leaves[l.ID] = value); }
        }
    }
}
