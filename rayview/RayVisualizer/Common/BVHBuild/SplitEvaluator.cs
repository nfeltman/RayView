using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface SplitEvaluator<T>
    {
        T SetState(Box3 toBeDivided, T parentState);
        float EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, T state);
    }

    public class StatelessSplitEvaluator : SplitEvaluator<bool>
    {
        private Func<int, Box3, int, Box3, float> _costEstimator;

        public StatelessSplitEvaluator(Func<int, Box3, int, Box3, float> costEstimator)
        {
            _costEstimator = costEstimator;
        }

        public float EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, bool state)
        {
            return _costEstimator(leftNu, leftBox, rightNu, rightBox);
        }

        public bool SetState(Box3 splitCandidate, bool parentState) { return false; }
    }
}
