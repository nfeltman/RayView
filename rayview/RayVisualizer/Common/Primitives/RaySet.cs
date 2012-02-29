using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace RayVisualizer.Common
{
    
    public abstract class RaySet
    {
        public bool HitsNotAnotated { get; set; }
        public abstract IEnumerable<RayQuery> AllQueries { get; }
        public abstract IEnumerable<CastQuery> AllCastQueries { get; }
        public abstract IEnumerable<CastHitQuery> CastHitQueries { get; }
        public abstract IEnumerable<CastMissQuery> CastMissQueries { get; }
        public abstract IEnumerable<ShadowQuery> ShadowQueries { get; }


        public static RaySet operator +(RaySet set1, RaySet set2)
        {
            return new UnionRaySet() { r1 = set1, r2 = set2 };
        }

        public RaySet Filter(Func<CastHitQuery, int, bool> castHitFilter, Func<CastMissQuery, int, bool> castMissFilter, Func<ShadowQuery, int, bool> shadowFilter)
        {
            return new FilterSet() { r1 = this, castHitFilter = castHitFilter, castMissFilter = castMissFilter, shadowFilter = shadowFilter };
        }

        public RaySet CastOnlyFilter(Func<CastHitQuery, int, bool> castHitFilter, Func<CastMissQuery, int, bool> castMissFilter)
        {
            return new FilterSet() { r1 = this, castHitFilter = castHitFilter, castMissFilter = castMissFilter, shadowFilter = null };
        }

        public RaySet CastOnlyFilter(Func<CastQuery, int, bool> castFilter)
        {
            return new FilterSet() { r1 = this, castHitFilter = (Func<CastHitQuery, int, bool>)castFilter, castMissFilter = (Func<CastMissQuery, int, bool>)castFilter, shadowFilter = null };
        }

        public SimpleRaySet FlattenAndCopy()
        {
            List<CastHitQuery> castHits = new List<CastHitQuery>();
            List<CastMissQuery> castMisses = new List<CastMissQuery>();
            List<ShadowQuery> shadows = new List<ShadowQuery>();
            castMisses.AddRange(CastMissQueries);
            castHits.AddRange(CastHitQueries);
            shadows.AddRange(ShadowQueries);
            return new SimpleRaySet() { CastMisses=castMisses.ToArray(), CastHits = castHits.ToArray(), Shadows = shadows.ToArray() };
        }

        private class UnionRaySet : RaySet
        {
            public RaySet r1, r2;

            public override IEnumerable<RayQuery> AllQueries { get { return r1.AllQueries.Concat(r2.AllQueries); } }
            public override IEnumerable<CastQuery> AllCastQueries { get { return r1.AllCastQueries.Concat(r2.AllCastQueries); } }
            public override IEnumerable<CastHitQuery> CastHitQueries { get { return r1.CastHitQueries.Concat(r2.CastHitQueries); } }
            public override IEnumerable<CastMissQuery> CastMissQueries { get { return r1.CastMissQueries.Concat(r2.CastMissQueries); } }
            public override IEnumerable<ShadowQuery> ShadowQueries { get { return r1.ShadowQueries.Concat(r2.ShadowQueries); } }
        }

        private class FilterSet : RaySet
        {
            public RaySet r1;
            public Func<CastHitQuery, int, bool> castHitFilter;
            public Func<CastMissQuery, int, bool> castMissFilter;
            public Func<ShadowQuery, int, bool> shadowFilter;

            public override IEnumerable<RayQuery> AllQueries { get { return AllCastQueries.Concat<RayQuery>(ShadowQueries); } }
            public override IEnumerable<CastQuery> AllCastQueries { get { return CastHitQueries.Concat<CastQuery>(CastMissQueries); } }
            public override IEnumerable<CastHitQuery> CastHitQueries { get { return castHitFilter == null ? Enumerable.Empty<CastHitQuery>() : r1.CastHitQueries.Where(castHitFilter); } }
            public override IEnumerable<CastMissQuery> CastMissQueries { get { return castMissFilter == null ? Enumerable.Empty<CastMissQuery>() : r1.CastMissQueries.Where(castMissFilter); } }
            public override IEnumerable<ShadowQuery> ShadowQueries { get { return shadowFilter == null ? Enumerable.Empty<ShadowQuery>() : r1.ShadowQueries.Where(shadowFilter); } }
        }
    }

    public static class RayFileParser
    {
        public static RaySet ReadFromFile1(Stream file)
        {
            List<CastHitQuery> castHits = new List<CastHitQuery>();
            List<CastMissQuery> castMisses = new List<CastMissQuery>();
            List<ShadowQuery> shadows = new List<ShadowQuery>();

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

                if (type == 0)
                {
                    castHits.Add(new CastHitQuery() { Depth = depth, Origin = origin, Difference = dir });
                }
                else if (type == 1)
                {
                    castMisses.Add(new CastMissQuery() { Depth = depth, Origin = origin, Direction = dir });
                }
                else if (type == 2)
                {
                    shadows.Add(new ShadowQuery() { Depth = depth, Origin = origin, Difference = dir, Connected = false });
                }
                else if (type == 3)
                {
                    shadows.Add(new ShadowQuery() { Depth = depth, Origin = origin, Difference = dir, Connected = true });
                }
                else
                {
                    throw new Exception("Error parsing file.");
                }
            }

            return new SimpleRaySet() { Shadows = shadows.ToArray(), CastHits = castHits.ToArray(), CastMisses = castMisses.ToArray() };
        }

        public static RaySet ReadFromFile2(Stream file)
        {
            List<CastHitQuery> castHits = new List<CastHitQuery>();
            List<CastMissQuery> castMisses = new List<CastMissQuery>();
            List<ShadowQuery> shadows = new List<ShadowQuery>();
            
            bool readsT;
            BinaryReader reader = new BinaryReader(file);
            int header = reader.ReadInt32();
            if (header != 1234)
                throw new Exception("Error on first ");
            int version = reader.ReadInt32();
            if (version == 1)
                readsT = true;
            else if (version == 2)
                readsT = false;
            else
                throw new Exception(String.Format("Wrong version! Expected 1 or 2, got {0}", version));
            int numRays = reader.ReadInt32();

            for (int k = 0; k < numRays; k++)
            {
                int type = reader.ReadInt32();
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
                float t =  readsT? reader.ReadSingle():0;
                int rayFooter = reader.ReadInt32();
                if (rayFooter != 0)
                    throw new Exception(String.Format("Ray {0} had bad footer", rayFooter));


                if (type == 0)
                {
                    // do nothing
                }
                else if (type == 1)
                {
                    shadows.Add(new ShadowQuery()
                    {
                        Origin = origin,
                        Difference = dir,
                        Depth = -1,
                        Connected = t > 10000
                    });
                }
                else
                {
                    throw new Exception("Error parsing file.");
                }
            }

            return new SimpleRaySet() { Shadows = shadows.ToArray(), CastHits = castHits.ToArray(), CastMisses = castMisses.ToArray(), HitsNotAnotated = !readsT };
        } 
    }

    public class SimpleRaySet : RaySet
    {
        public override IEnumerable<RayQuery> AllQueries { get { return CastHits.Concat<RayQuery>(CastMisses).Concat(Shadows); } }
        public override IEnumerable<CastQuery> AllCastQueries { get { return CastHits.Concat<CastQuery>(CastMisses); } }
        public override IEnumerable<CastHitQuery> CastHitQueries { get { return CastHits; } }
        public override IEnumerable<CastMissQuery> CastMissQueries { get { return CastMisses; } }
        public override IEnumerable<ShadowQuery> ShadowQueries { get { return Shadows; } }
        
        public CastHitQuery[] CastHits { get; set; }
        public CastMissQuery[] CastMisses { get; set; }
        public ShadowQuery[] Shadows { get; set; }
    }
}
