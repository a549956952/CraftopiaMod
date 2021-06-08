using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Oc;
using UnityEngine;

namespace BowCanCrit
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.BowCanCrit", "BowCanCrit", "1.1")]
	public class BowCanCrit : BaseUnityPlugin
	{
		private ConfigEntry<float> BaseCritRate;
		private ConfigEntry<float> CharaCritRate;
		private ConfigEntry<bool> UseRayCheckHit;
		private ConfigEntry<bool> MagicalQuiverUseMATK;
		private ConfigEntry<float> MATKRate;

		private static float baseCritRate;
		private static float charaCritRate;
		private static bool useRayCheckHit;
		private static bool magicalQuiverUseMATK;
		private static float mATKRate;
		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.BowCanCrit").PatchAll();
			BaseCritRate = Config.Bind("General",   // The section under which the option is shown
										 "Skill Crit Rate",  // The key of the configuration option in the configuration file
										 1f, // The default value
										 "Physical Crit  Chance = Skill Physical Crit Chance * Skill Crit Rate + Chara Physical Crit Chance * CharaCritRate"); // Description of the option to show in the config file
			CharaCritRate = Config.Bind("General",   // The section under which the option is shown
										 "Chara Crit Rate",  // The key of the configuration option in the configuration file
										 1f, // The default value
										 "Physical Crit Chance = Skill Physical Crit Chance * Skill Crit Rate + Chara Physical Crit Chance * CharaCritRate"); // Description of the option to show in the config file
			UseRayCheckHit = Config.Bind("General",   // The section under which the option is shown
										 "Use Ray Check Hit",  // The key of the configuration option in the configuration file
										 true, // The default value
										 "The collision operation of bow and arrow is carried out by ray, so as to avoid penetration without hitting."); // Description of the option to show in the config file
			MagicalQuiverUseMATK = Config.Bind("General",   // The section under which the option is shown
										 "Magical Quiver Use MATK",  // The key of the configuration option in the configuration file
										 true, // The default value
										 "Magic arrows should be affected by magic attacks, right?"); // Description of the option to show in the config file
			MATKRate = Config.Bind("General",   // The section under which the option is shown
							 "Magical Quiver MATK Rate",  // The key of the configuration option in the configuration file
							 2f, // The default value
							 "Final Attack = Original Attack + Magical Quiver MATK Rate * Magical Quiver Skill Dmg * MATK"); // Description of the option to show in the config file

			baseCritRate = BaseCritRate.Value;
			charaCritRate = CharaCritRate.Value;
			useRayCheckHit = UseRayCheckHit.Value;
			magicalQuiverUseMATK = MagicalQuiverUseMATK.Value;
			mATKRate = MATKRate.Value;
		}

		[HarmonyPatch(typeof(OcShell), "setup", new Type[] { typeof(OcShell.ShootInfo) })]
		public class OcShellSetup_Patch
		{
			public static void Prefix(OcShell __instance, OcShell.ShootInfo shootInfo)
			{
				if ((shootInfo.damageMsg.attackType & OcAttackType.Arrow) > 0 && shootInfo.ownerHealth.IsPl)
				{
					OcPl pl = (OcPl)Traverse.Create(shootInfo.ownerHealth).Field("_Pl").GetValue();
					shootInfo.damageMsg.CriticalRate_Physics = shootInfo.damageMsg.CriticalRate_Physics * baseCritRate + pl.PlStatusCtrl.getFinalParam(PlParam.CriticalProb_Physical) * charaCritRate;
					shootInfo.damageMsg.CriticalDamageRate_Physical = pl.PlStatusCtrl.getFinalParam(PlParam.CriticalDmgRate_Physical) + 0.2f;
					//Traverse.Create(__instance).Field("_UseRayCheckHit").SetValue(true);
					//__instance.setExternal_HitAttach_Use(true);
					if(useRayCheckHit)
						Traverse.Create(__instance).Field("_HitAttach_Use").SetValue(true);
				}
			}

			public static void Postfix(OcShell __instance, OcShell.ShootInfo shootInfo)
			{
				if ((shootInfo.damageMsg.attackType & OcAttackType.Arrow) > 0 && shootInfo.ownerHealth.IsPl)
				{
					OcDamageMsg damageMsg = shootInfo.damageMsg;
					OcPl pl = (OcPl)Traverse.Create(shootInfo.ownerHealth).Field("_Pl").GetValue();
					if (magicalQuiverUseMATK && pl.PlBuffCtrl.isActive(OcPlBuff.MagicalQuiver))
					{
						damageMsg.atk += pl.HealthPl.MATK * pl.SkillCtrl.MagicalQuiver_SkillDmg * mATKRate;
					}
				}
			}

		}


		[HarmonyPatch(typeof(OcShell_JudgementShot_Thunder), "setup", new Type[] { typeof(OcShell.ShootInfo) })]
		public class OcShell_JudgementShot_ThunderSetup_Patch
		{
			public static void Postfix(OcShell __instance, ref OcShell.ShootInfo shootInfo)
			{
				if ((shootInfo.damageMsg.attackType & OcAttackType.Arrow) > 0 && shootInfo.ownerHealth.IsPl)
				{
					if (useRayCheckHit)
						Traverse.Create(__instance).Field("_HitAttach_Use").SetValue(false);
				}
			}
		}

		[HarmonyPatch(typeof(OcShell_JudgementShot_ThunderChain), "setup", new Type[] { typeof(OcShell.ShootInfo) })]
		public class OcShell_JudgementShot_ThunderChainSetup_Patch
		{
			public static void Postfix(OcShell __instance, ref OcShell.ShootInfo shootInfo)
			{
				if ((shootInfo.damageMsg.attackType & OcAttackType.Arrow) > 0 && shootInfo.ownerHealth.IsPl)
				{
					if (useRayCheckHit)
						Traverse.Create(__instance).Field("_HitAttach_Use").SetValue(false);
				}
			}
		}

		[HarmonyPatch(typeof(OcObj_Arrow), "setupForPl")]
		public class SetupForPl_Patch
		{
			public static void Postfix(OcObj_Arrow __instance, int chargeCount)
			{
				OcPl pl = __instance.Pool.OwnerPl;
				OcAttack _Attack = (OcAttack)Traverse.Create(__instance).Field("_Attack").GetValue();
                OcDamageMsg damageMsg = _Attack.DmgMsg;
				damageMsg.CriticalRate_Physics = (0.1f * baseCritRate + pl.PlStatusCtrl.getFinalParam(PlParam.CriticalProb_Physical)) * charaCritRate * (0.6f + 0.2f * chargeCount);
				damageMsg.CriticalDamageRate_Physical = (pl.PlStatusCtrl.getFinalParam(PlParam.CriticalDmgRate_Physical) + 0.2f) * (0.8f + 0.1f * chargeCount);
				if (magicalQuiverUseMATK && pl.PlBuffCtrl.isActive(OcPlBuff.MagicalQuiver))
				{
					damageMsg.atk += pl.HealthPl.MATK * pl.SkillCtrl.MagicalQuiver_SkillDmg * mATKRate;
					pl.setDmg_Magic(damageMsg, false);
				}
			}
        }
	}
}
