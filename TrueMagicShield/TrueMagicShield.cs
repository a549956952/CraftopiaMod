using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Oc;

namespace TrueMagicShield
{
    [BepInPlugin("me.lazycat.plugin.Craftopia.TrueMagicShield", "TrueMagicShield", "1.0")]
    public class TrueMagicShield : BaseUnityPlugin
    {
        private ConfigEntry<bool> MagicShieldSuperArmor;
        private ConfigEntry<bool> MagicShieldAutoCharge;

        private static bool magicShieldSuperArmor;
        private static bool magicShieldAutoCharge;

        public TrueMagicShield()
        {
            this.MagicShieldSuperArmor = base.Config.Bind("General",
                                                "Magic Shield Super Armor",
                                                true,
                                                "Magic shield with super armor effect");
            this.MagicShieldAutoCharge = base.Config.Bind("General",
                                                "Magic Shield Auto Charge",
                                                true,
                                                "Automatically use overflow mana recovery to recharge the magic shield.");
            magicShieldSuperArmor = MagicShieldSuperArmor.Value;
            magicShieldAutoCharge = MagicShieldAutoCharge.Value;

        }
        private void Start()
        {
            new Harmony("me.lazycat.plugin.Craftopia.TrueMagicShield").PatchAll();
        }

        [HarmonyPatch(typeof(OcCharacter), "setAf_Damaged")]
        public class setAf_Damaged_Patch
        {
            public static bool Prefix(OcCharacter __instance)
            {
                if (magicShieldSuperArmor && __instance.Health.IsPl)
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
                if (!magicShieldAutoCharge)
                    return;
                float restoreManaVal = restoreMsg.restoreManaVal;
                if (restoreManaVal <= 0f)
                    return;
                float overflowManaVal = restoreManaVal + __instance.MP - __instance.MaxMP;
                if (overflowManaVal <= 0f)
                    return;
                OcPl pl = (OcPl)Traverse.Create(__instance).Field("_Pl").GetValue();
                if (pl.PlBuffCtrl.isActive(OcPlBuff.MagicShield))
                {
                    float _CurrShieldDurability = (float)Traverse.Create(__instance).Field("_CurrShieldDurability").GetValue();
                    float _MaxShieldDurability = (float)Traverse.Create(__instance).Field("_MaxShieldDurability").GetValue();
                    __instance.ChangeShieldDurability(Math.Min((pl.SkillCtrl.MagicShield_ShieldRate * overflowManaVal) / pl.SkillCtrl.calcConsumeManaRate(), _MaxShieldDurability - _CurrShieldDurability));
                }
            }
        }
    }
}
