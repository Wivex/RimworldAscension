﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RA
{
    public class CompUpgradeable : ThingComp
    {
        public CompUpgradeable_Properties Properties => (CompUpgradeable_Properties)props;

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            var gizmo = new Command_Action
            {
                defaultDesc = "Upgrade this so you can get advanced version and save some resources",
                defaultLabel = "Upgrade",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Upgrade"),
                activateSound = SoundDef.Named("Click"),
                action = UpgradeBuilding
            };

            if (!ResearchPrereqsFulfilled)
                gizmo.Disable(DisabledReasonString());

            if (Properties.upgradeTargetDef == null || ThingDef.Named(Properties.upgradeTargetDef.ToString()) == null)
            {
                Log.Error("No such def to upgrade to.");
                yield return null;
            }

            yield return gizmo;
        }

        public string DisabledReasonString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Research Required:");
            foreach (var research in Properties.researchPrerequisites)
            {
                if (!research.IsFinished)
                {
                    stringBuilder.AppendLine(research.LabelCap);
                }
            }
            return stringBuilder.ToString();
        }

        public void UpgradeBuilding()
        {
            parent.Destroy();
            
            // NOTE: GenStuff.DefaultStuffFor require more default stuff types
            PlaceFrameForBuild(Properties.upgradeTargetDef, parent.Position, parent.Rotation, Faction.OfPlayer, GenStuff.DefaultStuffFor(Properties.upgradeTargetDef));
        }

        public void PlaceFrameForBuild(BuildableDef sourceDef, IntVec3 center, Rot4 rotation, Faction faction, ThingDef stuff)
        {
            var frame = (Frame)ThingMaker.MakeThing(sourceDef.frameDef, stuff);
            frame.SetFactionDirect(faction);

            foreach (var resource in frame.MaterialsNeeded())
            {
                var resource1 = ThingMaker.MakeThing(resource.thingDef);
                resource1.stackCount = (int)Math.Round(resource.count * Properties.upgradeDiscountMultiplier);
                frame.resourceContainer.TryAdd(resource1);
            }
            frame.workDone = (int)Math.Round(frame.def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToMake, frame.Stuff) * Properties.upgradeDiscountMultiplier);
            GenSpawn.Spawn(frame, center, rotation);
        }
        
        public bool ResearchPrereqsFulfilled => Properties.researchPrerequisites.All(research => research.IsFinished);
    }

    public class CompUpgradeable_Properties : CompProperties
    {
        public List<ResearchProjectDef> researchPrerequisites = new List<ResearchProjectDef>();
        public ThingDef upgradeTargetDef = new ThingDef();
        public float upgradeDiscountMultiplier = 0.5f;

        public CompUpgradeable_Properties()
        {
            compClass = typeof(CompUpgradeable);
        }
    }
}
