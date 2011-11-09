using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    class RunAnalysis
    {
        public static void Main()
        {
            TestRoutine();
        }

        public static void TestRoutine()
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream("..\\..\\..\\..\\..\\traces\\bvhTrace.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RaySet.ReadFromFile(new FileStream("..\\..\\..\\..\\..\\traces\\castTrace.txt", FileMode.Open, FileAccess.Read));

            RayOrderOperations ops = new RayOrderOperations();
            int myOwnCount = 0;

            RayCast[] firstGen = allrays[1].Rays;
            for (int k = 0; k < firstGen.Length; k++)
            {
                if (firstGen[k].Kind == RayKind.FirstHit_Hit) myOwnCount++;
                if (firstGen[k].Kind == RayKind.AnyHit_Broken || firstGen[k].Kind == RayKind.AnyHit_Connected) continue; 
                RayOrderOpCounter.RunOpCounter(bvh, firstGen[k], ops);
            }

            Console.WriteLine(ops.primitiveNodeInspections);
        }
    }
}
