using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace embassy
{
    public static class Constants
    {
        public static string YouMayNotKickOutARefugeeCamp = "You may not kick out a refugee camp, you monster.";
        public static string CreateDiploZone = "Create Diplomatic Zone";
        public static string DiploZone = "Diplomatic Zone";
        public static string RefugeeCamp = "Refugee camp";
        public static string Embassy = "Embassy";
        public static Texture2D IconEmbassy() => ContentFinder<Texture2D>.Get("embassy", true);


        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions()
        {
            foreach (var faction in Factions())
            {
                if (faction != null)
                {
                    yield return new FloatMenuOption(faction.Name,
                        delegate() { Designator_ZoneAdd_Embassy.selectedFaction = faction; });
                }
            }
        }

        private static IEnumerable<Faction> Factions()
        {
            return Find.FactionManager.AllFactionsVisibleInViewOrder.Where(f=>!f.HostileTo(Faction.OfPlayer) && f != Faction.OfPlayer);
        }
    }
}