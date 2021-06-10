using System;
using BepInEx;
using HarmonyLib;
using Oc;
using UnityEngine;

namespace TrueMagicShield
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.TrueMagicShield", "TrueMagicShield", "1.0")]
	public class TrueMagicShield : BaseUnityPlugin
	{
		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.TrueMagicShield").PatchAll();
		}

		[HarmonyPatch(typeof(OcCharacter), "setAf_Damaged")]
		public class setAf_Damaged_Patch
		{
			public static bool Prefix(OcCharacter __instance)
			{
				if (__instance.Health.IsPl)
				{
					OcPl pl = __instance as OcPl;
					//OcHealthPl healthPl = __instance.Health as OcHealthPl;
					//float _CurrShieldDurability = (float)Traverse.Create(healthPl).Field("_CurrShieldDurability").GetValue();
					if (pl.PlBuffCtrl.isActive(OcPlBuff.MagicShield))
						return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(OcHealthPl), "solveRestoreMsg")]
		public class solveRestoreMsg_Patch
		{
			public static void Prefix(OcHealthPl __instance, OcRestoreMsg restoreMsg)
			{
				OcPl pl = (OcPl)Traverse.Create(__instance).Field("_Pl").GetValue();
				if (pl.PlBuffCtrl.isActive(OcPlBuff.MagicShield) && __instance.MP_Rate >= 1f - float.Epsilon && restoreMsg.restoreManaVal > 0f)
				{
					float _CurrShieldDurability = (float)Traverse.Create(__instance).Field("_CurrShieldDurability").GetValue();
					float _MaxShieldDurability = (float)Traverse.Create(__instance).Field("_MaxShieldDurability").GetValue();
					float restoreManaVal = restoreMsg.restoreManaVal;
					__instance.ChangeShieldDurability(Math.Min((pl.SkillCtrl.MagicShield_ShieldRate * restoreManaVal) / pl.SkillCtrl.calcConsumeManaRate(), _MaxShieldDurability - _CurrShieldDurability));
				}
			}
		}
	}
}
