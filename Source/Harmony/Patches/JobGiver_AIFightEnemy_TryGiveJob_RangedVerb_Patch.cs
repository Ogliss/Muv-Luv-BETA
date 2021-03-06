﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace MuvLuvAnnihilation.HarmonyInstance
{

    //Because of the way patching work, tamed animal are saviour and smarter at shooting than wild one
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    internal static class JobGiver_AIFightEnemy_TryGiveJob_RangedVerb_Patch
    {
        static bool Prefix(ref JobGiver_AIFightEnemy __instance, ref Job __result, ref Pawn pawn)
        {
            //    Log.Warning(string.Format("Tame animal job detected: {0}", pawn.LabelCap));

            if (pawn.RaceProps.Humanlike)
                return true;
            if (pawn.equipment != null)
            {
                if (pawn.equipment.Primary != null)
                {
                    if (pawn.equipment.PrimaryEq != null)
                    {

                        if (pawn.equipment.PrimaryEq.verbTracker.PrimaryVerb.verbProps.range > 1.5)
                        {
                            return true;
                        }
                    }
                }
            }
            bool hasRangedVerb = false;


            List<Verb> verbList = pawn.verbTracker.AllVerbs;
            List<Verb> rangeList = new List<Verb>();
            if (pawn.health.hediffSet.hediffs.Any(x => x.TryGetComp<HediffComp_VerbGiverBETA>() != null))
            {
                List<Hediff> list = pawn.health.hediffSet.hediffs.FindAll(x => x.TryGetComp<HediffComp_VerbGiverBETA>() != null);

                foreach (Hediff item in list)
                {
                    HediffComp_VerbGiverBETA _VerbGiver = item.TryGetComp<HediffComp_VerbGiverBETA>();
                    if (_VerbGiver != null)
                    {
                        for (int i = 0; i < _VerbGiver.verbTracker.AllVerbs.Count; i++)
                        {
                            //    Log.Warning("Checkity");
                            //    Log.Warning(string.Format("verbList: {0}, Name: {1} RangeMax: {2}", i, verbList[i].verbProps.label, verbList[i].verbProps.range));
                            //It corresponds with verbs anyway
                            if (_VerbGiver.verbTracker.AllVerbs[i].verbProps.range > 1.5f)
                            {
                                if (!rangeList.Contains(_VerbGiver.verbTracker.AllVerbs[i]))
                                {
                                    rangeList.Add(_VerbGiver.verbTracker.AllVerbs[i]);
                                    hasRangedVerb = true;
                                }
                            }
                            //Log.Warning("Added Ranged Verb");
                        }
                    }
                }
                if (hasRangedVerb == false)
                {
                    Log.Warning("I don't have range verb");
                    return true;
                }
            }
            
            // this.SetCurMeleeVerb(updatedAvailableVerbsList.RandomElementByWeight((VerbEntry ve) => ve.SelectionWeight).verb);

            //    Log.Warning(string.Format("rangeVerbs: {0}", rangeList.Count));
            Verb rangeVerb;
            if (rangeList.Count > 1)
            {
                rangeVerb = rangeList.RandomElementByWeightWithDefault((Verb rangeItem) => rangeItem.verbProps.commonality, 0.5f);
            }
            else if (rangeList.Count == 1)
            {
                rangeVerb = rangeList.First();
            }
            else
            {
                rangeVerb = null;
            }

            if (rangeVerb == null)
            {
                //    Log.Warning("Can't get random range verb");
                return true;
            }
            else
            {

                //    Log.Warning(string.Format("rangeVerb: {0}, Range Max: {1}, Min: {2}, Burst: {3}, Projectile: {4}", rangeVerb.verbProps.label, rangeVerb.verbProps.range, rangeVerb.verbProps.minRange, rangeVerb.verbProps.burstShotCount, rangeVerb.verbProps.defaultProjectile));

            }


            Thing enemyTarget = (Thing)AttackTargetFinder.BestAttackTarget((IAttackTargetSearcher)pawn, TargetScanFlags.NeedThreat, (Predicate<Thing>)(x =>
            x is Pawn || x is Building), 0.0f, rangeVerb.verbProps.range, new IntVec3(), float.MaxValue, false);


            if (enemyTarget == null)
            {
                //    Log.Warning("I can't find anything to fight.");
                return true;
            }
            bool useranged = rangeVerb != null;
            //    Log.Warning(string.Format("useranged: {0}", useranged));
            if (!useranged)
            {
                //If adjacent melee attack
                if (enemyTarget.Position.AdjacentTo8Way(pawn.Position))
                {
                    __result = new Job(RimWorld.JobDefOf.AttackMelee, enemyTarget)
                    {
                        maxNumMeleeAttacks = 1,
                        expiryInterval = Rand.Range(420, 900),
                        attackDoorIfTargetLost = false
                    };
                    return false;
                }
                //Only go if I am to be released. This prevent animal running off.
                if (pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly, false))
                {
                    //    Log.Warning("Melee Attack");
                    __result = new Job(RimWorld.JobDefOf.AttackMelee, enemyTarget)
                    {
                        maxNumMeleeAttacks = 1,
                        expiryInterval = Rand.Range(420, 900),
                        attackDoorIfTargetLost = false
                    };
                    return false;
                }
                else
                {
                    return true;
                }
            }

            //Log.Warning("got list of ranged verb");

            //    Log.Warning("Attempting flag");
            bool flag1 = (double)CoverUtility.CalculateOverallBlockChance(pawn.Position, enemyTarget.Position, pawn.Map) > 0.00999999977648258;
            bool flag2 = pawn.Position.Standable(pawn.Map);
            bool flag3 = rangeVerb.CanHitTarget(enemyTarget);
            bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;



            if (flag1 && flag2 && flag3 || flag4 && flag3)
            {
                __result = new Job(DefDatabase<JobDef>.GetNamed("BETARangeAttack"), enemyTarget, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true)
                {
                    verbToUse = rangeVerb

                };
                return false;
            }
            IntVec3 dest;
            bool canShootCondition = false;

            canShootCondition = CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = rangeVerb,
                maxRangeFromTarget = rangeVerb.verbProps.range,
                wantCoverFromTarget = (double)rangeVerb.verbProps.range > 7.0,
                locus = pawn.Position,
                maxRangeFromLocus = Traverse.Create(__instance).Method("GetFlagRadius", pawn).GetValue<float>(),
                maxRegions = 50
            }, out dest);

            if (!canShootCondition)
            {
                //   Log.Warning("I can't move to shooting position");


                return true;
            }

            if (dest == pawn.Position)
            {
                //   Log.Warning("I will stay here and attack");
                __result = new Job(DefDatabase<JobDef>.GetNamed("BETARangeAttack"), enemyTarget, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true)
                {
                    verbToUse = rangeVerb
                };
                return false;
            }
            //   Log.Warning("Going to new place");
            __result = new Job(RimWorld.JobDefOf.Goto, (LocalTargetInfo)dest)
            {
                expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange,
                checkOverrideOnExpire = true
            };
            return false;
        }

    }
}
