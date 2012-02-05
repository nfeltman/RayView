using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface BVHSplitEvaluator<StackState, MemoData, Aggregate>
    {
        EvalResult<MemoData> EvaluateSplit(Aggregate leftAgg, Box3 leftBox, Aggregate rightAgg, Box3 rightBox, StackState state, Func<BuildTriangle, bool> leftFilter);
    }

    public interface BVHSplitEvaluator<StackState, MemoData, BranchData, EntranceData, Aggregate> : BVHSplitEvaluator<StackState, MemoData, Aggregate>
    {
        EntranceData GetDefault();
        StackState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, EntranceData parentState);
        BuildReport<EntranceData, BranchData> FinishEvaluations(EvalResult<MemoData> selected, StackState currentState);
        //ExitData EndBothChildren(ExitData firstChildsExit, ExitData secondChildsExit);
        //ExitData GetLeafExit();
    }

    public abstract class TransitionlessEvaluator<StackState, BuildMemo, Aggregate> : BVHSplitEvaluator<StackState, BuildMemo, BuildMemo, StackState, Aggregate>
    {
        public abstract StackState GetDefault();
        public abstract StackState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, StackState parentState);
        public abstract EvalResult<BuildMemo> EvaluateSplit(Aggregate leftNu, Box3 leftBox, Aggregate rightNu, Box3 rightBox, StackState state, Func<BuildTriangle, bool> leftFilter);
        public BuildReport<StackState, BuildMemo> FinishEvaluations(EvalResult<BuildMemo> selected, StackState currentState)
        {
            return new BuildReport<StackState, BuildMemo>(selected.Data, currentState, currentState);
        }
    }

    public class EvalResult<MemoData>
    {
        public MemoData Data { get; set; }
        public double Cost { get; set; }
        public bool BuildLeftFirst { get; set; }
        public EvalResult(double cost, MemoData data, bool leftFirst)
        {
            Data = data;
            Cost = cost;
            BuildLeftFirst = BuildLeftFirst;
        }
    }

    public class BuildReport<EntranceData, BranchData>
    {
        public EntranceData LeftTransition { get; set; }
        public EntranceData RightTransition { get; set; }
        public BranchData BranchBuildData { get; set; }
        public BuildReport(BranchData branchBuildData, EntranceData leftTransition, EntranceData rightTransition)
        {
            BranchBuildData = branchBuildData;
            LeftTransition = leftTransition;
            RightTransition = rightTransition;
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

        public BuildReport<Unit, Unit> FinishEvaluations(EvalResult<Unit> selected, Unit currentState)
        {
            return new BuildReport<Unit,Unit>(Unit.ONLY, Unit.ONLY, Unit.ONLY);
        }
    }
}
