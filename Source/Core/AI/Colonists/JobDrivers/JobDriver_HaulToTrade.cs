﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class JobDriver_HaulToTrade : JobDriver
    {
        public const TargetIndex CarryThingIndex = TargetIndex.A;
        public const TargetIndex DestIndex = TargetIndex.B;

        public override string GetReport()
        {
            var hauledThing = pawn.carrier.CarriedThing ?? TargetThingA;

            return "ReportHaulingTo".Translate(hauledThing.LabelCap, CurJob.targetB.Thing.LabelBaseShort);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CarryThingIndex);
            this.FailOnDestroyedOrNull(DestIndex);

            yield return Toils_Reserve.Reserve(CarryThingIndex);
            yield return Toils_Reserve.ReserveQueue(CarryThingIndex);

            // TODO check that
            //Gather tradeables loop
            {
                var extractTarget = Toils_JobTransforms.ExtractNextTargetFromQueue(CarryThingIndex);
                yield return extractTarget;
                yield return Toils_Goto.GotoThing(CarryThingIndex, PathEndMode.ClosestTouch)
                    .FailOnDespawnedOrNull(CarryThingIndex)
                    .FailOnBurningImmobile(CarryThingIndex);
                yield return Toils_Haul.StartCarryThing(CarryThingIndex);
                yield return Toils_Jump.JumpIfHaveTargetInQueue(CarryThingIndex, extractTarget);
            }

            yield return Toils_Haul.CarryHauledThingToContainer();
            yield return RA_Toils.DepositHauledThingInContainer(DestIndex, PostTradeAction);
        }

        public void PostTradeAction()
        {
            var tradeCenter = TargetThingB as TradeCenter;
            tradeCenter.colonyGoodsCost += tradeCenter.ThingFinalCost(TargetThingA, TradeAction.PlayerSells);
        }
    }
}

