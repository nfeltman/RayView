using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface BVHSplitEvaluator<StackState, BranchData>
    {
        StackState GetDefaultState(Box3 toBeDivided);
        StackState SetState(Box3 objectBounds, StackState parentState);
        EvalResult<BranchData> EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, StackState state, AASplit split);
    }

    public class EvalResult<BuildData>
    {
        public BuildData Data { get; set; }
        public double Cost { get; set; }
        public EvalResult(double cost, BuildData data)
        {
            Data = data;
            Cost = cost;
        }
    }

    public class StatelessSplitEvaluator : BVHSplitEvaluator<Unit, Unit>
    {
        private Func<int, Box3, int, Box3, float> _costEstimator;

        public StatelessSplitEvaluator(Func<int, Box3, int, Box3, float> costEstimator)
        {
            _costEstimator = costEstimator;
        }

        public EvalResult<Unit> EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, Unit state, AASplit split)
        {
            return new EvalResult<Unit>(_costEstimator(leftNu, leftBox, rightNu, rightBox), Unit.ONLY);
        }

        public Unit SetState(Box3 splitCandidate, Unit parentState) { return Unit.ONLY; }
        public Unit GetDefaultState(Box3 toBeDivided) { return Unit.ONLY; }
    }
}
