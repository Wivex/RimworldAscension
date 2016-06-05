﻿using Verse;

namespace RA
{
    public class ShipWreckFlying : DropPodFlying
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            ticksToImpact = Rand.RangeInclusive(300, 500);
            impactResultThing = (DropPodLanded)ThingMaker.MakeThing(ThingDef.Named("ShipWreckLanded"));
            (impactResultThing as DropPodLanded).cargo = cargo;

            // fixed rotAngle, without tick based rotation
            rotSpeed = 0;
            rotAngle = 45;
        }
    }
}
