﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace RayVisualizer.Common
{
    
    public class RaySet : IEnumerable<RayCast>
    {
        public RayCast[] Rays { get { return rays; } }

        private RayCast[] rays;

        private RaySet()
        {
        }

        public static RaySet[] ReadFromFile(Stream file)
        {
            List<RayCast>[] sets = new List<RayCast>[0];

            BinaryReader reader = new BinaryReader(file);

            while (file.CanRead)
            {
                int type = reader.ReadInt32();
                if (type == 9215) // the end of file sentinel
                    break;
                int depth = reader.ReadInt32();
                CVector3 origin = new CVector3() 
                { 
                    x = reader.ReadSingle(), 
                    y = reader.ReadSingle(), 
                    z = reader.ReadSingle(), 
                };
                CVector3 dir = new CVector3()
                {
                    x = reader.ReadSingle(), 
                    y = reader.ReadSingle(), 
                    z = reader.ReadSingle(), 
                };
                RayCast cast; 
                if (type == 0)
                {
                    cast = new RayCast() { Kind = RayKind.FirstHit_Hit, Depth = depth, Origin = origin, Direction = dir };
                }
                else if (type == 1)
                {
                    cast = new RayCast() { Kind = RayKind.FirstHit_Miss, Depth = depth, Origin = origin, Direction = dir };
                }
                else if (type == 2)
                {
                    cast = new RayCast() { Kind = RayKind.AnyHit_Broken, Depth = depth, Origin = origin, Direction = dir };
                }
                else if (type == 3)
                {
                    cast = new RayCast() { Kind = RayKind.AnyHit_Connected, Depth = depth, Origin = origin, Direction = dir };
                }
                else
                {
                    throw new Exception("Error parsing file.");
                }

                if (sets.Length <= cast.Depth)
                {
                    List<RayCast>[] sets2 = new List<RayCast>[cast.Depth+1];
                    Array.Copy(sets, sets2, sets.Length);
                    for (int k = sets.Length; k < sets2.Length; k++)
                        sets2[k] = new List<RayCast>();
                    sets = sets2;
                }

                sets[cast.Depth].Add(cast);
            }

            RaySet[] ret = new RaySet[sets.Length];
            for (int k = 0; k < ret.Length; k++)
            {
                ret[k] = new RaySet();
                ret[k].rays = sets[k].ToArray();
            }

            return ret;
        }

        public IEnumerator<RayCast> GetEnumerator()
        {
            return ((IEnumerable<RayCast>)Rays).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rays.GetEnumerator();
        }
    }
}
