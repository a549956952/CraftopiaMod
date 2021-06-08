using BepInEx;
using HarmonyLib;
using Oc.Item.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace BetterLockEnchant
{
    [BepInPlugin("me.lazycat.plugin.Craftopia.BetterLockEnchant", "BetterLockEnchant", "1.0")]
    public class BetterLockEnchant : BaseUnityPlugin
    {
        private void Start()
        {
            new Harmony("me.lazycat.plugin.Craftopia.BetterLockEnchant").PatchAll();
        }

        [HarmonyPatch(typeof(OcUI_CraftEnchantList), "Awake")]
        public class Awake_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                codes[52].opcode = OpCodes.Ldc_I4_0;
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(OcUI_CraftEnchantList), "Sort")]
        public class Sort_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                codes[64].opcode = OpCodes.Ldc_I4_5;
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(OcUI_CraftEnchantList), "TryLockEnchant")]
        public class TryLockEnchant_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                codes[36].opcode = OpCodes.Ldc_I4_4;
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(OcUI_CraftEnchantList), "Refresh")]
        public class Refresh_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                codes[57].opcode = OpCodes.Ldc_I4_4;
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(OcUI_CraftEnchantList), "GetLockedEnchantID")]
        public class GetLockedEnchantID_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                codes[24].opcode = OpCodes.Ldc_I4_0;
                return codes.AsEnumerable();
            }
        }
    }
}
