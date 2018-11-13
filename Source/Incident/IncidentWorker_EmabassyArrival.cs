using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace embassy
{
    public class IncidentWorker_EmabassyArrival : IncidentWorker_NeutralGroup
    {
        public IncidentWorker_EmabassyArrival()
        {
        }

        // Token: 0x17000229 RID: 553
        // (get) Token: 0x06000EF7 RID: 3831 RVA: 0x0006F30D File Offset: 0x0006D70D
        protected override PawnGroupKindDef PawnGroupKindDef
        {
            get { return PawnGroupKindDefOf.Trader; }
        }

        // Token: 0x06000EF8 RID: 3832 RVA: 0x0006F314 File Offset: 0x0006D714
        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        { 
            return base.FactionCanBeGroupSource(f, map, desperate) && f.def.caravanTraderKinds.Any<TraderKindDef>();
        }

        // Token: 0x06000EF9 RID: 3833 RVA: 0x0006F338 File Offset: 0x0006D738
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            Map map = (Map) parms.target;
            return parms.faction == null || !NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, parms.faction);
        }

        // Token: 0x06000EFA RID: 3834 RVA: 0x0006F380 File Offset: 0x0006D780
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map) parms.target;
            if (!base.TryResolveParms(parms))
            {
                return false;
            }

            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }

            
            Zone_Embassy zone = null;
            foreach (var zone1 in map.zoneManager.AllZones.Where(z =>
                z is Zone_Embassy zoneEmbassy && zoneEmbassy.GetFaction().Equals(parms.faction) &&
                zoneEmbassy.GetAmbassador() == null))
            {
                zone = (Zone_Embassy) zone1;
                break;
            }

            if (zone == null) return false;
            
            List<Pawn> list = base.SpawnPawns(parms);
            if (list.Count == 0)
            {
                return false;
            }


            foreach (var t in list)
            {
                if (t.needs != null && t.needs.food != null)
                {
                    t.needs.food.CurLevel = t.needs.food.MaxLevel;
                }
            }

            TraderKindDef traderKindDef = null;
            foreach (var pawn in list)
            {
                if (pawn.TraderKind == null) continue;
                
                traderKindDef = pawn.TraderKind;
                    
                zone.SetAmbassador(pawn);
                break;
            }

            string label = "WIP:Embassy Arrival : " + "LetterLabelTraderCaravanArrival"
                               .Translate(parms.faction.Name, traderKindDef.label).CapitalizeFirst();
            string text = "WIP:Embassy Arrival " + "LetterTraderCaravanArrival"
                              .Translate(parms.faction.Name, traderKindDef.label).CapitalizeFirst();
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref label, ref text,
                "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, list[0], parms.faction, null);


            LordJob_Embassy lordJob = new LordJob_Embassy(parms.faction, zone);
            LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
            return true;
        }

        // Token: 0x06000EFB RID: 3835 RVA: 0x0006F554 File Offset: 0x0006D954
        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            parms.points = TraderCaravanUtility.GenerateGuardPoints();
        }
    }
}