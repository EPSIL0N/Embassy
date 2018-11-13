#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace embassy.EmbassyBuilder
{
    public static class CellRectExtd
    {
        public static CellRect MinimumBoundingRectangle(IEnumerable<IntVec3> cells)
        {
            int xMin = cells.Min(x => x.x);
            int xMax = cells.Max(x => x.x);
            
            int zMin = cells.Min(x => x.z);
            int zMax = cells.Max(x => x.z);

            return CellRect.FromLimits(xMin, zMin, xMax, zMax);
        }


        public static CellRect? Union(this CellRect r1, CellRect r2)
        {
            if (shareEdge(r1,r2))
            {
                return MinimumBoundingRectangle(r1.Cells.Union(r2.Cells));
            }

            return null;
        }

        private static bool shareEdge(this CellRect r1, CellRect r2)
        {
            int need = Math.Min(Math.Max(r1.EdgeCellsCount, r2.EdgeCellsCount), 2);
            
            var share = CommonCorners(r1.Corners,r2.Corners);
            int have = share.Count();

#if DEBUG
            Log.Message(r1.ToString() + " " + r2 + " need: " + need + " have shared : <=? " + have +" "+share);
#endif
            return have >= need;
        }

        private static IEnumerable<IntVec3> CommonCorners(IEnumerable<IntVec3> r1Corners, IEnumerable<IntVec3> r2Corners)
        {
            var r1s = r1Corners.ToList();
            foreach (var r2Corner in r2Corners)
                foreach (var r1 in r1s)
                {
                    if (r1 == r2Corner)
                        yield return r1;
                    else if (r2Corner.AdjacentToCardinal(r1))
                        yield return r1;
                }
        }


        public static bool doOverlap(this CellRect r1, CellRect r2)
        {
            return doOverlap(r1.TopRight, r1.BottomLeft, r2.TopRight, r2.BottomLeft);
        }
        
        // Returns true if two rectangles (l1, r1) and (l2, r2) overlap 
        public static bool doOverlap(IntVec3 l1, IntVec3 r1, IntVec3 l2, IntVec3 r2) 
        { 
            // If one rectangle is on left side of other 
            if (l1.x > r2.x || l2.x > r1.x) 
                return false; 
  
            // If one rectangle is above other 
            if (l1.z < r2.z || l2.z < r1.z) 
                return false; 
  
            return true; 
        } 
    }
}