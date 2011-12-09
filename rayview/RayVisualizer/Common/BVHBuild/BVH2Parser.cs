﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public static class BVH2Parser
    {
        public static BVH2 ReadFromFile(Stream stream)
        {
            int branchCounter = 0, leafCounter = 0;
            BinaryReader reader = new BinaryReader(stream);
            BVH2Node root = ParseNode(reader, 0, ref branchCounter, ref leafCounter);
            int endSentinel = reader.ReadInt32();
            if (endSentinel != 9215) throw new IOException("Sentinel not found!");
            if (branchCounter != leafCounter - 1) throw new IOException("Branch/leaf mismatch, somehow.");
            return new BVH2(root, branchCounter);
        }

        private static BVH2Node ParseNode(BinaryReader reader, int depth, ref int branchCounter, ref int leafCounter)
        {
            int type = reader.ReadInt32();
            if (type == 0 || type == 2) //branch type
            {
                Box3? bbox = null;
                if (type == 0) // explicit branch
                    bbox = ReadBoundingBox(reader);
                BVH2Node left = ParseNode(reader, depth + 1, ref branchCounter, ref leafCounter);
                BVH2Node right = ParseNode(reader, depth + 1, ref branchCounter, ref leafCounter);
                if (!bbox.HasValue) // implicit branch
                    bbox = left.BBox | right.BBox;
                return new BVH2Branch() { BBox = bbox.Value, Left = left, Right = right, ID = branchCounter++, Depth = depth };
            }
            else if (type == 1 || type == 3) // leaf type
            {
                int numTriangles = reader.ReadInt32();
                Box3? bbox = null;
                if (type == 1) // explicit leaf
                    bbox = ReadBoundingBox(reader);
                Triangle[] tris = new Triangle[numTriangles];
                for (int k = 0; k < numTriangles; k++)
                {
                    tris[k] = new Triangle()
                    {
                        p1 = new CVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        p2 = new CVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        p3 = new CVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                    };
                }
                if (!bbox.HasValue) // implicit leaf
                    throw new Exception("can't handle implicit leaves just yet");
                return new BVH2Leaf() { BBox = bbox.Value, Primitives = tris, ID = leafCounter++, Depth = depth };
            }
            else
            {
                throw new IOException("Unexpected block header: " + type);
            }
        }

        private static Box3 ReadBoundingBox(BinaryReader reader)
        {
            return new Box3(reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
        }
    }
}