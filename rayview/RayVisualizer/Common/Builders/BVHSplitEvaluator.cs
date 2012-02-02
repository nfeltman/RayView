using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface BVHSplitEvaluator<StackState, BranchData, Aggregate>
    {
        EvalResult<BranchData> EvaluateSplit(Aggregate leftAgg, Box3 leftBox, Aggregate rightAgg, Box3 rightBox, StackState state, Func<BuildTriangle, bool> leftFilter);
    }

    public interface BVHSplitEvaluator<StackState, BranchData, EntranceData, ExitData, Aggregate> : BVHSplitEvaluator<StackState, BranchData, Aggregate>
    {
        EntranceData GetDefault();
        StackState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, EntranceData parentState);
        Tuple<EntranceData, EntranceData> PrepareChildren(EvalResult<BranchData> selected, StackState currentState);
        ExitData EndBothChildren(ExitData firstChildsExit, ExitData secondChildsExit);
        ExitData GetLeafExit();
    }

    public abstract class ExitlessEvaluator<StackState, BranchData, EntranceData, Aggregate> : BVHSplitEvaluator<StackState, BranchData, EntranceData, Unit, Aggregate>
    {
        public abstract EntranceData GetDefault();
        public abstract StackState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, EntranceData parentState);
        public abstract EvalResult<BranchData> EvaluateSplit(Aggregate leftNu, Box3 leftBox, Aggregate rightNu, Box3 rightBox, StackState state, Func<BuildTriangle, bool> leftFilter);
        public abstract Tuple<EntranceData, EntranceData> PrepareChildren(EvalResult<BranchData> selected, StackState currentState);
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
        public override Tuple<StackState, StackState> PrepareChildren(EvalResult<BranchData> selected, StackState currentState)
        {
            return new Tuple<StackState,StackState>(currentState, currentState);
        }
    }

    public class EvalResult<BuildData>
    {
        public BuildData Data { get; set; }
        public double Cost { get; set; }
        public bool BuildLeftFirst { get; set; }
        public EvalResult(double cost, BuildData data, bool leftFirst)
        {
            Data = data;
            Cost = cost;
            BuildLeftFirst = BuildLeftFirst;
        }
    }

    public class StatelessSplitEvaluator : BVHSplitEvaluator<Unit, Unit, Unit, Unit, int>
    {
        private Func<int, Box3, int, Box3, float> _costEstimator;

        public StatelessSplitEvaluator(Func<int, Box3, int, Box3, float> costEstimator)
        {
            _costEstimator = costEstimator;
        }

        public EvalResult<Unit> EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, Unit state, Func<BuildTriangle, bool> leftFilter)
        {
            return new EvalResult<Unit>(_costEstimator(leftNu, leftBox, rightNu, rightBox), Unit.ONLY, true);
        }

        public Unit BeginEvaluations(int startTri, int endTri, Box3 splitCandidate, Unit parentState) { return Unit.ONLY; }
        
        public Unit GetDefault()
        {
            return Unit.ONLY;
        }

        public Tuple<Unit, Unit> PrepareChildren(EvalResult<Unit> selected, Unit currentState)
        {
            return new Tuple<Unit, Unit>(currentState, currentState);
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
