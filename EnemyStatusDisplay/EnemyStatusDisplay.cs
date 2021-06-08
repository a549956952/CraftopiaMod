using System;
using BepInEx;
using HarmonyLib;
using Oc;
using Oc.Em;

namespace EnemyStatusDisplay
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.EnemyStatusDisplay", "EnemyStatusDisplay", "1.0")]
	public class ShowStatusValue : BaseUnityPlugin
	{
		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.EnemyStatusDisplay").PatchAll();
		}

		[HarmonyPatch(typeof(OcEnemyHeader), "restart")]
		public class restart_Patch
		{
			public static void Postfix(OcEnemyHeader __instance)
			{
				OcEm _owner = (OcEm)Traverse.Create(__instance).Field("_owner").GetValue();
				if (_owner != null)
				{
					String str = string.Format("Lv.{0} Pw.{1} {2} Hp.{3} Def.{4}", new object[]
					{
					_owner.Level,
					_owner.Strength,
					_owner.CurLv_SoEmData.Name,
					_owner.Health.MaxHP,
					_owner.Health.DEF
					});
					Traverse.Create(__instance).Field("_nameString").SetValue(str);
				} 
			}
		}
	}
}
