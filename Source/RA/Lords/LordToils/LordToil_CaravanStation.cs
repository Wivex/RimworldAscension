﻿using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RA
{
    public class LordToil_CaravanStation : LordToil
    {
        public override IntVec3 FlagLoc => TraderCaravanUtility.FindTrader(lord).Position;

        public override void UpdateAllDuties()
        {
            var trader = TraderCaravanUtility.FindTrader(lord);
            if (trader != null)
            {
                trader.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Station"));
                foreach (var pawn in lord.ownedPawns)
                {
                    switch (pawn.GetCaravanRole())
                    {
                        case TraderCaravanRole.Carrier:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, trader, 5f);
                            break;
                        case TraderCaravanRole.Guard:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, trader.Position, 10);
                            break;
                        case TraderCaravanRole.Chattel:
                            pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, trader, 8f);
                            break;
                    }
                }
            }
        }
    }
}