using embassy.EmbassyBuilder;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace embassy
{
    public class SymbolResolver_Embassy : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Builder.current.GetFaction() ?? Faction.OfPlayer;
            var widthForDef = WidthForDef(rp, faction);
            SetThingsToBuild(rp);

            Lightin(rp);
            FireFoam(rp, faction);
            Defenses(rp, widthForDef, faction);
            Validate(rp, widthForDef, faction);
            Actual(rp, widthForDef, faction);
            Bridged(rp);
        }

        private static void SetThingsToBuild(ResolveParams rp)
        {
            float thingsToBuild = (float) rp.rect.Area / 144f * 0.17f;
            BaseGen.globalSettings.minEmptyNodes = ((thingsToBuild >= 1f) ? GenMath.RoundRandom(thingsToBuild) : 0);
        }

        private static int WidthForDef(ResolveParams rp, Faction faction)
        {
            int num = 0;
            int? edgeDefenseWidth = rp.edgeDefenseWidth;
            if (edgeDefenseWidth != null)
            {
                num = rp.edgeDefenseWidth.Value;
            }
            else if (rp.rect.Width >= 20 && rp.rect.Height >= 20 &&
                     (faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
            {
                num = ((!Rand.Bool) ? 4 : 2);
            }

            return num;
        }

        private static void Defenses(ResolveParams rp, int num, Faction faction)
        {
            if (num > 0)
            {
                ResolveParams resolveParams3 = rp;
                resolveParams3.faction = faction;
                resolveParams3.edgeDefenseWidth = new int?(num);
                BaseGen.symbolStack.Push("edgeDefense", resolveParams3);
            }
        }

        private static void FireFoam(ResolveParams rp, Faction faction)
        {
            if (faction.def.techLevel >= TechLevel.Industrial)
            {
                int num3 = (!Rand.Chance(0.75f)) ? 0 : GenMath.RoundRandom((float) rp.rect.Area / 400f);
                for (int i = 0; i < num3; i++)
                {
                    ResolveParams resolveParams2 = rp;
                    resolveParams2.faction = faction;
                    BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
                }
            }
        }

        private static void Lightin(ResolveParams rp)
        {
            BaseGen.symbolStack.Push("outdoorLighting", rp);
        }

        private static void Validate(ResolveParams rp, int num, Faction faction)
        {
            ResolveParams resolveParams4 = rp;
            resolveParams4.rect = rp.rect.ContractedBy(num);
            resolveParams4.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4);
        }

        private static void Actual(ResolveParams rp, int num, Faction faction)
        {
            ResolveParams resolveParams5 = rp;
            resolveParams5.rect = rp.rect.ContractedBy(num);
            resolveParams5.faction = faction;
            BaseGen.symbolStack.Push("basePart_outdoors", resolveParams5);
            ResolveParams resolveParams6 = rp;
        }

        private static void Bridged(ResolveParams rp)
        {
            ResolveParams resolveParams6 = rp;
            resolveParams6.floorDef = TerrainDefOf.Bridge;
            bool? floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports;
            resolveParams6.floorOnlyIfTerrainSupports =
                new bool?(floorOnlyIfTerrainSupports == null || floorOnlyIfTerrainSupports.Value);
            BaseGen.symbolStack.Push("floor", resolveParams6);
        }
    }
}