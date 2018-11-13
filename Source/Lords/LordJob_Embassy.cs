using RimWorld;
using Verse;
using Verse.AI.Group;

namespace embassy
{
    public class LordJob_Embassy : LordJob
    {
	    public LordJob_Embassy()
	    {
	    }

	    public LordJob_Embassy(Faction faction, Zone_Embassy defendZone)
	    {
		    this.faction = faction;
		    this.defendZone = defendZone;
	    }

	    public override bool AddFleeToil => false;

	    // Token: 0x060007E9 RID: 2025 RVA: 0x00044884 File Offset: 0x00042C84
		public override StateGraph CreateGraph()
		{
			var stateGraph = new StateGraph();
			var travel = new LordToil_Travel(defendZone.Cells.RandomElement());
			
			stateGraph.StartingToil = travel;
			
			var buildEmbassy = new LordToil_BuildEmbassy(defendZone);
			stateGraph.AddToil(buildEmbassy);
			
			var defendEmbassy = new LordToil_DefendEmbassy(defendZone);
			stateGraph.AddToil(defendEmbassy);
			
			var exitMapTraderFighting = new LordToil_ExitMapTraderFighting();
			stateGraph.AddToil(exitMapTraderFighting);
			
			
			var fightAndLeave = FightAndLeave(travel, exitMapTraderFighting, defendEmbassy);
			stateGraph.AddTransition(fightAndLeave, false);
			
			stateGraph.AddTransition(ToArms(buildEmbassy, defendEmbassy),false);

			stateGraph.AddTransition(DoneDefending(defendEmbassy, travel), false); 

			var gotThere = GotThere(travel, buildEmbassy);
			stateGraph.AddTransition(gotThere, false);

			var ohNo = OhNo(travel, defendEmbassy);
			stateGraph.AddTransition(ohNo);
			
			return stateGraph;
		}

	    private static Transition ToArms(LordToil_BuildEmbassy buildEmbassy, LordToil_DefendEmbassy defendEmbassy)
	    {
		    Transition transition5 = new Transition(buildEmbassy, defendEmbassy, false, true);
		    transition5.AddTrigger(new Trigger_PawnHarmed(1f, false, null));
		    transition5.AddPreAction(new TransitionAction_SetDefendTrader());
		    transition5.AddPostAction(new TransitionAction_WakeAll());
		    transition5.AddPostAction(new TransitionAction_EndAllJobs());
		    return transition5;
	    }

	    private static Transition DoneDefending(LordToil_DefendEmbassy defendEmbassy, LordToil_Travel travel)
	    {
		    Transition transition6 = new Transition(defendEmbassy, travel, false, true);
		    transition6.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
		    return transition6;
	    }

	    private static Transition FightAndLeave(LordToil_Travel travel, LordToil_ExitMapTraderFighting exitMapTraderFighting,
		    LordToil_DefendEmbassy defendEmbassy)
	    {
		    Transition fightAndLeave = new Transition(travel, exitMapTraderFighting, false, true);
		    fightAndLeave.AddSources(new LordToil[]
		    {
			    defendEmbassy,
		    });
		    fightAndLeave.AddTrigger(new Trigger_FractionPawnsLost(0.8f)); //the embassy is lost.
		    fightAndLeave.AddPostAction(new TransitionAction_EndAllJobs());
		    return fightAndLeave;
	    }

	    private static Transition GotThere(LordToil_Travel travel, LordToil defendEmbassy)
	    {
		    Transition gotThere = new Transition(travel, defendEmbassy, false, true);
		    gotThere.AddTrigger(new Trigger_Memo("TravelArrived"));
		    return gotThere;
	    }

	    private static Transition OhNo(LordToil_Travel travel, LordToil_DefendEmbassy defendEmbassy)
	    {
		    Transition ohNo = new Transition(travel, defendEmbassy, false, true);
		    ohNo.AddSources(new LordToil[]
		    {
			    defendEmbassy,
		    });
		    ohNo.canMoveToSameState = true;
		    ohNo.AddTrigger(new Trigger_PawnHarmed(1f, false, null));
		    ohNo.AddPreAction(new TransitionAction_SetDefendTrader());
		    ohNo.AddPostAction(new TransitionAction_WakeAll());
		    ohNo.AddPostAction(new TransitionAction_EndAllJobs());
		    return ohNo;
	    }
	    
	    // Token: 0x060007EA RID: 2026 RVA: 0x00044CA0 File Offset: 0x000430A0
		public override void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			
			Scribe_References.Look<Zone_Embassy>(ref this.defendZone, "defendZone");
		}

		// Token: 0x04000372 RID: 882
		private Faction faction;
	    private Zone_Embassy defendZone;
    }
}