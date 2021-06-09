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

		[HarmonyPatch(typeof(OcPlSkillCtrl), "move")]
		public class move_Patch
		{
			public static void Prefix(OcPlSkillCtrl __instance)
			{
				OcPl pl = (OcPl)Traverse.Create(__instance).Field("_Pl").GetValue();
				OcHealthPl Health = pl.HealthPl;
				float _SkillPassive_GenMP_Timer = (float)Traverse.Create(__instance).Field("_SkillPassive_GenMP_Timer").GetValue();
				if (pl.PlBuffCtrl.isActive(OcPlBuff.MagicShield) && Health.MP_Rate >= 1f - float.Epsilon && _SkillPassive_GenMP_Timer - Time.deltaTime < 0f)
				{
					float _CurrShieldDurability = (float)Traverse.Create(Health).Field("_CurrShieldDurability").GetValue();
					float _MaxShieldDurability = (float)Traverse.Create(Health).Field("_MaxShieldDurability").GetValue();
					float restoreManaVal = pl.SkillCtrl.GenMP_Regen * pl.SkillCtrl.MPRegeneration_Rate * pl.PlStatusCtrl.getFinalParam(PlParam.ManaRegenerateRate);
					pl.PlStatusCtrl.getFinalParam(PlParam.ManaConsumeRate);
					Health.ChangeShieldDurability(Math.Min((pl.SkillCtrl.MagicShield_ShieldRate * restoreManaVal) / pl.SkillCtrl.calcConsumeManaRate(), _MaxShieldDurability - _CurrShieldDurability));
				}
			}
		}
	}
}
