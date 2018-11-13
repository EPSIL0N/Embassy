using System.Collections.Generic;
using System.Linq;
using embassy.EmbassyBuilder;
using RimWorld;
using UnityEngine;
using Verse;

namespace embassy
{
    public class Designator_ZoneAdd_Embassy : Designator_ZoneAdd
    {
        public static Faction selectedFaction;

        public Designator_ZoneAdd_Embassy()
        {
            zoneTypeToPlace = typeof(Zone_Embassy);
            defaultLabel = Constants.DiploZone;
            defaultDesc = Constants.CreateDiploZone;
            icon = Constants.IconEmbassy();
            hotKey = KeyBindingDefOf.Misc3;
        }
   
        protected override string NewZoneLabel => Constants.CreateDiploZone;

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return base.CanDesignateCell(c).Accepted;
        }

        protected override Zone MakeNewZone()
        {
            return new Zone_Embassy(selectedFaction, Find.CurrentMap.zoneManager);
        }

        public override void ProcessInput(Event ev)
        {
            if (!CheckCanInteract())
                return;

            if (selectedFaction != null)
                base.ProcessInput(ev);

            Find.WindowStack.Add(new FloatMenu(Constants.GetFloatMenuOptions().ToList()));
        }
        
        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
        
            if (Find.Selector.SelectedZone is Zone_Embassy zone)
            {
                var rectangle = CellRectExtd.MinimumBoundingRectangle(cells.Union(zone.cells));
                Log.Message("Rectangle : "+rectangle);
                cells = rectangle.Cells.Where(cell => !zone.Cells.Contains(cell));
            }

            base.DesignateMultiCell(cells);
        }
    }
}