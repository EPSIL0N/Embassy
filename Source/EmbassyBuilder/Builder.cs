using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace embassy.EmbassyBuilder
{
    public class Builder
    {
        public Builder(Zone_Embassy embassy)
        {
            this.embassy = embassy;
        }

        public void Build()
        {
	        
            var map = Find.CurrentMap;
            BaseGen.globalSettings.map = map;


            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = CellRectExtd.MinimumBoundingRectangle(embassy.Cells);
            resolveParams.faction = embassy.GetFaction();


            BaseGen.symbolStack.Push("embassy", resolveParams);
	        current = embassy;
            var lookup = current.AllContainedThings.ToLookup(x => x.GetUniqueLoadID());
            
            BaseGen.Generate();//not thread safe.
            
            
            
            foreach (Thing _thing in current.AllContainedThings.Where(x=>!lookup.Contains(x.GetUniqueLoadID())))
            {
                
                var buildingDef = _thing.def.entityDefToBuild;
                if (buildingDef == null || !(_thing is Building building)) continue;
                building.Destroy();
                
                Blueprint(buildingDef, _thing, map);
                
                
            }
            
            embassy.AddCell(resolveParams.rect.TopRight);
            embassy.AddCell(resolveParams.rect.BottomLeft);
            
            
        }

        private void Blueprint(BuildableDef buildingDef, Thing _thing, Map map)
        {
            List<Thing> list = new List<Thing>();
            var bp = GenConstruct.PlaceBlueprintForBuild(buildingDef, _thing.Position, map, _thing.Rotation,
                embassy.GetFaction(), _thing.Stuff);
            foreach (ThingDefCountClass cost in bp.MaterialsNeeded())
            {
                Thing thing = list.FirstOrDefault((Thing t) => t.def == cost.thingDef);
                if (thing != null)
                {
                    thing.stackCount += cost.count;
                }
                else
                {
                    Thing thing2 = ThingMaker.MakeThing(cost.thingDef, null);
                    thing2.stackCount = cost.count;
                    list.Add(thing2);
                }
            }

            foreach (var t in list)
            {
                t.stackCount = Mathf.CeilToInt((float) t.stackCount * Rand.Range(1f, 1.2f));
            }
            
            List<List<Thing>> list2 = new List<List<Thing>>();
            for (int j = 0; j < list.Count; j++)
            {
                while (list[j].stackCount > list[j].def.stackLimit)
                {
                    int num = Mathf.CeilToInt((float)list[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
                    Thing thing4 = ThingMaker.MakeThing(list[j].def, null);
                    thing4.stackCount = num;
                    list[j].stackCount -= num;
                    list.Add(thing4);
                }
            }
            DropPodUtility.DropThingGroupsNear(_thing.Position, map, list2, 110, false, false, true);
        }

        private Zone_Embassy embassy;

	    public static Zone_Embassy current;
    }
}