using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topaz
{
    static class Sandbox
    {
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
