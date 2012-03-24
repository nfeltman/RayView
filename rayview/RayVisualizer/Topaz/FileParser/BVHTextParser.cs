using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;

namespace Topaz.FileParser
{
    public static class BVHTextParser
    {
        public static void WriteBVH_Text(Tree<BackedRBVH2Branch,BackedRBVH2Leaf> bvh, TopazStreamWriter output)
        {
            output.Write("267534 202");
            bvh.PrefixEnumerate(
                (branch) =>
                {
                    output.Write(" 2 {0}", branch.PLeft);
                },
                (leaf) =>
                {
                    output.Write(" 3 {0}", leaf.Primitives.Length);
                    foreach (int i in leaf.Primitives)
                        output.Write(" {0}", i);
                });
            output.Write(" 9215");
        }

        public static Tree<RBVH2Branch, RBVH2Leaf> ReadRBVH_Text(StreamReader reader, List<Triangle> triangles)
        {
            StreamTokenizer tok = new StreamTokenizer(reader);
            if (tok.ReadInt() != 267534) throw new IOException("Bad Header");
            if (tok.ReadInt() != 202) throw new IOException("Bad BVH Type (only works for 202 right now)");
            int branchID = 0, leafID = 0;
            Box3 bounds;
            TreeNode<RBVH2Branch, RBVH2Leaf> node = ParseNode(tok, triangles, ref branchID, ref leafID, 0, out bounds);
            Tree<RBVH2Branch, RBVH2Leaf> tree = new Tree<RBVH2Branch, RBVH2Leaf>(node, node.RollUp((b, l, r) => l + r + 1, (l) => 0));
            if (tok.ReadInt() != 9215) throw new IOException("Bad Sentinel");
            return tree;
        }

        private static TreeNode<RBVH2Branch, RBVH2Leaf> ParseNode(StreamTokenizer tok, List<Triangle> triangles, ref int branchID, ref int leafID, int depth, out Box3 bounds)
        {
            int nodetype = tok.ReadInt();
            if (nodetype == 2)
            {
                float pLeft = tok.ReadFloat();
                Box3 leftBounds;
                Box3 rightBounds;
                TreeNode<RBVH2Branch, RBVH2Leaf> left = ParseNode(tok, triangles, ref branchID, ref leafID, depth + 1, out leftBounds);
                TreeNode<RBVH2Branch, RBVH2Leaf> right = ParseNode(tok, triangles, ref branchID, ref leafID, depth + 1, out rightBounds);
                bounds = leftBounds | rightBounds;
                return new Branch<RBVH2Branch, RBVH2Leaf>(left, right, new RBVH2Branch() { PLeft = pLeft, BBox = bounds, Depth = depth, ID = branchID++ });
            }
            else if (nodetype == 3)
            {
                int leafSize = tok.ReadInt();
                Triangle[] prims = new Triangle[leafSize];
                BoundBuilder builder = new BoundBuilder(true);
                for (int k = 0; k < leafSize; k++)
                {
                    prims[k] = triangles[tok.ReadInt()];
                    builder.AddTriangle(prims[k]);
                }
                bounds = builder.GetBox();
                return new Leaf<RBVH2Branch, RBVH2Leaf>(new RBVH2Leaf() { BBox = bounds, Primitives = prims, Depth = depth, ID = leafID++ });
            }
            else
            {
                throw new IOException("Bad node type.");
            }
        }
    }

    public class StreamTokenizer
    {
        private StreamReader _reader;

        public StreamTokenizer(StreamReader reader)
        {
            _reader = reader;
        }

        public string ReadString()
        {
            StringBuilder word = new StringBuilder();
            int letter = _reader.Read();
            if (letter < 0) return null;
            while (letter >=0 && letter != ' ')
            {
                word.Append((char)letter);
                letter = _reader.Read();
            }
            return word.ToString();
        }

        public int ReadInt()
        {
            return int.Parse(ReadString());
        }

        public float ReadFloat()
        {
            return float.Parse(ReadString());
        }
    }
}
