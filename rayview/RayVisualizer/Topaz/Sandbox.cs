using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace Topaz
{
    public static class Sandbox
    {
        public static void SIMD_Tests()
        {
            Console.WriteLine("   CompareLessEqual  (all >): {0}", new Vector4f(1, 1, 1, 1).CompareLessEqual(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine("   CompareLessEqual  (all <): {0}", new Vector4f(0, 0, 0, 0).CompareLessEqual(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("   CompareLessEqual  (all =): {0}", new Vector4f(1, 1, 1, 1).CompareLessEqual(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("   CompareLessEqual    (mix): {0}", new Vector4f(1, 0, 1, 1).CompareLessEqual(new Vector4f(0, 1, 0, 1)));
            
            Console.WriteLine("    CompareLessThan  (all >): {0}", new Vector4f(1, 1, 1, 1).CompareLessThan(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine("    CompareLessThan  (all <): {0}", new Vector4f(0, 0, 0, 0).CompareLessThan(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("    CompareLessThan  (all =): {0}", new Vector4f(1, 1, 1, 1).CompareLessThan(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("    CompareLessThan    (mix): {0}", new Vector4f(1, 0, 1, 1).CompareLessThan(new Vector4f(0, 1, 0, 1)));

            Console.WriteLine(" CompareNotLessThan  (all >): {0}", new Vector4f(1, 1, 1, 1).CompareNotLessThan(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine(" CompareNotLessThan  (all <): {0}", new Vector4f(0, 0, 0, 0).CompareNotLessThan(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine(" CompareNotLessThan  (all =): {0}", new Vector4f(1, 1, 1, 1).CompareNotLessThan(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine(" CompareNotLessThan    (mix): {0}", new Vector4f(1, 0, 1, 1).CompareNotLessThan(new Vector4f(0, 1, 0, 1)));

            Console.WriteLine("CompareNotLessEqual  (all >): {0}", new Vector4f(1, 1, 1, 1).CompareNotLessEqual(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine("CompareNotLessEqual  (all <): {0}", new Vector4f(0, 0, 0, 0).CompareNotLessEqual(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("CompareNotLessEqual  (all =): {0}", new Vector4f(1, 1, 1, 1).CompareNotLessEqual(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("CompareNotLessEqual    (mix): {0}", new Vector4f(1, 0, 1, 1).CompareNotLessEqual(new Vector4f(0, 1, 0, 1)));

            Console.WriteLine("     CompareOrdered  (all >): {0}", new Vector4f(1, 1, 1, 1).CompareOrdered(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine("     CompareOrdered  (all <): {0}", new Vector4f(0, 0, 0, 0).CompareOrdered(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("     CompareOrdered  (all =): {0}", new Vector4f(1, 1, 1, 1).CompareOrdered(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("     CompareOrdered    (mix): {0}", new Vector4f(1, 0, 1, 1).CompareOrdered(new Vector4f(0, 1, 0, 1)));
            
            Console.WriteLine("   CompareUnOrdered  (all >): {0}", new Vector4f(1, 1, 1, 1).CompareUnordered(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine("   CompareUnOrdered  (all <): {0}", new Vector4f(0, 0, 0, 0).CompareUnordered(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("   CompareUnOrdered  (all =): {0}", new Vector4f(1, 1, 1, 1).CompareUnordered(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("   CompareUnOrdered    (mix): {0}", new Vector4f(1, 0, 1, 1).CompareUnordered(new Vector4f(0, 1, 0, 1)));

            Console.WriteLine("             AndNot  (all >): {0}", new Vector4f(1, 1, 1, 1).AndNot(new Vector4f(0, 0, 0, 0)));
            Console.WriteLine("             AndNot  (all <): {0}", new Vector4f(0, 0, 0, 0).AndNot(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("             AndNot  (all =): {0}", new Vector4f(1, 1, 1, 1).AndNot(new Vector4f(1, 1, 1, 1)));
            Console.WriteLine("             AndNot    (mix): {0}", new Vector4f(1, 0, 1, 1).AndNot(new Vector4f(0, 1, 0, 1)));
        }

        static void QuickTest()
        {
            S s;
            s = new S(3);
            Console.WriteLine("Unmodified: {0}", s.v);

            s = new S(3);
            s.v++;
            Console.WriteLine("Manually Change: {0}", s.v);

            s = new S(3);
            S s2 = s;
            s2.v++;
            Console.WriteLine("Copy and Change: {0}", s.v);

            s = new S(3);
            s.IncrementInternal();
            Console.WriteLine("Internal Method: {0}", s.v);

            s = new S(3);
            IncrementExternal(s);
            Console.WriteLine("External Method: {0}", s.v);

            s = new S(3);
            IncrementExternalRef(ref s);
            Console.WriteLine("External Method by Reference: {0}", s.v);

            s = new S(3);
            s.IncrementExtension();
            Console.WriteLine("Extension Method: {0}", s.v);

            s = new S(3);
            IncrementExtension(s);
            Console.WriteLine("Explicit Extension Method: {0}", s.v);

            Console.ReadLine();
        }

        private static void IncrementExternal(S s)
        {
            s.v++;
        }

        private static void IncrementExternalRef(ref S s)
        {
            s.v++;
        }

        private static void IncrementExtension(this S s)
        {
            s.v++;
        }

        private struct S
        {
            public int v;
            public S(int v0) { v = v0; }
            public void IncrementInternal() { v++; }
        }
    }
}
