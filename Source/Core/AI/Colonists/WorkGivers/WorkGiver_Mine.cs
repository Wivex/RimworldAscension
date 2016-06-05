﻿using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RA
{
    public class WorkGiver_Mine : WorkGiver_WorkWithTools
    {
        public WorkGiver_Mine()
        {
            workType = "Mining";
        }

        public Job ActualJob(Thing target)
        {
            // new mining scheme without hauling rubble
            return new Job(JobDefOf.Mine, target, 1500, true);
        }

        // NonScanJob performed everytime previous(current) job is completed
        public override Job NonScanJob(Pawn pawn)
        {
            // search things throught designations is faster than searching designations through all things
            // all things marked for plantcutting or harvesting
            var designatedTargets = Find.DesignationManager.DesignationsOfDef(DesignationDefOf.Mine).Select(designation => MineUtility.MineableInCell(designation.target.Cell));

            var availableTargets = designatedTargets.Where(target => pawn.CanReserveAndReach(target, PathEndMode.Touch, pawn.NormalMaxDanger()));

            return DoJobWithTool(pawn, availableTargets, ActualJob, availableTargets.Any());
        }
    }
}
