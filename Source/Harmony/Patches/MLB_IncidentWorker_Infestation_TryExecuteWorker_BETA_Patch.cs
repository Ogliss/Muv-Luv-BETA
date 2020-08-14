﻿using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace MuvLuvBeta.HarmonyInstance
{
    [HarmonyPatch(typeof(ExtraHives.IncidentWorker_Infestation), "TryExecuteWorker")]
    public static class MLB_IncidentWorker_Infestation_TryExecuteWorker_BETA_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ref IncidentParms parms)
        {
            if (parms.target is Map && (parms.target as Map).IsPlayerHome)
            {
                if (parms.faction != null && (parms.faction.def.defName.Contains("MuvLuv_BETA")))
                {
                    if ((parms.target is Map map))
                    {
                        float mult = 1f;
                        if (parms.faction.def.defName.Contains("2P"))
                        {
                            mult = 1.2f;
                        }
                        if (parms.faction.def.defName.Contains("3P"))
                        {
                            mult = 1.4f;
                        }
                        if (parms.faction.def.defName.Contains("4P"))
                        {
                            mult = 1.6f;
                        }
                        if (parms.faction.def.defName.Contains("5P"))
                        {
                            mult = 1.8f;
                        }
                        if (parms.faction.def.defName.Contains("6P"))
                        {
                            mult = 2f;
                        }
                	//        Log.Message("IncidentWorker_RaidEnemy points: " + parms.points + " Mult: " + mult + " Result: " + parms.points * mult);
                        parms.points = parms.points * mult;

                    }
                }
            }
        }
        private static readonly IntRange RaidDelay = new IntRange(1000, 2000);
    }
}