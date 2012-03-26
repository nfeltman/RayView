﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface SplitEvaluator<StackState, MemoData, Aggregate>
    {
        EvalResult<MemoData> EvaluateSplit(Aggregate leftAgg, Aggregate rightAgg, StackState state, Func<CenterIndexable, bool> leftFilter);
    }

    public delegate EvalResult<MemoData> Evaluator<MemoData, Aggregate>(Aggregate leftAgg, Aggregate rightAgg, Func<CenterIndexable, bool> leftFilter);

    public interface SplitEvaluator<StackState, MemoData, KernelData, TransitionData, Aggregate> : SplitEvaluator<StackState, MemoData, Aggregate>
    {
        TransitionData GetDefault();
        StackState BeginEvaluations(int startTri, int endTri, Aggregate objectBounds, TransitionData parentState);
        BuildReport<TransitionData, KernelData> FinishEvaluations(EvalResult<MemoData> selected, StackState currentState);
    }

    public abstract class TransitionlessEvaluator<StackState, BuildMemo, Aggregate> : SplitEvaluator<StackState, BuildMemo, BuildMemo, StackState, Aggregate>
    {
        public abstract StackState GetDefault();
        public abstract StackState BeginEvaluations(int startTri, int endTri, Aggregate objectBounds, StackState parentState);
        public abstract EvalResult<BuildMemo> EvaluateSplit(Aggregate leftNu, Aggregate rightNu, StackState state, Func<CenterIndexable, bool> leftFilter);
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
        public EvalResult(double cost, MemoData data, bool buildLeftFirst)
        {
            Data = data;
            Cost = cost;
            BuildLeftFirst = buildLeftFirst;
        }
    }

    public class BuildReport<TransitionData, BranchData>
    {
        public TransitionData LeftTransition { get; set; }
        public TransitionData RightTransition { get; set; }
        public BranchData BranchBuildData { get; set; }
        public BuildReport(BranchData branchBuildData, TransitionData leftTransition, TransitionData rightTransition)
        {
            BranchBuildData = branchBuildData;
            LeftTransition = leftTransition;
            RightTransition = rightTransition;
        }
    }

    public class StatelessSplitEvaluator : SplitEvaluator<Unit, Unit, Unit, Unit, BoundAndCount>
    {
        private Func<int, Box3, int, Box3, float> _costEstimator;

        public StatelessSplitEvaluator(Func<int, Box3, int, Box3, float> costEstimator)
        {
            _costEstimator = costEstimator;
        }

        public EvalResult<Unit> EvaluateSplit(BoundAndCount left, BoundAndCount right, Unit state, Func<CenterIndexable, bool> leftFilter)
        {
            return new EvalResult<Unit>(_costEstimator(left.Count, left.Box, right.Count, right.Box), Unit.ONLY, false);
        }

        public Unit BeginEvaluations(int startTri, int endTri, BoundAndCount splitCandidate, Unit parentState) { return Unit.ONLY; }
        
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
