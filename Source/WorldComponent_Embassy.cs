using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace embassy
{
    public class WorldComponent_Embassy : WorldComponent
    {
        public WorldComponent_Embassy(World world) : base(world)
        {
            
        }

        public override void WorldComponentTick()
        {
            foreach (var map1 in Find.Maps)
            {
                foreach (var zone1 in map1.zoneManager.AllZones.Where(x=> x is Zone_Embassy))
                {
                    var zone = (Zone_Embassy) zone1;
                    if(zone.GetFaction() == null) continue;

                    if (zone.GetAmbassador() == null)
                    {
                        //lay blueprints if no rooms exists in the zone.
                    }
                    else
                    {
                        foreach (var thing in zone.AllContainedThings)
                        {
                            if (thing is Pawn pawn)
                            {
                                if (pawn.Faction != zone.GetFaction()) continue;
                                
                                foreach (var need in pawn.needs.AllNeeds)
                                    need.CurLevel = need.MaxLevel;
                                
                            }else if(thing.def.CanHaveFaction)
                                thing.SetFactionDirect(zone.GetFaction());
                            
                        }
                        //TODO diplomatic action  ticks
                    }
                }
            }
            
            base.WorldComponentTick();
        }
    }
}