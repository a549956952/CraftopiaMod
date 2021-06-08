using System;
using BepInEx;
using HarmonyLib;
using Oc;
using Oc.Em;

namespace PartDamageRateDisplay
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.ShowPartsDamageRate", "ShowPartsDamageRate", "1.0")]
	public class ShowPartsDamageRate : BaseUnityPlugin
	{
		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.ShowPartsDamageRate").PatchAll();
		}

		[HarmonyPatch(typeof(OcHealth), "solveDmgMsg")]
		public class solveDmgMsg_Patch
		{
			public static void Postfix(OcHealth __instance)
			{
				//OcEm em = (OcEm)Traverse.Create(__instance).Field("_Em").GetValue();
				OcDamageMsg LastDmgMsg = (OcDamageMsg)Traverse.Create(__instance).Field("_LastDmgMsg").GetValue();
				bool IsEm = (bool)Traverse.Create(__instance).Field("_IsEm").GetValue();
				OcDamageMsg_RcvChange RcvChange = (OcDamageMsg_RcvChange)Traverse.Create(__instance).Field("_LastDmgMsg_RcvChange").GetValue();
				if (RcvChange != null && LastDmgMsg != null /*&& em != null */&& LastDmgMsg.attackerTrans !=null  && IsEm && LastDmgMsg.attackerTrans.GetComponent<OcPl>()!=null /*&& em.SoEm.IsBoss*/)
				{
					/*String str = string.Format("Damage:{0}\nPart Damage Rate:{1}\nWeakness Rate:{2}\nAtkBoss:{3}\nMAtkBoss:{4}\nFinalDamageRate:{5}", new object[]
					{
					LastDmgMsg.motDamage,
					RcvChange.PartsDamageRate,
					RcvChange.WeaknessRate,
					OcPlMaster.Inst.PlStatusCtrl.getFinalParam(PlParam.AtkBoss),
					OcPlMaster.Inst.PlStatusCtrl.getFinalParam(PlParam.MAtkBoss),
					OcPlMaster.Inst.PlStatusCtrl.getFinalParam(PlParam.FinalDamageRate)
				});*/
					String str = string.Format("Part Damage Rate:{0}", new object[]
					{
					RcvChange.PartsDamageRate
					});
					SingletonMonoBehaviour<OcMessageMng>.Inst.LocalPlMessage(str, false, -1f);
				}
			}
		}
	}
}
