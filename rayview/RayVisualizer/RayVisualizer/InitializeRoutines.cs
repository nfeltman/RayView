using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;

namespace RayVisualizer
{
    public class InitializeRoutines
    {
        public static ViewerState Initialize(SceneData scene, List<ViewerState> statesList)
        {
            return FocusIntoHemisphere(scene, statesList);
        }

        public static ViewerState FocusOnSpehere(SceneData scene, List<ViewerState> statesList)
        {
            CVector3 strangeVec = new CVector3(2, 4, 5).Normalized();
            BVH2 initialBuild = BVH2Builder.BuildFullBVH(Shapes.BuildSphere(new CVector3(0, 0, 0), new CVector3(100, 0, 0), new CVector3(0, 100, 0), 12).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea);
            float prop = 0;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(strangeVec*400, strangeVec*100, 80, 80 * prop, 3000);
            FHRayResults res = RayCompiler.CompileCasts(rays, initialBuild);
            BVH2 rebuild = BVH2Builder.BuildFullBVH(BVH2Builder.GetTriangleList(initialBuild), new RayCostEvaluator(res, 1), new RayCostEvaluator.RayShuffleState(res));

            BVHTriangleViewer bvhViewer = new BVHTriangleViewer(initialBuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Hits));
            scene.Viewables.Add(new RaysViewer(res.Misses, 100));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new BVHExploreState(bvhViewer));
            return statesList[0];
        }

        public static ViewerState FocusOntoPerpPlane(SceneData scene, List<ViewerState> statesList)
        {
            CVector3 strangeVec = new CVector3(0f,1f,1f);
            BVH2 initialBuild = BVH2Builder.BuildFullBVH(Shapes.BuildParallelogram(new CVector3(0, -200, -200), new CVector3(0, 400, 0), new CVector3(0, 0, 400), 29, 29).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea);
            float prop = 1;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(strangeVec * 120 + new CVector3(300, 0, 0), strangeVec * 120 + new CVector3(0, .1f, 0), 80, 80 * prop, 3000);
            FHRayResults res = RayCompiler.CompileCasts(rays, initialBuild);
            BVH2 rebuild = BVH2Builder.BuildFullBVH(BVH2Builder.GetTriangleList(initialBuild), new RayCostEvaluator(res, 1), new RayCostEvaluator.RayShuffleState(res));

            BVHTriangleViewer bvhViewer = new BVHTriangleViewer(initialBuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Hits));
            scene.Viewables.Add(new RaysViewer(res.Misses, 100));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new BVHExploreState(bvhViewer));
            return statesList[0];
        }

        public static ViewerState FocusIntoHemisphere(SceneData scene, List<ViewerState> statesList)
        {
            float t = (float)((.76)*2*Math.PI);
            float r = .5f*(float)Math.Sqrt(1);
            CVector3 strangeVec = new CVector3(-(float)Math.Sqrt(1 - r * r), (float)Math.Cos(t) * r, (float)Math.Sin(t) * r);
            BVH2 initialBuild = BVH2Builder.BuildFullBVH(Shapes.BuildHemisphere(new CVector3(0, 0, 0), new CVector3(-100, 0, 0), new CVector3(0, 100, 0), 17).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea);
            float prop = .78f;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(new CVector3(300, 0, 0), strangeVec * 100, 80, 80 * prop, 3000);
            FHRayResults res = RayCompiler.CompileCasts(rays, initialBuild);
            BVH2 rebuild = BVH2Builder.BuildFullBVH(BVH2Builder.GetTriangleList(initialBuild), new RayCostEvaluator(res, 1), new RayCostEvaluator.RayShuffleState(res));

            BVHTriangleViewer bvhViewer = new BVHTriangleViewer(initialBuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Hits));
            scene.Viewables.Add(new RaysViewer(res.Misses, 100));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new BVHExploreState(bvhViewer));
            return statesList[0];
        }
    }
}
