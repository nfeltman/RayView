using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace RayVisualizer.Common
{
    
    public abstract class RaySet : IEnumerable<RayQuery>
    {
        public abstract IEnumerator<RayQuery> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
        {
            return GetEnumerator();
        }

        public static RaySet operator +(RaySet set1, RaySet set2)
        {
            return new UnionRaySet() { r1 = set1, r2 = set2 };
        }

        public RaySet Filter(Func<RayQuery, int, bool> filter)
        {
            return new FilterSet() { _filter = filter, r1 = this };
        }

        public RayQuery[] FlattenAndCopy()
        {
            List<RayQuery> newset = new List<RayQuery>();
            newset.AddRange(this);
            return newset.ToArray();
        }

        private class UnionRaySet : RaySet
        {
            public RaySet r1, r2;

            public override IEnumerator<RayQuery> GetEnumerator()
            {
                // Concat is deferred
                return r1.Concat(r2).GetEnumerator();
            }
        }

        private class FilterSet : RaySet
        {
            public RaySet r1;
            public Func<RayQuery,int,bool> _filter;

            public override IEnumerator<RayQuery> GetEnumerator()
            {
                // filter is deferred
                return r1.Where(_filter).GetEnumerator();
            }
        }
    }

    public static class RayFileParser
    {
        public static RaySet[] ReadFromFile(Stream file)
        {
            List<RayQuery>[] sets = new List<RayQuery>[0];

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
                RayQuery cast;
                if (type == 0)
                {
                    cast = new RayQuery() { Kind = RayKind.FirstHit_Hit, Depth = depth, Origin = origin, Direction = dir };
                }
                else if (type == 1)
                {
                    cast = new RayQuery() { Kind = RayKind.FirstHit_Miss, Depth = depth, Origin = origin, Direction = dir };
                }
                else if (type == 2)
                {
                    cast = new RayQuery() { Kind = RayKind.AnyHit_Broken, Depth = depth, Origin = origin, Direction = dir };
                }
                else if (type == 3)
                {
                    cast = new RayQuery() { Kind = RayKind.AnyHit_Connected, Depth = depth, Origin = origin, Direction = dir };
                }
                else
                {
                    throw new Exception("Error parsing file.");
                }

                if (sets.Length <= cast.Depth)
                {
                    List<RayQuery>[] sets2 = new List<RayQuery>[cast.Depth + 1];
                    Array.Copy(sets, sets2, sets.Length);
                    for (int k = sets.Length; k < sets2.Length; k++)
                        sets2[k] = new List<RayQuery>();
                    sets = sets2;
                }

                sets[cast.Depth].Add(cast);
            }

            RaySet[] ret = new RaySet[sets.Length];
            for (int k = 0; k < ret.Length; k++)
            {
                ret[k] = new SimpleRaySet() { rays = sets[k].ToArray() };
            }

            return ret;
        }        
    }

    public class SimpleRaySet : RaySet
    {
        public RayQuery[] rays;

        public override IEnumerator<RayQuery> GetEnumerator()
        {
            return ((IEnumerable<RayQuery>)rays).GetEnumerator();
        }
    }
}
