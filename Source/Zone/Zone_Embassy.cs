#define DEBUG
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if DEBUG
using embassy.EmbassyBuilder;
#endif
using RimWorld;
using UnityEngine;
using Verse;

namespace embassy
{
    public class Zone_Embassy : Zone
    {
        private string _factionName;
        private bool _embassyAllow = true;
        private Faction _iFaction;
        private Pawn ambassador;

        public Zone_Embassy() { }

        public Zone_Embassy(Faction faction, ZoneManager zoneManager) : base(Constants.DiploZone, zoneManager)
        {
            SetFaction(faction);
        }

        public void SetAmbassador(Pawn pawn)
        {
            this.ambassador = pawn;
        }
        
        public Pawn GetAmbassador()
        {
            return ambassador;
        }

        public Faction GetFaction()
        {
            if (_iFaction != null) return _iFaction;

            foreach (var faction in Find.FactionManager.AllFactions.Where(x => _factionName.EqualsIgnoreCase(x.Name)))
            {
                _iFaction = faction;
                break;
            }

            return _iFaction;
        }

        public void SetFaction(Faction faction)
        {
            if(faction == null)
            {
                _iFaction = null;
                _factionName = null;
                color = NextZoneColor;
                label = Constants.DiploZone;
            }
            else
            {
                _iFaction = faction;
                _factionName = faction.Name;
                color = faction.Color;
                label = WhatIsLabel(faction);
            }

            color.a = 0.22f;
        }

        public override bool IsMultiselectable => false;

        protected override Color NextZoneColor => Color.blue;


        public override void ExposeData()
        {
            base.ExposeData();
            if (_iFaction != null) //in-case it changed >_>
                _factionName = _iFaction.Name;

            Scribe_Values.Look(ref _factionName, "factionName");
            Scribe_Values.Look(ref _embassyAllow, "embassyAllow");
            Scribe_References.Look(ref ambassador, "ambassador");
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();

            if (TooSmall())
            {
                sb.Append("Not large enough.");
            }

            if (_embassyAllow)
            {
                var faction = GetFaction();

                sb.Append(TypeName(faction)).Append(" of ").Append(faction?.Name ?? _factionName)
                    .Append("\nFaction lead by : ").Append(faction?.leader?.Name?.ToStringFull ?? "(Vacant)")
                    .Append("\nAmbassador : ").Append(ambassador != null ? ambassador.Name.ToStringFull : "(Vacant)");
            }
            else
            {
                sb.Append("Previous Embassy of ").Append(_factionName);
            }

            return sb.ToString();
        }

        public bool TooSmall()
        {
            return this.Cells.Count < 90;
        }

        private string WhatIsLabel(Faction faction)
        {
            return new StringBuilder().Append(TypeName(faction)).Append(" of ").Append(faction?.Name ?? _factionName ?? "(Unassigned)").ToString();
        }
        
        private static string TypeName(Faction faction)
        {
            return faction != null ? (faction.defeated ? Constants.RefugeeCamp: Constants.Embassy) : Constants.DiploZone;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            var faction = GetFaction();
            foreach (var gizmo1 in AddDefaultGizmosForFaction(faction)) yield return gizmo1;

            yield return AllowEmbassy(faction);
        }

        private Command_Toggle AllowEmbassy(Faction faction)
        {
            return new Command_Toggle
            {
                defaultLabel = TypeName(faction),
                defaultDesc = GetInspectString() + Constants.YouMayNotKickOutARefugeeCamp,
                hotKey = KeyBindingDefOf.Command_TogglePower,
                icon = TexCommand.ForbidOff,
                isActive = () => faction.defeated || _embassyAllow,
                toggleAction = delegate
                {
                    if (faction.defeated)
                        Messages.Message(Constants.YouMayNotKickOutARefugeeCamp, ambassador, MessageTypeDefOf.CautionInput,
                            false);
                    else
                        _embassyAllow = !_embassyAllow;
                    //Add penalty for being a bad ally.
                }
            };
        }

        private IEnumerable<Gizmo> AddDefaultGizmosForFaction(Faction faction)
        {
            #if DEBUG
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/AutoRebuild", true),
                defaultLabel = "Build",
                defaultDesc = "Build",
                action = delegate {new Builder(this).Build(); },
                hotKey = KeyBindingDefOf.Misc1
            };
            #endif
            
            if (faction == null || !IsActive(faction))
                foreach (Gizmo g in base.GetGizmos())
                    yield return g;
            else
            {
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone", true),
                    defaultLabel = "CommandRenameZoneLabel".Translate(),
                    defaultDesc = "CommandRenameZoneDesc".Translate(),
                    action = delegate { Find.WindowStack.Add(new Dialog_RenameZone(this)); },
                    hotKey = KeyBindingDefOf.Misc1
                };
                yield return new Command_Toggle
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone", true),
                    defaultLabel = ((!hidden)
                        ? "CommandHideZoneLabel".Translate()
                        : "CommandUnhideZoneLabel".Translate()),
                    defaultDesc = "CommandHideZoneDesc".Translate(),
                    isActive = (() => hidden),
                    toggleAction = delegate
                    {
                        hidden = !hidden;
                        foreach (var loc in Cells)
                            Map.mapDrawer.MapMeshDirty(loc, MapMeshFlag.Zone);
                    },
                    hotKey = KeyBindingDefOf.Misc2
                };
                foreach (Gizmo gizmo in GetZoneAddGizmos())
                {
                    yield return gizmo;
                }
            }
        }

        private bool IsActive(Faction faction)
        {
            return faction.defeated || _embassyAllow;
        }
        
        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Embassy_Expand>();
        }
        
        
    }
}