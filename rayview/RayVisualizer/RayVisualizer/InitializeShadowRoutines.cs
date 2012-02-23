using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;

namespace RayVisualizer
{
    public class InitializeShadowRoutines
    {
        public static ViewerState InitializeShadow(SceneData scene, List<ViewerState> statesList)
        {
            return FromFile(scene, statesList);
        }

        private static ViewerState FromFile(SceneData scene, List<ViewerState> statesList)
        {
            RaySet rays = RayFileParser.ReadFromFile2(File.OpenRead("..\\..\\..\\..\\..\\..\\..\\fastrays\\results\\madsci_one\\madsci_one_spp-1_spl-1_aos-0\\madsci_one.ray"));
            BuildTriangle[] tris = OBJParser.ParseOBJTriangles(File.OpenRead("..\\..\\..\\..\\..\\..\\..\\fastrays\\results\\madsci_one\\madsci_one_spp-1_spl-1_aos-0\\madsci_one.obj")).Select(t=>new Triangle(t.p1*100,t.p2*100,t.p3*100)).ToArray().GetTriangleList();
            RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY, TripleAASplitter.ONLY);

            RBVHTriangleViewer bvhViewer = new RBVHTriangleViewer(build);
            scene.Viewables.Add(bvhViewer);
            Segment3[] segs = rays.ShadowQueries.Where(q=>q.Connected).Select(q => new Segment3(q.Origin * 100, q.Difference * 100)).ToArray();
            Console.WriteLine(segs.Length);
            scene.Viewables.Add(new RaysViewer(segs));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new RBVHExploreState(bvhViewer));
            return statesList[0];
        }

        private static ViewerState FocusOnSpehere(SceneData scene, List<ViewerState> statesList)
        {
            CVector3 strangeVec = new CVector3(1, 0, 0).Normalized();
            BVH2 initialBuild = GeneralBVH2Builder.BuildFullStructure(Shapes.BuildSphere(new CVector3(0, 0, 0), new CVector3(100, 0, 0), new CVector3(0, 100, 0), 12).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, BVHNodeFactory.ONLY, TripleAASplitter.ONLY);
            float prop = .1f;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(strangeVec*400, strangeVec*-120, 80, 80 * prop, 3000);
            ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays.AsSegements(1f), initialBuild);
            RBVH2 rebuild = GeneralBVH2Builder.BuildFullStructure(res.Triangles, new ShadowRayCostEvaluator(res, 1f), RBVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY);

            RBVHTriangleViewer bvhViewer = new RBVHTriangleViewer(rebuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Broken.Select(t=>t.Ray).ToArray()));
            scene.Viewables.Add(new RaysViewer(res.Connected));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new RBVHExploreState(bvhViewer));
            return statesList[0];
        }

        private static ViewerState FocusOntoPerpPlane(SceneData scene, List<ViewerState> statesList)
        {
            CVector3 strangeVec = new CVector3(0f,.5f,.5f);
            BVH2 initialBuild = GeneralBVH2Builder.BuildFullStructure(Shapes.BuildParallelogram(new CVector3(0, -200, -200), new CVector3(0, 400, 0), new CVector3(0, 0, 400), 29, 29).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, BVHNodeFactory.ONLY, TripleAASplitter.ONLY);
            float prop = .4f;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(strangeVec * 120 + new CVector3(300, 0, 0), strangeVec * 120 + new CVector3(-20, .1f, 0), 80, 80 * prop, 3000);
            ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays.AsSegements(1f), initialBuild);
            RBVH2 rebuild = GeneralBVH2Builder.BuildFullStructure(res.Triangles, new ShadowRayCostEvaluator(res, 1f), RBVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY);

            RBVHTriangleViewer bvhViewer = new RBVHTriangleViewer(rebuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Broken.Select(t => t.Ray).ToArray()));
            scene.Viewables.Add(new RaysViewer(res.Connected));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new RBVHExploreState(bvhViewer));
            return statesList[0];
        }

        private static ViewerState FocusIntoHemisphere(SceneData scene, List<ViewerState> statesList)
        {
            float t = (float)((.75f)*2*Math.PI);
            float r = .5f*(float)Math.Sqrt(.7);
            CVector3 strangeVec = new CVector3(-(float)Math.Sqrt(1 - r * r), (float)Math.Cos(t) * r, (float)Math.Sin(t) * r);
            BVH2 initialBuild = GeneralBVH2Builder.BuildFullBVH(Shapes.BuildHemisphere(new CVector3(0, 0, 0), new CVector3(-100, 0, 0), new CVector3(0, 100, 0), 17).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, TripleAASplitter.ONLY);
            float prop = .4f;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(new CVector3(300, 0, 0), strangeVec * 115, 80, 80 * prop, 3000);
            ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays.AsSegements(1f), initialBuild);
            RBVH2 rebuild = GeneralBVH2Builder.BuildFullStructure(res.Triangles, new ShadowRayCostEvaluator(res, 1f), RBVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY);

            RBVHTriangleViewer bvhViewer = new RBVHTriangleViewer(rebuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Broken.Select(tr => tr.Ray).ToArray()));
            scene.Viewables.Add(new RaysViewer(res.Connected));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new RBVHExploreState(bvhViewer));
            return statesList[0];
        }

        private static ViewerState FocusThroughPipe(SceneData scene, List<ViewerState> statesList)
        {
            CVector3 strangeVec = new CVector3(1, 0, 0).Normalized();
            BVH2 initialBuild = GeneralBVH2Builder.BuildFullBVH(Shapes.BuildTube(strangeVec * (-200), strangeVec * 400, 100, new CVector3(1.3f, 23.4f, 12.2f), 23, 37).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, TripleAASplitter.ONLY);
            float prop = .7f;
            Ray3[] rays = RayDistributions.UnalignedCircularFrustrum(strangeVec*(-250), strangeVec * 250, 100 * prop, 100 * prop, 3000);
            ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays.AsSegements(1f), initialBuild);
            RBVH2 rebuild = GeneralBVH2Builder.BuildFullStructure(res.Triangles, new ShadowRayCostEvaluator(res, 1f), RBVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY);

            RBVHTriangleViewer bvhViewer = new RBVHTriangleViewer(rebuild);
            scene.Viewables.Add(bvhViewer);
            scene.Viewables.Add(new RaysViewer(res.Broken.Select(t => t.Ray).ToArray()));
            scene.Viewables.Add(new RaysViewer(res.Connected));

            // states
            statesList.Add(new ExploreState());
            statesList.Add(new RBVHExploreState(bvhViewer));
            return statesList[0];
        }
    }
}
