﻿using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace CompTurret.HarmonyInstance
{
    [StaticConstructorOnStartup]
    class MainHarmonyInstance
    {
        static MainHarmonyInstance()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.CompTurret");
        //    harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}