﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Komishne.SanguophageTweaks
{
    [HarmonyPatch(typeof(SurgeryOutcome_Death), nameof(SurgeryOutcome_Death.Apply))]
    static class SurgeryOutcome_Death__Apply__Patch
    {
        // This is a transpiler on the method that applies the "death" effect on surgery failure. The goal is to
        // replace the actual call to Pawn::Kill with a helper method, which will more elegantly affect the pawn. A
        // high-level summary of the intended effect of this method is to:
        //   1) Give the pawn a new hediff that indicates they were victim to lethal surgical failure.
        // And that's it, actually. This new hediff will have a lethal severity equal to or less than the initial
        // severity, so it is always fatal. However, this effect is separate from the surgery, and will properly allow
        // a pawn's deathless gene to kick in.
        //
        // The [decompiled] C# lines we are looking for is:
        //   if (!patient.Dead)
        //     patient.Kill();
        //
        // The IL code for these lines are:
        //    // [23 7 - 23 25]
        //    IL_0017: ldarg.s patient
        //    IL_0019: callvirt instance bool Verse.Pawn::get_Dead()
        //    IL_001e: brtrue.s IL_0031
        //    // [24 9 - 24 23]
        //    IL_0020: ldarg.s patient
        //    IL_0022: ldloca.s V_0
        //    IL_0024: initobj valuetype[mscorlib]System.Nullable`1<valuetype Verse.DamageInfo>
        //    IL_002a: ldloc.0      // V_0
        //    IL_002b: ldnull
        //    IL_002c: callvirt instance void Verse.Thing::Kill(valuetype[mscorlib] System.Nullable`1<valuetype Verse.DamageInfo>, class Verse.Hediff)
        //
        // The goal of this transpiler is to find and replace the call to patient.Kill() with a call to a helper
        // method. This helper method will receive at least some of the parameters that Apply itself receives, and the
        // helper method handles killing the pawn, albeit indirectly, so deathless pawns do not also necessarily die.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool abort = false;
            MethodInfo deadPropertyGetterMethodInfo = typeof(Pawn).GetMethod("get_Dead");
            if (deadPropertyGetterMethodInfo is null)
            {
                if (SanguophageTweaksSettings.EnableDebugMode)
                {
                    Log.Error(
                        "[KOM.SanguophageTweaks] MethodInfo for Pawn.Dead (Verse.Pawn::get_Dead()) is null. " +
                        "Aborting transpiler for SurgeryOutcome_Death.Apply.");
                }
                abort = true;
            }

            MethodInfo killMethodInfo = typeof(Thing).GetMethod(nameof(Thing.Kill));
            if (killMethodInfo is null)
            {
                if (SanguophageTweaksSettings.EnableDebugMode)
                {
                    Log.Error(
                        "[KOM.SanguophageTweaks] MethodInfo for Thing.Kill (Verse.Thing::Kill(~~~)) is null. " +
                        "Aborting transpiler for SurgeryOutcome_Death.Apply.");
                }
                abort = true;
            }

            MethodInfo substitueHelperMethod = typeof(Utility).GetMethod(nameof(Utility.ApplySurgeryOnDeathHediff));
            if (substitueHelperMethod is null)
            {
                if (SanguophageTweaksSettings.EnableDebugMode)
                {
                    Log.Error(
                        "[KOM.SanguophageTweaks] MethodInfo for Utility.ApplySurgeryOnDeathHediff " +
                        "(Komishne.SanguophageTweaks.Utility::ApplySurgeryOnDeathHediff(~~~)) is null. " +
                        "Aborting transpiler for SurgeryOutcome_Death.Apply.");
                }
                abort = true;
            }

            if (abort)
            {
                foreach (CodeInstruction instruction in instructions)
                    yield return instruction;
                yield break;
            }

            int seekedInstructionLength = 9;
            var found = false;
            List<CodeInstruction> codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (i < (codes.Count - seekedInstructionLength) &&
                    // first line of code
                    codes[i].opcode == OpCodes.Ldarg_S &&
                    codes[i + 1].Calls(deadPropertyGetterMethodInfo) &&
                    codes[i + 2].opcode == OpCodes.Brtrue_S &&

                    // second line of code
                    codes[i + 3].opcode == OpCodes.Ldarg_S &&
                    codes[i + 4].opcode == OpCodes.Ldloca_S &&
                    codes[i + 5].opcode == OpCodes.Initobj &&
                    codes[i + 6].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 7].opcode == OpCodes.Ldnull &&
                    codes[i + 8].Calls(killMethodInfo))
                {
                    found = true;

                    yield return codes[i];
                    yield return codes[i + 1];
                    yield return codes[i + 2];

                    yield return new CodeInstruction(OpCodes.Ldarg_3);  // i + 3 (surgeon)
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);  // i + 4 (patient)
                    yield return new CodeInstruction(OpCodes.Ldarg_2);  // i + 5 (recipe)
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);  // i + 6 (part)
                    yield return new CodeInstruction(OpCodes.Call, substitueHelperMethod);  // i + 7
                    yield return new CodeInstruction(OpCodes.Nop);  // i + 8

                    i += (seekedInstructionLength - 1);  // Move forward one fewer to account for the loop's i++.
                    continue;
                }

                yield return codes[i];
            }

            if (found == false)
            {
                Log.Error(
                    "[KOM.SanguophageTweaks] Could not find code block to modify in transpiler for " +
                    "SurgeryOutcome_Death.Apply.");
            }
        }
    }
}