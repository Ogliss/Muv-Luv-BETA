﻿using HarmonyLib;
using Verse;

namespace MuvLuvAnnihilation.HarmonyInstance
{
    [HarmonyPatch(typeof(Hediff), "get_BleedRate")]
    public static class HediffWithComps_BleedRate_BETA_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Hediff __instance, ref float __result)
        {
            if (__instance.pawn.def.race.FleshType.defName == "BETAFlesh")
            {
                __result = __result * 0.25f;
            }
        }
    }
}
