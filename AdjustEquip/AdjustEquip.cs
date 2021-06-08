using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Oc;
using Oc.Item;
using UnityEngine;

namespace AdjustEquip
{
	[BepInPlugin("me.lazycat.plugin.Craftopia.AdjustEquipmentModel", "AdjustEquipmentModel", "1.3")]
	public class AdjustEquip : BaseUnityPlugin
	{
		private static readonly OcEquipSlot[] slots = { OcEquipSlot.Accessory, OcEquipSlot.Accessory_Dummy01, OcEquipSlot.FlightUnit, OcEquipSlot.WpMain, OcEquipSlot.WpSub, OcEquipSlot.WpDual, OcEquipSlot.WpTwoHand, OcEquipSlot.Ammo };

		private ConfigEntry<Vector3>[] Off = new ConfigEntry<Vector3>[12];
		private ConfigEntry<Vector3>[] Sca = new ConfigEntry<Vector3>[12];
		private ConfigEntry<Vector3>[] Rot = new ConfigEntry<Vector3>[12];
		private ConfigEntry<Vector3> WpOnBackOff;
		private ConfigEntry<Vector3> WpOnBackSca;
		private ConfigEntry<Vector3> WpOnBackRot;
		private ConfigEntry<Vector3> BookOff;
		private ConfigEntry<Vector3> BookSca;
		private ConfigEntry<Vector3> BookRot;
		private ConfigEntry<bool> AdjustmentMode;
		private ConfigEntry<bool> RemovJetPack;

		private static EquipData[] equipDatas = new EquipData[12];
		private static EquipData wpOnBack;
		private static EquipData book;
		private static bool Amode;
		private static bool jet;
		private static int delay;
		private void LoadConf()
		{
			Vector3 def = new Vector3(0, 0, 0);

			foreach (OcEquipSlot slot in slots)
			{ 
				Off[(int)slot] = Config.Bind("General",   // The section under which the option is shown
										 slot + " Offset",  // The key of the configuration option in the configuration file
										 def, // The default value
										 slot + " Offset"); // Description of the option to show in the config file
				Sca[(int)slot] = Config.Bind("General",   // The section under which the option is shown
										 slot + " Scale",  // The key of the configuration option in the configuration file
										 new Vector3(0.8f,0.8f,0.8f), // The default value
										 slot + " Scale"); // Description of the option to show in the config file
				Rot[(int)slot] = Config.Bind("General",   // The section under which the option is shown
										 slot + " Rotate",  // The key of the configuration option in the configuration file
										 def, // The default value
										 slot + " Rotate"); // Description of the option to show in the config file
				equipDatas[(int)slot] = new EquipData(Off[(int)slot].Value, Sca[(int)slot].Value, Rot[(int)slot].Value);
			}

			WpOnBackOff = Config.Bind("General", "WpOnBack Offset", def, "WpOnBack Offset");
			WpOnBackSca = Config.Bind("General", "WpOnBack Scale", new Vector3(0.8f, 0.8f, 0.8f), "WpOnBack Scale");
			WpOnBackRot = Config.Bind("General", "WpOnBack Rotate", def, "WpOnBack Rotate");
			BookOff = Config.Bind("General", "Book Offset", def, "Book Offset");
			BookSca = Config.Bind("General", "Book Scale", new Vector3(1f, 1f, 1f), "Book Scale");
			BookRot = Config.Bind("General", "Book Rotate", def, "Book Rotate");

			wpOnBack = new EquipData(WpOnBackOff.Value, WpOnBackSca.Value, WpOnBackRot.Value);
			book = new EquipData(BookOff.Value, BookSca.Value, BookRot.Value);

			AdjustmentMode = base.Config.Bind("General",
												"AdjustmentMode",
												true,
												"In adjustment mode Keep reading settings");
			RemovJetPack = base.Config.Bind("General",
												"RemoveJetPack",
												false,
												"Remove jetpack model");


			Amode = AdjustmentMode.Value;
			jet = RemovJetPack.Value;
		}
		public AdjustEquip()
		{
			delay = 0;
			LoadConf();

		}

		private void Start()
		{
			new Harmony("me.lazycat.plugin.Craftopia.AdjustEquipmentModel").PatchAll();
		}

		private void Update()
		{
			if (Amode)
			{
				delay++;
				if (delay >= 30)
				{
					LoadConf();
					delay = 0;
				}
			}
			else
			{
				delay++;
				if (delay >= 300)
				{
					LoadConf();
					delay = 0;
				}
			}
		}

		[HarmonyPatch(typeof(OcPlEquip), "init")]
		public class init_Patch
		{
			public static void Prefix(OcPlEquip __instance, OcPl pl,ref GameObject itemModel, OcItem item)
			{
				
				if (jet)
				{
					//UnityEngine.Debug.Log("OcPlEquipinit_Patch is working");
					if (item.Id == 799)
					{
						itemModel = null;

					}
				}
			}
        }

		[HarmonyPatch(typeof(OcPl), "lateMove")]
		public class lateMove_Patch
		{
			private static EquipData GetVector3BySlot(OcEquipSlot slot)
			{
				return equipDatas[(int)slot];
			}

			private static void DoAdjust(OcPlEquip equip, EquipData data)
			{
				equip.transform.localPosition += data.Pos;
				equip.transform.localScale = data.Sca;
				equip.transform.Rotate(data.Rot);
			}

			private static readonly OcEquipSlot[] slots = { OcEquipSlot.Accessory, OcEquipSlot.Accessory_Dummy01, OcEquipSlot.FlightUnit, OcEquipSlot.WpMain, OcEquipSlot.WpSub, OcEquipSlot.WpDual, OcEquipSlot.WpTwoHand, OcEquipSlot.Ammo };
			public static void Prefix(OcPl __instance, out bool __state)
			{
				__state = __instance.EquipCtrl.isWpMain_On || __instance.EquipCtrl.isWpSub_On;
			}
			public static void Postfix(OcPl __instance, bool __state)
			{
				if (!__instance.IsInventoryPreviewSlave)
					foreach (OcEquipSlot slot in slots)
					{
						OcPlEquip equip = __instance.EquipCtrl.getEquip(slot);
						if (equip != null)
						{
							if (!__state && slot <= OcEquipSlot.WpDual)
							{
								DoAdjust(equip, wpOnBack);
							}
							else
							{
								EquipData data = GetVector3BySlot(slot);
								DoAdjust(equip, data);
							}

						}
					}
			}
		}

		[HarmonyPatch(typeof(OcVRoidEquipAdjustment), "LateUpdate")]
		public class LateUpdate_Patch
		{
			private static void DoAdjust(Transform transform, EquipData data)
			{
				transform.localPosition += data.Pos;
				transform.localScale = data.Sca;
				transform.Rotate(data.Rot);
			}

			public static void Postfix(OcVRoidEquipAdjustment __instance)
			{
				OcPl pl = (OcPl)Traverse.Create(__instance).Field("pl").GetValue();
				if (pl!=null && !pl.IsInventoryPreviewSlave)
				{
					Transform bookTrans = (Transform)Traverse.Create(__instance).Field("bookTrans").GetValue();
					if (bookTrans != null)
					{
						DoAdjust(bookTrans, book);
					}
				}

			}
		}

	}
}
