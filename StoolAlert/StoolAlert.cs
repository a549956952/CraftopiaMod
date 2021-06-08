using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Oc;
using UnityEngine;

namespace StoolAlert
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.StoolAlert", "StoolAlert", "1.0")]
	public class StoolAlert : BaseUnityPlugin
	{
		private ConfigEntry<String> WarningMessage;
		private ConfigEntry<String> FinalWarningMessage;
		private ConfigEntry<float> StoolInterval;
		private ConfigEntry<bool> Explosion;
		private static string warningMessage;
		private static string finalWarningMessage;
		private static float stoolInterval;
		private static bool explosion;
		public StoolAlert()
        {
			this.WarningMessage = base.Config.Bind("General",
												"WarningMessage",
												"I need to take a leak.",
												"Words to remind you that it's time to go to the toilet.");
			this.FinalWarningMessage = base.Config.Bind("General",
												"FinalWarningMessage",
												"I have to visit the John!",
												"Words to remind you that about to soil yourself.");
			this.StoolInterval = base.Config.Bind("General",
												"StoolInterval",
												1200f,
												"If you don't use other mod to modify the stool interval, you don't need to modify it.");
			this.Explosion = base.Config.Bind("General",
												"StoolExplosion",
												true,
												"If you don't go to the toilet, you'll explode.");

			warningMessage = WarningMessage.Value;
			finalWarningMessage = FinalWarningMessage.Value;
			stoolInterval = StoolInterval.Value;
			explosion = Explosion.Value;
		}
		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.StoolAlert").PatchAll();
		}

		[HarmonyPatch(typeof(OcCharacter), "moveStoolTiemr")]
		public class moveStoolTiemr_Patch
		{
			private static float LastTimer;
			private static bool ShouldWarning;
			public static void Prefix(OcCharacter __instance)
			{
				if (__instance.Health.IsPlMaster)
				{
					float StoolTimer = (float)Traverse.Create(__instance).Field("_StoolTimer").GetValue();
					if (LastTimer < StoolTimer)
						ShouldWarning = true;
					if (StoolTimer <= stoolInterval * .5f && ShouldWarning)
					{
						ShouldWarning = false;
						SingletonMonoBehaviour<OcMessageMng>.Inst.LocalPlMessage(warningMessage, true, 10f);
					}
					if (StoolTimer <= stoolInterval * .1f)
					{
						ShouldWarning = false;
						SingletonMonoBehaviour<OcMessageMng>.Inst.LocalPlMessage(finalWarningMessage, true, -1f);
					}
					LastTimer = StoolTimer;
					if (StoolTimer - Time.deltaTime <= 0 && explosion)
						StoolExplosion(__instance);
				}
			}

            private static void StoolExplosion(OcCharacter __instance)
            {
				Transform _Trans = __instance.transform;
				SingletonMonoBehaviour<OcFxMng>.Inst.callEfc(OcFxMng.GenaralEfcType.GroundShockWave, _Trans.position, _Trans.rotation, null, 1f);
				float lostHP = __instance.Health.HP * 0.5f;
				__instance.Health.setForceHP(Math.Max(lostHP, 1f));
				OcShell component = SingletonMonoBehaviour<OcCharaData>.Inst.Shell_Pl_Skill_Miasma.CreateInst(_Trans.position, SingletonMonoBehaviour<OcCharaData>.Inst.transform).GetComponent<OcShell>();
				OcShell.ShootInfo shootInfo = new OcShell.ShootInfo();
				shootInfo.shooterFront = _Trans.position;
				shootInfo.shooterUp = Vector3.up;
				shootInfo.shootVecN = _Trans.position;
				shootInfo.degLimitPitch = 0f;
				shootInfo.degLimitYaw = 0f;
				shootInfo.speed = 0f;
				shootInfo.ownerHealth = __instance.Health;
				shootInfo.HitEfcRot = new Quaternion();
				shootInfo.HitCamShake = SoCameraShake.PresetType.VH_Mid;
				shootInfo.damageMsg.CriticalRate_Physics = 0f;
				shootInfo.damageMsg.damageReactionType = OcDamageReactionType.None;
				shootInfo.damageMsg.attackAttribute = OcAttackAttribute.PoisonStarter;
				shootInfo.damageMsg.BadCond_PoisonStarterRate = 1f;
				shootInfo.damageMsg.Pushback = OcPushback.Lv4;
				shootInfo.damageMsg.BadCond_PoisonVal = lostHP;
				shootInfo.damageMsg.attackType = OcAttackType.Magic;
				shootInfo.useAttitudeControl = false;
				component.setup(shootInfo);
			}
        }
	}
}
