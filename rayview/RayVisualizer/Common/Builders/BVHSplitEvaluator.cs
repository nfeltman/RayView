using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface BVHSplitEvaluator<StackState, BranchData, Aggregate>
    {
        EvalResult<BranchData> EvaluateSplit(Aggregate leftAgg, Box3 leftBox, Aggregate rightAgg, Box3 rightBox, StackState state, AASplitSeries split, int threshold);
    }

    public interface BVHSplitEvaluator<StackState, BranchData, EntranceData, ExitData, Aggregate> : BVHSplitEvaluator<StackState, BranchData, Aggregate>
    {
        EntranceData GetDefault();
        StackState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, EntranceData parentState);
        EntranceData PrepareFirstChild(BranchData selected, StackState currentState);
        EntranceData PrepareSecondChild(ExitData firstChildsExit, BranchData selected, StackState currentState);
        ExitData EndBothChildren(ExitData firstChildsExit, ExitData secondChildsExit);
        ExitData GetLeafExit();
    }

    public abstract class ExitlessEvaluator<StackState, BranchData, EntranceData, Aggregate> : BVHSplitEvaluator<StackState, BranchData, EntranceData, Unit, Aggregate>
    {
        public abstract EntranceData GetDefault();
        public abstract StackState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, EntranceData parentState);
        public abstract EvalResult<BranchData> EvaluateSplit(Aggregate leftNu, Box3 leftBox, Aggregate rightNu, Box3 rightBox, StackState state, AASplitSeries split, int threshold);
        public abstract EntranceData PrepareFirstChild(BranchData selected, StackState currentState);
        public abstract EntranceData PrepareSecondChild(Unit firstChildsExit, BranchData selected, StackState currentState);
        public Unit EndBothChildren(Unit firstChildsExit, Unit secondChildsExit)
        {
            return Unit.ONLY;
        }
        public Unit GetLeafExit()
        {
            return Unit.ONLY;
        }
    }

    public abstract class TransitionlessEvaluator<StackState, BranchData, Aggregate> : ExitlessEvaluator<StackState, BranchData, StackState, Aggregate>
    {
        public override StackState PrepareSecondChild(Unit firstChildsExit, BranchData selected, StackState currentState)
        {
            return currentState;
        }
        public override StackState PrepareFirstChild(BranchData selected, StackState currentState)
        {
            return currentState;
        }
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

    public class StatelessSplitEvaluator : BVHSplitEvaluator<Unit, Unit, Unit, Unit, int>
    {
        private Func<int, Box3, int, Box3, float> _costEstimator;

        public StatelessSplitEvaluator(Func<int, Box3, int, Box3, float> costEstimator)
        {
            _costEstimator = costEstimator;
        }

        public EvalResult<Unit> EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, Unit state, AASplitSeries split, int threshold)
        {
            return new EvalResult<Unit>(_costEstimator(leftNu, leftBox, rightNu, rightBox), Unit.ONLY);
        }

        public Unit BeginEvaluations(int startTri, int endTri, Box3 splitCandidate, Unit parentState) { return Unit.ONLY; }
        
        public Unit GetDefault()
        {
            return Unit.ONLY;
        }
        
        public Unit PrepareFirstChild(Unit selected, Unit currentState)
        {
            return Unit.ONLY;
        }

        public Unit PrepareSecondChild(Unit firstChildsExit, Unit selected, Unit currentState)
        {
            return Unit.ONLY;
        }

        public Unit EndBothChildren(Unit firstChildsExit, Unit secondChildsExit)
        {
            return Unit.ONLY;
        }

        public Unit GetLeafExit()
        {
            return Unit.ONLY;
        }
    }
}
