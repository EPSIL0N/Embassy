
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace embassy
{
    public class LordToil_DefendEmbassy : LordToil_DefendPoint
    {
        public LordToil_DefendEmbassy() : base(true)
        {
        }

        public LordToil_DefendEmbassy(Zone_Embassy defendZone) : base(defendZone.Cells.RandomElement(), 28f)
        {
        }

        public override bool AllowSatisfyLongNeeds => true;

        public override float? CustomWakeThreshold => new float?(0.5f);

        public override void UpdateAllDuties()
        {
            LordToilData_DefendPoint data = base.Data;
            Pawn pawn = TraderCaravanUtility.FindTrader(this.lord);
            if (pawn != null)
            {
                pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint, data.defendRadius);
                foreach (var pawn2 in this.lord.ownedPawns)
                {
                    //TODO own roles for embassy but this basically applies for MVP.
                    switch (pawn2.GetTraderCaravanRole()) 
                    {
                        case TraderCaravanRole.Carrier:
                            pawn2.mindState.duty = new PawnDuty(DutyDefOf.Follow, pawn, 5f);
                            pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
                            break;
                        case TraderCaravanRole.Guard:
                            pawn2.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint, data.defendRadius);
                            break;
                        case TraderCaravanRole.Chattel:
                            pawn2.mindState.duty = new PawnDuty(DutyDefOf.Escort, pawn, 5f);
                            pawn2.mindState.duty.locomotion = LocomotionUrgency.Walk;
                            break;
                    }
                }
                return;
            }
        }
    }
}