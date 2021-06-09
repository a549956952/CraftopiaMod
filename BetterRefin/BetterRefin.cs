using System;
using BepInEx;
using HarmonyLib;
using I2.Loc;
using Oc;
using Oc.Item;
using UnityEngine;

namespace BetterRefin
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.BetterRefin", "BetterRefin", "1.0")]
	public class BetterRefin : BaseUnityPlugin
	{
        private const float lv100 = 1.2f;
        private const float lv200 = 1.2f;
		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.BetterRefin").PatchAll();
		}

		[HarmonyPatch(typeof(ItemData), "ResistColdLv", MethodType.Getter)]
		public class ResistColdLv_Patch
		{
			public static bool Prefix(ItemData __instance, ref int __result)
			{
				if (__instance.Id == 2367)
				{
					__result = 5;
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(ItemData), "CalcAtkMatk")]
		public class CalcAtkMatk_Patch
		{
			public static bool Prefix(ItemData __instance,int value, int level , ref int __result)
			{
				if (!__instance.IsRefinable())
				{
						__result = value;
					return false;
				}
				if (__instance.Id == 2367)
					value *= 10;
				if (level >= 100)
				{
					value = (int)(value * lv100);
				}
				if (level >= 200)
				{
					value = (int)(value * lv200);
				}
				__result = (int)(value * Math.Pow(1.01, level));
				return false;
			}
		}

		[HarmonyPatch(typeof(ItemData), "GetDef")]
		public class GetDef_Patch
		{
			public static void Postfix(ItemData __instance, int level, ref int __result)
			{
				int value = __result;
				if (!__instance.IsRefinable())
				{
					return;
				}
				if (__instance.Id == 2367)
					value = 5;
				if (level >= 100)
				{
					value = (int)(value * lv100);
				}
				if (level >= 200)
				{
					value = (int)(value * lv200);
				}
				__result = (int)(value * Math.Pow(1.005, level));
				return;
			}
		}

		[HarmonyPatch(typeof(ItemData), "CreateLevelText")]
		public class CreateLevelText_Patch
		{
			public static bool Prefix(ItemData __instance,int level, ref string __result)
			{
				if (level >= 255)
				{
					__result = ""+ LocalizationManager.GetTranslation("100_UI/Text/Renew", true, 0, true, false, null, null)+"II";
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(ItemData), "IsRefinable")]
		public class IsRefinable_Patch
		{
			public static bool Prefix(ItemData __instance, ref bool __result)
			{
				if (__instance.ItemType != ItemType.Equipment)
				return true;
				__result = true;
				return false;
			}
		}
	}
}
